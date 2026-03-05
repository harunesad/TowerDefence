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
        
        [Header("Selection UIs")]
        [SerializeField] private UnitSelectionUI unitSelectionUI;

        [Header("Game Over Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

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
            if (abilityButton != null) abilityButton.onClick.AddListener(OnAbilityClicked);
            
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
        }

        private void UpdateLivesUI(int current, int max)
        {
            if (livesText != null)
            {
                livesText.text = $"Can: {current}/{max}";
            }
        }

        private void UpdateCurrencyUI(Side side, int amount)
        {
            if (side != SideController.Instance.GetPlayerSide()) return;

            string label = side == Side.Light ? "Altın" : "Ruh";
            currencyText.text = $"{label}: {amount}";
        }

        private void UpdateTimerUI(float time)
        {
            timerText.text = $"Süre: {Mathf.CeilToInt(time)}s";
        }

        private void UpdatePhaseUI(GamePhase phase)
        {
            phaseText.text = phase == GamePhase.Preparation ? "HAZIRLIK" : "ÇATIŞMA";
            skipPrepButton.gameObject.SetActive(phase == GamePhase.Preparation);
        }

        private void UpdateGameStateUI(GameState state)
        {
            if (state == GameState.Victory) victoryPanel.SetActive(true);
            if (state == GameState.Defeat) defeatPanel.SetActive(true);
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
