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
        [SerializeField] private TextMeshProUGUI levelTitleText;
        [SerializeField] private TextMeshProUGUI firstStarsRewardText;
        [SerializeField] private TextMeshProUGUI secondStarsRewardText;
        [SerializeField] private TextMeshProUGUI thirdStarsRewardText;

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

        private void OnEnable()
        {
            UpdateLevelInfo();
        }

        public void UpdateLevelInfo()
        {
            if (CampaignManager.Instance == null) return;
            LevelData level = CampaignManager.Instance.GetCurrentLevel();
            if (level != null && levelTitleText != null)
            {
                string rawName = level.levelName.ToUpper();
                if (rawName.Contains(":"))
                {
                    string[] parts = rawName.Split(new char[] { ':' }, 2);
                    levelTitleText.text = parts[0].Trim() + "\n" + parts[1].Trim();
                }
                else
                {
                    levelTitleText.text = rawName;
                }
            }

            int globalDifficulty = CampaignManager.Instance.CurrentDifficultyLevel;
            if (globalDifficulty < 1) globalDifficulty = 1;

            int levelDifficulty = (level != null && level.difficulty > 0) ? level.difficulty : 1;
            int totalMultiplier = levelDifficulty * globalDifficulty;

            ShowStarReward(firstStarsRewardText, 25 * 3 * totalMultiplier, 5 * 3 * totalMultiplier);
            ShowStarReward(secondStarsRewardText, 25 * 2 * totalMultiplier, 5 * 2 * totalMultiplier);
            ShowStarReward(thirdStarsRewardText, 25 * 1 * totalMultiplier, 5 * 1 * totalMultiplier);
        }

        private static void ShowStarReward(TextMeshProUGUI label, int karma, int crystals)
        {
            if (label == null) return;
            label.text = "";
            CurrencyRowView.Apply(label.transform, karma, crystals, true);
        }

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
                // Taraf deÄŸistiÄŸinde o tarafÄ±n kayÄ±tlÄ± unit, hero ve bÃ¼yÃ¼lerini yÃ¼kle
                MetaProgressionManager.Instance.SwitchSideLoadout(side);
                if (TutorialDirector.Instance != null)
                    TutorialDirector.Instance.OnSideChosen(side);
                if (TutorialDirector.Instance != null && TutorialDirector.Instance.ConsumePreparedArmy())
                    PrepareTutorialArmy(side);
                if (side == Side.Dark && TutorialDirector.Instance != null && TutorialDirector.Instance.IsDarkLoadoutLesson())
                    PrepareDarkLesson();
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
            "Abyssal Behemoth", "Blood Mage", "Bone Dragon", "Death Knight", "Death Knight Commander", "Demon King", "Shadow Leviathan",
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

            bool tutorialOk = TutorialDirector.Instance == null || TutorialDirector.Instance.AllowsStartMatch();
            if (startMatchButton != null) 
                startMatchButton.interactable = (normalOk && bossOk && tutorialOk);
        }

        public bool IsLoadoutOpen()
        {
            return loadoutGroup != null && loadoutGroup.activeInHierarchy;
        }

        public void ResumeChosenSide(Side side)
        {
            OnSideSelected(side);
        }

        public RectTransform LightTarget => lightSideButton != null ? lightSideButton.transform as RectTransform : null;
        public RectTransform DarkTarget => darkSideButton != null ? darkSideButton.transform as RectTransform : null;
        public RectTransform StartTarget => startMatchButton != null ? startMatchButton.transform as RectTransform : null;
        public RectTransform FindLoadoutItem(string kind, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (kind == "hero") return MatchItem(heroItemContainer, id);
            if (kind == "spell") return MatchItem(spellItemContainer, id);
            RectTransform unit = MatchItem(unitItemContainer, id);
            if (unit != null) return unit;
            return MatchItem(bossItemContainer, id);
        }

        static RectTransform MatchItem(Transform root, string id)
        {
            if (root == null) return null;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                HeroLoadoutItemUI hero = child.GetComponent<HeroLoadoutItemUI>();
                if (hero != null && hero.ItemId == id) return child as RectTransform;
                UnitLoadoutItemUI unit = child.GetComponent<UnitLoadoutItemUI>();
                if (unit != null && unit.ItemId == id) return child as RectTransform;
                SpellLoadoutItemUI spell = child.GetComponent<SpellLoadoutItemUI>();
                if (spell != null && spell.ItemId == id) return child as RectTransform;
            }
            return null;
        }

        public RectTransform HeroTarget => heroItemContainer as RectTransform;
        public RectTransform UnitTarget => unitItemContainer as RectTransform;
        public RectTransform SpellTarget => spellItemContainer as RectTransform;

        public int HeroRowCount()
        {
            return heroItemContainer != null ? heroItemContainer.childCount : 0;
        }

        public int UnitRowCount()
        {
            int count = unitItemContainer != null ? unitItemContainer.childCount : 0;
            if (bossItemContainer != null) count += bossItemContainer.childCount;
            return count;
        }

        public int SpellRowCount()
        {
            return spellItemContainer != null ? spellItemContainer.childCount : 0;
        }

        void LateUpdate()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (lightSideButton != null)
                lightSideButton.interactable = tutorial == null || !tutorial.IsSideLocked(Side.Light);
            if (darkSideButton != null)
                darkSideButton.interactable = tutorial == null || !tutorial.IsSideLocked(Side.Dark);
            if (backButton != null)
                backButton.interactable = tutorial == null || !tutorial.LocksSideBack();
            RevealLessonItem(tutorial);
        }

        void RevealLessonItem(TutorialDirector tutorial)
        {
            if (tutorial == null || !IsLoadoutOpen()) return;
            string step = tutorial.Current();
            RectTransform item = null;
            if (step == "hero_info") item = FindLoadoutItem("hero", tutorial.TaughtHeroId);
            else if (step == "unit_info") item = FindLoadoutItem("unit", tutorial.CurrentLessonUnit());
            else if (step == "spell_info") item = FindLoadoutItem("spell", tutorial.TaughtSpellId);
            if (item == null) return;
            ScrollRect scroll = item.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.content == null) return;
            RectTransform view = scroll.viewport != null ? scroll.viewport : scroll.transform as RectTransform;
            RectTransform content = scroll.content;
            float hidden = content.rect.width - view.rect.width;
            if (hidden <= 1f) return;
            Vector3 center = content.InverseTransformPoint(item.TransformPoint(item.rect.center));
            float fromLeft = center.x - content.rect.xMin;
            float target = fromLeft - view.rect.width * 0.5f;
            scroll.horizontalNormalizedPosition = Mathf.Clamp01(target / hidden);
        }

        void PrepareDarkLesson()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            TutorialDirector director = TutorialDirector.Instance;
            if (meta == null || director == null) return;
            ClearLessonPicks(meta);

            string boughtId = director.TaughtDarkHeroId();
            if (boughtId != "Hero_Dark_Bone")
                meta.RevokeUnlockedHero("Hero_Dark_Bone");

            HeroData hero = null;
            foreach (HeroData candidate in meta.GetAllHeroes())
            {
                if (candidate == null || candidate.side != Side.Dark) continue;
                if (!meta.IsHeroUnlocked(candidate.heroID)) continue;
                if (candidate.heroID == boughtId)
                {
                    hero = candidate;
                    break;
                }
                if (hero == null) hero = candidate;
            }

            if (!meta.IsSkillUnlocked("Skill_Unlock_Bloodlust"))
                meta.RevokeUnlockedSpell("Spell_Dark_Bloodlust");

            SpellData spell = null;
            SpellData fallback = null;
            if (allPossibleSpells != null)
            {
                for (int i = 0; i < allPossibleSpells.Count; i++)
                {
                    SpellData candidate = allPossibleSpells[i];
                    if (candidate == null || !meta.IsSpellUnlocked(candidate.spellID)) continue;
                    if (candidate.spellID == "Spell_Dark_Freeze")
                    {
                        spell = candidate;
                        break;
                    }
                    if (fallback == null && candidate.side == Side.Dark)
                        fallback = candidate;
                }
            }
            if (spell == null) spell = fallback;

            List<UnitData> normals = new List<UnitData>();
            string bossName = null;
            HashSet<string> seen = new HashSet<string>();
            if (allPossibleUnits != null)
            {
                for (int i = 0; i < allPossibleUnits.Count; i++)
                {
                    UnitData unit = allPossibleUnits[i];
                    if (unit == null || unit.side != Side.Dark || string.IsNullOrEmpty(unit.unitName)) continue;
                    if (!seen.Add(unit.unitName)) continue;
                    if (BossNames.Contains(unit.unitName))
                    {
                        if (bossName == null || string.Compare(unit.unitName, bossName, System.StringComparison.Ordinal) < 0)
                            bossName = unit.unitName;
                    }
                    else normals.Add(unit);
                }
            }

            normals.Sort((a, b) => string.Compare(a.unitName, b.unitName, System.StringComparison.Ordinal));
            List<string> taught = new List<string>();
            for (int i = 0; i < normals.Count && taught.Count < 4; i++)
                taught.Add(normals[i].unitName);

            director.SetDarkLesson(
                hero != null ? hero.heroID : null,
                hero != null ? hero.displayName : null,
                spell != null ? spell.spellID : null,
                spell != null ? spell.spellName : null,
                taught,
                bossName);
        }

        static void ClearLessonPicks(MetaProgressionManager meta)
        {
            List<string> units = new List<string>(meta.GetEquippedUnitNames());
            for (int i = 0; i < units.Count; i++) meta.UnequipUnit(units[i]);
            List<string> heroes = new List<string>(meta.GetEquippedHeroIDs());
            for (int i = 0; i < heroes.Count; i++) meta.UnequipHero(heroes[i]);
            List<string> spells = new List<string>(meta.GetEquippedSpellIDs());
            for (int i = 0; i < spells.Count; i++) meta.UnequipSpell(spells[i]);
        }

        void PrepareTutorialArmy(Side side)
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return;

            System.Collections.Generic.List<string> equipped = new System.Collections.Generic.List<string>(meta.GetEquippedUnitNames());
            for (int i = 0; i < equipped.Count; i++)
                meta.UnequipUnit(equipped[i]);

            int normals = 0;
            UnitData boss = null;
            if (allPossibleUnits != null)
            {
                for (int i = 0; i < allPossibleUnits.Count; i++)
                {
                    UnitData unit = allPossibleUnits[i];
                    if (unit == null || unit.side != side || string.IsNullOrEmpty(unit.unitName)) continue;
                    if (BossNames.Contains(unit.unitName))
                    {
                        if (boss == null) boss = unit;
                        continue;
                    }
                    if (normals >= 4 || meta.IsUnitEquipped(unit.unitName)) continue;
                    meta.EquipUnit(unit.unitName);
                    normals++;
                }
            }

            if (boss != null) meta.EquipUnit(boss.unitName);

            foreach (HeroData hero in meta.GetAllHeroes())
            {
                if (hero == null || hero.side != side) continue;
                if (!meta.IsHeroUnlocked(hero.heroID)) continue;
                meta.EquipHero(hero.heroID);
                break;
            }
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
                if (normalCount != 4 && bossCount != 1)
                    WarningBannerUI.Show("SELECT 4 NORMAL UNITS AND 1 BOSS UNIT!");
                else if (normalCount != 4)
                    WarningBannerUI.Show($"SELECT EXACTLY 4 NORMAL UNITS! ({normalCount}/4)");
                else
                    WarningBannerUI.Show("SELECT 1 BOSS UNIT!");

                RefreshStartMatchButton();
                return;
            }

            // Kahraman seçilmemişse bile savaşa girilebilir, auto-equip yapmıyoruz.
            // Bu sayede oyuncu istemezse kahraman olmadan oynayabilir.

            Debug.Log("Starting match with side: " + currentSelectedSide);
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.OnMatchStarted();
            CampaignManager.Instance.LoadSelectedLevel();
        }
    }
}
