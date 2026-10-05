using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class SideSelectionUI : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button lightSideButton;
        [SerializeField] private Button darkSideButton;
        [SerializeField] private Button backButton;

        [Header("Side Selection")]
        [SerializeField] private GameObject sideSelectionGroup;
        [SerializeField] private GameObject loadoutGroup;

        [Header("Hero Loadout")]
        [SerializeField] private Transform heroItemContainer;
        [SerializeField] private GameObject heroItemPrefab;
        [SerializeField] private TextMeshProUGUI heroLoadoutHint;

        [Header("Unit Loadout")]
        public Transform unitItemContainer;
        public GameObject unitItemPrefab;
        public TextMeshProUGUI unitLoadoutHint;
        [SerializeField] private List<UnitData> allPossibleUnits;

        [Header("Spell Loadout")]
        [SerializeField] private Transform spellItemContainer;
        [SerializeField] private GameObject spellItemPrefab;

        [Header("Match")]
        [SerializeField] private Button startMatchButton;
        [SerializeField] private List<SpellData> allPossibleSpells;

        private Side currentSelectedSide;

        private void Start()
        {
            lightSideButton.onClick.AddListener(() => OnSideSelected(Side.Light));
            darkSideButton.onClick.AddListener(() => OnSideSelected(Side.Dark));

            if (startMatchButton != null)
                startMatchButton.onClick.AddListener(StartMatch);

            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                {
                    if (loadoutGroup != null && loadoutGroup.activeSelf)
                    {
                        loadoutGroup.SetActive(false);
                        sideSelectionGroup.SetActive(true);
                    }
                    else
                    {
                        MainMenuController mc = GetComponentInParent<MainMenuController>();
                        if (mc != null) mc.ShowLevelSelect();
                    }
                });
            }

            if (loadoutGroup != null) loadoutGroup.SetActive(false);
        }

        private void OnSideSelected(Side side)
        {
            currentSelectedSide = side;
            SideController.Instance.SetPlayerSide(side);

            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.SanitizeEquippedHeroesForSide(side);

                // Taraf dışındaki üniteleri loadout'tan çıkar (yanlışlıkla kalmasın)
                var equippedUnits = new List<string>(MetaProgressionManager.Instance.GetEquippedUnitNames());
                foreach (string uName in equippedUnits)
                {
                    UnitData ud = allPossibleUnits.Find(u => u != null && u.unitName == uName);
                    if (ud != null && ud.side != side)
                    {
                        MetaProgressionManager.Instance.UnequipUnit(uName);
                    }
                }

                // Taraf dışındaki büyüleri de çıkar
                var equippedSpells = new List<string>(MetaProgressionManager.Instance.GetEquippedSpellIDs());
                foreach (string sID in equippedSpells)
                {
                    SpellData sd = allPossibleSpells.Find(s => s != null && s.spellID == sID);
                    if (sd != null && sd.side != side && sd.side != Side.Neutral)
                    {
                        MetaProgressionManager.Instance.UnequipSpell(sID);
                    }
                }
            }

            if (sideSelectionGroup != null) sideSelectionGroup.SetActive(false);
            if (loadoutGroup != null)
            {
                loadoutGroup.SetActive(true);
                PopulateLoadout(side);
            }
            else
            {
                StartMatch();
            }
        }

        private void PopulateLoadout(Side side)
        {
            PopulateHeroLoadout(side);
            PopulateSpellLoadout(side);
            PopulateUnitLoadout(side);
            RefreshStartMatchButton();
        }

        private void PopulateHeroLoadout(Side side)
        {
            if (heroItemContainer == null || heroItemPrefab == null) return;

            foreach (Transform child in heroItemContainer)
                Destroy(child.gameObject);

            if (MetaProgressionManager.Instance == null) return;

            foreach (HeroData hero in MetaProgressionManager.Instance.GetAllHeroes())
            {
                if (hero == null || hero.side != side) continue;
                if (!MetaProgressionManager.Instance.IsHeroUnlocked(hero.heroID)) continue;

                GameObject go = Instantiate(heroItemPrefab, heroItemContainer);
                HeroLoadoutItemUI itemUI = go.GetComponent<HeroLoadoutItemUI>();
                if (itemUI != null) itemUI.Setup(hero);
            }

            if (heroLoadoutHint != null)
                heroLoadoutHint.text = "SELECT HEROES (Max 2)";
        }

        private void PopulateSpellLoadout(Side side)
        {
            if (spellItemContainer == null || spellItemPrefab == null) return;

            foreach (Transform child in spellItemContainer)
                Destroy(child.gameObject);

            // En yüksek seviyeli büyüleri bulmak için gruplandır
            Dictionary<string, SpellData> bestSpells = new Dictionary<string, SpellData>();
            Dictionary<string, int> highestTiers = new Dictionary<string, int>();

            foreach (SpellData spell in allPossibleSpells)
            {
                if (spell == null) continue;
                if (spell.side != side && spell.side != Side.Neutral) continue;
                if (!MetaProgressionManager.Instance.IsSpellUnlocked(spell.spellID)) continue;

                // Spell_Light_Thunder_1 -> family: Spell_Light_Thunder, tier: 1
                string family = spell.spellID;
                int tier = 1;
                int lastUnderscore = spell.spellID.LastIndexOf('_');
                if (lastUnderscore > 0 && lastUnderscore < spell.spellID.Length - 1)
                {
                    string suffix = spell.spellID.Substring(lastUnderscore + 1);
                    if (int.TryParse(suffix, out int parsedTier))
                    {
                        family = spell.spellID.Substring(0, lastUnderscore);
                        tier = parsedTier;
                    }
                }

                if (!highestTiers.ContainsKey(family) || tier > highestTiers[family])
                {
                    highestTiers[family] = tier;
                    bestSpells[family] = spell;
                }
            }

            foreach (SpellData spell in bestSpells.Values)
            {
                GameObject go = Instantiate(spellItemPrefab, spellItemContainer);
                SpellLoadoutItemUI itemUI = go.GetComponent<SpellLoadoutItemUI>();
                if (itemUI != null) itemUI.Setup(spell);
            }
        }

        public static readonly HashSet<string> BossNames = new HashSet<string> {
            "Abyssal Behemoth", "Blood Mage", "Bone Dragon", "Death Knight Commander", "Demon King", "Shadow Leviathan",
            "Archangel", "Avatar of Light", "Dragon of the Sun", "Grand Paladin", "Holy Colossus", "Phoenix Summoner"
        };

        private Transform bossItemContainer;
        private TextMeshProUGUI bossLoadoutHint;

        private void SetupBossUI()
        {
            if (bossItemContainer != null) return;
            
            // unitItemContainer = UnitContent, onun parent'i UnitScroll
            if (unitItemContainer == null || unitItemContainer.parent == null) return;

            Transform unitScroll = unitItemContainer.parent; 
            GameObject bossScrollObj = Instantiate(unitScroll.gameObject, unitScroll.parent);
            bossScrollObj.name = "BossScroll";
            
            RectTransform bossRT = bossScrollObj.GetComponent<RectTransform>();
            bossRT.anchoredPosition = new Vector2(bossRT.anchoredPosition.x, bossRT.anchoredPosition.y - 150f);
            
            ScrollRect bossScroll = bossScrollObj.GetComponent<ScrollRect>();
            if (bossScroll != null)
                bossItemContainer = bossScroll.content;
            else
                bossItemContainer = bossScrollObj.transform.Find("UnitContent") ?? bossScrollObj.transform; // Fallback

            // UnitHint kopyası
            if (unitLoadoutHint != null)
            {
                GameObject bossHintObj = Instantiate(unitLoadoutHint.gameObject, unitLoadoutHint.transform.parent);
                bossHintObj.name = "BossLoadoutHint";
                RectTransform hintRT = bossHintObj.GetComponent<RectTransform>();
                hintRT.anchoredPosition = new Vector2(hintRT.anchoredPosition.x, hintRT.anchoredPosition.y - 150f);
                bossLoadoutHint = bossHintObj.GetComponent<TextMeshProUGUI>();
            }
        }

        private void PopulateUnitLoadout(Side side)
        {
            SetupBossUI();

            if (unitItemContainer == null || unitItemPrefab == null) return;

            foreach (Transform child in unitItemContainer) Destroy(child.gameObject);
            foreach (Transform child in bossItemContainer) Destroy(child.gameObject);

            if (MetaProgressionManager.Instance == null) return;

            HashSet<string> addedNames = new HashSet<string>();
            
            foreach (UnitData unit in allPossibleUnits)
            {
                if (unit == null || unit.side != side) continue;
                if (string.IsNullOrEmpty(unit.unitName) || addedNames.Contains(unit.unitName)) continue;

                addedNames.Add(unit.unitName);

                bool isBoss = BossNames.Contains(unit.unitName);
                Transform targetContainer = isBoss ? bossItemContainer : unitItemContainer;

                GameObject go = Instantiate(unitItemPrefab, targetContainer);
                UnitLoadoutItemUI itemUI = go.GetComponent<UnitLoadoutItemUI>();
                if (itemUI != null) itemUI.Setup(unit);
            }
            
            RefreshStartMatchButton();
        }

        public void RefreshStartMatchButton()
        {
            if (MetaProgressionManager.Instance == null) return;

            List<string> equipped = MetaProgressionManager.Instance.GetEquippedUnitNames();
            int normalCount = 0;
            int bossCount = 0;

            foreach(var u in equipped)
            {
                if (BossNames.Contains(u)) bossCount++;
                else normalCount++;
            }

            bool normalOk = normalCount == 4;
            bool bossOk = bossCount == 1;

            if (unitLoadoutHint != null)
            {
                unitLoadoutHint.text = normalOk ? "NORMAL UNITS (4/4)" : $"SELECT 4 NORMAL UNITS! ({normalCount}/4)";
                unitLoadoutHint.color = normalOk ? Color.green : Color.red;
            }

            if (bossLoadoutHint != null)
            {
                bossLoadoutHint.text = bossOk ? "BOSS UNIT (1/1)" : $"SELECT 1 BOSS UNIT! ({bossCount}/1)";
                bossLoadoutHint.color = bossOk ? Color.green : Color.red;
            }

            if (startMatchButton != null) 
                startMatchButton.interactable = (normalOk && bossOk);
        }

        private void StartMatch()
        {
            if (MetaProgressionManager.Instance == null) return;

            List<string> equipped = MetaProgressionManager.Instance.GetEquippedUnitNames();
            int normalCount = 0;
            int bossCount = 0;
            foreach(var u in equipped)
            {
                if (BossNames.Contains(u)) bossCount++;
                else normalCount++;
            }

            if (normalCount != 4 || bossCount != 1)
            {
                RefreshStartMatchButton();
                return;
            }

            // Kahraman seçilmemişse bile savaşa girilebilir, auto-equip yapmıyoruz.
            // Bu sayede oyuncu istemezse kahraman olmadan oynayabilir.

            Debug.Log("Starting match with side: " + currentSelectedSide);
            CampaignManager.Instance.LoadSelectedLevel();
        }
    }
}
