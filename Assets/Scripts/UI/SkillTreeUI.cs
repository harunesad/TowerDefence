using UnityEngine;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Data;
using System.Collections.Generic;
using UnityEngine.EventSystems;
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

        [Header("Skill Limits")]
        [SerializeField] private TextMeshProUGUI skillsUnlockedText; // Current/unlocked count
        [SerializeField] private TextMeshProUGUI skillsMaxText; // Max limit

        [Header("Background")]
        [SerializeField] private BackgroundScroller backgroundScroller;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDesc;
        [SerializeField] private TextMeshProUGUI detailCost;
        [SerializeField] private Button detailBuyButton;
        [SerializeField] private Button detailCloseButton;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSFX;
        [SerializeField] private AudioClip errorSFX;

        private SkillNodeData selectedSkill;

        private float backgroundScrollOffset;

        private void Start()
        {
            if (unlockSFX == null)
            {
#if UNITY_EDITOR
                unlockSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/HeroUnlock.mp3");
#endif
            }
            if (errorSFX == null)
            {
#if UNITY_EDITOR
                errorSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/WrongAction.wav");
#endif
            }

            if (backgroundScroller != null)
            {
                // Use this gameobject's RectTransform so entire panel moves together
                backgroundScroller.SetUseThisGameObjectRect(true);
                backgroundScroller.enabled = true;
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    if (TutorialDirector.Instance != null)
                        TutorialDirector.Instance.Notify("skill_back");
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
                detailCloseButton.onClick.AddListener(OnDetailCloseClicked);
            }
            
            if (detailPanel != null) detailPanel.SetActive(false);
            SetupTreeVisuals();
        }

        public string SelectedSkillId => selectedSkill != null ? selectedSkill.skillID : null;
        public RectTransform BackTarget => backButton != null ? backButton.transform as RectTransform : null;

        public RectTransform BuyTarget
        {
            get
            {
                if (detailPanel == null || !detailPanel.activeInHierarchy || detailBuyButton == null) return null;
                return detailBuyButton.transform as RectTransform;
            }
        }

        public RectTransform CloseTarget
        {
            get
            {
                if (detailPanel == null || !detailPanel.activeInHierarchy || detailCloseButton == null) return null;
                return detailCloseButton.transform as RectTransform;
            }
        }

        public void CenterOn(RectTransform node)
        {
            if (contentRect == null || node == null) return;
            ScrollRect scroll = contentRect.GetComponentInParent<ScrollRect>();
            RectTransform view = scroll != null && scroll.viewport != null
                ? scroll.viewport
                : contentRect.parent as RectTransform;
            if (view == null) return;

            Canvas.ForceUpdateCanvases();
            Vector2 nodeCenter = view.InverseTransformPoint(node.TransformPoint(node.rect.center));
            Vector2 delta = view.rect.center - nodeCenter;
            if (delta.sqrMagnitude < 0.25f) return;
            contentRect.anchoredPosition += delta;
            if (scroll != null)
            {
                scroll.StopMovement();
                scroll.velocity = Vector2.zero;
            }
        }

        public void CloseDetailPanel()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
                RectTransform rt = detailPanel.GetComponent<RectTransform>();
                if (rt != null) rt.localScale = Vector3.one;
            }
            selectedSkill = null;
        }

        void OnDetailCloseClicked()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial != null && tutorial.LocksSkillClose()) return;
            bool finishing = tutorial != null && tutorial.WaitsSkillClose();
            CloseDetailPanel();
            if (finishing) tutorial.OnSkillBuyClosed();
        }

        void KeepDetailInside()
        {
            RectTransform rt = detailPanel.GetComponent<RectTransform>();
            RectTransform area = rt.parent as RectTransform;
            if (rt == null || area == null) return;
            rt.localScale = Vector3.one;
            float bottom = 260f;
            float top = 120f;
            float availH = area.rect.height - bottom - top;
            float panelH = rt.rect.height;
            if (availH > 200f && panelH > availH)
            {
                float fit = availH / panelH;
                rt.localScale = new Vector3(fit, fit, 1f);
                panelH = availH;
            }
            float half = area.rect.height * 0.5f;
            float minY = bottom + panelH * 0.5f - half;
            float maxY = half - top - panelH * 0.5f;
            float y = minY > maxY ? (minY + maxY) * 0.5f : Mathf.Clamp(0f, minY, maxY);
            rt.anchoredPosition = new Vector2(0f, y);
        }

        public RectTransform DetailTarget
        {
            get
            {
                if (detailPanel == null || !detailPanel.activeInHierarchy) return null;
                return detailPanel.transform as RectTransform;
            }
        }

        public void OnNodeClicked(SkillNodeData data)
        {
            if (data == null) return;
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial != null && (tutorial.Current() == "skill_info" || tutorial.Current() == "skill_info_top"))
                return;

            if (detailPanel != null && detailPanel.activeSelf && selectedSkill == data)
            {
                if (tutorial != null && (tutorial.LocksSkillClose() || tutorial.WaitsSkillClose())) return;
                CloseDetailPanel();
                return;
            }

            selectedSkill = data;
            if (detailPanel != null)
            {
                detailPanel.SetActive(true);
                KeepDetailInside();
            }
            
            if (detailIcon != null) detailIcon.sprite = data.icon;
            if (detailName != null) detailName.text = data.skillName;
            if (detailDesc != null)
            {
                string body = data.description ?? "";
                SpellData spell = data.grantedSpell;
                if (spell != null)
                {
                    if (body.Length > 0) body += "\n";
                    body += $"Power {spell.power}  |  {StatIcons.Range} {spell.radius}\n";
                    body += $"{StatIcons.Price(spell.side)} {spell.manaCost}  |  {StatIcons.Time} {spell.cooldown}s";
                }
                detailDesc.color = new Color(0.22f, 0.13f, 0.06f, 1f);
                StatIcons.Set(detailDesc, body);
            }
            if (detailCost != null)
            {
                detailCost.text = "";
                CurrencyRowView.Apply(detailCost.transform, data.karmaCost, data.crystalCost, false);
            }

            UpdateBuyButtonState();
        }

        private void BuyCurrentSkill()
        {
            if (selectedSkill == null) return;

            if (MetaProgressionManager.Instance.TryUnlockSkill(selectedSkill))
            {
                if (AudioManager.Instance != null && unlockSFX != null)
                {
                    AudioManager.Instance.PlaySFX(unlockSFX);
                }
                ShowFeedback($"{selectedSkill.skillName} Unlocked!", Color.green);
                UpdateAllNodes();
                TutorialDirector lesson = TutorialDirector.Instance;
                if (lesson != null && (lesson.Current() == "skill_buy" || lesson.Current() == "skill_buy_top"))
                {
                    lesson.OnSkillPurchased();
                    UpdateBuyButtonState();
                    return;
                }
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
                
                if (AudioManager.Instance != null && errorSFX != null)
                    AudioManager.Instance.PlaySFX(errorSFX);
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
            float startY = -800f; // Skilleri daha aşağıda başlat

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
            if (backgroundScroller != null)
            {
                backgroundScroller.enabled = true;
            }

            if (feedbackTimer > 0)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0 && feedbackText != null)
                {
                    feedbackText.text = "";
                }
            }

            TutorialDirector tutorial = TutorialDirector.Instance;
            string step = tutorial != null ? tutorial.Current() : null;
            if ((step == "skill_info" || step == "skill_info_top") && detailPanel != null && detailPanel.activeSelf)
                CloseDetailPanel();
            bool lesson = tutorial != null && tutorial.SkillHoverLocked();
            ScrollRect scroll = contentRect != null ? contentRect.GetComponentInParent<ScrollRect>() : null;
            if (scroll != null) scroll.enabled = !lesson;
            if (!lesson) return;
            if (detailPanel != null && detailPanel.activeSelf && (selectedSkill == null || selectedSkill.skillID != tutorial.CurrentSkillId()))
                CloseDetailPanel();
            if ((step == "skill_buy" || step == "skill_buy_top")
                && selectedSkill != null
                && selectedSkill.skillID == tutorial.CurrentSkillId()
                && MetaProgressionManager.Instance != null
                && MetaProgressionManager.Instance.IsSkillUnlocked(selectedSkill.skillID))
                tutorial.OnSkillPurchased();
        }

        void LateUpdate()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            string step = tutorial != null ? tutorial.Current() : null;
            bool buying = step == "skill_buy" || step == "skill_buy_top";
            if (!buying)
            {
                DropShade(detailBuyButton);
                DropShade(detailCloseButton);
                return;
            }

            bool closing = tutorial.WaitsSkillClose();
            if (detailBuyButton != null)
            {
                detailBuyButton.interactable = !closing;
                if (closing) DropShade(detailBuyButton);
                else RaiseAboveShade(detailBuyButton.gameObject);
            }
            if (detailCloseButton != null)
            {
                detailCloseButton.interactable = closing;
                if (closing) RaiseAboveShade(detailCloseButton.gameObject);
                else DropShade(detailCloseButton);
            }
        }

        static void RaiseAboveShade(GameObject go)
        {
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null) canvas = go.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 560;
            if (go.GetComponent<GraphicRaycaster>() == null)
                go.AddComponent<GraphicRaycaster>();
        }

        static void DropShade(Button button)
        {
            if (button == null) return;
            Canvas canvas = button.GetComponent<Canvas>();
            if (canvas == null) return;
            canvas.overrideSorting = false;
        }

        private void OnEnable()
        {
            UpdateAllNodes();
        }

        public void UpdateAllNodes()
        {
            if (totalKarmaText != null)
            {
                totalKarmaText.text = MetaProgressionManager.Instance.GetTotalKarma().ToString();
                CurrencyRowView.Apply(totalKarmaText.transform, MetaProgressionManager.Instance.GetTotalKarma(), 0, false);
            }
            if (totalCrystalsText != null)
            {
                totalCrystalsText.text = MetaProgressionManager.Instance.GetTotalCrystals().ToString();
                CurrencyRowView.Apply(totalCrystalsText.transform, 0, MetaProgressionManager.Instance.GetTotalCrystals(), false);
            }

            // Skill limit info gösterimi
            if (skillsUnlockedText != null || skillsMaxText != null)
            {
                int current = MetaProgressionManager.Instance.GetCurrentSkillsUnlocked();
                int maximum = MetaProgressionManager.Instance.GetMaxSkillsUnlocked();

                if (skillsUnlockedText != null)
                    skillsUnlockedText.text = $"Unlocked: {current}";

                if (skillsMaxText != null)
                {
                    if (maximum > 0)
                        skillsMaxText.text = $"/ {maximum}";
                    else
                        skillsMaxText.text = "";
                }
            }

            foreach (var node in allNodes)
            {
                if (node != null) node.RefreshStatus();
            }
        }
    }
}
