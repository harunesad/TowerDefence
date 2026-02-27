using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;

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

                // Eğer harita içinde Spawner'lar varsa onları aktive etmeye hazırız.
                // Bizim sistemimizde Spawner'lar genelde prefabın içinde olur.
            }
            else
            {
                Debug.LogWarning($"LevelInitializer: No map prefab assigned for {currentLevel.levelName}");
            }
        }
    }
}
