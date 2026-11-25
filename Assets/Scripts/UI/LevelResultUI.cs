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
        [SerializeField] private Button closeButton; // Only in Sweep mode or to close

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
            // If restartButton is not assigned, clone it from menuButton automatically
            if (restartButton == null && menuButton != null)
            {
                GameObject restartGO = Instantiate(menuButton.gameObject, menuButton.transform.parent);
                restartGO.name = "RestartButton";
                restartButton = restartGO.GetComponent<Button>();
                
                var btnText = restartGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (btnText != null) btnText.text = "RESTART";

                // Push next to/below menu button (Simple offset)
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
                closeButton.onClick.AddListener(CloseResult);
        }

        public void Show(bool isVictory, int livesRemaining, int totalLives)
        {
            gameObject.SetActive(true);
            hasDoubled = false;
            titleText.text = isVictory ? "VICTORY!" : "DEFEAT...";
            titleText.color = isVictory ? Color.green : Color.red;

            int globalDifficulty = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
            if (globalDifficulty < 1) globalDifficulty = 1;

            var currentLvl = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.OnLevelResult(isVictory, currentLvl != null ? currentLvl.levelID : null);
            int levelDifficulty = (currentLvl != null && currentLvl.difficulty > 0) ? currentLvl.difficulty : 1;

            int stars = 0;

            if (isVictory)
            {
                float lifePercent = (float)livesRemaining / totalLives;
                if (lifePercent >= 1f) stars = 3;
                else if (lifePercent >= 0.5f) stars = 2;
                else stars = 1;

                if (currentLvl != null && currentLvl.levelID == "Level1"
                    && TutorialDirector.Instance != null
                    && TutorialDirector.Instance.Current() != null
                    && (MetaProgressionManager.Instance == null || !MetaProgressionManager.Instance.IsTutorialStepDone("tutorial_rest")))
                    stars = 3;

                int oldStars = 0;
                if (currentLvl != null && MetaProgressionManager.Instance != null)
                    oldStars = MetaProgressionManager.Instance.GetLevelStars(currentLvl.levelID, globalDifficulty);

                CampaignManager.CalculateManualPayout(oldStars, stars, levelDifficulty, globalDifficulty, out int offeredKarma, out int offeredCrystals, out _);
                pendingKarma = offeredKarma;
                pendingCrystals = offeredCrystals;

                if (CampaignManager.Instance != null)
                {
                    (pendingKarma, pendingCrystals) = CampaignManager.Instance.CompleteCurrentLevel(stars, offeredKarma, offeredCrystals);
                }

                if (currentLvl != null && currentLvl.levelID == "Level1"
                    && MetaProgressionManager.Instance != null
                    && MetaProgressionManager.Instance.HasClearedLevel("Level1"))
                    MetaProgressionManager.Instance.CommitTutorialCheckpoint();

                if (doubleRewardButton != null)
                {
                    bool showDouble = pendingKarma > 0 || pendingCrystals > 0;
                    doubleRewardButton.gameObject.SetActive(showDouble);
                    doubleRewardButton.interactable = showDouble;
                }
            }
            else
            {
                pendingKarma = 0;
                pendingCrystals = 0;

                if (doubleRewardButton != null)
                    doubleRewardButton.gameObject.SetActive(false);
            }

            // Update reward text
            UpdateRewardText();

            // Visualize stars
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = (i < stars) ? activeStarColor : inactiveStarColor;
            }

            // Next Level button control
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(isVictory);
            
            // Enable other buttons, disable Close button
            if (menuButton != null) 
            {
                menuButton.gameObject.SetActive(true);
                var txt = menuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = "MAIN MENU"; // Restore default text
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(OnMenuClicked); // Rebind default method
            }
            if (restartButton != null) restartButton.gameObject.SetActive(true);
            if (closeButton != null) closeButton.gameObject.SetActive(false);

            if (!isVictory && TutorialDirector.Instance != null && TutorialDirector.Instance.WantsRestart())
                LockToRestart();
            if (isVictory && TutorialDirector.Instance != null && TutorialDirector.Instance.WantsMenuReturn())
                LockToMenu();
            if (isVictory && TutorialDirector.Instance != null && TutorialDirector.Instance.WantsMapReturn())
                LockToNext();
        }

        public RectTransform RestartTarget => restartButton != null ? restartButton.transform as RectTransform : null;
        public RectTransform MenuTarget => menuButton != null ? menuButton.transform as RectTransform : null;
        public RectTransform NextTarget => nextLevelButton != null ? nextLevelButton.transform as RectTransform : null;

        public void LockToNext()
        {
            if (menuButton != null) menuButton.interactable = false;
            if (restartButton != null) restartButton.interactable = false;
            if (closeButton != null) closeButton.interactable = false;
            if (doubleRewardButton != null) doubleRewardButton.interactable = false;
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(true);
                nextLevelButton.interactable = true;
            }
        }

        public void LockToMenu()
        {
            if (nextLevelButton != null) nextLevelButton.interactable = false;
            if (restartButton != null) restartButton.interactable = false;
            if (closeButton != null) closeButton.interactable = false;
            if (doubleRewardButton != null) doubleRewardButton.interactable = false;
            if (menuButton != null) menuButton.interactable = true;
        }

        public void LockToRestart()
        {
            if (menuButton != null) menuButton.interactable = false;
            if (nextLevelButton != null)
            {
                nextLevelButton.interactable = false;
                nextLevelButton.gameObject.SetActive(false);
            }
            if (closeButton != null) closeButton.interactable = false;
            if (doubleRewardButton != null) doubleRewardButton.interactable = false;
            if (restartButton != null) restartButton.interactable = true;
        }

        bool tutorialSweep;

        public void ShowSweepResult(bool tutorialSweep = false)
        {
            this.tutorialSweep = tutorialSweep;
            gameObject.SetActive(true);
            hasDoubled = false;
            titleText.text = "LEVEL CLEARED!";
            titleText.color = Color.green;

            int globalDifficulty = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
            if (globalDifficulty < 1) globalDifficulty = 1;

            var currentLvl = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            int levelDifficulty = (currentLvl != null && currentLvl.difficulty > 0) ? currentLvl.difficulty : 1;
            int totalMultiplier = levelDifficulty * globalDifficulty;

            int stars = 3;

            int offeredKarma = 10 * totalMultiplier;
            int offeredCrystals = 2 * totalMultiplier;
            pendingKarma = offeredKarma;
            pendingCrystals = offeredCrystals;

            if (CampaignManager.Instance != null)
            {
                (pendingKarma, pendingCrystals) = CampaignManager.Instance.CompleteCurrentLevel(stars, offeredKarma, offeredCrystals);
            }

            // Update reward text
            UpdateRewardText();

            // Visualize stars
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = activeStarColor;
            }

            // Hide normal buttons, show only Close button
            if (menuButton != null) menuButton.gameObject.SetActive(false);
            if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(false);
            if (doubleRewardButton != null) doubleRewardButton.gameObject.SetActive(false);

            if (closeButton != null) 
            {
                closeButton.gameObject.SetActive(true);
                closeButton.interactable = true;
            }
            else 
            {
                // Fallback: If user didn't place a Close button, use Menu button to close
                if (menuButton != null)
                {
                    menuButton.gameObject.SetActive(true);
                    var txt = menuButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null) txt.text = "CLOSE";
                    menuButton.onClick.RemoveAllListeners();
                    menuButton.onClick.AddListener(CloseResult);
                }
            }
            RaiseAboveShade();
        }

        void RaiseAboveShade()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 560;
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        void CloseResult()
        {
            gameObject.SetActive(false);
            if (!tutorialSweep) return;
            tutorialSweep = false;
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.Notify("sweep_1");
        }

        private void UpdateRewardText()
        {
            if (rewardText == null) return;

            Transform row = rewardText.transform.Find("CurrencyRow");
            if (pendingKarma <= 0 && pendingCrystals <= 0)
            {
                if (row != null) row.gameObject.SetActive(false);
                rewardText.text = "No Rewards";
                return;
            }

            if (row != null) row.gameObject.SetActive(true);
            rewardText.text = hasDoubled ? "2X" : "";
            CurrencyRowView.Apply(rewardText.transform, pendingKarma, pendingCrystals, true);
        }

        private void OnDoubleRewardClicked()
        {
            if (hasDoubled) return;
            if (pendingKarma <= 0 && pendingCrystals <= 0) return;

            if (AdManager.Instance != null)
            {
                int karmaToAdd = pendingKarma;
                int crystalsToAdd = pendingCrystals;
                AdManager.Instance.ShowRewardedAd(
                    onSuccess: () =>
                    {
                        if (hasDoubled) return;
                        hasDoubled = true;

                        // Extra reward is 1x of recorded amount
                        if (MetaProgressionManager.Instance != null)
                        {
                            if (karmaToAdd > 0)
                                MetaProgressionManager.Instance.AddKarma(karmaToAdd);
                            if (crystalsToAdd > 0)
                                MetaProgressionManager.Instance.AddCrystals(crystalsToAdd);
                        }

                        pendingKarma = karmaToAdd * 2;
                        pendingCrystals = crystalsToAdd * 2;
                        UpdateRewardText();

                        if (doubleRewardButton != null)
                            doubleRewardButton.gameObject.SetActive(false);
                    },
                    onFailed: () =>
                    {
                    }
                );
            }
        }

        private void OnMenuClicked()
        {
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.ClearMenuReturn();
            gameObject.SetActive(false);

            // Reset time to normal via GameSpeedManager
            if (TowerDefence.Core.GameSpeedManager.Instance != null)
                TowerDefence.Core.GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;
            
            TowerDefence.Core.SceneLoader.EnsureInstance();
            if (TowerDefence.Core.SceneLoader.Instance != null) TowerDefence.Core.SceneLoader.Instance.LoadScene(1);
        }

        private void OnNextLevelClicked()
        {
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.WantsMapReturn())
            {
                OnMenuClicked();
                return;
            }

            // Close panel
            gameObject.SetActive(false);

            // Reset time to normal
            if (TowerDefence.Core.GameSpeedManager.Instance != null)
                TowerDefence.Core.GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;

            // Load next level, return to menu on failure
            if (TowerDefence.Core.CampaignManager.Instance != null &&
                TowerDefence.Core.CampaignManager.Instance.LoadNextLevel())
            {
                return; // Next level loaded
            }

            // Return to menu if on last level or error
            OnMenuClicked();
        }

        private void OnRestartClicked()
        {
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.ClearDefeatRetry();
            gameObject.SetActive(false);

            // Reset time to normal via GameSpeedManager
            if (TowerDefence.Core.GameSpeedManager.Instance != null)
                TowerDefence.Core.GameSpeedManager.Instance.SetSpeedX1();
            else
                Time.timeScale = 1f;
            
            // Clean restart via CampaignManager
            if (TowerDefence.Core.CampaignManager.Instance != null)
            {
                TowerDefence.Core.CampaignManager.Instance.RestartLevel();
            }
            else
            {
                // Fallback: Reload scene if manager is missing (gray screen risk)
                TowerDefence.Core.SceneLoader.EnsureInstance();
                if (TowerDefence.Core.SceneLoader.Instance != null) TowerDefence.Core.SceneLoader.Instance.ReloadCurrentScene();
            }
        }
    }
}
