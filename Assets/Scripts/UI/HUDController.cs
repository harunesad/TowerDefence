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
        [SerializeField] private TextMeshProUGUI livesText;
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

        private readonly Color speedActiveColor   = new Color(1f,   0.75f, 0.1f); // Sarı - aktif
        private readonly Color speedInactiveColor = new Color(0.2f, 0.2f,  0.2f); // Koyu - pasif
        
        [Header("Selection UIs")]
        [SerializeField] private UnitSelectionUI unitSelectionUI;

        [Header("Game Over Panels")]
        [SerializeField] private LevelResultUI levelResultUI;

        private void Start()
        {
            // Event abonelikleri
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyUI;
            
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnTimerUpdated += UpdateTimerUI;
                PhaseManager.Instance.OnPhaseChanged += UpdatePhaseUI;
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

            // Hız değişikliği eventine abone ol
            if (GameSpeedManager.Instance != null)
            {
                GameSpeedManager.Instance.OnSpeedChanged += UpdateSpeedButtonHighlights;
                UpdateSpeedButtonHighlights(GameSpeedManager.Instance.GetCurrentSpeedIndex());
            }
            
            // Başlangıç değerleri
            if (SideController.Instance != null)
            {
                Side playerSide = SideController.Instance.GetPlayerSide();
                if (CurrencyManager.Instance != null)
                    UpdateCurrencyUI(playerSide, CurrencyManager.Instance.GetCurrency(playerSide));
            }

            if (PhaseManager.Instance != null)
                UpdatePhaseUI(PhaseManager.Instance.GetCurrentPhase());
            
            if (LivesManager.Instance != null)
                UpdateLivesUI(LivesManager.Instance.GetCurrentLives(), LivesManager.Instance.GetMaxLives());
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyUI;
            
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnTimerUpdated -= UpdateTimerUI;
                PhaseManager.Instance.OnPhaseChanged -= UpdatePhaseUI;
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

        private void UpdateCurrencyUI(Side side, int amount)
        {
            if (side != SideController.Instance.GetPlayerSide()) return;

            string label = side == Side.Light ? "Gold" : "Soul";
            currencyText.text = $"{label}: {amount}";
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

        private void UpdateSpeedButtonHighlights(int activeIndex)
        {
            SetButtonHighlight(speedX1Button, activeIndex == 0);
            SetButtonHighlight(speedX2Button, activeIndex == 1);
            SetButtonHighlight(speedX3Button, activeIndex == 2);
        }

        private void SetButtonHighlight(Button btn, bool active)
        {
            if (btn == null) return;
            btn.GetComponent<Image>().color = active ? speedActiveColor : speedInactiveColor;
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}
