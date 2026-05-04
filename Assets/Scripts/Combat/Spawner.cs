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
            if (currentLevel != null && waveIndex < currentLevel.waves.Count)
            {
                WaveData wave = currentLevel.waves[waveIndex];
                
                // Sadece bu spawner'a ait grupları filtrele
                foreach (var group in wave.unitGroups)
                {
                    if (group.spawnerIndex == spawnerIndex)
                    {
                        activeWaveGroups.Add(group);
                    }
                }

                if (activeWaveGroups.Count > 0)
                {
                    isSpawning = true;
                    nextSpawnTime = Time.time;
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
                        SpawnUnit(group.unitData);
                        unitsSpawnedInGroup++;
                        nextSpawnTime = Time.time + group.spawnInterval;
                    }
                }
                else
                {
                    // Grubu bitir, sonrakine geç
                    currentGroupIndex++;
                    unitsSpawnedInGroup = 0;
                }
            }
            else
            {
                // Tüm gruplar bitti
                isSpawning = false;
                Debug.Log($"Spawner [{spawnerIndex}]: Wave {currentWaveIndex + 1} complete.");
            }
        }

        public void ManualSpawnAtPosition(UnitData unitData, Vector3 position, PathWaypoints path, int targetWpIdx = -1)
        {
            if (unitData == null || unitData.prefab == null) return;

            Vector3 spawnPos = position;

            GameObject unitGO = Instantiate(unitData.prefab, spawnPos, Quaternion.identity);
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Initialize(unitData);
                if (targetWpIdx >= 0)
                {
                    unit.SetPathWithExactTarget(path, targetWpIdx);
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

            if (AudioManager.Instance != null && unitData.spawnSFX != null)
            {
                AudioManager.Instance.PlaySFX(unitData.spawnSFX);
            }
        }

        public void ManualSpawn(UnitData unitData)
        {
            if (unitData == null) return;
            SpawnUnit(unitData, true);
        }

        private void SpawnUnit(UnitData unitData, bool isManual = false)
        {
            if (unitData == null) return;

            UnitData finalUnitData = unitData;

            // --- Dinamik Taraf Kontrolü ---
            // Sadece otomatik dalga üretimlerinde aynalama yap (Düşman ordusu oluşturmak için)
            // Oyuncu kendisi basıyorsa direkt kendi tarafını basar
            if (!isManual)
            {
                Side playerSide = SideController.Instance.GetPlayerSide();
                if (unitData.side == playerSide && unitData.enemyCounterpart != null)
                {
                    finalUnitData = unitData.enemyCounterpart;
                }
            }

            if (finalUnitData.prefab == null) return;

            Vector3 spawnPos = spawnPoint.position;
            // NavMesh.SamplePosition blokları kaldırıldı.

            GameObject unitGO = Instantiate(finalUnitData.prefab, spawnPos, spawnPoint.rotation);
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Initialize(finalUnitData);
                
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

            if (AudioManager.Instance != null && finalUnitData.spawnSFX != null)
            {
                AudioManager.Instance.PlaySFX(finalUnitData.spawnSFX);
            }
        }
    }
}
