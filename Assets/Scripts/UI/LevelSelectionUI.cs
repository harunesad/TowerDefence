using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Data;
using TowerDefence.Core;
using System.Collections.Generic;

namespace TowerDefence.UI
{
    public class LevelSelectionUI : MonoBehaviour
    {
        [SerializeField] private List<LevelData> levels;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Transform container;

        [SerializeField] private GameObject sideSelectionPanel; // Seviye seçince aktif olacak panel
        [SerializeField] private Button backButton;
        [SerializeField] private bool useMapMode = false; // Harita üzerinde elle yerleştirme modu

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }

            InitializeUI();
        }

        private void InitializeUI()
        {
            if (useMapMode)
            {
                // Mevcut butonları (Hierarchy'deki) kullan
                for (int i = 0; i < levels.Count; i++)
                {
                    if (i >= container.childCount) break;
                    
                    GameObject buttonGO = container.GetChild(i).gameObject;
                    SetupButton(buttonGO, levels[i]);
                }
            }
            else
            {
                // Klasik liste şeklinde oluştur
                foreach (LevelData level in levels)
                {
                    GameObject buttonGO = Instantiate(levelButtonPrefab, container);
                    SetupButton(buttonGO, level);
                }
            }
        }

        private void SetupButton(GameObject buttonGO, LevelData level)
        {
            Button button = buttonGO.GetComponent<Button>();
            
            // Seviye ismini ata
            var nameTxt = buttonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (nameTxt != null) nameTxt.text = level.levelName;

            // Önizleme resmini ata
            Transform preview = buttonGO.transform.Find("PreviewImage");
            if (preview != null) preview.GetComponent<Image>().sprite = level.levelPreview;

            // Zorluk ikonlarını ayarla
            Transform diffContainer = buttonGO.transform.Find("DifficultyContainer");
            if (diffContainer != null)
            {
                for (int i = 0; i < diffContainer.childCount; i++)
                {
                    diffContainer.GetChild(i).gameObject.SetActive(i < level.difficulty);
                }
            }

            // Kilit durumunu kontrol et
            bool isUnlocked = CampaignManager.Instance.IsLevelUnlocked(level);
            Transform lockedOverlay = buttonGO.transform.Find("LockedOverlay");
            if (lockedOverlay != null) lockedOverlay.gameObject.SetActive(!isUnlocked);

            button.interactable = isUnlocked;
            button.onClick.RemoveAllListeners(); // Harita modunda temizlik önemli
            button.onClick.AddListener(() => OnLevelSelected(level));
        }

        private void OnLevelSelected(LevelData level)
        {
            if (!CampaignManager.Instance.IsLevelUnlocked(level)) return;

            CampaignManager.Instance.SelectLevel(level);
            Debug.Log($"LevelSelectionUI: {level.levelName} selected.");
            
            if (sideSelectionPanel != null)
            {
                sideSelectionPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}
