using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    public enum HeroAbilityType
    {
        RallyHeal,
        HolyShield,
        ArrowRain,
        SwiftStrike,
        FortifyTaunt,
        SolarSmite,
        LifeDrain,
        SoulExecute,
        GroundSlam,
        PlagueCloud,
        BoneArmor,
        ShadowStep
    }

    [CreateAssetMenu(fileName = "New Hero", menuName = "Tower Defence/Hero Data")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroID;
        public string displayName;
        public Side side;

        [Header("Combat")]
        public UnitData unitData;
        public Sprite icon;

        [Header("Hero Roster (Menu) Icon Settings")]
        [Tooltip("Icon settings for Hero menu and Hero Shop cards.")]
        public UIIconSettings rosterIcon = new UIIconSettings { scale = 1f };

        [Header("Prepare For Battle (Loadout) Icon Settings")]
        [Tooltip("Hero icon settings on Prepare for Battle screen.")]
        public UIIconSettings loadoutIcon = new UIIconSettings { scale = 1f };

        [Header("In-Game HUD Icon Settings")]
        [Tooltip("Icon settings on in-game hero button.")]
        public UIIconSettings hudIcon = new UIIconSettings { scale = 1f };

        [Header("Unique Ability")]
        public HeroAbilityType abilityType;
        public string abilityName;
        [TextArea(2, 4)] public string abilityDescription;
        public float abilityCooldown = 12f;
        public float abilityPower = 1f;
        public float abilityRadius = 3f;
        public AudioClip abilitySFX;

        [Header("Meta Progression")]
        public int unlockKarmaCost = 500;
        public int unlockCrystalCost = 50; // New: Crystal cost
        public bool isStarterHero;
        public int maxUpgradeLevel = 10;
        public int upgradeKarmaCost = 150;
        public int upgradeCrystalCost = 15;
        [Range(0.05f, 0.5f)] public float healthBonusPerLevel = 0.08f;
        [Range(0.05f, 0.5f)] public float damageBonusPerLevel = 0.08f;

        public int KarmaCostForNextLevel(int currentLevel)
        {
            return ScaledUpgradeCost(upgradeKarmaCost, currentLevel);
        }

        public int CrystalCostForNextLevel(int currentLevel)
        {
            return ScaledUpgradeCost(upgradeCrystalCost, currentLevel);
        }

        static int ScaledUpgradeCost(int baseCost, int currentLevel)
        {
            int numer;
            int denom;
            switch (currentLevel)
            {
                case 1: numer = 1; denom = 1; break;
                case 2: numer = 5; denom = 3; break;
                case 3: numer = 8; denom = 3; break;
                case 4: numer = 4; denom = 1; break;
                case 5: numer = 16; denom = 3; break;
                case 6: numer = 22; denom = 3; break;
                case 7: numer = 10; denom = 1; break;
                case 8: numer = 40; denom = 3; break;
                default: numer = 52; denom = 3; break;
            }
            return Mathf.Max(0, Mathf.RoundToInt(baseCost * numer / (float)denom));
        }

        public string AbilityDescriptionAtLevel(int level)
        {
            float power = AbilityPowerAtLevel(Mathf.Max(1, level));
            switch (abilityType)
            {
                case HeroAbilityType.RallyHeal:
                    return $"Restores {Percent(0.15f * power)}% of max health.";
                case HeroAbilityType.HolyShield:
                    return $"Restores {Percent(0.2f * power)}% of max health below 35%, and grants a shield for {Percent(0.35f * power)}%.";
                case HeroAbilityType.ArrowRain:
                    return $"Deals {Percent(2f * power)}% attack damage in an area.";
                case HeroAbilityType.SwiftStrike:
                    return $"Deals {Percent(2.5f * power)}% attack damage.";
                case HeroAbilityType.FortifyTaunt:
                    return "Taunts nearby enemies.";
                case HeroAbilityType.SolarSmite:
                    return $"Deals {Percent(2f * power)}% attack damage around the hero.";
                case HeroAbilityType.LifeDrain:
                    return $"Deals {Percent(1.5f * power)}% attack damage and restores half of it.";
                case HeroAbilityType.SoulExecute:
                    return $"Deals {Percent(4f * power)}% attack damage to an enemy below 40% health.";
                case HeroAbilityType.GroundSlam:
                    return $"Deals {Percent(2.2f * power)}% attack damage around the hero.";
                case HeroAbilityType.PlagueCloud:
                    return $"Poisons for {Percent(0.5f * power)}% attack damage each second for 4s.";
                case HeroAbilityType.BoneArmor:
                    return $"Grants a shield for {Percent(0.25f * power)}% of max health.";
                case HeroAbilityType.ShadowStep:
                    return $"Steps behind the target and deals {Percent(3f * power)}% attack damage.";
                default:
                    return abilityDescription;
            }
        }

        static string Percent(float ratio)
        {
            return (ratio * 100f).ToString("0.##");
        }

        public float AbilityCooldownAtLevel(int level)
        {
            float scale = Mathf.Max(0.5f, 1f - Mathf.Max(0, level - 1) * 0.05f);
            return abilityCooldown * scale;
        }

        public static string FormatSeconds(float seconds)
        {
            return seconds.ToString("0.##");
        }

        public float AbilityPowerAtLevel(int level)
        {
            return abilityPower * (1f + Mathf.Max(0, level - 1) * 0.05f);
        }

        public Sprite GetIcon() => icon != null ? icon : (unitData != null ? unitData.icon : null);
    }
}
