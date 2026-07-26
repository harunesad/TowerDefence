using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SkillNodeUI : MonoBehaviour
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

        private void Start()
        {
            if (skillData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            SetupUI(skillData);

            buyButton.onClick.RemoveAllListeners();

            EventTrigger trigger = buyButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = buyButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(OnNodePointerClick);
            trigger.triggers.Add(entry);
        }

        public void SetupUI(SkillNodeData data)
        {
            skillData = data;
            iconImage.sprite = data.icon;

            if (data.crystalCost > 0 && data.karmaCost == 0)
                costText.text = $"{data.crystalCost} C";
            else if (data.crystalCost > 0)
                costText.text = $"{data.karmaCost}/{data.crystalCost}";
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

            purchasedOverlay.gameObject.SetActive(isUnlocked);
            buyButton.interactable = true;

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
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject go = new GameObject("SkillInfoPanelUI", typeof(RectTransform), typeof(SkillInfoPanelUI));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<SkillInfoPanelUI>().Setup(skillData, GetComponent<RectTransform>());
        }

        public SkillNodeData GetSkillData() => skillData;

        private void OnDisable()
        {
            CancelInvoke(nameof(ExecuteSingleClick));
        }
    }
}
