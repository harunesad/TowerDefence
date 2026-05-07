using UnityEngine;
using UnityEngine.UI;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SpellLoadoutItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionHighlight;
        [SerializeField] private Button button;

        private SpellData spellData;

        public void Setup(SpellData data)
        {
            spellData = data;
            if (iconImage != null) iconImage.sprite = data.icon;
            
            button.onClick.AddListener(ToggleSelection);
            RefreshUI();
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
    }
}
