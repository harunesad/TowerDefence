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

            if (MetaProgressionManager.Instance == null) return;

            List<string> equippedIDs = MetaProgressionManager.Instance.GetEquippedSpellIDs();
            if (equippedIDs == null || equippedIDs.Count == 0) return;

            // Tüm büyüleri topla: Inspector listesi + Resources'tan dinamik yükleme
            Dictionary<string, SpellData> allSpellsMap = new Dictionary<string, SpellData>();

            // Inspector'daki listeler varsa ekle
            if (lightSpells != null)
                foreach (var s in lightSpells)
                    if (s != null && !allSpellsMap.ContainsKey(s.spellID))
                        allSpellsMap[s.spellID] = s;

            if (darkSpells != null)
                foreach (var s in darkSpells)
                    if (s != null && !allSpellsMap.ContainsKey(s.spellID))
                        allSpellsMap[s.spellID] = s;

            // Resources/Data/Spells altından da yükle (eksik olanları yakalar)
            SpellData[] resourceSpells = Resources.LoadAll<SpellData>("Data/Spells");
            if (resourceSpells != null)
                foreach (var s in resourceSpells)
                    if (s != null && !allSpellsMap.ContainsKey(s.spellID))
                        allSpellsMap[s.spellID] = s;

            // Assets/Data/Spells klasöründen de yükle (Resources dışındakiler için)
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpellData", new[] { "Assets/Data/Spells" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                SpellData s = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellData>(path);
                if (s != null && !allSpellsMap.ContainsKey(s.spellID))
                    allSpellsMap[s.spellID] = s;
            }
            #endif

            // Equipped büyüleri göster
            foreach (string spellID in equippedIDs)
            {
                if (allSpellsMap.TryGetValue(spellID, out SpellData spell))
                {
                    GameObject btnGO = Instantiate(spellButtonPrefab, spellContainer);
                    SpellButtonUI btnScript = btnGO.GetComponent<SpellButtonUI>();
                    if (btnScript != null)
                    {
                        btnScript.Setup(spell);
                    }
                }
                else
                {
                    Debug.LogWarning($"SpellSelectionUI: Equipped spell '{spellID}' bulunamadı!");
                }
            }
        }
    }
}
