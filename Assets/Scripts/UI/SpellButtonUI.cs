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
        
        private Outline selectionOutline;

        private void Start()
        {
            if (spellData != null) Setup(spellData);
            button.onClick.AddListener(OnClicked);
            
            // Seçim (Selected) parlaması için Outline ekle
            selectionOutline = gameObject.GetComponent<Outline>();
            if (selectionOutline == null) selectionOutline = gameObject.AddComponent<Outline>();
            
            selectionOutline.effectColor = Color.green;
            selectionOutline.effectDistance = new Vector2(4f, -4f);
            selectionOutline.enabled = false;
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
            bool canAfford = CurrencyManager.Instance.CanAfford(SideController.Instance.GetPlayerSide(), spellData.manaCost);
            bool isSelected = SpellManager.Instance.SelectedSpell == spellData;

            if (remaining > 0)
            {
                cooldownOverlay.fillAmount = remaining / total;
                button.interactable = false;
                
                // Bekleme süresindeyken saniye yazsın
                costText.text = $"{remaining:F1}s";
                costText.color = Color.yellow;
                iconImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Karart
            }
            else
            {
                cooldownOverlay.fillAmount = 0;
                button.interactable = canAfford;
                
                // Bekleme bitince mana miktarını yazsın
                costText.text = spellData.manaCost.ToString();
                
                if (canAfford)
                {
                    costText.color = Color.white;
                    iconImage.color = Color.white; // Normal
                }
                else
                {
                    costText.color = Color.red; // Mana yetersizse kırmızı yazsın
                    iconImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Karart
                }
            }
            
            // Seçim durumu (Outline'ı aç/kapat)
            if (selectionOutline != null)
            {
                selectionOutline.enabled = isSelected;
            }
        }

        private void OnClicked()
        {
            if (SpellManager.Instance != null && spellData != null)
            {
                // Eğer zaten seçiliyse seçimi iptal et, değilse seç
                if (SpellManager.Instance.SelectedSpell == spellData)
                {
                    SpellManager.Instance.ClearSelectedSpell();
                }
                else
                {
                    SpellManager.Instance.SelectSpell(spellData);
                }
            }
        }
    }
}
