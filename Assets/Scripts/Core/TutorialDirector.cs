using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Grid;
using TowerDefence.UI;

namespace TowerDefence.Core
{
    public class TutorialDirector : MonoBehaviour
    {
        public static TutorialDirector Instance { get; private set; }

        public const string TargetSkillId = "Skill_Dark_Root";
        public const string TopSkillId = "Skill_Unlock_Freeze";

        public static readonly string[] Required =
        {
            "war_intro", "force_light", "to_battle_1",
            "currency_gold", "the_road", "lives", "wave_time", "place_tower", "tower_menu", "incoming", "speed_info", "unit_drop", "skip_wave", "hero_move", "hero_skill", "victory_1",
            "daily_go", "daily_tell", "fortune_go", "fortune_tell",
            "compendium_go", "compendium_tell", "heroes_go", "heroes_tell",
            "skill_go", "skill_info", "skill_buy", "skill_info_top", "skill_buy_top", "skill_back", "sweep_1", "force_dark", "to_battle_2",
            "hero_info", "unit_info", "spell_info", "start_dark",
            "wave_info", "hero_unit_info", "boss_intro", "boss_spell", "victory_2",
            "tutorial_rest",
            "map_next",
            "war_end",
            "normal_end",
            "hard_end"
        };

        public static readonly string[] AllSavedIds =
        {
            "war_intro", "force_light", "to_battle_1",
            "currency_gold", "the_road", "lives", "wave_time", "place_tower", "tower_menu", "incoming", "speed_info", "unit_drop", "skip_wave", "hero_move", "hero_skill", "victory_1",
            "daily_go", "daily_tell", "fortune_go", "fortune_tell",
            "compendium_go", "compendium_tell", "heroes_go", "heroes_tell",
            "skill_go", "skill_info", "skill_buy", "skill_info_top", "skill_buy_top", "skill_back", "sweep_1", "force_dark", "to_battle_2",
            "hero_info", "unit_info", "spell_info", "start_dark",
            "wave_info", "hero_unit_info", "boss_intro", "boss_spell", "victory_2",
            "tutorial_rest",
            "map_next", "war_end", "normal_end", "hard_end", "expert_finish", "barracks", "spell_cast"
        };

        static readonly string[] Level1BeforeVictory =
        {
            "currency_gold", "the_road", "lives", "wave_time", "place_tower", "tower_menu", "incoming", "speed_info", "unit_drop", "skip_wave", "hero_move", "hero_skill"
        };

        static readonly string[] Level2BeforeVictory =
        {
            "wave_info", "hero_unit_info", "boss_intro", "boss_spell"
        };

        bool routedThisLoad;

        MainMenuController menu;
        string pendingMoment;
        string visibleId;
        bool prepareArmy;
        bool toppedUp;
        bool pausedByCard;

        GameObject root;
        RectTransform bandRect;
        RectTransform frameRect;
        RectTransform nameRect;
        RectTransform textRect;
        Image shadeFull;
        Image shadeTop;
        Image shadeBottom;
        Image shadeLeft;
        Image shadeRight;
        Button skillCatch;
        Image panelImage;
        Image portrait;
        Image dropGhost;
        Image tapRing;
        Sprite ringSprite;
        TextMeshProUGUI nameText;
        TextMeshProUGUI bodyText;
        Button nextButton;
        GameObject nextChrome;

        public string TaughtHeroId { get; private set; }
        public string TaughtHeroName { get; private set; }
        public string TaughtSpellId { get; private set; }
        public string TaughtSpellName { get; private set; }
        public string TaughtBossName { get; private set; }
        readonly List<string> taughtUnits = new List<string>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            menu = null;
            routedThisLoad = false;
            visibleId = null;
            forceMenuReturn = false;
            forceMapReturn = false;
            if (pausedByCard)
            {
                pausedByCard = false;
                Time.timeScale = 1f;
            }
        }

        void Update()
        {
            if (MetaProgressionManager.Instance == null) return;
            FlushShortcuts();
            if (forceRestart)
            {
                PresentDefeat();
                TryRoute();
                PlaceCard();
                PlaceShade();
                return;
            }
            if (forceMenuReturn && InMatch())
            {
                PresentMenuReturn();
                TryRoute();
                PlaceCard();
                PlaceShade();
                return;
            }
            if (forceMapReturn && InMatch())
            {
                PresentMapReturn();
                TryRoute();
                PlaceCard();
                PlaceShade();
                return;
            }
            if (!PresentMoment()) PresentCurrent();
            TryRoute();
            PlaceCard();
            PlaceShade();
        }

        public void OnProgressReset()
        {
            pendingMoment = null;
            toppedUp = false;
            visibleId = null;
            prepareArmy = false;
            towerMenuShown = false;
            towerMenuPage = 0;
            towerLessonClosed = false;
            forceRestart = false;
            forceMenuReturn = false;
            forceMapReturn = false;
            heroMoveArmed = false;
            unitDropTopped = false;
            heroShopTopped = false;
            bottomSkillReleased = false;
            compendiumPage = 0;
            heroShopPage = 0;
            heroUnitPage = 0;
            waveInfoPage = 0;
            bossSpellPage = 0;
            spellLessonReady = false;
            spellTopped = false;
            heroInfoSeen = false;
            dailyClosePending = false;
            skillClosePending = false;
            fortuneClosePending = false;
            ClearDarkLesson();
            ApplyPause(false);
        }

        public void SetDarkLesson(string heroId, string heroName, string spellId, string spellName, List<string> units, string bossName)
        {
            TaughtHeroId = heroId;
            TaughtHeroName = heroName;
            TaughtSpellId = spellId;
            TaughtSpellName = spellName;
            TaughtBossName = bossName;
            taughtUnits.Clear();
            if (units != null) taughtUnits.AddRange(units);
            loadoutRead = false;
            unitCursor = 0;
        }

        public void ClearDarkLesson()
        {
            TaughtHeroId = null;
            TaughtHeroName = null;
            TaughtSpellId = null;
            TaughtSpellName = null;
            TaughtBossName = null;
            taughtUnits.Clear();
            loadoutRead = false;
            unitCursor = 0;
        }

        bool loadoutRead;
        int unitCursor;
        int heroUnitPage;
        int waveInfoPage;
        int bossSpellPage;
        bool spellLessonReady;
        bool spellTopped;
        bool heroInfoSeen;
        bool dailyClosePending;
        bool skillClosePending;
        bool fortuneClosePending;

        public string CurrentLessonUnit()
        {
            if (unitCursor < taughtUnits.Count) return taughtUnits[unitCursor];
            if (unitCursor == taughtUnits.Count && !string.IsNullOrEmpty(TaughtBossName)) return TaughtBossName;
            return null;
        }

        public bool LoadoutTarget(string kind, string id)
        {
            string step = Current();
            if (step == "hero_info") return kind == "hero" && id == TaughtHeroId;
            if (step == "unit_info") return kind == "unit" && id == CurrentLessonUnit();
            if (step == "spell_info") return kind == "spell" && id == TaughtSpellId;
            return false;
        }

        public bool BlocksLoadoutClick(string kind, string id)
        {
            string step = Current();
            if (step != "hero_info" && step != "unit_info" && step != "spell_info" && step != "start_dark")
                return false;
            if (step == "start_dark") return true;
            return !LoadoutTarget(kind, id);
        }

        public bool BlocksLoadoutSelect(string kind, string id)
        {
            if (BlocksLoadoutClick(kind, id)) return true;
            string step = Current();
            if (step == "unit_info" && unitCursor > 0) return false;
            if (step != "hero_info" && step != "unit_info" && step != "spell_info") return false;
            return !loadoutRead;
        }

        public void OnLoadoutRead(string kind, string id)
        {
            if (!LoadoutTarget(kind, id) || loadoutRead) return;
            loadoutRead = true;
        }

        public void OnLoadoutPicked(string kind, string id)
        {
            bool infoFirst = !(kind == "unit" && unitCursor > 0);
            if (!LoadoutTarget(kind, id) || (infoFirst && !loadoutRead)) return;
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return;
            if (kind == "hero" && !meta.GetEquippedHeroIDs().Contains(id)) return;
            if (kind == "unit" && !meta.IsUnitEquipped(id)) return;
            if (kind == "spell" && !meta.GetEquippedSpellIDs().Contains(id)) return;

            string step = Current();
            loadoutRead = false;
            if (step == "unit_info")
            {
                unitCursor++;
                if (CurrentLessonUnit() != null) return;
            }
            Complete(step);
        }

        public bool WaitsDailyClose()
        {
            return Current() == "daily_tell" && dailyClosePending;
        }

        public void OnDailyClaimed()
        {
            if (Current() != "daily_tell") return;
            dailyClosePending = true;
        }

        public void OnDailyClosed()
        {
            if (!WaitsDailyClose()) return;
            dailyClosePending = false;
            ReturnToMenuRoot();
            Complete("daily_tell");
        }

        public bool WaitsFortuneClose()
        {
            return Current() == "fortune_tell" && fortuneClosePending;
        }

        public void OnFortuneReward()
        {
            if (Current() != "fortune_tell") return;
            fortuneClosePending = true;
        }

        public void OnFortuneClosed()
        {
            if (!WaitsFortuneClose()) return;
            fortuneClosePending = false;
            ReturnToMenuRoot();
            Complete("fortune_tell");
        }

        public bool WaveUnitReady => Current() == "wave_info" && waveInfoPage > 0;

        public void OnWaveInfoOpened()
        {
            if (Current() != "wave_info" || waveInfoPage > 0) return;
            waveInfoPage = 1;
        }

        public void OnWaveUnitInfo()
        {
            if (Current() == "wave_info" && waveInfoPage > 0)
                Complete("wave_info");
        }

