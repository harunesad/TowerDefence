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

        [Header("Unique Ability")]
        public HeroAbilityType abilityType;
        public string abilityName;
        [TextArea(2, 4)] public string abilityDescription;
        public float abilityCooldown = 12f;
        public float abilityPower = 1f;
        public float abilityRadius = 3f;

        [Header("Meta Progression")]
        public int unlockKarmaCost = 500;
        public bool isStarterHero;
        public int maxUpgradeLevel = 5;
        public int upgradeKarmaCost = 150;
        [Range(0.05f, 0.5f)] public float healthBonusPerLevel = 0.15f;
        [Range(0.05f, 0.5f)] public float damageBonusPerLevel = 0.10f;

        public Sprite GetIcon() => icon != null ? icon : (unitData != null ? unitData.icon : null);
    }
}
