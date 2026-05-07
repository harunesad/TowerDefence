using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class SideSelectionUI : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button lightSideButton;
        [SerializeField] private Button darkSideButton;
        [SerializeField] private Button backButton;

        [Header("Spell Loadout UI")]
        [SerializeField] private GameObject sideSelectionGroup; // Taraf butonlarının olduğu grup
        [SerializeField] private GameObject spellLoadoutGroup;   // Büyü seçim paneli
        [SerializeField] private Transform spellItemContainer;   // Büyülerin listeleneceği yer
        [SerializeField] private GameObject spellItemPrefab;    // Seçilebilir büyü prefabı
        [SerializeField] private Button startMatchButton;        // Savaşı başlat butonu

        [SerializeField] private List<Data.SpellData> allPossibleSpells; // Inspector üzerinden veya otomasyonla doldurulur

        private Side currentSelectedSide;

        private void Start()
        {
            lightSideButton.onClick.AddListener(() => OnSideSelected(Side.Light));
            darkSideButton.onClick.AddListener(() => OnSideSelected(Side.Dark));

            if (startMatchButton != null)
                startMatchButton.onClick.AddListener(StartMatch);

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    if (spellLoadoutGroup != null && spellLoadoutGroup.activeSelf)
                    {
                        // Büyü seçiminden taraf seçimine geri dön
                        spellLoadoutGroup.SetActive(false);
                        sideSelectionGroup.SetActive(true);
                    }
                    else
                    {
                        MainMenuController mc = GetComponentInParent<MainMenuController>();
                        if (mc != null) mc.ShowLevelSelect();
                    }
                });
            }

            // Başlangıç durumu
            if (spellLoadoutGroup != null) spellLoadoutGroup.SetActive(false);
        }

        private void OnSideSelected(Side side)
        {
            currentSelectedSide = side;
            SideController.Instance.SetPlayerSide(side);
            
            // Taraf seçildikten sonra büyü seçme panelini aç
            if (sideSelectionGroup != null) sideSelectionGroup.SetActive(false);
            if (spellLoadoutGroup != null) 
            {
                spellLoadoutGroup.SetActive(true);
                PopulateSpellLoadout(side);
            }
            else
            {
                // Eğer panel yoksa direkt başla (Fallback)
                StartMatch();
            }
        }

        private void PopulateSpellLoadout(Side side)
        {
            if (spellItemContainer == null || spellItemPrefab == null) return;

            // Temizle
            foreach (Transform child in spellItemContainer) Destroy(child.gameObject);

            // Filtrele ve oluştur
            foreach (var spell in allPossibleSpells)
            {
                if (spell == null) continue;

                // Tarafı uymuyorsa veya satın alınmamışsa gösterme
                if (spell.side != side && spell.side != Side.Neutral) continue;
                if (!MetaProgressionManager.Instance.IsSpellUnlocked(spell.spellID)) continue;

                GameObject go = Instantiate(spellItemPrefab, spellItemContainer);
                var itemUI = go.GetComponent<SpellLoadoutItemUI>();
                if (itemUI != null) itemUI.Setup(spell);
            }
        }

        private void StartMatch()
        {
            Debug.Log("Starting match with side: " + currentSelectedSide);
            CampaignManager.Instance.LoadSelectedLevel();
        }
    }
}
