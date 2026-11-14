using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Data;
using TowerDefence.Core;
using System.Collections.Generic;

namespace TowerDefence.UI
{
    public class LevelSelectionUI : MonoBehaviour
    {
        [SerializeField] private List<LevelMapData> maps;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private List<Transform> slotContainers; // Her harita için 1 container (LevelSlot_0..N çocukları)
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;
        [SerializeField] private TMPro.TextMeshProUGUI mapNameText;

        [SerializeField] private GameObject sideSelectionPanel; // Seviye seçince aktif olacak panel
        [SerializeField] private Button backButton;

        [Header("Difficulty Selection")]
        [SerializeField] private Button btnDiffNormal;
        [SerializeField] private Button btnDiffHard;
        [SerializeField] private Button btnDiffExpert;
        [SerializeField] private Sprite diffSelectedSprite;
        [SerializeField] private Sprite diffUnselectedSprite;

        [Header("Sweep / Auto-Complete")]
        [SerializeField] private LevelResultUI sweepResultUI;

        private int currentMapIndex = 0;
        private bool initialized = false;
        private readonly Dictionary<string, RectTransform> levelTargets = new Dictionary<string, RectTransform>();

        private void Start()
        {
            SetupArrowButtons();

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }

            SetupDifficultyButtons();
            BuildAllMaps();
            RefreshDifficultyButtonsUI();

            initialized = true;
            ShowMap(0);
        }

        private void OnEnable()
        {
            if (!initialized) return;

            BuildAllMaps();
            ShowMap(currentMapIndex);
        }

        private void LateUpdate()
        {
            ApplyLessonAccess();
            TutorialDirector tutorial = TutorialDirector.Instance;
            bool locked = tutorial != null && tutorial.LocksLevelBack();
            if (backButton != null) backButton.interactable = !locked;
            if (!locked) return;

            if (leftArrowButton != null) leftArrowButton.interactable = false;
            if (rightArrowButton != null) rightArrowButton.interactable = false;
            int mapIndex = MapIndexForId(tutorial.ForcedMapLevel());
            if (mapIndex >= 0 && mapIndex != currentMapIndex) ShowMap(mapIndex);
            if (leftArrowButton != null) leftArrowButton.interactable = false;
            if (rightArrowButton != null) rightArrowButton.interactable = false;
            EnsureLessonSweep();
        }

        void ApplyLessonAccess()
        {
            if (levelTargets.Count == 0) return;
            foreach (var pair in levelTargets)
            {
                RectTransform rect = pair.Value;
                if (rect == null) continue;
                Button button = rect.GetComponent<Button>();
                LevelData level = FindLevelData(pair.Key);
                if (button == null || level == null) continue;
                bool isUnlocked = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelUnlocked(level);
                bool closed = TutorialDirector.Instance != null && TutorialDirector.Instance.LevelClosed(level.levelID);
                Transform lockedOverlay = rect.Find("LockedOverlay");
                if (lockedOverlay != null) lockedOverlay.gameObject.SetActive(!isUnlocked || closed);
                bool lessonOk = TutorialDirector.Instance == null || TutorialDirector.Instance.LevelAllowed(level.levelID);
                button.interactable = isUnlocked && lessonOk;
            }
        }

