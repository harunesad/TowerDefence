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

        private int currentMapIndex = 0;
        private bool initialized = false;

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
            // Kullanıcı geri döndüğünde ilk haritadan başlasın
            if (initialized) ShowMap(0);
        }

        private void SetupArrowButtons()
        {
            if (leftArrowButton != null) leftArrowButton.onClick.AddListener(() => OnNavigateMap(-1));
            if (rightArrowButton != null) rightArrowButton.onClick.AddListener(() => OnNavigateMap(1));
        }

        private void OnNavigateMap(int direction)
        {
            if (maps == null || maps.Count == 0) return;
            int next = Mathf.Clamp(currentMapIndex + direction, 0, maps.Count - 1);
            if (next != currentMapIndex) ShowMap(next);
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
        }

        private void RefreshDifficultyButtonsUI()
        {
            int maxDiff = MetaProgressionManager.Instance.GetHighestUnlockedGlobalDifficulty();

            if (btnDiffNormal != null) btnDiffNormal.interactable = (1 <= maxDiff);
            if (btnDiffHard != null) btnDiffHard.interactable = (2 <= maxDiff);
            if (btnDiffExpert != null) btnDiffExpert.interactable = (3 <= maxDiff);
        }

        // Tüm haritaların slot'larına seviye butonu kurar/bağlar
        private void BuildAllMaps()
        {
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
            if (lockedOverlay != null) lockedOverlay.gameObject.SetActive(!isUnlocked);

            button.interactable = isUnlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnLevelSelected(level));
        }

        private void OnLevelSelected(LevelData level)
        {
            if (CampaignManager.Instance == null || !CampaignManager.Instance.IsLevelUnlocked(level)) return;

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
    }
}