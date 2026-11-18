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
        public int maxUpgradeLevel = 5;
        public int upgradeKarmaCost = 150;
        public int upgradeCrystalCost = 15;
        [Range(0.05f, 0.5f)] public float healthBonusPerLevel = 0.08f;
        [Range(0.05f, 0.5f)] public float damageBonusPerLevel = 0.08f;

        public int KarmaCostForNextLevel(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1: return 150;
                case 2: return 250;
                case 3: return 400;
                default: return 600;
            }
        }

        public int CrystalCostForNextLevel(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1: return 15;
                case 2: return 25;
                case 3: return 40;
                default: return 60;
            }
        }

        public float AbilityCooldownAtLevel(int level)
        {
            float scale = Mathf.Max(0.5f, 1f - Mathf.Max(0, level - 1) * 0.1f);
            return abilityCooldown * scale;
        }

        public float AbilityPowerAtLevel(int level)
        {
            return abilityPower * (1f + Mathf.Max(0, level - 1) * 0.15f);
        }

        public Sprite GetIcon() => icon != null ? icon : (unitData != null ? unitData.icon : null);
    }
}
