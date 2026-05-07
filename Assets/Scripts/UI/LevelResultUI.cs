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
        [SerializeField] private Button restartButton;

        [Header("Settings")]
        [SerializeField] private Color activeStarColor = Color.yellow;
        [SerializeField] private Color inactiveStarColor = Color.gray;

        private void Awake()
        {
            // Eğer Restart butonu atanmamışsa, Menü butonundan otomatik kopyala
            if (restartButton == null && menuButton != null)
            {
                GameObject restartGO = Instantiate(menuButton.gameObject, menuButton.transform.parent);
                restartGO.name = "RestartButton";
                restartButton = restartGO.GetComponent<Button>();
                
                var btnText = restartGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (btnText != null) btnText.text = "RESTART";

                // Menü butonunun yanına/altına itmek için (Basit offset)
                restartGO.transform.localPosition += new Vector3(0, -120, 0); 
            }

            menuButton.onClick.AddListener(OnMenuClicked);
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
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
            // Zamanı normale döndür
            Time.timeScale = 1f;
            
            // Ana Menüye Dön
            SceneManager.LoadScene(1); 
        }

        private void OnNextLevelClicked()
        {
            // CampaignManager üzerinden sıradaki bölümü yükleme mantığı 
            // Şimdilik sadece menüye dönmek de yeterli olabilir
            OnMenuClicked();
        }

        private void OnRestartClicked()
        {
            // Zamanı normale döndür
            Time.timeScale = 1f;
            
            // CampaignManager üzerinden temiz bir restart at
            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.RestartLevel();
            }
            else
            {
                // Fallback: Manager yoksa sahneyi yükle (ama gri ekran riski var)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
