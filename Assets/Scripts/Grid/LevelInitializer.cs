using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;
using Unity.AI.Navigation;

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
                GameObject mapInstance = Instantiate(currentLevel.mapPrefab, Vector3.zero, Quaternion.identity);
                Debug.Log($"LevelInitializer: Map '{currentLevel.levelName}' spawned.");

                // Dinamik NavMesh Fırınlama (Bake)
                // Unity.AI.Navigation paketi yüklü ise NavMeshSurface kullanılır
                var navSurface = mapInstance.GetComponentInChildren<NavMeshSurface>();
                if (navSurface != null)
                {
                    navSurface.BuildNavMesh();
                    Debug.Log("LevelInitializer: NavMesh Baked successfully.");
                }
                else
                {
                    Debug.LogWarning("LevelInitializer: NavMeshSurface not found on map prefab! Soldiers might not move.");
                }
            }
            else
            {
                Debug.LogWarning($"LevelInitializer: No map prefab assigned for {currentLevel.levelName}");
            }
        }
    }
}
