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
            int nextIndex = currentIndex + 1;
            
            if (nextIndex < allLevels.Count)
            {
                // Eğer açılan yeni bölüm, kayıttaki en yüksek bölümden büyükse kaydı güncelle
                if (nextIndex > MetaProgressionManager.Instance.GetHighestUnlockedLevel())
                {
                    MetaProgressionManager.Instance.UpdateHighestLevel(nextIndex);
                    Debug.Log($"Next Level Unlocked and Saved: {allLevels[nextIndex].levelName}");
                }
            }
        }

        public bool IsLevelUnlocked(LevelData level)
        {
            if (allLevels == null || allLevels.Count == 0) return true;
            
            int index = allLevels.IndexOf(level);
            if (index <= 0) return true; // İlk bölüm veya liste dışı (hata koruması) her zaman açık
            
            // Kayıtlı en yüksek bölüm indeksinden küçük veya eşitse açıktır
            return index <= MetaProgressionManager.Instance.GetHighestUnlockedLevel();
        }

        public LevelData GetCurrentLevel() => currentSelectedLevel;
    }
}
