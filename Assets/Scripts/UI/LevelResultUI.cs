using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using TowerDefence.Core;
using TowerDefence.Combat;

namespace TowerDefence.UI
{
    public class LevelResultUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextLevelButton;

        [Header("Settings")]
        [SerializeField] private Color activeStarColor = Color.yellow;
        [SerializeField] private Color inactiveStarColor = Color.gray;

        private void Awake()
        {
            menuButton.onClick.AddListener(OnMenuClicked);
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        public void Show(bool isVictory, int livesRemaining, int totalLives)
        {
            gameObject.SetActive(true);
            titleText.text = isVictory ? "VICTORY!" : "DEFEAT...";
            titleText.color = isVictory ? Color.green : Color.red;

            int stars = 0;
            if (isVictory)
            {
                float lifePercent = (float)livesRemaining / totalLives;
                if (lifePercent >= 1f) stars = 3;
                else if (lifePercent >= 0.5f) stars = 2;
                else stars = 1;

                // Veriyi Kaydet
                var currentLevel = CampaignManager.Instance.GetCurrentLevel();
                if (currentLevel != null)
                {
                    MetaProgressionManager.Instance.SaveLevelProgress(currentLevel.levelID, stars);
                    CampaignManager.Instance.CompleteCurrentLevel();
                }
            }

            // Yıldızları Görselleştir
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = (i < stars) ? activeStarColor : inactiveStarColor;
            }

            // Next Level Butonu Kontrolü
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(isVictory);
            }
        }

        private void OnMenuClicked()
        {
            // Ana Menüye Dön (Senin projende index 1 MainScene/Menu sahnesidir)
            SceneManager.LoadScene(1); 
        }

        private void OnNextLevelClicked()
        {
            // CampaignManager üzerinden sıradaki bölümü yükleme mantığı 
            // Şimdilik sadece menüye dönmek de yeterli olabilir
            OnMenuClicked();
        }
    }
}
