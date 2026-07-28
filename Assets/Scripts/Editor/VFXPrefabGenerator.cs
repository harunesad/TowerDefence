using UnityEngine;
using UnityEditor;
using TowerDefence.Core;
using System.Collections.Generic;

namespace TowerDefence.Editor
{
    /// <summary>
    /// Bu script, Asset Store'dan indirilen AAA kaliteli (Hovl Studio ve JMO Assets)
    /// prefabları otomatik olarak bularak bizim sisteme (PooledVFX) entegre eder.
    /// </summary>
    public static class VFXPrefabGenerator
    {
        private const string VFX_PREFAB_PATH = "Assets/Prefabs/VFX";

        [MenuItem("Tower Defence/Generate/VFX Prefabs (Asset Store Integration)")]
        public static void GenerateVFXPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(VFX_PREFAB_PATH))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "VFX");
            }

            // Eşleştirmeler: Bizim enum adımız -> İndirilen paketteki orijinal Prefab adı
            var map = new Dictionary<string, string>()
            {
                { "VFX_LightImpact", "Holy hit" }, // Hovl
                { "VFX_DarkImpact", "Red energy explosion" }, // Hovl
                
                { "VFX_HealingAura", "Healing circle" }, // Hovl
                { "VFX_CorruptionPulse", "CFXR2 Poison Cloud" }, // JMO
                { "VFX_UnitSpawn", "CFXR Magic Poof" }, // JMO
                
                { "VFX_UnitDeath", "CFXR Explosion 1" }, // JMO
                { "VFX_SlowEffect", "Freeze circle" }, // Hovl
                
                { "VFX_SpellMeteor", "Explosion" }, // Hovl
                { "VFX_SpellEarthquake", "CFXR2 Ground Hit" }, // JMO
                { "VFX_SpellPlagueRain", "CFXR2 Poison Cloud" }, // JMO (Şimdilik zehir bulutu)
                
                { "VFX_MuzzleFlashLight", "CFXR Flash" }, // JMO
                { "VFX_MuzzleFlashDark", "CFXR Flash" }, // JMO
                
                { "VFX_EconomyGold", "CFXR2 Shiny Item (Loop)" }, // JMO
                { "VFX_EconomySoul", "CFXR2 Shiny Item (Loop)" }, // JMO 
                { "VFX_UpgradeSparkle", "CFXR2 Shiny Item (Loop)" } 
            };

            int successCount = 0;

            foreach (var kvp in map)
            {
                string targetName = kvp.Key;
                string sourceName = kvp.Value;

                // Asset veritabanında prefabı bul
                string[] guids = AssetDatabase.FindAssets(sourceName + " t:Prefab");
                if (guids.Length == 0)
                {
                    Debug.LogWarning($"[VFX] Bulunamadı: '{sourceName}' (Hedef: {targetName}). Lütfen paketlerin import edildiğinden emin ol.");
                    continue;
                }

                // Genellikle en kısa path doğru olan asıl prefabdır
                string sourcePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                
                if (sourcePrefab == null) continue;

                // Sahnede geçici olarak yarat
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
                
                // PooledVFX bileşenini ekle 
                var pooled = instance.GetComponent<TowerDefence.VFX.PooledVFX>();
                if (pooled == null)
                {
                    pooled = instance.AddComponent<TowerDefence.VFX.PooledVFX>();
                }

                // Türü belirle
                SerializedObject so = new SerializedObject(pooled);
                
                string typeStr = targetName.Replace("VFX_", "");
                if (System.Enum.TryParse(typeStr, out VFXType vfxType))
                {
                    so.FindProperty("type").intValue = (int)vfxType;
                }

                // Yaşam süresini (lifetime) hesapla
                float maxDuration = 3f; // Default
                var ps = instance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    maxDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                }
                else
                {
                    // Alt objelerdeki particle systemleri kontrol et
                    var pss = instance.GetComponentsInChildren<ParticleSystem>();
                    foreach(var p in pss)
                    {
                        float dur = p.main.duration + p.main.startLifetime.constantMax;
                        if (dur > maxDuration) maxDuration = dur;
                    }
                }
                
                so.FindProperty("lifeTime").floatValue = maxDuration + 0.5f;
                so.FindProperty("autoReturnByTime").boolValue = true;
                
                // Bütün efektler (Loop edenler dahil) belirli bir süre sonra havuza dönmeli
                // Çünkü oyun mimarimizde kalıcı aura takibi yapılmıyor (Büyüler ve Kahraman yetenekleri anlık).
                // so.FindProperty("autoReturnByTime").boolValue = false; // İPTAL EDİLDİ
                
                so.ApplyModifiedProperties();

                // Prefab olarak kaydet
                string savePath = $"{VFX_PREFAB_PATH}/{targetName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, savePath);
                GameObject.DestroyImmediate(instance);
                
                successCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[VFX] {successCount} / {map.Count} AAA kalite efekt başarıyla sisteme entegre edildi!");
        }
    }
}