        void EnsureLessonSweep()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial == null || !tutorial.SweepAllowed("Level1")) return;
            if (!levelTargets.TryGetValue("Level1", out RectTransform level) || level == null) return;
            Transform sweep = level.Find("SweepButton");
            if (sweep == null) return;
            if (sweep.gameObject.activeSelf) return;
            LevelData data = FindLevelData("Level1");
            if (data == null) return;
            sweep.gameObject.SetActive(true);
            Button sweepBtn = sweep.GetComponent<Button>();
            if (sweepBtn == null) return;
            sweepBtn.onClick.RemoveAllListeners();
            sweepBtn.onClick.AddListener(() => OnSweepButtonClicked(data));
        }

        LevelData FindLevelData(string levelId)
        {
            if (maps == null) return null;
            for (int i = 0; i < maps.Count; i++)
            {
                LevelMapData map = maps[i];
                if (map == null || map.levels == null) continue;
                for (int n = 0; n < map.levels.Count; n++)
                {
                    if (map.levels[n] != null && map.levels[n].levelID == levelId)
                        return map.levels[n];
                }
            }
            return null;
        }

        public RectTransform FindLevelTarget(string levelId)
        {
            if (string.IsNullOrEmpty(levelId)) return null;
            levelTargets.TryGetValue(levelId, out RectTransform rect);
            return rect;
        }

        public RectTransform RightArrowTarget => rightArrowButton != null ? rightArrowButton.transform as RectTransform : null;
        public RectTransform HardTarget => btnDiffHard != null ? btnDiffHard.transform as RectTransform : null;
        public RectTransform ExpertTarget => btnDiffExpert != null ? btnDiffExpert.transform as RectTransform : null;

        public void ShowMapFor(string levelId)
        {
            int mapIndex = MapIndexForId(levelId);
            if (mapIndex >= 0 && mapIndex != currentMapIndex) ShowMap(mapIndex);
        }

        public RectTransform FindSweepTarget(string levelId)
        {
            RectTransform level = FindLevelTarget(levelId);
            if (level == null) return null;
            Transform sweep = level.Find("SweepButton");
            if (sweep != null && sweep.gameObject.activeInHierarchy)
                return sweep as RectTransform;
            return null;
        }

        int MapIndexForId(string levelId)
        {
            if (string.IsNullOrEmpty(levelId) || maps == null) return -1;
            for (int i = 0; i < maps.Count; i++)
            {
                LevelMapData map = maps[i];
                if (map == null || map.levels == null) continue;
                for (int n = 0; n < map.levels.Count; n++)
                {
                    if (map.levels[n] != null && map.levels[n].levelID == levelId)
                        return i;
                }
            }
            return -1;
        }

        public int GetMapIndexForLevel(LevelData level)
        {
            if (level == null || maps == null) return -1;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i] != null && maps[i].levels != null && maps[i].levels.Contains(level))
                {
                    return i;
                }
            }
            return -1;
        }

        private void SetupArrowButtons()
        {
            if (leftArrowButton != null) leftArrowButton.onClick.AddListener(() => OnNavigateMap(-1));
            if (rightArrowButton != null) rightArrowButton.onClick.AddListener(() => OnNavigateMap(1));
        }

        private void OnNavigateMap(int direction)
        {
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.LocksLevelBack()) return;
            if (maps == null || maps.Count == 0) return;
            int next = Mathf.Clamp(currentMapIndex + direction, 0, maps.Count - 1);
            if (next == currentMapIndex) return;
            ShowMap(next);
            if (direction > 0 && TutorialDirector.Instance != null)
                TutorialDirector.Instance.OnNextMap();
        }

        private void ShowMap(int index)
        {
            if (maps == null || maps.Count == 0) return;
            currentMapIndex = Mathf.Clamp(index, 0, maps.Count - 1);
            LevelMapData map = maps[currentMapIndex];

            // Slot container'ları toggle et
            for (int i = 0; i < slotContainers.Count; i++)
            {
                if (slotContainers[i] != null)
                    slotContainers[i].gameObject.SetActive(i == currentMapIndex);
            }

            // Arkaplan görselini güncelle (yüklenmediyse düz renk korunur)
            if (backgroundImage != null)
            {
                if (map != null && map.mapBackground != null)
                {
                    backgroundImage.sprite = map.mapBackground;
                    backgroundImage.color = Color.white;
                }
                else
                {
                    backgroundImage.sprite = null;
                    backgroundImage.color = new Color(0.07f, 0.07f, 0.11f, 1f);
                }
            }

            // Kenar durdurma: ilk haritada sol ok, son haritada sağ ok kapalı
            if (leftArrowButton != null) leftArrowButton.interactable = currentMapIndex > 0;
            if (rightArrowButton != null) rightArrowButton.interactable = currentMapIndex < maps.Count - 1;

            // Harita adını güncelle
            if (mapNameText != null)
            {
                mapNameText.text = map != null ? map.mapName : "MAP";
            }
        }

        private void SetupDifficultyButtons()
        {
            if (btnDiffNormal != null) btnDiffNormal.onClick.AddListener(() => OnDifficultySelected(1));
            if (btnDiffHard != null) btnDiffHard.onClick.AddListener(() => OnDifficultySelected(2));
            if (btnDiffExpert != null) btnDiffExpert.onClick.AddListener(() => OnDifficultySelected(3));
        }

        private void OnDifficultySelected(int difficulty)
        {
            if (difficulty > MetaProgressionManager.Instance.GetHighestUnlockedGlobalDifficulty()) return;

            CampaignManager.Instance.SetDifficulty(difficulty);
            RefreshDifficultyButtonsUI();
            BuildAllMaps();
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.OnDifficultyPicked(difficulty);
        }

        private void RefreshDifficultyButtonsUI()
        {
            int maxDiff = MetaProgressionManager.Instance.GetHighestUnlockedGlobalDifficulty();
            int currentDiff = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;

            if (btnDiffNormal != null)
            {
                btnDiffNormal.interactable = (1 <= maxDiff);
                UpdateButtonSprite(btnDiffNormal, currentDiff == 1);
            }

            if (btnDiffHard != null)
            {
                btnDiffHard.interactable = (2 <= maxDiff);
                UpdateButtonSprite(btnDiffHard, currentDiff == 2);
            }

            if (btnDiffExpert != null)
            {
                btnDiffExpert.interactable = (3 <= maxDiff);
                UpdateButtonSprite(btnDiffExpert, currentDiff == 3);
            }
        }

        private void UpdateButtonSprite(Button btn, bool isSelected)
        {
            if (btn == null) return;
            Image img = btn.image;
            if (img == null) img = btn.GetComponent<Image>();
            if (img == null) return;

            if (isSelected)
            {
                if (diffSelectedSprite != null) img.sprite = diffSelectedSprite;
            }
            else
            {
                if (diffUnselectedSprite != null) img.sprite = diffUnselectedSprite;
            }
        }

        // Tüm haritaların slot'larına seviye butonu kurar/bağlar
        private void BuildAllMaps()
        {
            levelTargets.Clear();
            if (maps == null || slotContainers == null) return;

            int count = Mathf.Min(maps.Count, slotContainers.Count);
            for (int i = 0; i < count; i++)
            {
                BuildMapButtons(maps[i], slotContainers[i]);
            }
        }

        private void BuildMapButtons(LevelMapData map, Transform container)
        {
            if (map == null || container == null) return;

            for (int i = 0; i < map.levels.Count; i++)
            {
                // Slot'lar isimle bulunur; layer'a dekor eklense bile hizalama bozulmaz
                Transform slot = container.Find("LevelSlot_" + i);
                if (slot == null) continue;

                GameObject buttonGO = slot.childCount > 0 ? slot.GetChild(0).gameObject : null;
                if (buttonGO == null)
                {
                    buttonGO = Instantiate(levelButtonPrefab, slot);
                    SetStretchToParent(buttonGO.GetComponent<RectTransform>());
                }
                SetupButton(buttonGO, map.levels[i]);
            }
        }

        private static void SetStretchToParent(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void SetupButton(GameObject buttonGO, LevelData level)
        {
            Button button = buttonGO.GetComponent<Button>();
            if (button == null) return;

            // Tamamlanan yıldız sayısını al (tamamlanmamışsa 0 → 3 soluk yıldız)
            int earnedStars = 0;
            if (MetaProgressionManager.Instance != null)
            {
                int difficulty = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
                earnedStars = MetaProgressionManager.Instance.GetLevelStars(level.levelID, difficulty);
            }

            // Yıldızları marker'ın üstünde göster: kazanılan altın, kazanılmayan soluk
            Transform starContainer = buttonGO.transform.Find("StarContainer");
            if (starContainer != null)
            {
                for (int i = 0; i < starContainer.childCount; i++)
                {
                    Image starImg = starContainer.GetChild(i).GetComponent<Image>();
                    if (starImg == null) continue;
                    starImg.color = i < earnedStars
                        ? new Color(1f, 0.85f, 0.1f)
                        : new Color(0.3f, 0.3f, 0.38f);
                }
            }

            // Kilit durumunu kontrol et
            bool isUnlocked = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelUnlocked(level);
            Transform lockedOverlay = buttonGO.transform.Find("LockedOverlay");
            bool closed = TutorialDirector.Instance != null && TutorialDirector.Instance.LevelClosed(level.levelID);
            if (lockedOverlay != null) lockedOverlay.gameObject.SetActive(!isUnlocked || closed);

            // Sweep (Hızlı Geç) Butonu
            Transform sweepButtonTrans = buttonGO.transform.Find("SweepButton");
            if (sweepButtonTrans != null)
            {
                Button sweepBtn = sweepButtonTrans.GetComponent<Button>();
                if (sweepBtn != null)
                {
                    bool sweepLesson = TutorialDirector.Instance != null && TutorialDirector.Instance.SweepAllowed(level.levelID);
                    if ((earnedStars >= 3 && isUnlocked && !closed) || sweepLesson)
                    {
                        sweepBtn.gameObject.SetActive(true);
                        sweepBtn.onClick.RemoveAllListeners();
                        sweepBtn.onClick.AddListener(() => OnSweepButtonClicked(level));
                    }
                    else
                    {
                        sweepBtn.gameObject.SetActive(false);
                    }
                }
            }

            bool lessonOk = TutorialDirector.Instance == null || TutorialDirector.Instance.LevelAllowed(level.levelID);
            button.interactable = isUnlocked && lessonOk;
            levelTargets[level.levelID] = button.transform as RectTransform;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnLevelSelected(level));
        }

        private void OnLevelSelected(LevelData level)
        {
            if (CampaignManager.Instance == null || !CampaignManager.Instance.IsLevelUnlocked(level)) return;
            if (TutorialDirector.Instance != null && !TutorialDirector.Instance.LevelAllowed(level.levelID)) return;

            CampaignManager.Instance.SelectLevel(level);
            Debug.Log($"LevelSelectionUI: {level.levelName} selected.");

            MainMenuController mc = GetComponentInParent<MainMenuController>();
            if (mc != null)
            {
                mc.ShowSideSelection();
            }
            else if (sideSelectionPanel != null)
            {
                sideSelectionPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        private void OnSweepButtonClicked(LevelData level)
        {
            if (level == null) return;
            bool lesson = TutorialDirector.Instance != null && TutorialDirector.Instance.SweepAllowed(level.levelID);
            if (!lesson && TutorialDirector.Instance != null && !TutorialDirector.Instance.LevelAllowed(level.levelID)) return;
            if (lesson)
            {
                if (CampaignManager.Instance != null)
                    CampaignManager.Instance.SelectLevel(level);
                if (AdManager.Instance == null)
                {
                    Debug.LogWarning("AdManager not found!");
                    return;
                }
                AdManager.Instance.ShowRewardedAd(
                    onSuccess: () =>
                    {
                        if (CampaignManager.Instance != null)
                        {
                            CampaignManager.Instance.SelectLevel(level);
                            CampaignManager.Instance.AllowAnotherCompletionReward();
                        }
                        if (sweepResultUI != null)
                            sweepResultUI.ShowSweepResult(true);
                        else if (TutorialDirector.Instance != null)
                            TutorialDirector.Instance.Notify("sweep_1");
                    });
                return;
            }
            if (AdManager.Instance != null)
            {
                AdManager.Instance.ShowRewardedAd(
                    onSuccess: () =>
                    {
                        if (CampaignManager.Instance != null)
                        {
                            CampaignManager.Instance.SelectLevel(level);
                            CampaignManager.Instance.AllowAnotherCompletionReward();
                        }

                        if (sweepResultUI != null)
                        {
                            // 3 yıldız ödülünü sweep mantığıyla ver
                            sweepResultUI.ShowSweepResult();
                        }
                        else
                        {
                            Debug.LogWarning("SweepResultUI atanmamış! Manuel ödül veriliyor.");
                            if (CampaignManager.Instance != null)
                            {
                                int globalDifficulty = CampaignManager.Instance.CurrentDifficultyLevel;
                                if (globalDifficulty < 1) globalDifficulty = 1;
                                LevelData currentLvl = CampaignManager.Instance.GetCurrentLevel();
                                int levelDifficulty = (currentLvl != null && currentLvl.difficulty > 0) ? currentLvl.difficulty : 1;
                                int totalMultiplier = levelDifficulty * globalDifficulty;
                                CampaignManager.Instance.CompleteCurrentLevel(3, 10 * totalMultiplier, 2 * totalMultiplier);
                            }
                        }
                    },
                    onFailed: () =>
                    {
                        Debug.Log("Sweep rewarded ad failed or skipped.");
                    }
                );
            }
            else
            {
                Debug.LogWarning("AdManager not found!");
            }
        }
    }
}