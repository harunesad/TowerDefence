using UnityEngine;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Data;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI totalKarmaText;
        [SerializeField] private TextMeshProUGUI totalCrystalsText; // Yeni: Kristal göstergesi
        [SerializeField] private TextMeshProUGUI feedbackText; 
        [SerializeField] private List<SkillNodeUI> allNodes;
        [SerializeField] private Button backButton;
        [SerializeField] private RectTransform contentRect; // Tree nodes parent

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDesc;
        [SerializeField] private TextMeshProUGUI detailCost;
        [SerializeField] private Button detailBuyButton;
        [SerializeField] private Button detailCloseButton;

        private SkillNodeData selectedSkill;

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }

            if (detailBuyButton != null)
            {
                detailBuyButton.onClick.AddListener(BuyCurrentSkill);
            }

            if (detailCloseButton != null)
            {
                detailCloseButton.onClick.AddListener(CloseDetailPanel);
            }
            
            if (detailPanel != null) detailPanel.SetActive(false);
            SetupTreeVisuals();
        }

        public void CloseDetailPanel()
        {
            if (detailPanel != null) detailPanel.SetActive(false);
            selectedSkill = null;
        }

        public void OnNodeClicked(SkillNodeData data)
        {
            if (detailPanel.activeSelf)
            {
                detailPanel.SetActive(false);
                selectedSkill = null;
                return;
            }

            selectedSkill = data;
            detailPanel.SetActive(true);
            
            if (detailIcon != null) detailIcon.sprite = data.icon;
            if (detailName != null) detailName.text = data.skillName;
            if (detailDesc != null) detailDesc.text = data.description;
            if (detailCost != null)
            {
                if (data.crystalCost > 0 && data.karmaCost == 0)
                    detailCost.text = $"Cost: {data.crystalCost} Crystals";
                else if (data.crystalCost > 0)
                    detailCost.text = $"Cost: {data.karmaCost} Karma + {data.crystalCost} Crystals";
                else
                    detailCost.text = $"Cost: {data.karmaCost} Karma";
            }

            UpdateBuyButtonState();
        }

        private void BuyCurrentSkill()
        {
            if (selectedSkill == null) return;

            if (MetaProgressionManager.Instance.TryUnlockSkill(selectedSkill))
            {
                ShowFeedback($"{selectedSkill.skillName} Unlocked!", Color.green);
                UpdateAllNodes();
                detailPanel.SetActive(false);
                selectedSkill = null;
            }
            else
            {
                bool canAffordKarma = MetaProgressionManager.Instance.GetTotalKarma() >= selectedSkill.karmaCost;
                bool canAffordCrystals = MetaProgressionManager.Instance.GetTotalCrystals() >= selectedSkill.crystalCost;
                
                string msg = "";
                if (!canAffordKarma) msg = "Not enough Karma!";
                else if (!canAffordCrystals) msg = "Not enough Crystals!";
                else
                {
                    System.Collections.Generic.List<string> missing = new System.Collections.Generic.List<string>();
                    if (selectedSkill.requiredSkills != null)
                    {
                        foreach (var req in selectedSkill.requiredSkills)
                        {
                            if (!MetaProgressionManager.Instance.IsSkillUnlocked(req.skillID))
                            {
                                missing.Add(req.skillName);
                            }
                        }
                    }
                    
                    if (missing.Count > 0)
                        msg = "Requires: " + string.Join(", ", missing);
                    else
                        msg = "Prerequisites not met!";
                }
                
                ShowFeedback(msg, Color.red);
            }
        }

        private void UpdateBuyButtonState()
        {
            if (selectedSkill == null || detailBuyButton == null) return;

            bool isUnlocked = MetaProgressionManager.Instance.IsSkillUnlocked(selectedSkill.skillID);
            detailBuyButton.interactable = !isUnlocked;
            
            var btnText = detailBuyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = isUnlocked ? "PURCHASED" : "BUY SKILL";
            }
        }

        private void SetupTreeVisuals()
        {
            float columnSpacing = 280f;
            float rowSpacing = 240f;
            
            // Merkezden hizalamak veya en soldan başlatmak için ofset (örneğin 8 kategori var)
            float startX = -((8 - 1) * columnSpacing) / 2f; 
            float startY = -300f; // Aşağıdan (Tier 0) başlar

            // Tüm düğümleri tier ve category'ye göre otomatik yerleştir
            foreach (var node in allNodes)
            {
                if (node == null) continue;
                var data = node.GetSkillData();
                if (data != null)
                {
                    int colIndex = (int)data.category;
                    int rowIndex = data.tier;

                    Vector2 autoPosition = new Vector2(startX + (colIndex * columnSpacing), startY + (rowIndex * rowSpacing));
                    node.GetComponent<RectTransform>().anchoredPosition = autoPosition;
                }
            }
        }

        public void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
                feedbackTimer = 3f;
            }
        }

        private float feedbackTimer;
        private void Update()
        {
            if (feedbackTimer > 0)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0 && feedbackText != null)
                {
                    feedbackText.text = "";
                }
            }
        }

        private void OnEnable()
        {
            UpdateAllNodes();
        }

        public void UpdateAllNodes()
        {
            if (totalKarmaText != null)
            {
                totalKarmaText.text = $"Karma: {MetaProgressionManager.Instance.GetTotalKarma()}";
            }
            if (totalCrystalsText != null)
            {
                totalCrystalsText.text = $"Crystals: {MetaProgressionManager.Instance.GetTotalCrystals()}";
            }

            foreach (var node in allNodes)
            {
                if (node != null) node.RefreshStatus();
            }
        }
    }
}
