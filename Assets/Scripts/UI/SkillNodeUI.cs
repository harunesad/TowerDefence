using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SkillNodeUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Data")]
        [SerializeField] private SkillNodeData skillData;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image lockedOverlay;
        [SerializeField] private Image purchasedOverlay;
        [SerializeField] private TextMeshProUGUI typeText;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSFX;
        [SerializeField] private AudioClip errorSFX;

        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;
        Selectable.Transition restingTransition;
        bool transitionCaptured;

        private void LateUpdate()
        {
            if (buyButton == null) return;
            if (!transitionCaptured)
            {
                restingTransition = buyButton.transition;
                transitionCaptured = true;
            }
            bool lesson = TutorialDirector.Instance != null && TutorialDirector.Instance.SkillHoverLocked();
            bool lessonTarget = lesson && skillData != null && skillData.skillID == TutorialDirector.Instance.CurrentSkillId();
            buyButton.transition = lesson ? Selectable.Transition.None : restingTransition;
            if (lessonTarget && MetaProgressionManager.Instance != null && !MetaProgressionManager.Instance.IsSkillUnlocked(skillData.skillID))
                buyButton.interactable = true;
        }

        private void Start()
        {
            if (skillData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            SetupUI(skillData);

            buyButton.onClick.RemoveAllListeners();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnNodePointerClick(eventData);
        }

        public void SetupUI(SkillNodeData data)
        {
            skillData = data;
            iconImage.sprite = data.icon;

            bool showKarma = data.karmaCost > 0;
            bool showCrystal = data.crystalCost > 0;
            Transform karmaIcon = costText.transform.Find("KarmaIcon");
            Transform crystalIcon = costText.transform.Find("CrystalIcon");
            if (karmaIcon != null) karmaIcon.gameObject.SetActive(showKarma);
            if (crystalIcon != null) crystalIcon.gameObject.SetActive(showCrystal);

            if (showKarma && showCrystal)
                costText.text = $"{data.karmaCost}   {data.crystalCost}";
            else if (showCrystal)
                costText.text = data.crystalCost.ToString();
            else
                costText.text = data.karmaCost.ToString();

            if (typeText != null)
            {
                bool isActive = data.upgradeType == UpgradeType.UnlockSpell;
                bool isImportant = data.crystalCost > 0;
                
                string sideStr = data.side.ToString().ToUpper();
                string typeStr = isActive ? "ACTIVE" : "PASSIVE";

                if (isImportant)
                {
                    typeText.text = $"★ {sideStr} {typeStr} ★";
                    typeText.color = new Color(0.2f, 0.8f, 1f);
                }
                else
                {
                    typeText.text = $"{sideStr} {typeStr}";
                    typeText.color = isActive ? new Color(1f, 0.8f, 0.2f) : Color.white;
                }
            }

            RefreshStatus();
        }

        public void RefreshStatus()
        {
            if (skillData == null) return;

            bool isUnlocked = MetaProgressionManager.Instance.IsSkillUnlocked(skillData.skillID);
            int maxUnlocked = MetaProgressionManager.Instance.GetMaxSkillsUnlocked();
            int currentUnlocked = MetaProgressionManager.Instance.GetCurrentSkillsUnlocked();
            bool spellUnlock = skillData.upgradeType == UpgradeType.UnlockSpell;

            purchasedOverlay.gameObject.SetActive(isUnlocked);
            if (iconImage != null)
            {
                bool choiceLocked = TutorialDirector.Instance != null && TutorialDirector.Instance.SkillChoiceLocked(skillData.skillID);
                iconImage.color = choiceLocked ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
            }
            bool lessonTarget = TutorialDirector.Instance != null
                && TutorialDirector.Instance.SkillHoverLocked()
                && skillData.skillID == TutorialDirector.Instance.CurrentSkillId();
            buyButton.interactable = !isUnlocked && (lessonTarget || spellUnlock || maxUnlocked <= 0 || currentUnlocked < maxUnlocked);

            bool requirementsMet = true;
            foreach (var req in skillData.requiredSkills)
            {
                if (!MetaProgressionManager.Instance.IsSkillUnlocked(req.skillID))
                {
                    requirementsMet = false;
                    break;
                }
            }

            lockedOverlay.gameObject.SetActive(!requirementsMet && !isUnlocked);

            if (!isUnlocked)
            {
                bool canAffordKarma = true;
                bool canAffordCrystals = true;
                if (skillData.karmaCost > 0)
                    canAffordKarma = MetaProgressionManager.Instance.GetTotalKarma() >= skillData.karmaCost;
                if (skillData.crystalCost > 0)
                    canAffordCrystals = MetaProgressionManager.Instance.GetTotalCrystals() >= skillData.crystalCost;

                costText.color = (canAffordKarma && canAffordCrystals) ? new Color(0.6f, 1f, 0.6f) : Color.red;
            }
        }

        private void OnNodePointerClick(BaseEventData eventData)
        {
            if (eventData is PointerEventData pointerData && pointerData.button != PointerEventData.InputButton.Left) return;
            if (skillData != null && TutorialDirector.Instance != null && TutorialDirector.Instance.SkillChoiceLocked(skillData.skillID))
                return;

            float timeSinceLastClick = Time.unscaledTime - lastClickTime;
            lastClickTime = Time.unscaledTime;

            if (timeSinceLastClick < DoubleClickThreshold)
            {
                CancelInvoke(nameof(ExecuteSingleClick));
                ShowInfoPanel();
            }
            else
            {
                Invoke(nameof(ExecuteSingleClick), DoubleClickThreshold);
            }
        }

        private void ExecuteSingleClick()
        {
            SkillTreeUI treeUI = GetComponentInParent<SkillTreeUI>();
            if (treeUI != null)
            {
                treeUI.OnNodeClicked(skillData);
            }
        }

        private void ShowInfoPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            if (canvas.GetComponentInChildren<SkillInfoPanelUI>(true) != null) return;

            GameObject go = new GameObject("SkillInfoPanelUI", typeof(RectTransform), typeof(SkillInfoPanelUI));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<SkillInfoPanelUI>().Setup(skillData, GetComponent<RectTransform>());
            if (TutorialDirector.Instance != null)
            {
                string step = TutorialDirector.Instance.Current();
                if ((step == "skill_info" || step == "skill_info_top")
                    && skillData != null && skillData.skillID == TutorialDirector.Instance.CurrentSkillId())
                    TutorialDirector.Instance.Notify(step);
            }
        }

        public SkillNodeData GetSkillData() => skillData;
        public RectTransform IconRect => iconImage != null ? iconImage.rectTransform : transform as RectTransform;

        private void OnDisable()
        {
            CancelInvoke(nameof(ExecuteSingleClick));
        }
    }
}
