using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private GameObject heroShopPanel;
        [SerializeField] private GameObject sideSelectionPanel;
        [SerializeField] private GameObject compendiumPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button skillTreeButton;
        [SerializeField] private Button heroesButton;
        [SerializeField] private Button compendiumButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Audio")]
        [SerializeField] private AudioClip menuBGM;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);

        private GameObject currentActivePanel;

        [Header("Currency Display")]
        [SerializeField] private TMPro.TextMeshProUGUI karmaText;
        [SerializeField] private TMPro.TextMeshProUGUI crystalText;

        [Header("Daily Reward")]
        [SerializeField] private GameObject dailyRewardPanel;
        [SerializeField] private GameObject dailyRewardBackdrop;
        [SerializeField] private Button dailyRewardButton;
        [SerializeField] private TMPro.TextMeshProUGUI dailyRewardTimerText;

        [Header("Fortune Wheel")]
        [SerializeField] private GameObject fortuneWheelPanel;
        [SerializeField] private GameObject fortuneWheelBackdrop;
        [SerializeField] private Button fortuneWheelButton;
        [SerializeField] private TMPro.TextMeshProUGUI fortuneWheelTimerText;


        private void ShowSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                SettingsUI ui = settingsPanel.GetComponent<SettingsUI>();
                if (ui != null) ui.Open();
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                SettingsUI ui = settingsPanel.GetComponent<SettingsUI>();
                if (ui != null) ui.Close();
                else settingsPanel.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateCurrencyUI();
            UpdateDailyRewardUI();
            UpdateFortuneWheelUI();
        }

        private void UpdateCurrencyUI()
        {
            if (karmaText != null) karmaText.text = MetaProgressionManager.Instance.GetTotalKarma().ToString();
            if (crystalText != null) crystalText.text = MetaProgressionManager.Instance.GetTotalCrystals().ToString();
        }

        private void UpdateDailyRewardUI()
        {
            if (dailyRewardButton == null) return;

            bool available = DailyRewardManager.Instance.IsRewardAvailable();
            dailyRewardButton.interactable = true; // Buton her zaman tıklanabilir, panelde süre yazar

            if (dailyRewardTimerText != null)
            {
                if (available)
                {
                    dailyRewardTimerText.text = "CLAIM NOW!";
                    dailyRewardTimerText.color = Color.green;
                }
                else
                {
                    dailyRewardTimerText.text = DailyRewardManager.Instance.GetTimeToNextReward();
                    dailyRewardTimerText.color = Color.white;
                }
            }
        }

        private void UpdateFortuneWheelUI()
        {
            if (fortuneWheelButton == null || fortuneWheelTimerText == null || MetaProgressionManager.Instance == null) return;
            
            long lastTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelTime();
            bool available = true;

            if (lastTimeTicks > 0)
            {
                System.DateTime lastTime = new System.DateTime(lastTimeTicks);
                System.DateTime nextTime = lastTime.AddDays(1).Date;
                if (System.DateTime.Now < nextTime)
                    available = false;
            }

            if (available)
            {
                fortuneWheelTimerText.text = "SPIN NOW!";
                fortuneWheelTimerText.color = Color.green;
            }
            else
            {
                System.DateTime lastTime = new System.DateTime(lastTimeTicks);
                System.DateTime nextTime = lastTime.AddDays(1).Date;
                System.TimeSpan remaining = nextTime - System.DateTime.Now;
                
                fortuneWheelTimerText.text = string.Format("{0:D2}h {1:D2}m {2:D2}s", remaining.Hours, remaining.Minutes, remaining.Seconds);
                fortuneWheelTimerText.color = Color.white;
            }
        }

        public void ShowDailyRewardPanel()
        {
            if (dailyRewardPanel != null)
            {
                dailyRewardPanel.SetActive(true);
                if (dailyRewardBackdrop != null) dailyRewardBackdrop.SetActive(true);
            }
        }

        public void CloseDailyRewardPanel()
        {
            if (dailyRewardPanel != null) dailyRewardPanel.SetActive(false);
            if (dailyRewardBackdrop != null) dailyRewardBackdrop.SetActive(false);
        }

        public void ShowFortuneWheelPanel()
        {
            if (fortuneWheelPanel != null) fortuneWheelPanel.SetActive(true);
            if (fortuneWheelBackdrop != null) fortuneWheelBackdrop.SetActive(true);
        }

        public void CloseFortuneWheelPanel()
        {
            if (fortuneWheelPanel != null) fortuneWheelPanel.SetActive(false);
            if (fortuneWheelBackdrop != null) fortuneWheelBackdrop.SetActive(false);
        }

        private void Start()
        {
            // Referans yoksa isimle bul (prefab bağlantısı kopsa bile çalışır)
            if (mainMenuPanel   == null) mainMenuPanel   = FindChild("MainMenuPanel");
            if (levelSelectPanel == null) levelSelectPanel = FindChild("LevelSelectionPanel");
            if (skillTreePanel   == null) skillTreePanel   = FindChild("SkillTreePanel");

            if (playButton      == null) playButton      = FindChildButton("PlayButton");
            if (skillTreeButton == null) skillTreeButton = FindChildButton("SkillTreeButton");
            if (quitButton      == null) quitButton      = FindChildButton("QuitButton");

            // Para birimi metinlerini bul
            if (karmaText == null) karmaText = transform.Find("TopPanel/CurrencyContainer/Karma/Value")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (crystalText == null) crystalText = transform.Find("TopPanel/CurrencyContainer/Crystals/Value")?.GetComponent<TMPro.TextMeshProUGUI>();

            // Günlük ödül butonunu bul
            if (dailyRewardButton == null) dailyRewardButton = transform.Find("MainMenuPanel/DailyRewardButton")?.GetComponent<Button>();
            if (dailyRewardButton != null) dailyRewardButton.onClick.AddListener(ShowDailyRewardPanel);

            // Tüm panelleri kapat
            InitPanel(mainMenuPanel);
            InitPanel(levelSelectPanel);
            InitPanel(skillTreePanel);
            InitPanel(heroShopPanel);
            InitPanel(sideSelectionPanel);
            InitPanel(compendiumPanel);
            if (dailyRewardPanel != null) dailyRewardPanel.SetActive(false);
            if (dailyRewardBackdrop != null) dailyRewardBackdrop.SetActive(false);
            
            if (fortuneWheelBackdrop == null) fortuneWheelBackdrop = FindChild("FortuneWheelBackdrop");
            if (fortuneWheelPanel != null) fortuneWheelPanel.SetActive(false);
            if (fortuneWheelBackdrop != null) fortuneWheelBackdrop.SetActive(false);
            
            GameObject settingsBackdrop = FindChild("SettingsBackdrop");
            if (settingsBackdrop != null) settingsBackdrop.SetActive(false);

            // Buton olaylarını bağla
            if (playButton != null) playButton.onClick.AddListener(ShowLevelSelect);
            if (skillTreeButton != null) skillTreeButton.onClick.AddListener(ShowSkillTree);
            if (heroesButton != null) heroesButton.onClick.AddListener(ShowHeroShop);
            if (compendiumButton != null) compendiumButton.onClick.AddListener(ShowCompendium);
            if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
            
            if (dailyRewardButton != null)
                dailyRewardButton.onClick.AddListener(ShowDailyRewardPanel);

            if (fortuneWheelButton != null)
                fortuneWheelButton.onClick.AddListener(ShowFortuneWheelPanel);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            // Ana menüyü hemen görünür olarak aç (fade yok, anında)
            if (mainMenuPanel != null)
            {
                currentActivePanel = mainMenuPanel;
                mainMenuPanel.SetActive(true);
                AnimatePanelIn(mainMenuPanel, instant: true);
            }

            // Müzik çal
            if (AudioManager.Instance != null && menuBGM != null)
            {
                AudioManager.Instance.PlayBGM(menuBGM);
            }
        }

        private GameObject FindChild(string childName)
        {
            Transform t = transform.Find(childName);
            return t != null ? t.gameObject : null;
        }

        private Button FindChildButton(string path)
        {
            Transform t = transform.Find("MainMenuPanel/ButtonContainer/" + path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private void InitPanel(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(false);
            
            // Eğer CanvasGroup yoksa ekle (opsiyonel ama önerilir)
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null) group = panel.AddComponent<CanvasGroup>();
            group.alpha = 0;
            panel.transform.localScale = startScale;
        }

        public void ShowMainMenu() => TransitionToPanel(mainMenuPanel);
        public void ShowLevelSelect() => TransitionToPanel(levelSelectPanel);
        public void ShowSkillTree() => TransitionToPanel(skillTreePanel);
        public void ShowHeroShop() => TransitionToPanel(heroShopPanel);
        public void ShowSideSelection() => TransitionToPanel(sideSelectionPanel);
        public void ShowCompendium() => TransitionToPanel(compendiumPanel);

        private void TransitionToPanel(GameObject targetPanel)
        {
            if (targetPanel == null || targetPanel == currentActivePanel) return;

            // Mevcut paneli kapat
            if (currentActivePanel != null)
            {
                GameObject panelToClose = currentActivePanel;
                CanvasGroup group = panelToClose.GetComponent<CanvasGroup>();
                
                group.DOFade(0, fadeDuration).SetUpdate(true);
                panelToClose.transform.DOScale(startScale, fadeDuration).SetUpdate(true).OnComplete(() => {
                    panelToClose.SetActive(false);
                });
            }

            // Yeni paneli aç
            currentActivePanel = targetPanel;
            targetPanel.SetActive(true);
            AnimatePanelIn(targetPanel);
        }

        private void AnimatePanelIn(GameObject panel, bool instant = false)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null) group = panel.AddComponent<CanvasGroup>();

            if (instant)
            {
                group.alpha = 1f;
                panel.transform.localScale = Vector3.one;
                return;
            }

            group.alpha = 0;
            panel.transform.localScale = startScale;
            group.DOFade(1, fadeDuration).SetUpdate(true);
            panel.transform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Quit Game");
        }
    }
}
