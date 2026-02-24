using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    public class Spawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private PathWaypoints assignedPath;

        private int currentWaveIndex = -1;
        private int currentGroupIndex = 0;
        private int unitsSpawnedInGroup = 0;
        private float nextSpawnTime;
        private bool isSpawning = false;

        private void Update()
        {
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
            
            LevelData currentLevel = CampaignManager.Instance.GetCurrentLevel();
            if (currentLevel != null && waveIndex < currentLevel.waves.Count)
            {
                isSpawning = true;
                nextSpawnTime = Time.time;
                Debug.Log($"Spawner: Wave {waveIndex + 1} started!");
            }
            else
            {
                isSpawning = false;
                Debug.Log("Spawner: No more waves or LevelData missing.");
            }
        }

        private void HandleSpawning()
        {
            LevelData currentLevel = CampaignManager.Instance.GetCurrentLevel();
            WaveData wave = currentLevel.waves[currentWaveIndex];

            if (currentGroupIndex < wave.unitGroups.Count)
            {
                WaveUnitGroup group = wave.unitGroups[currentGroupIndex];

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
                Debug.Log($"Spawner: Wave {currentWaveIndex + 1} spawning complete.");
            }
        }

        private void SpawnUnit(UnitData unitData)
        {
            if (unitData == null || unitData.prefab == null) return;

            GameObject unitGO = Instantiate(unitData.prefab, spawnPoint.position, spawnPoint.rotation);
            Unit unit = unitGO.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Initialize(unitData);
                unit.SetPath(assignedPath);
            }

            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, spawnPoint.position, spawnPoint.rotation);
            }

            if (AudioManager.Instance != null && unitData.spawnSFX != null)
            {
                AudioManager.Instance.PlaySFX(unitData.spawnSFX);
            }
        }
    }
}
