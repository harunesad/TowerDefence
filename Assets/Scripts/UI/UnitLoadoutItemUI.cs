using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class UnitLoadoutItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionHighlight;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button button;

        private UnitData unitData;

        public void Setup(UnitData data)
        {
            unitData = data;
            if (iconImage != null) iconImage.sprite = data.icon;
            if (nameText != null) nameText.text = data.unitName;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ToggleSelection);
            }
            RefreshUI();
        }

        private void ToggleSelection()
        {
            if (unitData == null || MetaProgressionManager.Instance == null) return;

            if (MetaProgressionManager.Instance.IsUnitEquipped(unitData.unitName))
                MetaProgressionManager.Instance.UnequipUnit(unitData.unitName);
            else
                MetaProgressionManager.Instance.EquipUnit(unitData.unitName);

            RefreshAllLoadoutItems();
            
            // Invoke an action if needed to refresh the Start Match button
            SideSelectionUI parentUI = GetComponentInParent<SideSelectionUI>();
            if (parentUI != null) parentUI.RefreshStartMatchButton();
        }

        public void RefreshUI()
        {
            if (unitData == null || MetaProgressionManager.Instance == null) return;

            bool isEquipped = MetaProgressionManager.Instance.IsUnitEquipped(unitData.unitName);
            if (selectionHighlight != null)
                selectionHighlight.gameObject.SetActive(isEquipped);
        }

        private void RefreshAllLoadoutItems()
        {
            var items = transform.parent.GetComponentsInChildren<UnitLoadoutItemUI>(true);
            foreach (var item in items)
                item.RefreshUI();
        }
    }
}
