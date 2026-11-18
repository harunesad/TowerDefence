using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    public enum UpgradeType
    {
        HealthBonus,
        DamageBonus,
        RangeBonus,
        SpeedBonus,
        CostReduction,
        CurrencyStartBonus,
        TowerDamageBonus,
        UnitSpeedBonus,
        UnlockSpell,
        CurrencyStartFlatBonus,
        TowerHealthBonus,
        CrystalStartFlatBonus
    }
    public enum SkillCategory
    {
        General_Economy,
        General_Base,
        Light_Towers,
        Light_Units,
        Light_Spells,
        Dark_Towers,
        Dark_Units,
        Dark_Spells
    }

    [CreateAssetMenu(fileName = "New Skill Node", menuName = "Tower Defence/Meta/Skill Node")]
    public class SkillNodeData : ScriptableObject
    {
        public string skillID;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Tree Positioning")]
        public SkillCategory category; // Which branch it belongs to
        public int tier; // Which row (from bottom to top) it resides in (0, 1, 2...)

        [Header("Unlock Settings")]
        public int karmaCost;
        public int crystalCost; // New: Crystal cost
        public List<SkillNodeData> requiredSkills;

        [Header("Effect Settings")]
        public UpgradeType upgradeType;
        public float multiplier = 1.1f; // 1.1 for 10% increase
        public SpellData grantedSpell; // If upgradeType == UnlockSpell, this spell is unlocked
        public Side side; // Specifies which side it belongs to (Light/Dark/General)

        // Manual position is no longer needed, UI handles layout automatically.
        // public Vector2 visualPosition; 

        public bool IsUnlocked()
        {
            // This logic is controlled by MetaProgressionManager
            return false; 
        }
    }
}
