using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class SpellSelectionUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject spellButtonPrefab;
        [SerializeField] private Transform spellContainer;
        
        [Header("Spell Lists")]
        [SerializeField] private List<SpellData> lightSpells;
        [SerializeField] private List<SpellData> darkSpells;

        private void Start()
        {
            // HUD yüklendiğinde tarafa göre büyüleri listele
            PopulateSpells();
        }

        public void PopulateSpells()
        {
            if (spellContainer == null || spellButtonPrefab == null) return;

            // Mevcut butonları temizle
            foreach (Transform child in spellContainer)
            {
                Destroy(child.gameObject);
            }

            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            List<SpellData> targetList = (playerSide == Side.Light) ? lightSpells : darkSpells;

            if (targetList == null || targetList.Count == 0)
            {
                Debug.LogWarning("SpellSelectionUI: No spells found for side " + playerSide);
                return;
            }

            List<string> equippedIDs = MetaProgressionManager.Instance.GetEquippedSpellIDs();

            foreach (var spell in targetList)
            {
                if (spell == null) continue;
                
                // Sadece kuşanılmış olanları göster
                if (!equippedIDs.Contains(spell.spellID)) continue;

                GameObject btnGO = Instantiate(spellButtonPrefab, spellContainer);
                SpellButtonUI btnScript = btnGO.GetComponent<SpellButtonUI>();
                if (btnScript != null)
                {
                    btnScript.Setup(spell);
                }
            }
        }
    }
}
