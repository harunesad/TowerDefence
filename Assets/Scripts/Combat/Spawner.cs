using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    public class Spawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform spawnPoint;
        [Header("Settings")]
        public int spawnerIndex = 0;
        public System.Collections.Generic.List<PathWaypoints> assignedPaths = new System.Collections.Generic.List<PathWaypoints>();
        public bool isPlayerSpawner = false;

        public static System.Collections.Generic.List<Spawner> AllSpawners { get; private set; } = new System.Collections.Generic.List<Spawner>();

        private int currentWaveIndex = -1;
        public int CurrentWaveIndex => currentWaveIndex;
        private System.Collections.Generic.List<WaveUnitGroup> activeWaveGroups = new System.Collections.Generic.List<WaveUnitGroup>();
        private int currentGroupIndex = 0;
        private int unitsSpawnedInGroup = 0;
        private float nextSpawnTime;
        private bool isSpawning = false;
        public bool IsSpawning => isSpawning;

        private void Awake()
        {
            AllSpawners.Add(this);
        }

        private void OnDestroy()
        {
            AllSpawners.Remove(this);
        }

        public static void SpawnPlayerUnits(UnitData unitData)
        {
            foreach (var spawner in AllSpawners)
            {
                if (spawner.isPlayerSpawner)
                {
                    spawner.ManualSpawn(unitData);
                }
            }
        }
        private void Update()
        {
            if (isPlayerSpawner) return; // Oyuncu spawner'ı dalga beklemek yerine manuel tetiklenir
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.HoldsUnitMarch()) return;
            if (GameManager.Instance != null && GameManager.Instance.HasMatchEnded()) return;

            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat)
            {
                isSpawning = false;
                return;
            }

            // Yeni bir dalga başladığında kontrol et
            int phaseWaveIndex = PhaseManager.Instance.GetCurrentWaveIndex();
            if (currentWaveIndex != phaseWaveIndex)
            {
                StartNewWave(phaseWaveIndex);
            }

            if (isSpawning)
            {
                HandleSpawning();
            }
        }

        private void StartNewWave(int waveIndex)
        {
            currentWaveIndex = waveIndex;
            currentGroupIndex = 0;
            unitsSpawnedInGroup = 0;
            activeWaveGroups.Clear();

            LevelData currentLevel = CampaignManager.Instance.GetCurrentLevel();
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.ReplaceWithOneBoss(waveIndex))
            {
                UnitData boss = TutorialDirector.Instance.LessonBossData();
                if (boss != null)
                {
                    if (spawnerIndex != 0) return;
                    activeWaveGroups.Add(new WaveUnitGroup
                    {
                        unitData = boss,
                        count = 1,
                        spawnInterval = 0.5f,
                        groupDelay = 0.4f,
                        spawnerIndex = 0
                    });
                    isSpawning = true;
                    nextSpawnTime = Time.time + 0.4f;
                    return;
                }
            }
            if (currentLevel != null && waveIndex < currentLevel.waves.Count)
            {
                WaveData wave = currentLevel.waves[waveIndex];
                
                // Sadece bu spawner'a ait grupları filtrele
                foreach (var group in wave.unitGroups)
                {
                    if (group.spawnerIndex == spawnerIndex)
                    {
                        activeWaveGroups.Add(new WaveUnitGroup
                        {
                            unitData = group.unitData,
                            count = CampaignManager.ScaleGroupCount(currentLevel.levelID, group.count),
                            spawnInterval = group.spawnInterval,
                            groupDelay = group.groupDelay,
                            spawnerIndex = group.spawnerIndex
                        });
                    }
                }

                if (activeWaveGroups.Count > 0)
                {
                    isSpawning = true;
                    nextSpawnTime = Time.time + activeWaveGroups[0].groupDelay;
                    Debug.Log($"Spawner [{spawnerIndex}]: Wave {waveIndex + 1} started with {activeWaveGroups.Count} groups.");
                }
                else
                {
                    isSpawning = false;
                    Debug.Log($"Spawner [{spawnerIndex}]: No groups for this spawner in Wave {waveIndex + 1}.");
                }
            }
            else
            {
                isSpawning = false;
            }
        }

        private void HandleSpawning()
        {
            if (currentGroupIndex < activeWaveGroups.Count)
            {
                WaveUnitGroup group = activeWaveGroups[currentGroupIndex];

                if (unitsSpawnedInGroup < group.count)
                {
                    if (Time.time >= nextSpawnTime)
                    {
                        if (SpawnUnit(group.unitData))
                            unitsSpawnedInGroup++;
                        nextSpawnTime = Time.time + group.spawnInterval;
                    }
                }
                else
                {
                    // Grubu bitir, sonrakine geç
                    currentGroupIndex++;
                    unitsSpawnedInGroup = 0;
                    
                    // Yeni grup varsa onun delay süresini uygula
                    if (currentGroupIndex < activeWaveGroups.Count)
                    {
                        nextSpawnTime = Time.time + activeWaveGroups[currentGroupIndex].groupDelay;
                    }
                }
            }
            else
            {
                // Tüm gruplar bitti
                isSpawning = false;
                Debug.Log($"Spawner [{spawnerIndex}]: Wave {currentWaveIndex + 1} complete.");
            }
        }

        public void ManualSpawnAtPosition(UnitData unitData, Vector3 position, PathWaypoints path, int targetWpIdx = -1, bool walkBackward = false)
        {
            if (unitData == null || unitData.prefab == null) return;

            Vector3 spawnPos = position;

            GameObject unitGO = Instantiate(unitData.prefab, spawnPos, Quaternion.identity);
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null)
            {
                float diffMultiplier = 1f;
                if (SideController.Instance != null && unitData.side != SideController.Instance.GetPlayerSide() && CampaignManager.Instance != null)
                    diffMultiplier = CampaignManager.Instance.GetCombatDifficultyMultiplier();

                unit.Initialize(unitData, diffMultiplier);
                if (diffMultiplier > 1f) unit.SetKillRewardMultiplier(diffMultiplier);
                if (targetWpIdx >= 0)
                {
                    unit.SetPathWithExactTarget(path, targetWpIdx, walkBackward);
                }
                else
                {
                    unit.SetPathAtNearestWaypoint(path, position);
                }
            }

            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, position, Quaternion.identity);
            }
        }

        public void ManualSpawn(UnitData unitData)
        {
            if (unitData == null) return;
            SpawnUnit(unitData, true);
        }

        private bool SpawnUnit(UnitData unitData, bool isManual = false)
        {
            if (unitData == null) return false;

            UnitData finalUnitData = unitData;

            // --- Dinamik Taraf Kontrolü ---
            // Sadece otomatik dalga üretimlerinde aynalama yap (Düşman ordusu oluşturmak için)
            // Oyuncu kendisi basıyorsa direkt kendi tarafını basar
            if (!isManual)
            {
                Side playerSide = SideController.Instance.GetPlayerSide();
                if (unitData.side == playerSide)
                {
                    if (unitData.enemyCounterpart == null)
                    {
                        Debug.LogWarning($"[Spawner] {unitData.name} has no enemy counterpart. Wave unit skipped.");
                        return false;
                    }
                    finalUnitData = unitData.enemyCounterpart;
                }
            }

            if (finalUnitData.prefab == null) return false;

            Vector3 spawnPos = spawnPoint.position;

            GameObject unitGO = Instantiate(finalUnitData.prefab, spawnPos, spawnPoint.rotation);
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null)
            {
                float diffMultiplier = 1f;
                if (SideController.Instance != null && finalUnitData.side != SideController.Instance.GetPlayerSide() && CampaignManager.Instance != null)
                    diffMultiplier = CampaignManager.Instance.GetCombatDifficultyMultiplier();

                unit.Initialize(finalUnitData, diffMultiplier);
                if (diffMultiplier > 1f) unit.SetKillRewardMultiplier(diffMultiplier);
                
                // Atanan yollardan rastgele birini seç
                if (assignedPaths != null && assignedPaths.Count > 0)
                {
                    int randomIndex = Random.Range(0, assignedPaths.Count);
                    unit.SetPath(assignedPaths[randomIndex]);
                }
            }

            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, spawnPoint.position, spawnPoint.rotation);
            }

            return true;
        }
    }
}
