using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TowerDefence.Data;
using TowerDefence.Core;

[InitializeOnLoad]
public static class AutoWaveBalancer
{
    static AutoWaveBalancer()
    {
        if (File.Exists("Assets/RunBalancer.txt"))
        {
            File.Delete("Assets/RunBalancer.txt");
            EditorApplication.delayCall += BalanceAll;
        }
    }

    public static void Trigger()
    {
        File.WriteAllText("Assets/RunBalancer.txt", "run");
        AssetDatabase.Refresh();
    }

    static void BalanceAll()
    {
        Debug.Log("Starting AutoWaveBalancer with dynamic path/tower analysis...");

        // Tier'lar (Sadece normal unit'ler)
        string[][] tiers = new string[][]
        {
            new string[] { "Goblin_Grunt", "Orc_Marauder", "Skeleton_Warrior", "Zombie_Shambler" }, // 1-10
            new string[] { "Orc_Berserker", "Skeleton_Archer", "Dark_Elf_Sniper", "Troll_Brute" }, // 11-20
            new string[] { "Cultist_Initiate", "Spider_Rider", "Gargoyle", "Succubus" }, // 21-30
            new string[] { "Dark_Knight", "Shadow_Assassin", "Necromancer", "Vampire_Lord" }, // 31-40
            new string[] { "Wraith", "Bone_Golem", "Lich" }  // 41-50
        };
        // Gerçek Boss'lar (Dosya yapısına göre)
        string[] bosses = new string[] { "Abyssal_Behemoth", "Blood_Mage", "Bone_Dragon", "Death_Knight_Commander", "Demon_King", "Shadow_Leviathan" };

        Dictionary<string, UnitData> unitDict = new Dictionary<string, UnitData>();
        string[] unitGuids = AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/Data/Units" });
        foreach (var guid in unitGuids)
        {
            UnitData ud = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (ud != null) unitDict[ud.name] = ud;
        }

        string[] levelGuids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Levels" });
        foreach (var guid in levelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (levelData == null) continue;

            int levelNum = ExtractLevelNumber(levelData.name);
            if (levelNum < 1) levelNum = 1;
            
            int tierIndex = (levelNum - 1) / 10;
            if (tierIndex > 4) tierIndex = 4;
            if (tierIndex < 0) tierIndex = 0;

            int levelSubTier = ((levelNum - 1) % 10) + 1; // 1 ile 10 arası güç eğrisi (Örn: Level 31 -> 1, Level 40 -> 10)
            
            string[] currentTierUnits = tiers[tierIndex];

            // 1. Path ve Tower analizini yap
            int numPaths = Mathf.Max(1, levelData.paths.Count);
            int numTowers = levelData.customSlotPositions.Count;
            
            int[] towersPerPath = new int[numPaths];
            float interactionRange = 15f; // Bir kulenin bir path'i "görebileceği" tahmini menzil

            for (int p = 0; p < numPaths; p++)
            {
                var pPoints = levelData.paths[p].points;
                if (pPoints.Count < 2) continue;

                int towersCovering = 0;
                foreach (var tPos in levelData.customSlotPositions)
                {
                    bool covers = false;
                    for (int i = 0; i < pPoints.Count - 1; i++)
                    {
                        Vector3 closest = GetClosestPointOnSegment(tPos, pPoints[i], pPoints[i+1]);
                        if (Vector3.Distance(tPos, closest) <= interactionRange)
                        {
                            covers = true;
                            break;
                        }
                    }
                    if (covers) towersCovering++;
                }
                towersPerPath[p] = towersCovering;
            }

            // 2. Wave sayısını belirle (Az tower/path = az wave, çok tower/path = çok wave)
            // Ortalama: 3 base wave + her path için 2 + her 3 kule için 1.
            int totalWaves = Mathf.Clamp(3 + (numPaths * 2) + (numTowers / 3), 3, 15);
            levelData.waves = new List<WaveData>();
            string levelDir = Path.GetDirectoryName(path);

            // Mevcut wave'leri temizle (Artık sayı değiştiği için eskilere güvenemeyiz, ama dosyaları tutabiliriz)
            for (int w = 1; w <= totalWaves; w++)
            {
                string waveAssetPath = $"{levelDir}/Wave{w}.asset";
                WaveData waveData = AssetDatabase.LoadAssetAtPath<WaveData>(waveAssetPath);
                
                if (waveData == null)
                {
                    waveData = ScriptableObject.CreateInstance<WaveData>();
                    AssetDatabase.CreateAsset(waveData, waveAssetPath);
                }
                waveData.unitGroups = new List<WaveUnitGroup>();
                waveData.timeBeforeNextWave = 15f;

                if (w < totalWaves)
                {
                    // NORMAL WAVES
                    // Hem level zorluğu (levelSubTier 1-10) hem de wave numarasına göre unit sayısı artar.
                    for (int p = 0; p < numPaths; p++)
                    {
                        // Path'i ne kadar kule görüyorsa oradan o kadar fazla ve sık düşman gelmeli!
                        int covering = Mathf.Max(1, towersPerPath[p]);
                        
                        // Temel Unit Count: (Wave başına) + (Kule başına) + (Level Zorluğu)
                        int baseCount = 3 + (w * 2) + (covering * 2) + levelSubTier;
                        float baseInterval = Mathf.Max(0.5f, 2.5f - (w * 0.1f) - (levelSubTier * 0.05f));

                        string unitName = currentTierUnits[(w + p) % currentTierUnits.Length];
                        if (unitDict.TryGetValue(unitName, out UnitData ud))
                        {
                            waveData.unitGroups.Add(new WaveUnitGroup
                            {
                                unitData = ud,
                                count = baseCount,
                                spawnInterval = baseInterval,
                                groupDelay = p * 1.5f,
                                spawnerIndex = p
                            });
                        }
                        
                        // Path'i gören kule sayısı ve level zorluğu (örneğin 1-10 arası) yüksekse
                        // İkinci bir düşman grubu da aynı yoldan ekle (zorlaştırma)
                        if (w > 3 && (covering > 3 || levelSubTier > 5))
                        {
                            string unitName2 = currentTierUnits[(w + p + 1) % currentTierUnits.Length];
                            if (unitDict.TryGetValue(unitName2, out UnitData ud2))
                            {
                                waveData.unitGroups.Add(new WaveUnitGroup
                                {
                                    unitData = ud2,
                                    count = baseCount / 2,
                                    spawnInterval = baseInterval * 0.8f,
                                    groupDelay = (p * 1.5f) + 4f, // Biraz sonra çıksınlar
                                    spawnerIndex = p
                                });
                            }
                        }
                    }
                }
                else
                {
                    // SON WAVE: SADECE BOSS
                    waveData.timeBeforeNextWave = 30f;
                    
                    for (int p = 0; p < numPaths; p++)
                    {
                        int covering = Mathf.Max(1, towersPerPath[p]);
                        
                        // Kule sayısı çoksa veya level 8-10 arasıysa daha fazla boss!
                        int bossCount = 1 + (levelSubTier / 4) + (covering / 4);

                        // 1. Boss Grubu
                        string bossName = bosses[(tierIndex + p) % bosses.Length];
                        if (unitDict.TryGetValue(bossName, out UnitData bd))
                        {
                            waveData.unitGroups.Add(new WaveUnitGroup
                            {
                                unitData = bd,
                                count = bossCount,
                                spawnInterval = 3f,
                                groupDelay = p * 2f,
                                spawnerIndex = p
                            });
                        }
                        
                        // Birden fazla unit group olmalı
                        string bossName2 = bosses[(tierIndex + p + 1) % bosses.Length];
                        if (unitDict.TryGetValue(bossName2, out UnitData bd2))
                        {
                            waveData.unitGroups.Add(new WaveUnitGroup
                            {
                                unitData = bd2,
                                count = Mathf.Max(1, bossCount / 2),
                                spawnInterval = 3f,
                                groupDelay = (p * 2f) + 6f, // 1. bosslardan 6 sn sonra
                                spawnerIndex = p
                            });
                        }
                    }
                }
                EditorUtility.SetDirty(waveData);
                levelData.waves.Add(waveData);
            }
            EditorUtility.SetDirty(levelData);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("AutoWaveBalancer completed successfully! Analysed paths and towers.");
    }

    static int ExtractLevelNumber(string name)
    {
        string numStr = new string(name.Where(char.IsDigit).ToArray());
        if (int.TryParse(numStr, out int num)) return num;
        return 1;
    }
    
    static Vector3 GetClosestPointOnSegment(Vector3 pt, Vector3 pA, Vector3 pB)
    {
        Vector3 ap = pt - pA;
        Vector3 ab = pB - pA;
        float magAB2 = ab.sqrMagnitude;
        float abapProduct = Vector3.Dot(ap, ab);
        float dist = abapProduct / magAB2;

        if (dist < 0) return pA;
        else if (dist > 1) return pB;
        else return pA + ab * dist;
    }
}
