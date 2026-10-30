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
        [SerializeField] private Button closeButton; // Sadece Sweep modunda veya kapatmak için

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
            if (closeButton != null)
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public void Show(bool isVictory, int livesRemaining, int totalLives)
        {
            gameObject.SetActive(true);
            hasDoubled = false;
            titleText.text = isVictory ? "VICTORY!" : "DEFEAT...";
            titleText.color = isVictory ? Color.green : Color.red;

            int difficulty = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
            int stars = 0;

            if (isVictory)
            {
                float lifePercent = (float)livesRemaining / totalLives;
                if (lifePercent >= 1f) stars = 3;
                else if (lifePercent >= 0.5f) stars = 2;
                else stars = 1;

                // Yıldız sayısına göre ödül (Örn: 3 yıldız -> 75 karma, 1 yıldız -> 25 karma)
                pendingKarma = 25 * stars * difficulty;
                pendingCrystals = 5 * stars * difficulty;

                // Bölümü tamamla ve ödülleri oyuncuya ver
                if (CampaignManager.Instance != null)
                {
                    CampaignManager.Instance.CompleteCurrentLevel(stars, pendingKarma, pendingCrystals);
                }

                if (doubleRewardButton != null)
                    doubleRewardButton.gameObject.SetActive(true);
            }
            else
            {
                pendingKarma = 0;
                pendingCrystals = 0;

                if (doubleRewardButton != null)
                    doubleRewardButton.gameObject.SetActive(false);
            }

            // Ödül metnini güncelle
            UpdateRewardText();

            // Yıldızları Görselleştir
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = (i < stars) ? activeStarColor : inactiveStarColor;
            }

            // Next Level Butonu Kontrolü
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(isVictory);
            
            // Diğer butonları aç, Close'u kapat
            if (menuButton != null) 
            {
                menuButton.gameObject.SetActive(true);
                var txt = menuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = "MAIN MENU"; // Varsayılan metni geri yükle
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(OnMenuClicked); // Varsayılan metodu geri bağla
            }
            if (restartButton != null) restartButton.gameObject.SetActive(true);
            if (closeButton != null) closeButton.gameObject.SetActive(false);
        }

        public void ShowSweepResult()
        {
            gameObject.SetActive(true);
            hasDoubled = false;
            titleText.text = "LEVEL CLEARED!";
            titleText.color = Color.green;

            int difficulty = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
            int stars = 3;

            // Oynamadan geçildiği için normalin (75 karma, 15 kristal) çok daha azı verilir. (Örn: 10 karma, 2 kristal * zorluk)
            pendingKarma = 10 * difficulty;
            pendingCrystals = 2 * difficulty;

            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.CompleteCurrentLevel(stars, pendingKarma, pendingCrystals);
            }

            // Ödül metnini güncelle
            UpdateRewardText();

            // Yıldızları Görselleştir
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = activeStarColor;
            }

            // Normal butonları gizle, sadece Close butonunu göster
            if (menuButton != null) menuButton.gameObject.SetActive(false);
            if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(false);
            if (doubleRewardButton != null) doubleRewardButton.gameObject.SetActive(false);

            if (closeButton != null) 
            {
                closeButton.gameObject.SetActive(true);
            }
            else 
            {
                // Fallback: Eğer kullanıcı Close butonu koymamışsa, Menu butonunu kapatıcı olarak kullan
                if (menuButton != null)
                {
                    menuButton.gameObject.SetActive(true);
                    var txt = menuButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null) txt.text = "CLOSE";
                    menuButton.onClick.RemoveAllListeners();
                    menuButton.onClick.AddListener(() => gameObject.SetActive(false));
                }
            }
        }

        private void UpdateRewardText()
        {
            if (rewardText == null) return;

            if (pendingKarma <= 0 && pendingCrystals <= 0)
            {
                rewardText.text = "No Rewards";
                return;
            }

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
            // Zamanı GameSpeedManager üzerinden normale döndür
            if (TowerDefence.Core.GameSpeedManager.Instance != null)
                TowerDefence.Core.GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;
            
            // Interstitial reklam göster, sonra ana menüye dön
            if (TowerDefence.Core.AdManager.Instance != null)
            {
                TowerDefence.Core.AdManager.Instance.ShowInterstitialAd(onCompleted: () =>
                {
                    TowerDefence.Core.SceneLoader.EnsureInstance();
                    if (TowerDefence.Core.SceneLoader.Instance != null) TowerDefence.Core.SceneLoader.Instance.LoadScene(1);
                });
            }
            else
            {
                TowerDefence.Core.SceneLoader.EnsureInstance();
                if (TowerDefence.Core.SceneLoader.Instance != null) TowerDefence.Core.SceneLoader.Instance.LoadScene(1);
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
            // Zamanı GameSpeedManager üzerinden normale döndür
            if (TowerDefence.Core.GameSpeedManager.Instance != null)
                TowerDefence.Core.GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;
            
            // CampaignManager üzerinden temiz bir restart at
            if (TowerDefence.Core.CampaignManager.Instance != null)
            {
                TowerDefence.Core.CampaignManager.Instance.RestartLevel();
            }
            else
            {
                // Fallback: Manager yoksa sahneyi yükle (ama gri ekran riski var)
                TowerDefence.Core.SceneLoader.EnsureInstance();
                if (TowerDefence.Core.SceneLoader.Instance != null) TowerDefence.Core.SceneLoader.Instance.ReloadCurrentScene();
            }
        }
    }
}
