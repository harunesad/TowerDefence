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
            if (detailCost != null) detailCost.text = $"Cost: {data.karmaCost} Karma & {data.crystalCost} Crystals";

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
                else msg = "Prerequisites not met!";
                
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
            // Tüm düğümleri pozisyonlarına göre yerleştir
            foreach (var node in allNodes)
            {
                if (node == null) continue;
                var data = node.GetSkillData();
                if (data != null)
                {
                    node.GetComponent<RectTransform>().anchoredPosition = data.visualPosition;
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
