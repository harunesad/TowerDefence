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

        [Header("Reward UI")]
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button doubleRewardButton;

        private int pendingKarma = 0;
        private int pendingCrystals = 0;
        private bool hasDoubled = false;

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
            if (doubleRewardButton != null)
                doubleRewardButton.onClick.AddListener(OnDoubleRewardClicked);
        }

        public void Show(bool isVictory, int livesRemaining, int totalLives)
        {
            gameObject.SetActive(true);
            hasDoubled = false;
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

            // Ödül miktarlarını hesapla (CompleteCurrentLevel zaten verdi, burada sadece göstermek için)
            int difficulty = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
            if (isVictory)
            {
                pendingKarma = 50 * difficulty;
                pendingCrystals = 10 * difficulty;
            }
            else
            {
                pendingKarma = 20; // Teselli ödülü (GameManager'da verildi)
                pendingCrystals = 0;
            }

            // Ödül metnini güncelle
            UpdateRewardText();

            // 2X butonunu göster
            if (doubleRewardButton != null)
                doubleRewardButton.gameObject.SetActive(true);

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

        private void UpdateRewardText()
        {
            if (rewardText == null) return;

            string text = $"+{pendingKarma} Karma";
            if (pendingCrystals > 0)
                text += $"  +{pendingCrystals} Crystals";
            if (hasDoubled)
                text += "  (2X!)";
            rewardText.text = text;
        }

        private void OnDoubleRewardClicked()
        {
            if (hasDoubled) return;

            if (AdManager.Instance != null)
            {
                AdManager.Instance.ShowRewardedAd(
                    onSuccess: () =>
                    {
                        hasDoubled = true;

                        // Ekstra ödül ver (ilk ödüller zaten verilmişti, şimdi bir katını daha ekle)
                        if (MetaProgressionManager.Instance != null)
                        {
                            MetaProgressionManager.Instance.AddKarma(pendingKarma);
                            if (pendingCrystals > 0)
                                MetaProgressionManager.Instance.AddCrystals(pendingCrystals);
                        }

                        pendingKarma *= 2;
                        pendingCrystals *= 2;
                        UpdateRewardText();

                        if (doubleRewardButton != null)
                            doubleRewardButton.gameObject.SetActive(false);
                    },
                    onFailed: () =>
                    {
                        Debug.Log("Double reward ad failed.");
                    }
                );
            }
        }

        private void OnMenuClicked()
        {
            // Zamanı normale döndür
            Time.timeScale = 1f;
            
            // Interstitial reklam göster, sonra ana menüye dön
            if (AdManager.Instance != null)
            {
                AdManager.Instance.ShowInterstitialAd(onCompleted: () =>
                {
                    SceneManager.LoadScene(1);
                });
            }
            else
            {
                SceneManager.LoadScene(1);
            }
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
