using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;
using TowerDefence.Combat;
using Unity.AI.Navigation;
using UnityEngine.AI;

namespace TowerDefence.Grid
{
    public class LevelInitializer : MonoBehaviour
    {
        private void Start()
        {
            InitializeLevel();
        }

        public void InitializeLevel()
        {
            LevelData currentLevel = CampaignManager.Instance.GetCurrentLevel();

            if (currentLevel == null)
            {
                Debug.LogError("LevelInitializer: No level data found!");
                return;
            }

            if (currentLevel.mapPrefab != null)
            {
                // Haritayı oluştur
                GameObject mapInstance = Instantiate(currentLevel.mapPrefab, currentLevel.mapPrefab.transform.position, currentLevel.mapPrefab.transform.rotation);
                Debug.Log($"LevelInitializer: Map '{currentLevel.levelName}' spawned.");

                // Haritadaki kuleleri tara ve oyuncunun seçtiği tarafa göre rakip karşılıklarına dönüştür
                Side playerSide = SideController.Instance.GetPlayerSide();
                Tower[] mapTowers = mapInstance.GetComponentsInChildren<Tower>();
                
                foreach (var tower in mapTowers)
                {
                    TowerData currentData = tower.GetTowerData();
                    if (currentData != null && currentData.side == playerSide)
                    {
                        if (currentData.enemyCounterpart != null)
                        {
                            tower.Initialize(currentData.enemyCounterpart);
                            Debug.Log($"LevelInitializer: Map tower '{currentData.towerName}' swapped to opponent counterpart '{currentData.enemyCounterpart.towerName}'.");
                        }
                    }
                }

                // NavMesh sistemi kaldırıldı, waypoint bazlı hareket sistemine geçildi.
            }
            else
            {
                Debug.LogWarning($"LevelInitializer: No map prefab assigned for {currentLevel.levelName}");
            }
        }
    }
}
