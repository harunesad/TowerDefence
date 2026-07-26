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
        UnlockSpell
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
        public SkillCategory category; // Hangi dalda yer alacağı
        public int tier; // Hangi satırda (aşağıdan yukarıya) yer alacağı (0, 1, 2...)

        [Header("Unlock Settings")]
        public int karmaCost;
        public int crystalCost; // Yeni: Kristal maliyeti
        public List<SkillNodeData> requiredSkills;

        [Header("Effect Settings")]
        public UpgradeType upgradeType;
        public float multiplier = 1.1f; // %10 artış için 1.1
        public SpellData grantedSpell; // Eğer upgradeType == UnlockSpell ise bu büyü açılır
        public Side side; // Hangi tarafa ait olduğunu belirtir (Aydınlık/Karanlık/Genel)

        // Artık manuel pozisyona gerek kalmadı, UI tarafı otomatik düzecek.
        // public Vector2 visualPosition; 

        public bool IsUnlocked()
        {
            // Bu mantık MetaProgressionManager tarafından kontrol edilecek
            return false; 
        }
    }
}
