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
                // Generate map
                GameObject mapInstance = Instantiate(currentLevel.mapPrefab, currentLevel.mapPrefab.transform.position, currentLevel.mapPrefab.transform.rotation);

                // Scan towers on map and convert to opposing counterparts based on player's chosen side
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
                        }
                    }
                }

                // NavMesh system removed; migrated to waypoint-based movement.
            }
        }
    }
}
