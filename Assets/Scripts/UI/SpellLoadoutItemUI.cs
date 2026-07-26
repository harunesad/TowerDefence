using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SpellLoadoutItemUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionHighlight;
        [SerializeField] private Button button;

        private SpellData spellData;
        private GameObject activePanel;
        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;

        public void Setup(SpellData data)
        {
            spellData = data;
            if (iconImage != null) iconImage.sprite = data.icon;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
            RefreshUI();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

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
            ToggleSelection();
        }

        private void ShowInfoPanel()
        {
            if (activePanel != null)
            {
                Destroy(activePanel);
                activePanel = null;
                return;
            }

            var existing = FindObjectOfType<SpellInfoPanelUI>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
                activePanel = null;
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject go = new GameObject("SpellInfoPanelUI", typeof(RectTransform), typeof(SpellInfoPanelUI));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<SpellInfoPanelUI>().Setup(spellData, GetComponent<RectTransform>());
            activePanel = go;
        }

        private void ToggleSelection()
        {
            if (MetaProgressionManager.Instance.GetEquippedSpellIDs().Contains(spellData.spellID))
            {
                MetaProgressionManager.Instance.UnequipSpell(spellData.spellID);
            }
            else
            {
                MetaProgressionManager.Instance.EquipSpell(spellData.spellID);
            }

            RefreshUI();
        }

        public void RefreshUI()
        {
            bool isEquipped = MetaProgressionManager.Instance.GetEquippedSpellIDs().Contains(spellData.spellID);
            if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(isEquipped);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(ExecuteSingleClick));
        }
    }
}