        public bool ReplaceWithOneBoss(int waveIndex)
        {
            if (!Teaching()) return false;
            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level == null || level.levelID != "Level2") return false;
            int planned = CampaignManager.Instance.GetPlannedWaveCount(level);
            return planned > 0 && waveIndex == planned - 1;
        }

        public UnitData LessonBossData()
        {
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Dark;
            Side enemy = player == Side.Light ? Side.Dark : Side.Light;
            UnitButton[] buttons = FindObjectsByType<UnitButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                UnitData data = buttons[i] != null ? buttons[i].UnitData : null;
                if (data == null || data.side != player || data.enemyCounterpart == null) continue;
                if (!SideSelectionUI.BossNames.Contains(data.unitName)) continue;
                if (data.enemyCounterpart.prefab != null) return data.enemyCounterpart;
            }
            UnitData best = null;
            if (TowerDefence.Combat.UnitPlacementManager.Instance != null)
                best = PickBoss(TowerDefence.Combat.UnitPlacementManager.Instance.AllUnits, enemy);
            if (best == null)
                best = PickBoss(Resources.FindObjectsOfTypeAll<UnitData>(), enemy);
            return best;
        }

        static UnitData PickBoss(System.Collections.Generic.IEnumerable<UnitData> units, Side side)
        {
            UnitData best = null;
            if (units == null) return null;
            foreach (UnitData unit in units)
            {
                if (unit == null || unit.side != side || unit.prefab == null) continue;
                if (!SideSelectionUI.BossNames.Contains(unit.unitName)) continue;
                if (best == null || string.Compare(unit.unitName, best.unitName, System.StringComparison.Ordinal) < 0)
                    best = unit;
            }
            return best;
        }

        public bool FocusesSpellButton(SpellButtonUI button)
        {
            return Current() == "boss_spell" && bossSpellPage <= 0 && BossAtPathMiddle()
                && button != null && LessonSpellButton() == button;
        }

        public bool WaitsBossCast()
        {
            return Current() == "boss_spell" && bossSpellPage > 0;
        }

        public bool BossCastHits(Vector3 point)
        {
            TowerDefence.Combat.Unit boss = LessonBoss();
            if (boss == null) return false;
            Vector3 delta = point - boss.transform.position;
            delta.y = 0f;
            return delta.magnitude <= 6f;
        }

        public void OnBossSpellCast()
        {
            if (Current() == "boss_spell" && bossSpellPage > 0)
                Complete("boss_spell");
        }

        SpellButtonUI LessonSpellButton()
        {
            SpellButtonUI[] buttons = FindObjectsByType<SpellButtonUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            SpellButtonUI fallback = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                SpellButtonUI button = buttons[i];
                if (button == null || !button.gameObject.activeInHierarchy) continue;
                if (fallback == null) fallback = button;
                string wanted = !string.IsNullOrEmpty(TaughtSpellId) ? TaughtSpellId : "Spell_Dark_Freeze";
                if (button.SpellId == wanted) return button;
            }
            return fallback;
        }

        bool BossAtPathMiddle()
        {
            TowerDefence.Combat.Unit boss = LessonBoss();
            return boss != null && boss.GetPathProgress01() >= 0.5f;
        }

        TowerDefence.Combat.Unit LessonBoss()
        {
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Dark;
            for (int i = 0; i < TowerDefence.Combat.Unit.AllUnits.Count; i++)
            {
                TowerDefence.Combat.Unit unit = TowerDefence.Combat.Unit.AllUnits[i];
                if (unit == null || unit.IsDead || unit.GetSide() == player || !unit.IsBoss) continue;
                return unit;
            }
            return null;
        }

        void PrepareBossSpell()
        {
            if (!spellLessonReady && MetaProgressionManager.Instance != null)
            {
                spellLessonReady = true;
                System.Collections.Generic.List<string> equipped = MetaProgressionManager.Instance.GetEquippedSpellIDs();
                if (equipped == null || equipped.Count == 0)
                {
                    string spellId = !string.IsNullOrEmpty(TaughtSpellId) ? TaughtSpellId : "Spell_Dark_Freeze";
                    if (MetaProgressionManager.Instance.IsSpellUnlocked(spellId))
                    {
                        MetaProgressionManager.Instance.EquipSpell(spellId);
                        SpellSelectionUI bar = FindAnyObjectByType<SpellSelectionUI>();
                        if (bar != null) bar.PopulateSpells();
                    }
                }
            }
            if (spellTopped || CurrencyManager.Instance == null) return;
            SpellButtonUI button = LessonSpellButton();
            if (button == null || button.ManaCost <= 0) return;
            spellTopped = true;
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Dark;
            int have = CurrencyManager.Instance.GetCurrency(player);
            if (have < button.ManaCost)
                CurrencyManager.Instance.AddCurrency(player, button.ManaCost - have);
        }

        public void OnBattleHeroInfo()
        {
            if (Current() == "hero_unit_info" && heroUnitPage == 0)
                heroInfoSeen = true;
        }

        public void OnBattleHeroInfoClosed()
        {
            if (Current() == "hero_unit_info" && heroInfoSeen && heroUnitPage == 0)
                heroUnitPage = 1;
        }

        public void OnBattleUnitInfo()
        {
            if (Current() == "hero_unit_info" && heroUnitPage >= 1)
                Complete("hero_unit_info");
        }

        void PrepareDailyLesson()
        {
            DailyRewardManager rewards = DailyRewardManager.Instance;
            if (rewards != null) rewards.PrepareLessonClaim();
            if (!dailyClosePending && rewards != null && rewards.GetPendingCount() == 0 && rewards.GetCurrentDayIndex() > 0)
                dailyClosePending = true;
            DailyRewardUI panel = FindAnyObjectByType<DailyRewardUI>();
            if (panel != null) panel.RefreshUI();
        }

        void PrepareFortuneLesson()
        {
            FortuneWheelUI wheel = FindAnyObjectByType<FortuneWheelUI>();
            if (wheel != null) wheel.PrepareLessonSpin();
        }

        public bool IsDarkLoadoutLesson()
        {
            switch (Current())
            {
                case "force_dark":
                case "to_battle_2":
                case "hero_info":
                case "unit_info":
                case "spell_info":
                case "start_dark":
                    return true;
                default:
                    return false;
            }
        }

        public bool RestrictsLoadout()
        {
            return IsDarkLoadoutLesson() && (taughtUnits.Count > 0 || !string.IsNullOrEmpty(TaughtHeroId) || !string.IsNullOrEmpty(TaughtSpellId));
        }

        public bool IsTaughtHero(string heroId)
        {
            return string.IsNullOrEmpty(TaughtHeroId) || heroId == TaughtHeroId;
        }

        public bool IsTaughtSpell(string spellId)
        {
            return string.IsNullOrEmpty(TaughtSpellId) || spellId == TaughtSpellId;
        }

        public bool IsTaughtUnit(string unitName)
        {
            if (taughtUnits.Count == 0 && string.IsNullOrEmpty(TaughtBossName)) return true;
            return taughtUnits.Contains(unitName) || unitName == TaughtBossName;
        }

        public bool SkillChoiceLocked(string skillId)
        {
            string step = Current();
            if (step == "skill_back") return true;
            if (step != "skill_go" && step != "skill_info" && step != "skill_info_top" && step != "skill_buy" && step != "skill_buy_top") return false;
            return skillId != CurrentSkillId();
        }

        public bool SkillHoverLocked()
        {
            string step = Current();
            return step == "skill_info" || step == "skill_info_top" || step == "skill_buy" || step == "skill_buy_top" || step == "skill_back";
        }

        public Transform OverlayRoot => root != null ? root.transform : null;

        public string CurrentSkillId()
        {
            string step = Current();
            if (step == "skill_buy_top" || step == "skill_info_top") return TopSkillId;
            return TargetSkillId;
        }

        public string Current()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return null;
            for (int i = 0; i < Required.Length; i++)
            {
                if (!meta.IsTutorialStepDone(Required[i])) return Required[i];
            }
            return null;
        }

        public bool IsSideLocked(Side side)
        {
            string step = Current();
            if (step == "force_light" || step == "to_battle_1") return side == Side.Dark;
            if (step == "force_dark" || step == "to_battle_2" || step == "hero_info"
                || step == "unit_info" || step == "spell_info" || step == "start_dark")
                return side == Side.Light;
            return false;
        }

        public bool LocksLevelBack()
        {
            if (!Teaching()) return false;
            switch (Current())
            {
                case "force_light":
                case "to_battle_1":
                case "sweep_1":
                case "force_dark":
                case "to_battle_2":
                case "hero_info":
                case "unit_info":
                case "spell_info":
                case "start_dark":
                    return true;
                default:
                    return false;
            }
        }

        public string ForcedMapLevel()
        {
            switch (Current())
            {
                case "force_light":
                case "to_battle_1":
                case "sweep_1":
                    return "Level1";
                case "force_dark":
                case "to_battle_2":
                case "hero_info":
                case "unit_info":
                case "spell_info":
                case "start_dark":
                    return "Level2";
                default:
                    return null;
            }
        }

        public bool LocksSideBack()
        {
            return LocksLevelBack();
        }

        public bool AllowsStartMatch()
        {
            switch (Current())
            {
                case "force_light":
                case "force_dark":
                case "to_battle_2":
                case "hero_info":
                case "unit_info":
                case "spell_info":
                    return false;
                default:
                    return true;
            }
        }

        public bool MenuButtonAllowed(string key)
        {
            if (!Teaching()) return true;
            string step = Current();
            if (IsBattleStep(step)) return false;
            switch (key)
            {
                case "play":
                    return step == "war_intro" || step == "force_light" || step == "to_battle_1"
                        || step == "sweep_1" || step == "force_dark" || step == "to_battle_2" || step == "hero_info"
                        || step == "unit_info" || step == "spell_info" || step == "start_dark";
                case "skill":
                    return step == "skill_go" || step == "skill_info" || step == "skill_info_top" || step == "skill_buy" || step == "skill_buy_top" || step == "skill_back";
                case "fortune":
                    return step == "fortune_go";
                case "daily":
                    return step == "daily_go";
                case "compendium":
                    return step == "compendium_go";
                case "heroes":
                    return step == "heroes_go";
                case "quit":
                    return false;
                default:
                    return true;
            }
        }

        public bool LevelClosed(string levelId)
        {
            if (levelId != "Level1" && levelId != "Level2") return false;
            if (CampaignManager.Instance != null && CampaignManager.Instance.CurrentDifficultyLevel != 1)
                return false;
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            return meta != null && meta.IsTutorialStepDone("tutorial_rest");
        }

        public bool LocksPauseMenu()
        {
            return Teaching();
        }

        public bool SweepAllowed(string levelId)
        {
            return Current() == "sweep_1" && levelId == "Level1";
        }

        public bool LevelAllowed(string levelId)
        {
            if (LevelClosed(levelId)) return false;
            if (!Teaching()) return true;
            string step = Current();
            if (step == "sweep_1") return false;
            if (step == "force_light" || step == "to_battle_1") return levelId == "Level1";
            if (step == "force_dark" || step == "to_battle_2" || step == "hero_info"
                || step == "unit_info" || step == "spell_info" || step == "start_dark")
                return levelId == "Level2";
            return false;
        }

        bool towerMenuShown;
        int towerMenuPage;
        bool towerLessonClosed;
        int compendiumPage;
        int heroShopPage;
        bool heroShopTopped;
        bool bottomSkillReleased;
        bool forceRestart;
        bool forceMenuReturn;
        bool forceMapReturn;
        bool heroMoveArmed;
        bool unitDropTopped;
        const float HeroSkillHealthRatio = 0.8f;

        bool CombatRunning()
        {
            return PhaseManager.Instance != null && PhaseManager.Instance.GetCurrentPhase() == GamePhase.Combat;
        }

        bool HeroNeedsSkill()
        {
            TowerDefence.Combat.HeroUnit[] heroes = FindObjectsByType<TowerDefence.Combat.HeroUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            for (int i = 0; i < heroes.Length; i++)
            {
                TowerDefence.Combat.HeroUnit hero = heroes[i];
                if (hero == null || hero.IsDead || hero.GetSide() != player) continue;
                float max = hero.GetMaxHealth();
                if (max <= 0.01f) continue;
                if (hero.GetHealth() / max <= HeroSkillHealthRatio) return true;
            }
            return false;
        }

        public bool LocksUnitTray()
        {
            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level == null || level.levelID != "Level1") return false;
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return false;
            return meta.IsTutorialStepDone("to_battle_1") && !meta.IsTutorialStepDone("speed_info");
        }

        public bool WantsRestart()
        {
            return forceRestart;
        }

        public bool WantsMenuReturn()
        {
            return forceMenuReturn;
        }

        public void ClearMenuReturn()
        {
            forceMenuReturn = false;
            forceMapReturn = false;
        }

        public bool WantsMapReturn()
        {
            return forceMapReturn;
        }

        public void ClearDefeatRetry()
        {
            forceRestart = false;
            Hide();
            ApplyPause(false);
        }

        void PresentDefeat()
        {
            ShowCard("defeat_retry", "The gate fell. Hit Restart and try the same fight again.", false, null);
            ApplyPause(false);
            LevelResultUI result = FindAnyObjectByType<LevelResultUI>();
            if (result != null) result.LockToRestart();
        }

        public bool OnlyArcher()
        {
            return Current() == "place_tower";
        }

        public bool SettingsLocked()
        {
            return Teaching();
        }

        bool Teaching()
        {
            string step = Current();
            return step != null && step != "map_next" && step != "war_end" && step != "normal_end" && step != "hard_end";
        }

        public bool HoldsUnitMarch()
        {
            string step = Current();
            return step == "skip_wave" || step == "hero_move";
        }

        public bool HoldsOpeningWave(int waveIndex)
        {
            if (waveIndex != 0) return false;
            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level == null || level.levelID != "Level1") return false;
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return false;
            return meta.IsTutorialStepDone("to_battle_1") && !meta.IsTutorialStepDone("unit_drop");
        }

        public void OnSideChosen(Side side)
        {
            string levelId = null;
            if (CampaignManager.Instance != null && CampaignManager.Instance.GetCurrentLevel() != null)
                levelId = CampaignManager.Instance.GetCurrentLevel().levelID;

            if (Current() == "force_light" && side == Side.Light && levelId == "Level1")
            {
                prepareArmy = true;
                Complete("force_light");
            }
            else if (Current() == "force_dark" && side == Side.Dark && levelId == "Level2")
            {
                Complete("force_dark");
                if (Current() == "to_battle_2") Complete("to_battle_2");
            }
        }

        public bool ConsumePreparedArmy()
        {
            if (!prepareArmy) return false;
            prepareArmy = false;
            return true;
        }

        public void OnMatchStarted()
        {
            if (Current() == "to_battle_1") Complete("to_battle_1");
            else if (Current() == "start_dark") Complete("start_dark");
        }

        public void OnPreparationBegan(int waveIndex)
        {
            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level != null && level.levelID == "Level1" && waveIndex >= 2 && Current() == "tower_menu")
                Complete("tower_menu");
        }

        public void OnLevelResult(bool victory, string levelId)
        {
            if (!victory)
            {
                if (Teaching() && (levelId == "Level1" || levelId == "Level2"))
                    forceRestart = true;
                return;
            }
            forceRestart = false;
            if (levelId == "Level10" && NormalDifficulty() && !MapLessonDone())
                forceMapReturn = true;
            string[] flush = null;
            if (levelId == "Level1") flush = Level1BeforeVictory;
            else if (levelId == "Level2") flush = Level2BeforeVictory;
            if (flush == null) return;

            int guard = 0;
            while (guard++ < flush.Length)
            {
                string pending = Current();
                bool listed = false;
                for (int i = 0; i < flush.Length; i++)
                    if (flush[i] == pending) listed = true;
                if (!listed) break;
                Complete(pending);
            }

            if (Teaching() && (levelId == "Level1" || levelId == "Level2"))
                forceMenuReturn = true;
        }

        void PresentMenuReturn()
        {
            ShowCard("back_to_menu", "Head back to the main menu. We'll continue there.", false, null);
            ApplyPause(true);
            LevelResultUI result = FindAnyObjectByType<LevelResultUI>();
            if (result != null) result.LockToMenu();
        }

        void PresentMapReturn()
        {
            ShowCard("map_return", "Hit Next. Back to the menu.", false, null);
            ApplyPause(true);
            LevelResultUI result = FindAnyObjectByType<LevelResultUI>();
            if (result != null) result.LockToNext();
        }

        bool NormalDifficulty()
        {
            return CampaignManager.Instance != null && CampaignManager.Instance.CurrentDifficultyLevel == 1;
        }

        bool MapLessonDone()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            return meta != null && meta.IsTutorialStepDone("map_next");
        }

        public void Notify(string stepId)
        {
            if (stepId == "barracks")
            {
                OfferMoment(stepId);
                return;
            }
            if (stepId == "spell_cast")
            {
                if (Current() == "boss_spell")
                {
                    if (bossSpellPage <= 0) bossSpellPage = 1;
                    return;
                }
                LevelData spellLevel = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
                if (spellLevel != null && spellLevel.levelID == "Level2") return;
                OfferMoment(stepId);
                return;
            }
            if (stepId == "tower_menu")
            {
                if (Current() == "tower_menu" && !towerMenuShown)
                {
                    towerMenuShown = true;
                    towerMenuPage = 0;
                }
                return;
            }
            Complete(stepId);
        }

        public void Complete(string stepId)
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null || string.IsNullOrEmpty(stepId) || meta.IsTutorialStepDone(stepId)) return;
            if (!IsSavedId(stepId)) return;
            if (IsRequired(stepId) && Current() != stepId) return;

            meta.CompleteTutorialStep(stepId);
            if (pendingMoment == stepId) pendingMoment = null;
            visibleId = null;

            SideSelectionUI sideUi = FindAnyObjectByType<SideSelectionUI>();
            if (sideUi != null) sideUi.RefreshStartMatchButton();
        }

        public int CompendiumPage => compendiumPage;
        public int HeroShopPage => heroShopPage;

        public void OnCompendiumUnit(string unitName)
        {
            if (Current() != "compendium_tell" || compendiumPage != 0) return;
            CompendiumUI book = FindAnyObjectByType<CompendiumUI>();
            if (book == null || book.FirstUnitName != unitName) return;
            compendiumPage = 1;
        }

        public bool BlocksCompendiumBack()
        {
            return Current() == "compendium_tell" && compendiumPage < 1;
        }

        public void OnNextMap()
        {
            if (Current() == "map_next" && LevelClearedOn("Level10", 1)) Complete("map_next");
        }

        public void OnDifficultyPicked(int difficulty)
        {
            if (difficulty == 2 && Current() == "normal_end") Complete("normal_end");
            else if (difficulty == 3 && Current() == "hard_end") Complete("hard_end");
        }

        public void OnCompendiumBack()
        {
            if (Current() == "compendium_tell" && compendiumPage >= 1)
                Complete("compendium_tell");
        }

        public string TaughtDarkHeroId()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return null;
            foreach (HeroData hero in meta.GetAllHeroesSorted())
            {
                if (hero != null && hero.side == Side.Dark)
                    return hero.heroID;
            }
            return null;
        }

        public void OnHeroCardChosen(string heroId)
        {
            if (Current() != "heroes_tell" || heroShopPage != 0) return;
            if (heroId != TaughtDarkHeroId()) return;
            if (MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.IsHeroUnlocked(heroId))
            {
                heroShopPage = 2;
                return;
            }
            EnsureDarkHeroAffordable();
            heroShopPage = 1;
        }

        public void OnHeroUnlocked(string heroId)
        {
            if (Current() != "heroes_tell" || heroShopPage != 1) return;
            if (heroId != TaughtDarkHeroId()) return;
            heroShopPage = 2;
        }

        public bool BlocksHeroShopBack()
        {
            return Current() == "heroes_tell" && heroShopPage < 2;
        }

        public void OnHeroShopBack()
        {
            if (Current() == "heroes_tell" && heroShopPage >= 2)
                Complete("heroes_tell");
        }

        void EnsureDarkHeroAffordable()
        {
            if (heroShopTopped || MetaProgressionManager.Instance == null) return;
            heroShopTopped = true;
            string id = TaughtDarkHeroId();
            HeroData hero = null;
            foreach (HeroData candidate in MetaProgressionManager.Instance.GetAllHeroesSorted())
            {
                if (candidate != null && candidate.heroID == id)
                {
                    hero = candidate;
                    break;
                }
            }
            if (hero == null || MetaProgressionManager.Instance.IsHeroUnlocked(hero.heroID)) return;
            int karmaGap = hero.unlockKarmaCost - MetaProgressionManager.Instance.GetTotalKarma();
            int crystalGap = hero.unlockCrystalCost - MetaProgressionManager.Instance.GetTotalCrystals();
            if (karmaGap > 0) MetaProgressionManager.Instance.AddKarma(karmaGap);
            if (crystalGap > 0) MetaProgressionManager.Instance.AddCrystals(crystalGap);
        }

        bool DarkHeroAlreadyOwned()
        {
            string id = TaughtDarkHeroId();
            return !string.IsNullOrEmpty(id)
                && MetaProgressionManager.Instance != null
                && MetaProgressionManager.Instance.IsHeroUnlocked(id);
        }

        bool AnyDarkHeroOwned()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return false;
            foreach (HeroData hero in meta.GetAllHeroes())
            {
                if (hero != null && hero.side == Side.Dark && meta.IsHeroUnlocked(hero.heroID))
                    return true;
            }
            return false;
        }

        void SkipOwnedLessons(MetaProgressionManager meta)
        {
            string step = Current();
            if (step == "heroes_go" || step == "heroes_tell")
            {
                if (TaughtDarkHeroId() != "Hero_Dark_Bone")
                    meta.RevokeUnlockedHero("Hero_Dark_Bone");
                if (step == "heroes_go" && AnyDarkHeroOwned())
                {
                    Complete("heroes_go");
                    if (Current() == "heroes_tell") Complete("heroes_tell");
                }
            }

            step = Current();
            if (step != "skill_go" && step != "skill_info" && step != "skill_buy"
                && step != "skill_info_top" && step != "skill_buy_top" && step != "skill_back")
                return;
            if (!meta.OwnsActiveDarkSkill()) return;

            string[] skillSteps = { "skill_go", "skill_info", "skill_buy", "skill_info_top", "skill_buy_top", "skill_back" };
            for (int i = 0; i < skillSteps.Length && Current() == skillSteps[i]; i++)
                Complete(skillSteps[i]);
        }

        void OfferMoment(string stepId)
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null || meta.IsTutorialStepDone(stepId) || meta.IsTutorialStepDone("victory_2")) return;
            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level == null) return;
            if (level.levelID != "Level1" && level.levelID != "Level2") return;
            pendingMoment = stepId;
        }

        void FlushShortcuts()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null) return;
            SkipOwnedLessons(meta);

            string step = Current();
            if (step == "skill_go" || step == "skill_info" || step == "skill_info_top" || step == "skill_buy" || step == "skill_buy_top")
            {
                ReleaseBottomSkillForLesson();
                PrepareTargetSkill();
            }
            if (step == "skill_buy" && meta.IsSkillUnlocked(TargetSkillId) && !SkillBuyPanelOpen())
                Complete("skill_buy");
            if (step == "skill_buy_top" && meta.IsSkillUnlocked(TopSkillId) && !SkillBuyPanelOpen())
                Complete("skill_buy_top");

            SideSelectionUI sideUi = FindAnyObjectByType<SideSelectionUI>();
            if (sideUi == null || !sideUi.IsLoadoutOpen()) return;
            if (Current() == "hero_info" && sideUi.HeroRowCount() == 0) Complete("hero_info");
            else if (Current() == "unit_info" && sideUi.UnitRowCount() == 0) Complete("unit_info");
            else if (Current() == "spell_info" && sideUi.SpellRowCount() == 0) Complete("spell_info");
        }

        bool PresentMoment()
        {
            if (string.IsNullOrEmpty(pendingMoment)) return false;
            if (MetaProgressionManager.Instance.IsTutorialStepDone(pendingMoment))
            {
                pendingMoment = null;
                return false;
            }

            string id = pendingMoment;
            ShowCard(id, MomentText(id), true, () => Complete(id));
            ApplyPause(InMatch());
            return true;
        }

        void PresentCurrent()
        {
            if (OfferExpertFinish()) return;
            string step = Current();
            if (step == null)
            {
                Hide();
                ApplyPause(false);
                return;
            }

            if (IsBattleStep(step) && InMenu())
            {
                ShowCard("resume-" + step, "The fight was left unfinished. Return to the same road.", true, () => ResumeBattle(step));
                ApplyPause(false);
                return;
            }

            switch (step)
            {
                case "war_intro":
                    ShowIfMenu(step, false);
                    break;
                case "force_light":
                    ShowIfMenu(step, false);
                    break;
                case "to_battle_1":
                    ShowIfMenu(step, false);
                    break;
                case "currency_gold":
                case "the_road":
                case "lives":
                case "wave_time":
                    ShowIfMatch(step, true);
                    break;
                case "incoming":
                    CloseTowerLesson();
                    ShowIfMatch(step, true);
                    break;
                case "speed_info":
                    ShowIfMatch(step, true);
                    break;
                case "unit_drop":
                    EnsureUnitAffordable();
                    ShowIfMatch(step, false, true);
                    break;
                case "skip_wave":
                    ShowIfMatch(step, false, true);
                    break;
                case "hero_move":
                    if (!CombatRunning())
                    {
                        Hide();
                        ApplyPause(false);
                    }
                    else ShowIfMatch(step, false, true);
                    break;
                case "hero_skill":
                    if (!HeroNeedsSkill())
                    {
                        Hide();
                        ApplyPause(false);
                    }
                    else ShowIfMatch(step, false, true);
                    break;
                case "place_tower":
                    ShowIfMatch(step, false);
                    break;
                case "tower_menu":
                    ShowIfMatch(step, towerMenuShown, false);
                    break;
                case "victory_1":
                case "victory_2":
                    ShowIfHere(step, true);
                    break;
                case "skill_go":
                    ShowUntilPanel(step, "skill", false);
                    break;
                case "skill_info":
                case "skill_info_top":
                case "skill_buy":
                case "skill_buy_top":
                    CenterTargetSkill();
                    ShowIfMenu(step, false);
                    break;
                case "skill_back":
                    ShowIfMenu(step, false);
                    break;
                case "sweep_1":
                case "force_dark":
                    ShowIfMenu(step, false);
                    break;
                case "to_battle_2":
                case "hero_info":
                case "unit_info":
                case "spell_info":
                case "start_dark":
                    ShowIfMenu(step, false);
                    break;
                case "wave_info":
                    ShowIfMatch(step, false);
                    break;
                case "hero_unit_info":
                    ShowIfMatch(step, false);
                    break;
                case "boss_intro":
                    if (LessonBoss() == null)
                    {
                        Hide();
                        ApplyPause(false);
                    }
                    else ShowIfMatch(step, true);
                    break;
                case "boss_spell":
                    if (!BossAtPathMiddle())
                    {
                        Hide();
                        ApplyPause(false);
                        break;
                    }
                    if (bossSpellPage > 0 && (SpellManager.Instance == null || SpellManager.Instance.SelectedSpell == null))
                        bossSpellPage = 0;
                    PrepareBossSpell();
                    ShowIfMatch(step, false);
                    break;
                case "fortune_go":
                    ShowUntilPanel(step, "fortune", false);
                    break;
                case "fortune_tell":
                    PrepareFortuneLesson();
                    ShowIfMenu(step, false);
                    break;
                case "daily_go":
                    ShowUntilPanel(step, "daily", false);
                    break;
                case "daily_tell":
                    PrepareDailyLesson();
                    ShowIfMenu(step, false);
                    break;
                case "compendium_go":
                    ShowUntilPanel(step, "compendium", false);
                    break;
                case "compendium_tell":
                    ShowIfMenu(step, false);
                    break;
                case "heroes_go":
                    ShowUntilPanel(step, "heroes", false);
                    break;
                case "heroes_tell":
                    if (heroShopPage < 2 && DarkHeroAlreadyOwned())
                        heroShopPage = 2;
                    ShowIfMenu(step, false);
                    break;
                case "tutorial_rest":
                    ShowIfMenu(step, true);
                    break;
                case "map_next":
                    if (OnLevelMap() && LevelClearedOn("Level10", 1))
                    {
                        LevelSelectionUI mapLevels = FindAnyObjectByType<LevelSelectionUI>();
                        if (mapLevels != null) mapLevels.ShowMapFor("Level10");
                        ShowIfMenu(step, false);
                    }
                    else { Hide(); ApplyPause(false); }
                    break;
                case "war_end":
                    if (OnLevelMap() && LevelCleared("Level50")) ShowIfMenu(step, true);
                    else { Hide(); ApplyPause(false); }
                    break;
                case "normal_end":
                    if (OnLevelMap() && CampaignCleared(1)) ShowIfMenu(step, false);
                    else { Hide(); ApplyPause(false); }
                    break;
                case "hard_end":
                    if (OnLevelMap() && CampaignCleared(2)) ShowIfMenu(step, false);
                    else { Hide(); ApplyPause(false); }
                    break;
            }
        }

        void ShowIfHere(string step, bool tell)
        {
            if (!InMenu() && !InMatch())
            {
                Hide();
                ApplyPause(false);
                return;
            }
            ShowCard(step, Body(step), tell, tell ? () => FinishTell(step) : null);
            ApplyPause(InMatch());
        }

        void ShowIfMenu(string step, bool tell)
        {
            if (!InMenu())
            {
                Hide();
                ApplyPause(false);
                return;
            }
            ShowCard(step, Body(step), tell, tell ? () => FinishTell(step) : null);
            ApplyPause(false);
        }

        void ShowIfMatch(string step, bool tell, bool pause = true)
        {
            if (!InMatch())
            {
                Hide();
                ApplyPause(false);
                return;
            }
            ShowCard(step, Body(step), tell, tell ? () => FinishTell(step) : null);
            ApplyPause(pause);
        }

        void ShowUntilPanel(string step, string panel, bool tell)
        {
            if (!InMenu())
            {
                Hide();
                ApplyPause(false);
                return;
            }
            MainMenuController currentMenu = Menu();
            if (currentMenu != null && currentMenu.IsPanelOpen(panel))
            {
                Complete(step);
                return;
            }
            ShowCard(step, Body(step), tell, null);
            ApplyPause(false);
        }

        void FinishTell(string step)
        {
            if (step == "tower_menu")
            {
                towerMenuPage++;
                if (towerMenuPage < 3) return;
                CloseTowerLesson();
            }
            Complete(step);
        }

        void CloseTowerLesson()
        {
            if (towerLessonClosed) return;
            towerLessonClosed = true;
            if (TowerPlacementManager.Instance != null)
                TowerPlacementManager.Instance.CloseAllTowerUI();
        }

        void ResumeBattle(string step)
        {
            string levelId = step == "wave_info" || step == "hero_unit_info" || step == "boss_intro" || step == "boss_spell" ? "Level2" : "Level1";
            CampaignManager campaign = CampaignManager.Instance;
            if (campaign == null) return;
            LevelData level = campaign.FindLevel(levelId);
            if (level == null) return;
            campaign.SelectLevel(level);
            if (SideController.Instance != null)
                SideController.Instance.SetPlayerSide(levelId == "Level1" ? Side.Light : Side.Dark);
            Hide();
            campaign.LoadSelectedLevel();
        }

        void ReturnToMenuRoot()
        {
            MainMenuController currentMenu = Menu();
            if (currentMenu == null) return;
            currentMenu.CloseFortuneWheelPanel();
            currentMenu.CloseDailyRewardPanel();
            currentMenu.ShowMainMenu();
        }

        bool InMenu()
        {
            return Menu() != null;
        }

        bool InMatch()
        {
            return !InMenu() && PhaseManager.Instance != null;
        }

        MainMenuController Menu()
        {
            if (menu == null) menu = FindAnyObjectByType<MainMenuController>();
            return menu;
        }

        bool LevelCleared(string levelId)
        {
            return MetaProgressionManager.Instance != null && MetaProgressionManager.Instance.HasClearedLevel(levelId);
        }

        bool CampaignCleared(int difficulty)
        {
            return CampaignManager.Instance != null && CampaignManager.Instance.AllLevelsCleared(difficulty);
        }

        bool LevelClearedOn(string levelId, int difficulty)
        {
            return MetaProgressionManager.Instance != null
                && MetaProgressionManager.Instance.IsLevelCompleted(levelId, difficulty);
        }

        bool OnLevelMap()
        {
            MainMenuController current = Menu();
            return InMenu() && current != null && current.IsPanelOpen("level");
        }

        bool OnRootMenu()
        {
            MainMenuController current = Menu();
            return current != null && current.IsRootMenu();
        }

        bool OfferExpertFinish()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null || meta.IsTutorialStepDone("expert_finish")) return false;
            if (!meta.IsLevelCompleted("Level50", 3)) return false;
            if (!OnRootMenu()) return false;
            ShowIfMenu("expert_finish", true);
            return true;
        }

        static bool IsBattleStep(string step)
        {
            switch (step)
            {
                case "currency_gold":
                case "place_tower":
                case "the_road":
                case "lives":
                case "wave_time":
                case "incoming":
                case "speed_info":
                case "unit_drop":
                case "skip_wave":
                case "hero_move":
                case "hero_skill":
                case "tower_menu":
                case "wave_info":
                case "hero_unit_info":
                case "boss_intro":
                case "boss_spell":
                    return true;
                default:
                    return false;
            }
        }

        static bool IsRequired(string stepId)
        {
            for (int i = 0; i < Required.Length; i++)
                if (Required[i] == stepId) return true;
            return false;
        }

        static bool IsSavedId(string stepId)
        {
            for (int i = 0; i < AllSavedIds.Length; i++)
                if (AllSavedIds[i] == stepId) return true;
            return false;
        }

        void ApplyPause(bool pause)
        {
            if (pause)
            {
                pausedByCard = true;
                Time.timeScale = 0f;
                return;
            }

            if (!pausedByCard) return;
            pausedByCard = false;
            if (GameSpeedManager.Instance != null) GameSpeedManager.Instance.SetSpeedX1();
            else Time.timeScale = 1f;
        }

        void LateUpdate()
        {
            if (pausedByCard) Time.timeScale = 0f;
        }

        void Hide()
        {
            if (root != null) root.SetActive(false);
            visibleId = null;
        }

        void ShowCard(string id, string body, bool tell, Action onNext)
        {
            EnsureCard();
            visibleId = id;
            root.SetActive(true);
            bodyText.text = CardText(id, body);
            portrait.color = Color.white;
            panelImage.raycastTarget = false;
            portrait.raycastTarget = false;
            PlaceCard();
            if (nextChrome != null) nextChrome.SetActive(tell);
            nextButton.interactable = tell;
            nextButton.onClick.RemoveAllListeners();
            if (tell && onNext != null) nextButton.onClick.AddListener(() => onNext());
        }

        void TryRoute()
        {
            if (routedThisLoad || !InMenu()) return;
            MainMenuController current = Menu();
            if (current == null) return;
            if (!current.IsRootMenu())
            {
                if (current.IsPanelOpen("level") || current.IsPanelOpen("side") || current.IsPanelOpen("skill")
                    || current.IsPanelOpen("fortune") || current.IsPanelOpen("daily")
                    || current.IsPanelOpen("compendium") || current.IsPanelOpen("heroes"))
                    routedThisLoad = true;
                return;
            }

            string step = Current();
            routedThisLoad = true;
            if (step == "force_light")
            {
                current.ShowLevelSelect();
                return;
            }
            if (step == "to_battle_1")
            {
                OpenForcedSide("Level1", Side.Light);
                return;
            }
            if (step == "sweep_1" || step == "force_dark")
            {
                current.ShowLevelSelect();
                return;
            }
            if (step == "map_next" && LevelClearedOn("Level10", 1))
            {
                current.ShowLevelSelect();
                return;
            }
            if (step == "hero_info" || step == "unit_info" || step == "spell_info" || step == "start_dark")
                OpenForcedSide("Level2", Side.Dark);
        }

        void OpenForcedSide(string levelId, Side side)
        {
            CampaignManager campaign = CampaignManager.Instance;
            MainMenuController current = Menu();
            if (campaign == null || current == null) return;
            LevelData level = campaign.FindLevel(levelId);
            if (level != null) campaign.SelectLevel(level);
            current.ShowSideSelection();
            SideSelectionUI sideUi = FindAnyObjectByType<SideSelectionUI>();
            if (sideUi != null) sideUi.ResumeChosenSide(side);
        }

        void PresentDropDemo()
        {
            bool show = Current() == "unit_drop" && InMatch() && root != null && root.activeSelf;
            if (!show)
            {
                if (dropGhost != null) dropGhost.gameObject.SetActive(false);
                return;
            }

            if (dropGhost == null)
            {
                GameObject go = new GameObject("DropDemo", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(root.transform, false);
                dropGhost = go.GetComponent<Image>();
                dropGhost.raycastTarget = false;
                dropGhost.preserveAspect = true;
                RectTransform rect = dropGhost.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(84f, 84f);
            }

            UnitButton source = DemoUnit();
            if (source == null || !DemoPoints(source, out Vector2 from, out Vector2 to))
            {
                dropGhost.gameObject.SetActive(false);
                return;
            }

            Image icon = source.transform.Find("Icon")?.GetComponent<Image>();
            Sprite sprite = icon != null && icon.sprite != null ? icon.sprite : source.UnitData != null ? source.UnitData.icon : null;
            if (sprite == null)
            {
                dropGhost.gameObject.SetActive(false);
                return;
            }

            dropGhost.sprite = sprite;
            dropGhost.gameObject.SetActive(true);
            if (bandRect != null) dropGhost.transform.SetSiblingIndex(bandRect.GetSiblingIndex());

            const float duration = 1.65f;
            const float gap = 0.4f;
            float cycle = Mathf.Repeat(Time.unscaledTime, duration + gap);
            if (cycle > duration)
            {
                dropGhost.enabled = false;
                return;
            }

            dropGhost.enabled = true;
            float u = cycle / duration;
            float move = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(u / 0.7f));
            Vector2 pos = Vector2.Lerp(from, to, move);
            pos.y += Mathf.Sin(move * Mathf.PI) * 42f;
            dropGhost.rectTransform.anchoredPosition = pos;
            float fade = u < 0.1f ? u / 0.1f : u > 0.7f ? 1f - (u - 0.7f) / 0.3f : 1f;
            float scale = u > 0.7f ? Mathf.Lerp(1f, 0.2f, (u - 0.7f) / 0.3f) : 1f;
            dropGhost.rectTransform.localScale = new Vector3(scale, scale, 1f);
            dropGhost.color = new Color(1f, 1f, 1f, Mathf.Clamp01(fade));
        }

        UnitButton DemoUnit()
        {
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            UnitButton[] buttons = FindObjectsByType<UnitButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            UnitButton best = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                UnitButton button = buttons[i];
                if (button == null || button.UnitData == null || !button.gameObject.activeInHierarchy) continue;
                if (button.UnitData.side != player) continue;
                if (SideSelectionUI.BossNames.Contains(button.UnitData.unitName)) continue;
                if (best == null || button.UnitData.spawnCost < best.UnitData.spawnCost) best = button;
                else if (button.UnitData.spawnCost == best.UnitData.spawnCost
                    && string.Compare(button.UnitData.unitName, best.UnitData.unitName, System.StringComparison.Ordinal) < 0)
                    best = button;
            }
            return best;
        }

        bool DemoPoints(UnitButton source, out Vector2 from, out Vector2 to)
        {
            from = to = default;
            if (root == null || source == null) return false;
            RectTransform space = root.GetComponent<RectTransform>();
            RectTransform button = source.transform as RectTransform;
            if (button == null) return false;
            Vector3[] corners = new Vector3[4];
            button.GetWorldCorners(corners);
            Canvas host = source.GetComponentInParent<Canvas>();
            Camera uiCam = host != null && host.renderMode != RenderMode.ScreenSpaceOverlay
                ? (host.worldCamera != null ? host.worldCamera : Camera.main)
                : null;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, (corners[0] + corners[2]) * 0.5f);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(space, screen, null, out from)) return false;

            Camera cam = Camera.main;
            if (cam == null) return false;
            TowerDefence.Combat.PathWaypoints[] paths = FindObjectsByType<TowerDefence.Combat.PathWaypoints>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < paths.Length; i++)
            {
                System.Collections.Generic.List<Vector3> points = paths[i] != null ? paths[i].GetPathPoints() : null;
                if (points == null || points.Count < 2) continue;
                if (LocalPoint(cam, space, PointAlong(points, 0.42f), out to)) return true;
            }
            return false;
        }

        static Vector3 PointAlong(System.Collections.Generic.List<Vector3> points, float t)
        {
            float total = 0f;
            for (int i = 1; i < points.Count; i++)
                total += Vector3.Distance(points[i - 1], points[i]);
            float want = total * Mathf.Clamp01(t);
            float walked = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                float seg = Vector3.Distance(points[i - 1], points[i]);
                if (walked + seg >= want || i == points.Count - 1)
                {
                    float u = seg < 0.001f ? 0f : Mathf.Clamp01((want - walked) / seg);
                    return Vector3.Lerp(points[i - 1], points[i], u);
                }
                walked += seg;
            }
            return points[points.Count - 1];
        }

        void PlaceShade()
        {
            PresentDropDemo();
            if (shadeFull == null) return;
            bool show = root != null && root.activeSelf && Current() != null && (InMenu() || InMatch());
            if (!show)
            {
                HideShade();
                return;
            }
            HideBars();
            List<Rect> holes = CollectHoles();
            PaintShade(holes);
            PlaceSkillCatch();
            PresentTapCue(holes);
            shadeFull.gameObject.SetActive(true);
        }

        void HideShade()
        {
            shadeFull.gameObject.SetActive(false);
            if (skillCatch != null) skillCatch.gameObject.SetActive(false);
            if (tapRing != null) tapRing.gameObject.SetActive(false);
            HideBars();
        }

        static bool IsMenuButtonStep(string step)
        {
            switch (step)
            {
                case "war_intro":
                case "skill_go":
                case "heroes_go":
                case "compendium_go":
                case "fortune_go":
                case "daily_go":
                    return true;
                default:
                    return false;
            }
        }

        int TapCueCount()
        {
            if (nextChrome == null || nextChrome.activeSelf) return 0;
            string step = forceRestart ? "defeat_retry" : forceMenuReturn ? "back_to_menu" : forceMapReturn ? "map_return" : Current();
            if (string.IsNullOrEmpty(step) || step == "unit_drop") return 0;
            if (step == "skill_info" || step == "skill_info_top") return 2;
            if (step == "hero_unit_info" && !heroInfoSeen) return 2;
            if ((step == "hero_info" || step == "spell_info") && !loadoutRead) return 2;
            if (step == "unit_info" && unitCursor <= 0 && !loadoutRead) return 2;
            return 1;
        }

        void PresentTapCue(List<Rect> holes)
        {
            int taps = TapCueCount();
            if (taps == 0 || root == null || !root.activeSelf || !CueAnchor(holes, out Vector2 anchor, out float size))
            {
                if (tapRing != null) tapRing.gameObject.SetActive(false);
                return;
            }

            if (tapRing == null)
            {
                GameObject go = new GameObject("TapCue", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(root.transform, false);
                tapRing = go.GetComponent<Image>();
                tapRing.raycastTarget = false;
                tapRing.sprite = RingSprite();
                RectTransform rect = tapRing.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }

            tapRing.gameObject.SetActive(true);
            if (bandRect != null) tapRing.transform.SetSiblingIndex(bandRect.GetSiblingIndex());

            float pulse = taps == 2 ? 0.26f : 0.42f;
            float innerGap = 0.04f;
            float outerGap = taps == 2 ? 0.55f : 0.48f;
            float span = taps == 2 ? pulse + innerGap + pulse + outerGap : pulse + outerGap;
            float time = Mathf.Repeat(Time.unscaledTime, span);
            bool second = taps == 2 && time >= pulse + innerGap;
            float local = second ? time - (pulse + innerGap) : time;
            if (local > pulse)
            {
                tapRing.enabled = false;
                return;
            }

            tapRing.enabled = true;
            float u = local / pulse;
            float scale = taps == 2 ? Mathf.Lerp(0.78f, 1.05f, u) : Mathf.Lerp(0.45f, 1.25f, u);
            tapRing.rectTransform.anchoredPosition = anchor;
            tapRing.rectTransform.sizeDelta = new Vector2(size, size);
            tapRing.rectTransform.localScale = new Vector3(scale, scale, 1f);
            tapRing.color = new Color(1f, 0.92f, 0.55f, (1f - u) * 0.95f);
        }

        bool CueAnchor(List<Rect> holes, out Vector2 anchor, out float size)
        {
            anchor = default;
            size = 72f;
            RectTransform target = forceRestart || forceMenuReturn || forceMapReturn ? null : FocusTarget();
            if (target != null && target.gameObject.activeInHierarchy && ScreenRect(target, out Rect ui, 0f))
            {
                anchor = ui.center;
                size = Mathf.Clamp(Mathf.Min(ui.width, ui.height) * 1.15f, 64f, 168f);
                return true;
            }
            if (holes != null)
            {
                for (int i = holes.Count - 1; i >= 0; i--)
                {
                    Rect hole = holes[i];
                    if (hole.width < 8f || hole.height < 8f) continue;
                    anchor = hole.center;
                    size = Mathf.Clamp(Mathf.Min(hole.width, hole.height) * 0.9f, 64f, 180f);
                    return true;
                }
            }
            if (Current() == "hero_move" && HeroReadyToMove() && pathStroke.Count > 0)
            {
                anchor = pathStroke[pathStroke.Count / 2];
                size = 96f;
                return true;
            }
            return false;
        }

        Sprite RingSprite()
        {
            if (ringSprite != null) return ringSprite;
            const int n = 64;
            Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[n * n];
            float center = (n - 1) * 0.5f;
            float outer = center - 1f;
            float inner = outer - 6f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    byte alpha = distance <= outer && distance >= inner ? (byte)255 : (byte)0;
                    pixels[y * n + x] = new Color32(255, 255, 255, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false);
            ringSprite = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
            return ringSprite;
        }

        void HideBars()
        {
            shadeTop.gameObject.SetActive(false);
            shadeBottom.gameObject.SetActive(false);
            shadeLeft.gameObject.SetActive(false);
            shadeRight.gameObject.SetActive(false);
        }

        List<Rect> CollectHoles()
        {
            pathStroke.Clear();
            List<Rect> holes = new List<Rect>();
            if (forceRestart)
            {
                LevelResultUI result = FindAnyObjectByType<LevelResultUI>();
                if (result != null && Cluster(result.RestartTarget, 22f, out Rect restart))
                    holes.Add(restart);
                return holes;
            }
            if (forceMenuReturn)
            {
                LevelResultUI result = FindAnyObjectByType<LevelResultUI>();
                if (result != null && Cluster(result.MenuTarget, 8f, out Rect menuHole))
                    holes.Add(menuHole);
                return holes;
            }
            if (forceMapReturn)
            {
                LevelResultUI result = FindAnyObjectByType<LevelResultUI>();
                if (result != null && Cluster(result.NextTarget, 8f, out Rect nextHole))
                    holes.Add(nextHole);
                return holes;
            }
            string step = Current();
            HUDController hud = FindAnyObjectByType<HUDController>();
            if (step == "boss_intro" || (step == "boss_spell" && bossSpellPage > 0))
            {
                AddBossDisc(holes);
                return holes;
            }
            if (step == "boss_spell")
            {
                SpellButtonUI spellButton = LessonSpellButton();
                if (spellButton != null && ScreenRect(spellButton.transform as RectTransform, out Rect spellHole, 0.08f))
                    holes.Add(spellHole);
                return holes;
            }
            if (visibleId == "spell_cast" && hud != null && hud.SpellTarget != null && ScreenRect(hud.SpellTarget, out Rect spell, 0.08f))
            {
                holes.Add(spell);
                return holes;
            }
            if (step == "place_tower")
            {
                if (ArcherButton() != null) AddArcherButton(holes);
                else if (OneSlot(false, out Rect slot)) holes.Add(slot);
                return holes;
            }
            if (step == "place_tower_2")
            {
                if (OneSlot(false, out Rect slot)) holes.Add(slot);
                return holes;
            }
            if (step == "compendium_tell")
            {
                CompendiumUI book = FindAnyObjectByType<CompendiumUI>();
                if (book == null) return holes;
                if (compendiumPage <= 0)
                {
                    if (book.FirstUnitTarget != null && ScreenRect(book.FirstUnitTarget, out Rect unitHole, 0.32f))
                        holes.Add(unitHole);
                }
                else if (Cluster(book.BackTarget, 4f, out Rect backHole))
                    holes.Add(backHole);
                return holes;
            }
            if (step == "heroes_tell")
            {
                HeroShopUI shop = FindAnyObjectByType<HeroShopUI>();
                RectTransform shopTarget = null;
                if (shop != null)
                {
                    if (heroShopPage <= 0) shopTarget = shop.DarkHeroTarget(TaughtDarkHeroId());
                    else if (heroShopPage == 1) shopTarget = shop.BuyTarget;
                    else shopTarget = shop.BackTarget;
                }
                if (Cluster(shopTarget, 8f, out Rect shopHole)) holes.Add(shopHole);
                return holes;
            }
            if (step == "tower_menu")
            {
                if (!towerMenuShown)
                {
                    if (OneSlot(true, out Rect built)) holes.Add(built);
                }
                else AddUpgradeButton(holes, towerMenuPage == 0 ? "Info" : towerMenuPage == 1 ? "Upgrade" : "Priority");
                return holes;
            }
            if (visibleId == "barracks")
            {
                if (OneSlot(true, out Rect built)) holes.Add(built);
                return holes;
            }
            if (step == "the_road")
            {
                AddPath(holes);
                return holes;
            }
            if (step == "hero_move")
            {
                if (HeroReadyToMove()) AddPath(holes);
                else AddHero(holes);
                return holes;
            }
            if (step == "hero_skill")
            {
                HeroButtonUI[] heroButtons = FindObjectsByType<HeroButtonUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < heroButtons.Length; i++)
                {
                    RectTransform ability = heroButtons[i].AbilityRect;
                    if (ability != null && ScreenRect(ability, out Rect abilityHole, -0.25f))
                        holes.Add(abilityHole);
                }
                return holes;
            }
            if (step == "lives")
            {
                AddCastle(holes);
                if (hud != null && Cluster(hud.LivesTarget, 8f, out Rect lives))
                    holes.Add(lives);
                return holes;
            }
            if (step == "wave_time" && hud != null)
            {
                if (Cluster(hud.WaveTarget, 4f, out Rect wave))
                {
                    float inset = wave.width * 0.06f;
                    wave.xMin += inset;
                    wave.xMax -= inset;
                    holes.Add(wave);
                }
                if (Cluster(hud.TimerTarget, 6f, out Rect timer)) holes.Add(timer);
                return holes;
            }
            if (step == "incoming")
            {
                AddUnitTray(holes);
                return holes;
            }
            if (step == "unit_drop")
            {
                AddUnitTray(holes);
                AddPath(holes);
                return holes;
            }
            if (step == "skip_wave")
            {
                if (hud != null && Cluster(hud.SkipTarget, 8f, out Rect skip))
                    holes.Add(skip);
                return holes;
            }
            if ((step == "currency_gold" || step == "currency_soul") && hud != null
                && ScreenRect(hud.CurrencyTarget, out Rect gold, 0.16f))
            {
                float extra = gold.width * 0.05f;
                gold.xMin -= extra;
                gold.xMax += extra;
                holes.Add(gold);
                return holes;
            }
            if (step == "speed_info" && hud != null)
            {
                RectTransform[] speeds = hud.SpeedTargets();
                for (int i = 0; i < speeds.Length; i++)
                {
                    if (speeds[i] != null && ScreenRect(speeds[i], out Rect speed, 0.02f))
                        holes.Add(speed);
                }
                return holes;
            }

            RectTransform target = FocusTarget();
            if (step == "sweep_1")
            {
                LevelSelectionUI sweepLevels = FindAnyObjectByType<LevelSelectionUI>();
                MainMenuController sweepMenu = Menu();
                RectTransform sweep = sweepMenu != null && sweepMenu.IsPanelOpen("level") && sweepLevels != null
                    ? sweepLevels.FindSweepTarget("Level1")
                    : null;
                if (sweep != null && Cluster(sweep, 8f, out Rect sweepHole))
                    holes.Add(sweepHole);
                else
                {
                    RectTransform play = sweepMenu != null ? sweepMenu.ButtonTarget("play") : null;
                    if (play != null && ScreenRect(play, out Rect playHole, 0.08f))
                        holes.Add(playHole);
                }
                return holes;
            }
            if (step == "skill_back")
            {
                SkillTreeUI backTree = FindAnyObjectByType<SkillTreeUI>();
                if (backTree != null && Cluster(backTree.BackTarget, 6f, out Rect backHole))
                    holes.Add(backHole);
                return holes;
            }
            if (step == "skill_info" || step == "skill_info_top" || step == "skill_buy" || step == "skill_buy_top")
            {
                SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
                bool buying = step == "skill_buy" || step == "skill_buy_top";
                if (buying && tree != null && tree.DetailTarget != null)
                {
                    RectTransform buyFocus = SkillFocus();
                    if (buyFocus != null && MeasuredRect(buyFocus, out Rect buyHole, 0f, 0f))
                    {
                        float pad = Mathf.Max(14f, buyHole.width * 0.12f);
                        buyHole.xMin -= pad;
                        buyHole.xMax += pad;
                        holes.Add(buyHole);
                        return holes;
                    }
                }
                RectTransform focus = SkillFocus();
                RectTransform visual = focus;
                if (focus != null)
                {
                    SkillNodeUI nodeUi = focus.GetComponent<SkillNodeUI>();
                    if (nodeUi == null) nodeUi = focus.GetComponentInParent<SkillNodeUI>();
                    if (nodeUi != null && nodeUi.IconRect != null) visual = nodeUi.IconRect;
                }
                if (visual != null && MeasuredRect(visual, out Rect skillHole, -0.15f, 108f))
                    holes.Add(skillHole);
                return holes;
            }
            if (step == "wave_info" && waveInfoPage <= 0 && target != null && target.gameObject.activeInHierarchy
                && ScreenRect(target, out Rect waveButton, -1f))
            {
                holes.Add(waveButton);
                return holes;
            }
            if ((step == "normal_end" || step == "hard_end") && target != null && target.gameObject.activeInHierarchy
                && ScreenRect(target, out Rect diffHole, 0f))
            {
                float pad = Mathf.Max(16f, diffHole.width * 0.12f);
                diffHole.xMin -= pad;
                diffHole.xMax += pad;
                diffHole.yMin -= pad;
                diffHole.yMax += pad;
                holes.Add(diffHole);
                return holes;
            }
            if (step == "map_next" && target != null && target.gameObject.activeInHierarchy
                && ScreenRect(target, out Rect arrowHole, 0f))
            {
                float pad = Mathf.Max(18f, arrowHole.width * 0.2f);
                arrowHole.xMin -= pad;
                arrowHole.xMax += pad;
                arrowHole.yMin -= pad;
                arrowHole.yMax += pad;
                holes.Add(arrowHole);
                return holes;
            }
            if (IsMenuButtonStep(step) && target != null && target.gameObject.activeInHierarchy
                && ScreenRect(target, out Rect menuButtonHole, 0f))
            {
                float pad = Mathf.Max(16f, menuButtonHole.width * 0.14f);
                menuButtonHole.xMin -= pad;
                menuButtonHole.xMax += pad;
                holes.Add(menuButtonHole);
                return holes;
            }
            if (target != null && target.gameObject.activeInHierarchy && ScreenRect(target, out Rect ui, 0.16f))
                holes.Add(ui);
            return holes;
        }

        RectTransform ArcherButton()
        {
            TowerSelectionUI selection = FindAnyObjectByType<TowerSelectionUI>();
            if (selection == null || !selection.gameObject.activeInHierarchy) return null;
            Transform[] all = selection.GetComponentsInChildren<Transform>(false);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "Archer Tower")
                    return all[i] as RectTransform;
            }
            return null;
        }

        bool Cluster(RectTransform target, float pad, out Rect hole)
        {
            hole = default;
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            if (!ScreenRect(target, out Rect box, 0f)) return false;
            for (int i = 0; i < target.childCount; i++)
            {
                RectTransform child = target.GetChild(i) as RectTransform;
                if (child != null && child.gameObject.activeInHierarchy && ScreenRect(child, out Rect part, 0f))
                    box = Encapsulate(box, part);
            }
            hole = Rect.MinMaxRect(box.xMin - pad, box.yMin - pad, box.xMax + pad, box.yMax + pad);
            return hole.width > 8f && hole.height > 8f;
        }

        void AddCastle(List<Rect> holes)
        {
            Camera cam = Camera.main;
            if (cam == null || root == null) return;
            RectTransform rootSpace = root.GetComponent<RectTransform>();
            TowerDefence.Combat.Base[] bases = FindObjectsByType<TowerDefence.Combat.Base>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            TowerDefence.Combat.Base best = null;
            float bestX = float.MaxValue;
            for (int i = 0; i < bases.Length; i++)
            {
                if (bases[i] == null) continue;
                Vector3 screen = cam.WorldToScreenPoint(bases[i].transform.position);
                if (screen.z <= 0f) continue;
                if (screen.x < bestX)
                {
                    bestX = screen.x;
                    best = bases[i];
                }
            }
            if (best == null) return;
            if (WorldDisc(cam, rootSpace, best.transform.position, 2.15f, 4f, out Rect castle))
                holes.Add(castle);
        }

        bool OneSlot(bool occupiedOnly, out Rect hole)
        {
            hole = default;
            Camera cam = Camera.main;
            if (cam == null || root == null) return false;
            TowerDefence.Grid.TowerSlot[] slots = FindObjectsByType<TowerDefence.Grid.TowerSlot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            RectTransform rootSpace = root.GetComponent<RectTransform>();
            TowerDefence.Grid.TowerSlot best = null;
            float bestDist = float.MaxValue;
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            for (int i = 0; i < slots.Length; i++)
            {
                TowerDefence.Grid.TowerSlot slot = slots[i];
                if (slot == null) continue;
                if (occupiedOnly && !slot.IsOccupied) continue;
                if (!occupiedOnly && slot.IsOccupied) continue;
                Vector3 screen = cam.WorldToScreenPoint(slot.transform.position);
                if (screen.z <= 0f) continue;
                float dist = Vector2.Distance(screenCenter, new Vector2(screen.x, screen.y));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = slot;
                }
            }
            if (best == null) return false;
            return WorldDisc(cam, rootSpace, best.transform.position, 2.1f, 10f, out hole);
        }

        readonly List<Vector2> pathStroke = new List<Vector2>();
        float pathStrokeRadius = 18f;

        void AddPath(List<Rect> holes)
        {
            Camera cam = Camera.main;
            if (cam == null || root == null) return;
            RectTransform rootSpace = root.GetComponent<RectTransform>();
            TowerDefence.Combat.PathWaypoints[] paths = FindObjectsByType<TowerDefence.Combat.PathWaypoints>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            pathStrokeRadius = 18f;
            for (int p = 0; p < paths.Length; p++)
            {
                List<Vector3> points = paths[p].GetPathPoints();
                if (points == null || points.Count == 0) continue;
                float half = Mathf.Max(0.8f, paths[p].PathWidth * 0.5f);
                if (LocalPoint(cam, rootSpace, points[0], out Vector2 origin)
                    && LocalPoint(cam, rootSpace, points[0] + Vector3.right * half, out Vector2 edge))
                    pathStrokeRadius = Mathf.Max(pathStrokeRadius, Vector2.Distance(origin, edge));
                for (int i = 0; i < points.Count; i++)
                {
                    if (LocalPoint(cam, rootSpace, points[i], out Vector2 local))
                        pathStroke.Add(local);
                }
            }
            pathStrokeRadius *= 2f;
        }

        void AddUpgradeButton(List<Rect> holes, string action)
        {
            TowerUpgradeUI[] menus = FindObjectsByType<TowerUpgradeUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < menus.Length; i++)
            {
                RectTransform button = menus[i].LessonButton(action);
                if (button == null || !button.gameObject.activeInHierarchy) continue;
                if (ScreenRect(button, out Rect hole, -0.2f))
                {
                    Rect area = root.GetComponent<RectTransform>().rect;
                    if (hole.width < area.width * 0.45f && hole.height < area.height * 0.45f)
                    {
                        holes.Add(hole);
                        return;
                    }
                }
                Camera cam = Camera.main;
                if (cam != null && root != null && WorldDisc(cam, root.GetComponent<RectTransform>(), button.position, 1.1f, 14f, out Rect disc))
                    holes.Add(disc);
                return;
            }
        }

        void AddArcherButton(List<Rect> holes)
        {
            RectTransform archer = ArcherButton();
            if (archer == null) return;
            if (ScreenRect(archer, out Rect archerHole, -0.2f))
            {
                Rect area = root.GetComponent<RectTransform>().rect;
                if (archerHole.width < area.width * 0.45f && archerHole.height < area.height * 0.45f)
                {
                    holes.Add(archerHole);
                    return;
                }
            }
            Camera cam = Camera.main;
            if (cam == null || root == null) return;
            if (WorldDisc(cam, root.GetComponent<RectTransform>(), archer.position, 1.3f, 16f, out Rect disc))
                holes.Add(disc);
        }

        void EnsureUnitAffordable()
        {
            if (unitDropTopped || CurrencyManager.Instance == null) return;
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            UnitButton[] buttons = FindObjectsByType<UnitButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int cheapest = int.MaxValue;
            for (int i = 0; i < buttons.Length; i++)
            {
                UnitButton button = buttons[i];
                if (button == null || button.UnitData == null) continue;
                if (button.UnitData.side != player) continue;
                if (SideSelectionUI.BossNames.Contains(button.UnitData.unitName)) continue;
                if (button.UnitData.spawnCost < cheapest) cheapest = button.UnitData.spawnCost;
            }
            if (cheapest == int.MaxValue) return;
            int have = CurrencyManager.Instance.GetCurrency(player);
            if (have < cheapest) CurrencyManager.Instance.AddCurrency(player, cheapest - have);
            unitDropTopped = true;
        }

        bool HeroReadyToMove()
        {
            return HeroManager.Instance != null && HeroManager.Instance.HasMoveSelection;
        }

        void AddHero(List<Rect> holes)
        {
            Camera cam = Camera.main;
            if (cam == null || root == null) return;
            RectTransform rootSpace = root.GetComponent<RectTransform>();
            Side player = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            TowerDefence.Combat.HeroUnit[] heroes = FindObjectsByType<TowerDefence.Combat.HeroUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < heroes.Length; i++)
            {
                TowerDefence.Combat.HeroUnit hero = heroes[i];
                if (hero == null || hero.IsDead || hero.GetSide() != player) continue;
                if (WorldDisc(cam, rootSpace, hero.transform.position, 1.5f, 8f, out Rect disc))
                    holes.Add(disc);
            }
        }

        void AddUnitTray(List<Rect> holes)
        {
            UnitButton[] buttons = FindObjectsByType<UnitButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            bool any = false;
            Rect box = default;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null) continue;
                if (!ScreenRect(buttons[i].transform as RectTransform, out Rect part, 0f)) continue;
                box = any ? Encapsulate(box, part) : part;
                any = true;
            }
            if (!any)
            {
                UnitSelectionUI units = FindAnyObjectByType<UnitSelectionUI>();
                if (units != null && ScreenRect(units.transform as RectTransform, out Rect panel, 0f))
                {
                    box = panel;
                    any = true;
                }
            }
            if (!any) return;
            holes.Add(Rect.MinMaxRect(box.xMin - 10f, box.yMin - 8f, box.xMax + 10f, box.yMax + 8f));
        }

        void AddBossDisc(List<Rect> holes)
        {
            TowerDefence.Combat.Unit boss = LessonBoss();
            if (boss == null || root == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            if (WorldDisc(cam, root.GetComponent<RectTransform>(), boss.transform.position, 2.2f, 18f, out Rect disc))
                holes.Add(disc);
        }

        static bool WorldDisc(Camera cam, RectTransform rootSpace, Vector3 world, float worldRadius, float extra, out Rect hole)
        {
            hole = default;
            if (!LocalPoint(cam, rootSpace, world, out Vector2 center)) return false;
            if (!LocalPoint(cam, rootSpace, world + Vector3.right * worldRadius, out Vector2 edge)) return false;
            float radius = Vector2.Distance(center, edge) + extra;
            hole = Rect.MinMaxRect(center.x - radius, center.y - radius, center.x + radius, center.y + radius);
            return hole.width > 8f;
        }

        Texture2D shadeTex;
        Sprite shadeSprite;
        Color32[] shadePixels;
        const int ShadeW = 480;
        const int ShadeH = 270;

        void PaintShade(List<Rect> holes)
        {
            if (shadeTex == null)
            {
                shadeTex = new Texture2D(ShadeW, ShadeH, TextureFormat.RGBA32, false);
                shadeTex.wrapMode = TextureWrapMode.Clamp;
                shadePixels = new Color32[ShadeW * ShadeH];
                shadeSprite = Sprite.Create(shadeTex, new Rect(0f, 0f, ShadeW, ShadeH), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                shadeFull.sprite = shadeSprite;
                shadeFull.color = Color.white;
                shadeFull.type = Image.Type.Simple;
                shadeFull.alphaHitTestMinimumThreshold = 0.35f;
                RectTransform full = shadeFull.rectTransform;
                full.anchorMin = Vector2.zero;
                full.anchorMax = Vector2.one;
                full.offsetMin = Vector2.zero;
                full.offsetMax = Vector2.zero;
            }

            Color32 dark = new Color32(0, 0, 0, 210);
            for (int i = 0; i < shadePixels.Length; i++) shadePixels[i] = dark;
            Rect area = root.GetComponent<RectTransform>().rect;
            for (int h = 0; h < holes.Count; h++)
                Punch(holes[h], area);
            StrokePath(area);
            shadeTex.SetPixels32(shadePixels);
            shadeTex.Apply(false);
        }

        void StrokePath(Rect area)
        {
            float radius = Mathf.Max(12f, pathStrokeRadius);
            float step = Mathf.Max(8f, radius * 0.55f);
            for (int i = 1; i < pathStroke.Count; i++)
            {
                Vector2 from = pathStroke[i - 1];
                Vector2 to = pathStroke[i];
                float length = Vector2.Distance(from, to);
                int count = Mathf.Max(1, Mathf.CeilToInt(length / step));
                for (int s = 0; s <= count; s++)
                    PunchDisc(Vector2.Lerp(from, to, s / (float)count), radius, area);
            }
        }

        void PunchDisc(Vector2 center, float radius, Rect area)
        {
            float cx = (center.x - area.xMin) / area.width * ShadeW;
            float cy = (center.y - area.yMin) / area.height * ShadeH;
            float rx = radius / area.width * ShadeW;
            float ry = radius / area.height * ShadeH;
            int x0 = Mathf.Clamp(Mathf.FloorToInt(cx - rx), 0, ShadeW);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(cx + rx), 0, ShadeW);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(cy - ry), 0, ShadeH);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(cy + ry), 0, ShadeH);
            float rx2 = Mathf.Max(0.01f, rx * rx);
            float ry2 = Mathf.Max(0.01f, ry * ry);
            for (int y = y0; y < y1; y++)
            {
                int row = y * ShadeW;
                float dy = (y + 0.5f - cy);
                for (int x = x0; x < x1; x++)
                {
                    float dx = (x + 0.5f - cx);
                    if (dx * dx / rx2 + dy * dy / ry2 <= 1f)
                        shadePixels[row + x] = new Color32(0, 0, 0, 0);
                }
            }
        }

        void Punch(Rect hole, Rect area)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt((hole.xMin - area.xMin) / area.width * ShadeW), 0, ShadeW);
            int x1 = Mathf.Clamp(Mathf.CeilToInt((hole.xMax - area.xMin) / area.width * ShadeW), 0, ShadeW);
            int y0 = Mathf.Clamp(Mathf.FloorToInt((hole.yMin - area.yMin) / area.height * ShadeH), 0, ShadeH);
            int y1 = Mathf.Clamp(Mathf.CeilToInt((hole.yMax - area.yMin) / area.height * ShadeH), 0, ShadeH);
            for (int y = y0; y < y1; y++)
            {
                int row = y * ShadeW;
                for (int x = x0; x < x1; x++)
                    shadePixels[row + x] = new Color32(0, 0, 0, 0);
            }
        }

        static bool LocalPoint(Camera cam, RectTransform rootSpace, Vector3 world, out Vector2 local)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rootSpace, screen, null, out local);
        }

        static RectTransform OpenBuildUi()
        {
            TowerSelectionUI selection = FindAnyObjectByType<TowerSelectionUI>();
            if (selection != null && selection.gameObject.activeInHierarchy)
                return selection.transform as RectTransform;
            TowerUpgradeUI upgrade = FindAnyObjectByType<TowerUpgradeUI>();
            if (upgrade != null && upgrade.gameObject.activeInHierarchy)
                return upgrade.transform as RectTransform;
            return null;
        }

        Rect FieldHole()
        {
            Rect area = root.GetComponent<RectTransform>().rect;
            float left = area.xMin + area.width * 0.05f;
            float right = area.xMax - area.width * 0.05f;
            float bottom = area.yMin + area.height * 0.06f;
            float top = area.yMin + area.height * 0.82f;
            return Rect.MinMaxRect(left, bottom, right, top);
        }

        static Rect Encapsulate(Rect a, Rect b)
        {
            return Rect.MinMaxRect(
                Mathf.Min(a.xMin, b.xMin),
                Mathf.Min(a.yMin, b.yMin),
                Mathf.Max(a.xMax, b.xMax),
                Mathf.Max(a.yMax, b.yMax));
        }

        bool MeasuredRect(RectTransform target, out Rect hole, float shrink, float maxSide)
        {
            hole = default;
            if (target == null || root == null) return false;
            Canvas host = target.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (host != null && host.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = host.worldCamera != null ? host.worldCamera : Camera.main;
            RectTransform rootSpace = root.GetComponent<RectTransform>();
            Vector3 world = target.TransformPoint(target.rect.center);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rootSpace, screen, null, out Vector2 center))
                return false;
            float width = target.rect.width * Mathf.Abs(target.lossyScale.x) / Mathf.Max(0.0001f, Mathf.Abs(root.transform.lossyScale.x));
            float height = target.rect.height * Mathf.Abs(target.lossyScale.y) / Mathf.Max(0.0001f, Mathf.Abs(root.transform.lossyScale.y));
            float scale = 1f - shrink;
            width *= scale;
            height *= scale;
            if (maxSide > 0f)
            {
                width = Mathf.Min(width, maxSide);
                height = Mathf.Min(height, maxSide);
            }
            if (width < 8f || height < 8f) return false;
            hole = Rect.MinMaxRect(center.x - width * 0.5f, center.y - height * 0.5f, center.x + width * 0.5f, center.y + height * 0.5f);
            return true;
        }

        bool ScreenRect(RectTransform target, out Rect hole, float shrink)
        {
            hole = default;
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Canvas host = target.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (host != null && host.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = host.worldCamera != null ? host.worldCamera : Camera.main;
            RectTransform rootSpace = root.GetComponent<RectTransform>();
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rootSpace, screen, null, out Vector2 local))
                    return false;
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            Vector2 center = (min + max) * 0.5f;
            Vector2 half = (max - min) * (0.5f * (1f - shrink));
            hole = Rect.MinMaxRect(center.x - half.x, center.y - half.y, center.x + half.x, center.y + half.y);
            return hole.width > 8f && hole.height > 8f;
        }

        void CutHole(Rect hole)
        {
            Rect area = root.GetComponent<RectTransform>().rect;
            float left = Mathf.Max(area.xMin, hole.xMin);
            float right = Mathf.Min(area.xMax, hole.xMax);
            float bottom = Mathf.Max(area.yMin, hole.yMin);
            float top = Mathf.Min(area.yMax, hole.yMax);
            shadeFull.gameObject.SetActive(false);
            SetBar(shadeTop, area.center.x, (top + area.yMax) * 0.5f, area.width, area.yMax - top);
            SetBar(shadeBottom, area.center.x, (area.yMin + bottom) * 0.5f, area.width, bottom - area.yMin);
            SetBar(shadeLeft, (area.xMin + left) * 0.5f, (bottom + top) * 0.5f, left - area.xMin, top - bottom);
            SetBar(shadeRight, (right + area.xMax) * 0.5f, (bottom + top) * 0.5f, area.xMax - right, top - bottom);
        }

        static void SetBar(Image bar, float x, float y, float width, float height)
        {
            if (width < 1f || height < 1f)
            {
                bar.gameObject.SetActive(false);
                return;
            }
            bar.gameObject.SetActive(true);
            RectTransform rect = bar.rectTransform;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        RectTransform FocusTarget()
        {
            string step = Current();
            HUDController hud = FindAnyObjectByType<HUDController>();
            if (step == "currency_gold" || step == "currency_soul")
                return hud != null ? hud.CurrencyTarget : null;
            if (step == "lives")
                return hud != null ? hud.LivesTarget : null;
            MainMenuController current = Menu();
            SideSelectionUI side = FindAnyObjectByType<SideSelectionUI>();
            LevelSelectionUI levels = FindAnyObjectByType<LevelSelectionUI>();
            switch (step)
            {
                case "war_intro":
                    return current != null ? current.ButtonTarget("play") : null;
                case "force_light":
                    if (current != null && current.IsPanelOpen("side") && side != null) return side.LightTarget;
                    if (current != null && current.IsPanelOpen("level") && levels != null) return levels.FindLevelTarget("Level1");
                    return current != null ? current.ButtonTarget("play") : null;
                case "to_battle_1":
                    if (side != null && side.StartTarget != null && side.StartTarget.gameObject.activeInHierarchy)
                        return side.StartTarget;
                    return side != null ? side.LightTarget : null;
                case "skill_go":
                    return current != null ? current.ButtonTarget("skill") : null;
                case "skill_info":
                case "skill_info_top":
                case "skill_buy":
                case "skill_buy_top":
                    return SkillFocus();
                case "skill_back":
                    SkillTreeUI skillTree = FindAnyObjectByType<SkillTreeUI>();
                    return skillTree != null ? skillTree.BackTarget : null;
                case "sweep_1":
                    if (current != null && current.IsPanelOpen("level") && levels != null)
                        return levels.FindSweepTarget("Level1");
                    return current != null ? current.ButtonTarget("play") : null;
                case "force_dark":
                    if (current != null && current.IsPanelOpen("side") && side != null) return side.DarkTarget;
                    if (current != null && current.IsPanelOpen("level") && levels != null) return levels.FindLevelTarget("Level2");
                    return current != null ? current.ButtonTarget("play") : null;
                case "hero_info":
                    if (side == null) return null;
                    return side.FindLoadoutItem("hero", TaughtHeroId) ?? side.HeroTarget;
                case "unit_info":
                    if (side == null) return null;
                    return side.FindLoadoutItem("unit", CurrentLessonUnit()) ?? side.UnitTarget;
                case "spell_info":
                    if (side == null) return null;
                    return side.FindLoadoutItem("spell", TaughtSpellId) ?? side.SpellTarget;
                case "start_dark":
                    return side != null ? side.StartTarget : null;
                case "wave_info":
                    WaveInfoUI waveInfo = FindAnyObjectByType<WaveInfoUI>();
                    if (waveInfo == null) return null;
                    if (waveInfoPage > 0 && waveInfo.IsOpen)
                        return waveInfo.FirstUnitTarget ?? waveInfo.ToggleTarget;
                    return waveInfo.ToggleTarget;
                case "hero_unit_info":
                    if (heroUnitPage <= 0 || !heroInfoSeen)
                        return FirstBattleHero();
                    return FirstBattleUnit();
                case "daily_tell":
                    DailyRewardUI daily = FindAnyObjectByType<DailyRewardUI>();
                    if (daily == null) return null;
                    return dailyClosePending ? daily.CloseTarget : daily.ClaimTarget;
                case "fortune_tell":
                    FortuneWheelUI wheel = FindAnyObjectByType<FortuneWheelUI>();
                    if (wheel == null) return null;
                    return fortuneClosePending ? wheel.CloseTarget : wheel.SpinTarget;
                case "fortune_go":
                    return current != null ? current.ButtonTarget("fortune") : null;
                case "daily_go":
                    return current != null ? current.ButtonTarget("daily") : null;
                case "compendium_go":
                    return current != null ? current.ButtonTarget("compendium") : null;
                case "heroes_go":
                    return current != null ? current.ButtonTarget("heroes") : null;
                case "map_next":
                    return levels != null ? levels.RightArrowTarget : null;
                case "normal_end":
                    return levels != null ? levels.HardTarget : null;
                case "hard_end":
                    return levels != null ? levels.ExpertTarget : null;
                default:
                    return null;
            }
        }

        RectTransform FirstBattleHero()
        {
            HeroButtonUI[] heroes = FindObjectsByType<HeroButtonUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i] != null && heroes[i].gameObject.activeInHierarchy)
                    return heroes[i].transform as RectTransform;
            }
            return null;
        }

        RectTransform FirstBattleUnit()
        {
            UnitButton[] units = FindObjectsByType<UnitButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] != null && units[i].gameObject.activeInHierarchy)
                    return units[i].transform as RectTransform;
            }
            return null;
        }

        RectTransform FindSkillTarget()
        {
            SkillNodeUI[] nodes = FindObjectsByType<SkillNodeUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < nodes.Length; i++)
            {
                SkillNodeData data = nodes[i].GetSkillData();
                if (data != null && data.skillID == CurrentSkillId())
                    return nodes[i].transform as RectTransform;
            }
            return null;
        }

        RectTransform SkillFocus()
        {
            SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
            if (Current() == "skill_buy" || Current() == "skill_buy_top")
            {
                if (skillClosePending && tree != null && tree.CloseTarget != null)
                    return tree.CloseTarget;
                if (tree != null && tree.BuyTarget != null && tree.SelectedSkillId == CurrentSkillId())
                    return tree.BuyTarget;
            }
            return FindSkillTarget();
        }

        void CenterTargetSkill()
        {
            SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
            RectTransform node = FindSkillTarget();
            if (tree != null && node != null)
                tree.CenterOn(node);
        }

        void ReleaseBottomSkillForLesson()
        {
            if (bottomSkillReleased) return;
            string step = Current();
            if (step != "skill_go" && step != "skill_info" && step != "skill_buy") return;
            bottomSkillReleased = true;
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta == null || !meta.IsSkillUnlocked(TargetSkillId)) return;
            meta.RevokeSkill(TargetSkillId);
            SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
            if (tree != null) tree.UpdateAllNodes();
        }

        void PrepareTargetSkill()
        {
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            RectTransform node = FindSkillTarget();
            if (meta == null || node == null) return;
            SkillNodeUI skillNode = node.GetComponent<SkillNodeUI>();
            SkillNodeData data = skillNode != null ? skillNode.GetSkillData() : null;
            if (data == null || meta.IsSkillUnlocked(data.skillID)) return;

            bool changed = false;
            if (meta.GetTotalKarma() < data.karmaCost)
            {
                meta.AddKarma(data.karmaCost - meta.GetTotalKarma());
                changed = true;
            }
            if (meta.GetTotalCrystals() < data.crystalCost)
            {
                meta.AddCrystals(data.crystalCost - meta.GetTotalCrystals());
                changed = true;
            }
            if (!changed) return;
            SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
            if (tree != null) tree.UpdateAllNodes();
        }

        void BuildShade()
        {
            shadeFull = MakeShade("ShadeFull");
            shadeTop = MakeShade("ShadeTop");
            shadeBottom = MakeShade("ShadeBottom");
            shadeLeft = MakeShade("ShadeLeft");
            shadeRight = MakeShade("ShadeRight");
            RectTransform full = shadeFull.rectTransform;
            full.anchorMin = Vector2.zero;
            full.anchorMax = Vector2.one;
            full.offsetMin = Vector2.zero;
            full.offsetMax = Vector2.zero;

            GameObject catchGo = new GameObject("SkillCatch", typeof(RectTransform), typeof(Image), typeof(Button));
            catchGo.transform.SetParent(root.transform, false);
            RectTransform catchRect = catchGo.GetComponent<RectTransform>();
            catchRect.anchorMin = new Vector2(0.5f, 0.5f);
            catchRect.anchorMax = new Vector2(0.5f, 0.5f);
            catchRect.pivot = new Vector2(0.5f, 0.5f);
            Image catchImage = catchGo.GetComponent<Image>();
            catchImage.sprite = WhiteSprite();
            catchImage.color = new Color(1f, 1f, 1f, 0f);
            catchImage.raycastTarget = true;
            skillCatch = catchGo.GetComponent<Button>();
            skillCatch.transition = Selectable.Transition.None;
            catchGo.SetActive(false);
        }

        void PlaceSkillCatch()
        {
            if (skillCatch != null) skillCatch.gameObject.SetActive(false);
        }

        Image MakeShade(string shadeName)
        {
            GameObject go = new GameObject(shadeName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            Image image = go.GetComponent<Image>();
            image.sprite = WhiteSprite();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = true;
            go.SetActive(false);
            return image;
        }

        void PlaceCard()
        {
            if (bandRect == null) return;
            if (InMatch())
            {
                bandRect.anchorMin = new Vector2(1f, 0f);
                bandRect.anchorMax = new Vector2(1f, 0f);
                bandRect.pivot = new Vector2(1f, 0f);
                bandRect.anchoredPosition = new Vector2(-18f, 188f);
                bandRect.sizeDelta = new Vector2(520f, 156f);
                frameRect.anchoredPosition = new Vector2(10f, 0f);
                frameRect.sizeDelta = new Vector2(92f, 92f);
                nameRect.anchoredPosition = new Vector2(112f, -16f);
                nameRect.sizeDelta = new Vector2(-250f, 24f);
                textRect.offsetMin = new Vector2(112f, 10f);
                textRect.offsetMax = new Vector2(-84f, -40f);
                FitNext(40f);
                if (nameText != null) nameText.fontSize = 20f;
                if (bodyText != null) bodyText.fontSize = 18f;
                return;
            }

            bandRect.anchorMin = new Vector2(0f, 0f);
            bandRect.anchorMax = new Vector2(0f, 0f);
            bandRect.pivot = new Vector2(0f, 0f);
            if (nameText != null) nameText.fontSize = 22f;
            if (bodyText != null) bodyText.fontSize = 20f;
            MainMenuController current = Menu();
            bool onMap = current != null && current.IsPanelOpen("level");
            bool onSide = current != null && current.IsPanelOpen("side");
            if (onSide)
            {
                bandRect.anchorMin = new Vector2(1f, 0f);
                bandRect.anchorMax = new Vector2(1f, 0f);
                bandRect.pivot = new Vector2(1f, 0f);
                bandRect.anchoredPosition = new Vector2(-110f, 12f);
                bandRect.sizeDelta = new Vector2(560f, 140f);
                frameRect.anchoredPosition = new Vector2(10f, 0f);
                frameRect.sizeDelta = new Vector2(96f, 96f);
                nameRect.anchoredPosition = new Vector2(116f, -14f);
                textRect.offsetMin = new Vector2(116f, 8f);
                textRect.offsetMax = new Vector2(-86f, -38f);
                FitNext(40f);
                return;
            }
            if (onMap)
            {
                bandRect.anchoredPosition = new Vector2(420f, 8f);
                bandRect.sizeDelta = new Vector2(640f, 132f);
                frameRect.anchoredPosition = new Vector2(10f, 0f);
                frameRect.sizeDelta = new Vector2(96f, 96f);
                nameRect.anchoredPosition = new Vector2(116f, -16f);
                textRect.offsetMin = new Vector2(116f, 8f);
                textRect.offsetMax = new Vector2(-86f, -40f);
                FitNext(40f);
                return;
            }

            bandRect.anchoredPosition = new Vector2(110f, 16f);
            bandRect.sizeDelta = new Vector2(620f, 156f);
            frameRect.anchoredPosition = new Vector2(14f, 0f);
            frameRect.sizeDelta = new Vector2(108f, 108f);
            nameRect.anchoredPosition = new Vector2(134f, -28f);
            textRect.offsetMin = new Vector2(134f, 12f);
            textRect.offsetMax = new Vector2(-90f, -56f);
            FitNext(40f);
        }

        void FitNext(float size)
        {
            if (nextChrome == null) return;
            nextChrome.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        }

        void EnsureCard()
        {
            if (root != null) return;

            root = new GameObject("TutorialCard", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(null, false);
            DontDestroyOnLoad(root);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.localScale = Vector3.one;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            BuildShade();

            TutorialArt artSet = Resources.Load<TutorialArt>("TutorialArt");
            Sprite paper = artSet != null ? artSet.card : null;
            Sprite face = artSet != null ? artSet.mira : null;
            Sprite nextArt = artSet != null ? artSet.next : null;
            Sprite plate = paper != null ? paper : WhiteSprite();
            Sprite nextIcon = nextArt != null ? nextArt : plate;

            GameObject band = new GameObject("Band", typeof(RectTransform), typeof(Image));
            band.transform.SetParent(root.transform, false);
            bandRect = band.GetComponent<RectTransform>();
            bandRect.anchorMin = new Vector2(0f, 0f);
            bandRect.anchorMax = new Vector2(0f, 0f);
            bandRect.pivot = new Vector2(0f, 0f);
            bandRect.anchoredPosition = new Vector2(16f, 16f);
            bandRect.sizeDelta = new Vector2(700f, 188f);
            panelImage = band.GetComponent<Image>();
            panelImage.sprite = plate;
            panelImage.type = Image.Type.Simple;
            panelImage.color = Color.white;
            panelImage.raycastTarget = false;

            GameObject frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(band.transform, false);
            frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0f, 0.5f);
            frameRect.anchorMax = new Vector2(0f, 0.5f);
            frameRect.pivot = new Vector2(0f, 0.5f);
            frameRect.anchoredPosition = new Vector2(18f, 0f);
            frameRect.sizeDelta = new Vector2(128f, 128f);
            Image frameImage = frame.GetComponent<Image>();
            frameImage.sprite = WhiteSprite();
            frameImage.type = Image.Type.Simple;
            frameImage.color = new Color(0.28f, 0.16f, 0.08f, 1f);
            frameImage.raycastTarget = false;

            GameObject art = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            art.transform.SetParent(frame.transform, false);
            RectTransform artRect = art.GetComponent<RectTransform>();
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = new Vector2(6f, 6f);
            artRect.offsetMax = new Vector2(-6f, -6f);
            portrait = art.GetComponent<Image>();
            portrait.sprite = face != null ? face : plate;
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            GameObject nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(band.transform, false);
            nameRect = nameGo.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0f, 1f);
            nameRect.anchoredPosition = new Vector2(162f, -46f);
            nameRect.sizeDelta = new Vector2(-330f, 28f);
            nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.text = "Mira";
            nameText.fontSize = 26f;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = new Color(0.42f, 0.24f, 0.08f, 1f);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.raycastTarget = false;
            if (font != null) nameText.font = font;

            GameObject textGo = new GameObject("Body", typeof(RectTransform));
            textGo.transform.SetParent(band.transform, false);
            textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(162f, 16f);
            textRect.offsetMax = new Vector2(-96f, -78f);
            bodyText = textGo.AddComponent<TextMeshProUGUI>();
            bodyText.fontSize = 24f;
            bodyText.color = new Color(0.2f, 0.12f, 0.06f, 1f);
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.enableWordWrapping = true;
            bodyText.overflowMode = TextOverflowModes.Truncate;
            bodyText.raycastTarget = false;
            if (font != null) bodyText.font = font;

            GameObject rimGo = new GameObject("NextRim", typeof(RectTransform), typeof(Image));
            nextChrome = rimGo;
            rimGo.transform.SetParent(band.transform, false);
            RectTransform rimRect = rimGo.GetComponent<RectTransform>();
            rimRect.anchorMin = new Vector2(1f, 0.5f);
            rimRect.anchorMax = new Vector2(1f, 0.5f);
            rimRect.pivot = new Vector2(1f, 0.5f);
            rimRect.anchoredPosition = new Vector2(-18f, 0f);
            rimRect.sizeDelta = new Vector2(40f, 40f);
            Image rimImage = rimGo.GetComponent<Image>();
            rimImage.color = new Color(1f, 1f, 1f, 0f);
            rimImage.raycastTarget = false;

            GameObject buttonGo = new GameObject("Next", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(rimGo.transform, false);
            RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            Image buttonImage = buttonGo.GetComponent<Image>();
            buttonImage.sprite = nextIcon;
            buttonImage.type = Image.Type.Simple;
            buttonImage.preserveAspect = true;
            buttonImage.color = Color.white;
            nextButton = buttonGo.GetComponent<Button>();
            nextButton.targetGraphic = buttonImage;
        }

        static Sprite whiteSprite;

        static Sprite WhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
            return whiteSprite;
        }

        public bool WaitsSkillClose() => skillClosePending;

        public bool LocksSkillClose()
        {
            string step = Current();
            return (step == "skill_buy" || step == "skill_buy_top") && !skillClosePending;
        }

        public void OnSkillPurchased()
        {
            string step = Current();
            if (step != "skill_buy" && step != "skill_buy_top") return;
            skillClosePending = true;
        }

        public void OnSkillBuyClosed()
        {
            if (!skillClosePending) return;
            string step = Current();
            skillClosePending = false;
            if (step == "skill_buy" || step == "skill_buy_top")
                Complete(step);
        }

        bool SkillBuyPanelOpen()
        {
            SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
            return tree != null && tree.DetailTarget != null && tree.SelectedSkillId == CurrentSkillId();
        }

        string SkillBuyText(string step)
        {
            if (skillClosePending) return "All right, close it.";
            SkillTreeUI tree = FindAnyObjectByType<SkillTreeUI>();
            if (tree != null && tree.BuyTarget != null && tree.SelectedSkillId == CurrentSkillId())
                return step == "skill_buy_top" ? "Buy Shadow Freeze." : "Buy Dark Initiation.";
            return step == "skill_buy_top"
                ? "Tap Shadow Freeze up and to the right once. You'll buy it from the big window."
                : "Tap Dark Initiation once. You'll buy it from the big window.";
        }

        string CardText(string step, string fallback)
        {
            if (step == "hero_info" && !string.IsNullOrEmpty(TaughtHeroName))
                return loadoutRead
                    ? "Now select " + TaughtHeroName + "."
                    : TaughtHeroName + " is this road's hero. Double-tap, read, then select.";
            if (step == "unit_info")
            {
                string unitName = CurrentLessonUnit();
                if (string.IsNullOrEmpty(unitName)) unitName = TaughtBossName;
                if (!string.IsNullOrEmpty(unitName))
                {
                    if (unitCursor > 0) return "Next up, " + unitName + ". Select them.";
                    return loadoutRead
                        ? "Now select " + unitName + "."
                        : unitName + " is this road's soldier. Double-tap, read, then select.";
                }
            }
            if (step == "spell_info" && !string.IsNullOrEmpty(TaughtSpellName))
                return loadoutRead
                    ? "Now select " + TaughtSpellName + "."
                    : TaughtSpellName + " is this road's spell. Double-tap, read, then select it if you want.";
            if (step == "skill_info")
                return "Double-tap Dark Initiation. See what it is first.";
            if (step == "skill_info_top")
                return "Double-tap Shadow Freeze. See what it is first.";
            if (step == "skill_back")
                return "Go back. Out to the menu.";
            if (step == "sweep_1")
                return "You can skip Level 1. Tap auto-complete.";
            if (step == "skill_buy" || step == "skill_buy_top")
                return SkillBuyText(step);
            if (step == "place_tower" || step == "place_tower_2")
            {
                if (step == "place_tower_2")
                    return OpenBuildUi() != null
                        ? "Tap a tower. Its info comes up. Tap the same tower again and it builds. Without enough souls, it won't."
                        : "Tap that stone ring. Your tower goes there.";
                return ArcherButton() != null
                    ? "Your first tower should be the archer. Tap Archer, then tap it again."
                    : "Tap that stone ring. We'll put your towers there.";
            }
            if (step == "tower_menu")
            {
                if (!towerMenuShown)
                    return "Tap the tower you built. Its menu opens.";
                if (towerMenuPage <= 0)
                    return "Check Info. It tells you what this tower does.";
                if (towerMenuPage == 1)
                    return "Upgrade makes the tower stronger. It costs gold.";
                return "Priority picks who it shoots. First, Last, Strong, or Weak.";
            }
            return fallback;
        }

        string Body(string step)
        {
            switch (step)
            {
                case "war_intro": return "Two armies are fighting. Light marches with the living, Dark with the dead. When you're ready, hit Play.";
                case "force_light": return "Open Green Glade. You'll hold this first fight with Light. Choose them.";
                case "to_battle_1": return "Your army is ready. Go on, start the fight.";
                case "currency_gold": return "The number up top is your gold. You build towers with it. If you can't afford one, the cost turns red.";
                case "place_tower": return "Tap that stone ring. We'll put your towers there.";
                case "the_road": return "This road is yours. The enemy walks it, and it ends at your gate. Your tower fires down this road.";
                case "lives": return "The castle on the left is your gate. The heart up top, 20/20, is your life. If an enemy reaches the castle, you lose life.";
                case "wave_time": return "Wave shows the next march. When Time runs out, the enemy takes the road.";
                case "incoming": return "The ones at the bottom left are your soldiers. The number under them is the cost. You'll drop them on the road.";
                case "speed_info": return "Speed is up on the right. X1 is normal, X2 and X3 are faster. Pause stops everything.";
                case "unit_drop": return "Grab a soldier, drag them onto the road, and let go. If you can't afford them, the cost turns red.";
                case "skip_wave": return "Hit Skip. Let them march without waiting.";
                case "hero_move":
                    return HeroReadyToMove()
                        ? "Selected. Tap somewhere on the road and they'll walk there."
                        : "Tap the hero. You need to select them first.";
                case "hero_skill": return "Your hero is low on life. Tap the ability button and let them recover.";
                case "tower_menu": return "Tap the tower you built. Its menu opens.";
                case "victory_1": return "The gate held. This fight belongs to Light. With the power you earned, you'll take a skill.";
                case "skill_go": return "You still have power from the fight. Open the skill tree.";
                case "skill_info": return "Double-tap Dark Initiation. See what it is first.";
                case "skill_buy": return "Tap Dark Initiation once. You'll buy it from the big window.";
                case "skill_info_top": return "Double-tap Shadow Freeze. See what it is first.";
                case "skill_buy_top": return "Tap Shadow Freeze up and to the right once. You'll buy it from the big window.";
                case "skill_back": return "Go back. Out to the menu.";
                case "sweep_1": return "You can skip Level 1. Tap auto-complete.";
                case "force_dark": return "Open Whispering Woods. This time choose Dark. You'll see the same fight from their side.";
                case "to_battle_2": return "This time you'll build the squad yourself. Look at the hero first.";
                case "hero_info": return "Double-tap a hero. Read who they are. Then pick up to two.";
                case "unit_info": return "Double-tap a soldier. Then pick four soldiers and a boss. The road is theirs.";
                case "spell_info": return "Double-tap a spell. Take all three if you want. You can leave them empty too.";
                case "start_dark": return "Dark is ready. Start the second fight.";
                case "wave_info":
                    if (waveInfoPage > 0)
                    {
                        WaveInfoUI wavePanel = FindAnyObjectByType<WaveInfoUI>();
                        if (wavePanel != null && wavePanel.IsOpen)
                            return "Tap a soldier and their info will open.";
                    }
                    return "Tap W. You'll see who's in this wave.";
                case "hero_unit_info":
                    if (!heroInfoSeen) return "Double-tap the hero and their info will open.";
                    if (heroUnitPage == 0) return "You've seen it. Close it now.";
                    return "Now tap a soldier. Their info opens too.";
                case "boss_intro": return "This is a boss. They're the only one in the last wave.";
                case "boss_spell": return bossSpellPage > 0 ? "Tap the boss and drop the spell there." : "Select your spell.";
                case "victory_2": return "You've seen both sides and held both gates. From here, you choose the side.";
                case "fortune_go": return "A fight isn't only the gate. Open the wheel.";
                case "fortune_tell": return fortuneClosePending ? "All right, close it." : "Spin the wheel and let your reward come.";
                case "daily_go": return "You get one each day. Open the daily reward.";
                case "daily_tell": return dailyClosePending ? "All right, close it." : "Take today's reward.";
                case "compendium_go": return "You should know both armies. Open the compendium.";
                case "compendium_tell":
                    return compendiumPage <= 0
                        ? "Tap that first soldier. See who they are."
                        : "Hit Back and return to the menu.";
                case "heroes_go": return "Open the hero shop. The new ones are waiting there.";
                case "heroes_tell":
                    if (heroShopPage <= 0) return "Tap the first Dark hero on the list.";
                    if (heroShopPage == 1) return "Buy them and let them join your army.";
                    return "Hit Back and return to the menu.";
                case "tutorial_rest": return "You've learned. The roads are yours now. We'll talk again when a fight ends.";
                case "map_next": return "This map is done. Hit the right arrow. The next map is there.";
                case "war_end": return "The last road fell too. You held both sides. The gates are yours.";
                case "normal_end": return "The Normal roads are done. Hard is next. Choose it.";
                case "hard_end": return "The Hard roads fell too. Expert is next. Choose it.";
                case "expert_finish": return "Expert's last gate fell too. Both armies stop here. You held both sides. That's all I have to say.";
                default: return "";
            }
        }

        static string MomentText(string step)
        {
            if (step == "barracks")
                return "A barracks doesn't shoot. It sends soldiers. Use Move to pick where they return. Keep the point inside the circle.";
            return "Tap the spell button, aim, and release. Light pays gold, Dark pays souls. Then the spell rests for a while.";
        }

    }
}
