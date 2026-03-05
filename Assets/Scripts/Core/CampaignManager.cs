using UnityEngine;
using UnityEngine.SceneManagement;
using TowerDefence.Data;

namespace TowerDefence.Core
{
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        [SerializeField] private LevelData currentSelectedLevel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [SerializeField] private System.Collections.Generic.List<LevelData> allLevels;

        public void SelectLevel(LevelData level)
        {
            currentSelectedLevel = level;
            Debug.Log($"Level Selected: {level.levelName}");
        }

        public void LoadSelectedLevel()
        {
            if (currentSelectedLevel != null)
            {
                // Mevcut wave indexini de sıfırlayalım
                PhaseManager.Instance.ResetWaveIndex();
                
                SceneManager.LoadScene(currentSelectedLevel.sceneIndex);
                GameManager.Instance.ChangeState(GameState.Playing);
                
                // Sahne yüklendiğinde ilk hazırlığı başlat
                PhaseManager.Instance.StartPreparationPhase();
            }
        }

        public void CompleteCurrentLevel()
        {
            if (currentSelectedLevel == null) return;

            Debug.Log($"Level Completed: {currentSelectedLevel.levelName}");

            // Seviye bitiş ödülü (Karma)
            int reward = 50; // Temel ödül, LevelData'dan çekilebilir
            MetaProgressionManager.Instance.AddKarma(reward);

            // Bir sonraki seviyenin kilidini aç (Yazılımsal mantık)
            UnlockNextLevel();

            // Veriyi kaydet
            // SaveManager.Instance.SaveGame(); // Gelecek için
        }

        private void UnlockNextLevel()
        {
            int currentIndex = allLevels.IndexOf(currentSelectedLevel);
            if (currentIndex >= 0 && currentIndex < allLevels.Count - 1)
            {
                LevelData nextLevel = allLevels[currentIndex + 1];
                // LevelData'da bir 'isUnlocked' bool'u olduğunu varsayıyoruz veya 
                // SaveManager üzerinden bir index tutabiliriz. 
                // Şimdilik debug mesajı:
                Debug.Log($"Next Level Unlocked: {nextLevel.levelName}");
            }
        }

        public bool IsLevelUnlocked(LevelData level)
        {
            if (allLevels == null || allLevels.Count == 0) return true;
            
            int index = allLevels.IndexOf(level);
            if (index == 0) return true; // İlk bölüm her zaman açık
            
            // Gerçek projede SaveData'dan kontrol edilir. 
            // Şimdilik basitlik için SaveManager'daki bir 'highestLevel' değerine bakabiliriz.
            // Ama şimdilik sadece yazılım iskeletini kuruyoruz:
            return true; // Şimdilik testi kolaylaştırmak için true dönüyoruz
        }

        public LevelData GetCurrentLevel() => currentSelectedLevel;
    }
}
