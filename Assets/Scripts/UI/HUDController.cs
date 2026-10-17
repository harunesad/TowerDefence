using UnityEngine;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Combat;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TowerDefence.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private UnityEngine.UI.Image currencyIcon;
        [SerializeField] private Sprite goldSprite;
        [SerializeField] private Sprite soulSprite;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI phaseText;

        [Header("Buttons")]
        [SerializeField] private Button skipPrepButton;
        [SerializeField] private Button abilityButton;
        [SerializeField] private Image abilityOverlay;

        [Header("Speed Control")]
        [SerializeField] private Button speedX1Button;
        [SerializeField] private Button speedX2Button;
        [SerializeField] private Button speedX3Button;
        [SerializeField] private Button pauseButton;

        private readonly Color speedActiveColor   = new Color(1f,   0.75f, 0.1f); // Sarı - aktif
        private readonly Color speedInactiveColor = new Color(0.2f, 0.2f,  0.2f); // Koyu - pasif
        
        [Header("Game Control Buttons")]
        [SerializeField] private Button menuButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Image soundIcon; // Boş bırakılırsa soundButton'ın kendi Image'ı kullanılır
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;
        
        [Header("Wave Info")]
        [SerializeField] private WaveInfoUI waveInfoUI;
        [Tooltip("WaveInfo panelinde düşman birimlerinin gösterildiği satır prefabı (WaveInfoUnitsRow.prefab)")]
        [SerializeField] private GameObject waveInfoUnitsRowPrefab;

        [Header("Selection UIs")]
        [SerializeField] private UnitSelectionUI unitSelectionUI;

        [Header("Game Over Panels")]
        [SerializeField] private LevelResultUI levelResultUI;

        [Header("Hero HUD")]
        [SerializeField] private GameObject heroPanel;
        [SerializeField] private GameObject heroButtonPrefab;
        [SerializeField] private HeroButtonUI[] preassignedHeroButtons;
        [SerializeField] private HeroStatsPanel heroStatsPanel;

        private void Start()
        {
            // Event abonelikleri
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyUI;
            
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnTimerUpdated += UpdateTimerUI;
                PhaseManager.Instance.OnPhaseChanged += UpdatePhaseUI;
                PhaseManager.Instance.OnWaveChanged += UpdateWaveUI;
            }

            if (AbilityManager.Instance != null)
                AbilityManager.Instance.OnAbilityCooldownChanged += UpdateAbilityUI;

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += UpdateGameStateUI;

            LivesManager.OnLivesChanged += UpdateLivesUI;

            if (skipPrepButton != null) skipPrepButton.onClick.AddListener(OnSkipPrepClicked);
            if (abilityButton  != null) abilityButton.onClick.AddListener(OnAbilityClicked);

            // Hız butonlarını bağla
            if (speedX1Button != null) speedX1Button.onClick.AddListener(() => OnSpeedButtonClicked(0));
            if (speedX2Button != null) speedX2Button.onClick.AddListener(() => OnSpeedButtonClicked(1));
            if (speedX3Button != null) speedX3Button.onClick.AddListener(() => OnSpeedButtonClicked(2));
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);

            // Hız değişikliği eventine abone ol
            if (GameSpeedManager.Instance != null)
            {
                GameSpeedManager.Instance.OnSpeedChanged += UpdateSpeedButtonHighlights;
                UpdateSpeedButtonHighlights(GameSpeedManager.Instance.GetCurrentSpeedIndex());
            }

            // Menü, Restart ve Ses butonları
            if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);
            if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
            if (soundButton != null) soundButton.onClick.AddListener(OnSoundButtonClicked);
            UpdateSoundIcon();
            
            // Başlangıç değerleri
            if (SideController.Instance != null)
            {
                Side playerSide = SideController.Instance.GetPlayerSide();
                if (CurrencyManager.Instance != null)
                    UpdateCurrencyUI(playerSide, CurrencyManager.Instance.GetCurrency(playerSide));
            }

            if (PhaseManager.Instance != null)
            {
                UpdatePhaseUI(PhaseManager.Instance.GetCurrentPhase());
                int total = 0;
                var lvl = CampaignManager.Instance?.GetCurrentLevel();
                if (lvl != null) total = lvl.waves.Count;
                UpdateWaveUI(PhaseManager.Instance.GetCurrentWaveIndex() + 1, total);
            }
            
            if (LivesManager.Instance != null)
                UpdateLivesUI(LivesManager.Instance.GetCurrentLives(), LivesManager.Instance.GetMaxLives());

            SetupHeroButtons();
            SetupWaveInfoUI();
            SetupWarningBannerUI();
        }

        private void SetupWarningBannerUI()
        {
            if (WarningBannerUI.Instance != null) return;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject go = new GameObject("WarningBannerUI", typeof(RectTransform), typeof(WarningBannerUI));
            go.transform.SetParent(canvas.transform, false);
        }

        private void SetupHeroButtons()
        {
            if (HeroManager.Instance == null || heroPanel == null) return;

            var hlg = heroPanel.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) hlg.spacing = 100;

            int heroCount = HeroManager.Instance.GetEquippedHeroCount();

            // 1. ÖNCELİK: Eğer Inspector'da veya HeroPanel altında zaten yerleştirilmiş butonlar varsa onları kullan
            HeroButtonUI[] existingButtons = preassignedHeroButtons;
            if (existingButtons == null || existingButtons.Length == 0)
            {
                existingButtons = heroPanel.GetComponentsInChildren<HeroButtonUI>(true);
            }

            if (existingButtons != null && existingButtons.Length > 0)
            {
                for (int i = 0; i < existingButtons.Length; i++)
                {
                    if (existingButtons[i] == null) continue;
                    if (i < heroCount)
                    {
                        existingButtons[i].gameObject.SetActive(true);
                        existingButtons[i].InitializeSlot(i);
                    }
                    else
                    {
                        // Kuşanılmış hero yoksa bu butonu gizle
                        existingButtons[i].gameObject.SetActive(false);
                    }
                }
                return;
            }

            // 2. ÖNCELİK: Sahnedeki eski butonları temizle ve heroButtonPrefab atanmışsa prefab'den oluştur
            for (int i = heroPanel.transform.childCount - 1; i >= 0; i--)
                Destroy(heroPanel.transform.GetChild(i).gameObject);

            for (int i = 0; i < heroCount; i++)
            {
                if (heroButtonPrefab != null)
                {
                    GameObject btnObj = Instantiate(heroButtonPrefab, heroPanel.transform, false);
                    btnObj.name = $"HeroButton_{i + 1}";
                    HeroButtonUI btnUI = btnObj.GetComponent<HeroButtonUI>();
                    if (btnUI == null) btnUI = btnObj.AddComponent<HeroButtonUI>();
                    btnUI.InitializeSlot(i);
                }
                else
                {
                    // Fallback: Kod ile oluştur
                    HeroButtonUI.Create(heroPanel.GetComponent<RectTransform>(), i);
                }
            }
        }

        private void SetupWaveInfoUI()
        {
            if (waveInfoUI != null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject go = new GameObject("WaveInfoUI", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            waveInfoUI = go.AddComponent<WaveInfoUI>();
            if (waveInfoUnitsRowPrefab != null)
            {
                waveInfoUI.SetUnitsRowPrefab(waveInfoUnitsRowPrefab);
            }
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyUI;
            
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnTimerUpdated -= UpdateTimerUI;
                PhaseManager.Instance.OnPhaseChanged -= UpdatePhaseUI;
                PhaseManager.Instance.OnWaveChanged -= UpdateWaveUI;
            }

            if (AbilityManager.Instance != null)
                AbilityManager.Instance.OnAbilityCooldownChanged -= UpdateAbilityUI;

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= UpdateGameStateUI;

            LivesManager.OnLivesChanged -= UpdateLivesUI;

            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.OnSpeedChanged -= UpdateSpeedButtonHighlights;
        }

        private void UpdateLivesUI(int current, int max)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {current}/{max}";
            }
        }

        private void UpdateWaveUI(int currentWave, int totalWaves)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {currentWave}/{totalWaves}";
            }
        }

        private void UpdateCurrencyUI(Side side, int amount)
        {
            if (side != SideController.Instance.GetPlayerSide()) return;

            currencyText.text = amount.ToString();
            
            if (currencyIcon != null)
            {
                currencyIcon.sprite = side == Side.Light ? goldSprite : soulSprite;
                currencyIcon.gameObject.SetActive(true);
            }
        }

        private void UpdateTimerUI(float time)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(time)}s";
        }

        private void UpdatePhaseUI(GamePhase phase)
        {
            if (phaseText != null)
                phaseText.text = phase == GamePhase.Preparation ? "PREPARATION" : "COMBAT";
            
            if (skipPrepButton != null)
            {
                skipPrepButton.gameObject.SetActive(phase == GamePhase.Preparation);
                skipPrepButton.interactable = (phase == GamePhase.Preparation);
            }
        }

        private void UpdateGameStateUI(GameState state)
        {
            if (levelResultUI == null) return;

            if (state == GameState.Victory)
            {
                levelResultUI.Show(true, LivesManager.Instance.GetCurrentLives(), LivesManager.Instance.GetMaxLives());
            }
            else if (state == GameState.Defeat)
            {
                levelResultUI.Show(false, 0, LivesManager.Instance.GetMaxLives());
            }
        }

        private void UpdateAbilityUI(float current, float max)
        {
            if (abilityOverlay != null)
            {
                abilityOverlay.fillAmount = current / max;
            }
            abilityButton.interactable = (current <= 0);
        }

        private void OnSkipPrepClicked()
        {
            PhaseManager.Instance.SkipPreparation();
        }

        private void OnAbilityClicked()
        {
            AbilityManager.Instance.UseAbility();
        }

        private void OnSpeedButtonClicked(int speedIndex)
        {
            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.SetSpeed(speedIndex);
        }

        private void OnPauseClicked()
        {
            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.TogglePause();
        }

        private void UpdateSpeedButtonHighlights(int activeIndex)
        {
            bool isPaused = activeIndex == -1;

            SetButtonHighlight(speedX1Button, activeIndex == 0);
            SetButtonHighlight(speedX2Button, activeIndex == 1);
            SetButtonHighlight(speedX3Button, activeIndex == 2);
            SetButtonHighlight(pauseButton, isPaused);

            if (pauseButton != null)
            {
                var label = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = isPaused ? "RESUME" : "PAUSE";
                }
            }
        }

        private void SetButtonHighlight(Button btn, bool active)
        {
            if (btn == null) return;
            btn.GetComponent<Image>().color = active ? speedActiveColor : speedInactiveColor;
        }

        public void RestartLevel()
        {
            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;

            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.RestartLevel();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void ReturnToMenu()
        {
            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;

            SceneManager.LoadScene("MainScene");
        }

        public void OnSoundButtonClicked()
        {
            if (AudioManager.Instance != null)
            {
                // MasterVolume 0 ise aç (1f veya önceki ses seviyesi), 0'dan büyükse kapat (0f)
                if (AudioManager.Instance.MasterVolume > 0.01f)
                {
                    PlayerPrefs.SetFloat("PreMuteMasterVolume", AudioManager.Instance.MasterVolume);
                    AudioManager.Instance.SetMasterVolume(0f);
                }
                else
                {
                    float prevVol = PlayerPrefs.GetFloat("PreMuteMasterVolume", 1f);
                    if (prevVol <= 0.01f) prevVol = 1f;
                    AudioManager.Instance.SetMasterVolume(prevVol);
                }
            }
            else
            {
                // Fallback: AudioListener
                AudioListener.pause = !AudioListener.pause;
            }

            UpdateSoundIcon();
        }

        private void UpdateSoundIcon()
        {
            // Eğer soundIcon ayrıca atanmamışsa, doğrudan soundButton'ın üzerindeki Image bileşenini kullan
            Image targetImage = soundIcon;
            if (targetImage == null && soundButton != null)
            {
                targetImage = soundButton.image;
            }

            if (targetImage == null) return;

            bool isMuted = false;
            if (AudioManager.Instance != null)
            {
                isMuted = AudioManager.Instance.MasterVolume <= 0.01f;
            }
            else
            {
                isMuted = AudioListener.pause;
            }

            if (isMuted && soundOffSprite != null)
            {
                targetImage.sprite = soundOffSprite;
            }
            else if (!isMuted && soundOnSprite != null)
            {
                targetImage.sprite = soundOnSprite;
            }
        }
    }
}
