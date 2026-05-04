using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SpellButtonUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SpellData spellData;

        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button button;

        private void Start()
        {
            if (spellData != null) Setup(spellData);
            button.onClick.AddListener(OnClicked);
        }

        public void Setup(SpellData data)
        {
            spellData = data;
            iconImage.sprite = data.icon;
            costText.text = data.manaCost.ToString();
        }

        private void Update()
        {
            if (spellData == null) return;

            float remaining = SpellManager.Instance.GetRemainingCooldown(spellData.spellID);
            float total = spellData.cooldown;

            if (remaining > 0)
            {
                cooldownOverlay.fillAmount = remaining / total;
                button.interactable = false;
            }
            else
            {
                cooldownOverlay.fillAmount = 0;
                // Para kontrolü
                button.interactable = CurrencyManager.Instance.CanAfford(SideController.Instance.GetPlayerSide(), spellData.manaCost);
            }
        }

        private void OnClicked()
        {
            if (SpellManager.Instance != null && spellData != null)
            {
                SpellManager.Instance.SelectSpell(spellData);
            }
        }
    }
}
