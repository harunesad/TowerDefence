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
        CurrencyStartBonus
    }

    [CreateAssetMenu(fileName = "New Skill Node", menuName = "Tower Defence/Meta/Skill Node")]
    public class SkillNodeData : ScriptableObject
    {
        public string skillID;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Unlock Settings")]
        public int karmaCost;
        public List<SkillNodeData> requiredSkills;

        [Header("Effect Settings")]
        public UpgradeType upgradeType;
        public float multiplier = 1.1f; // %10 artış için 1.1
        public Side side; // Hangi tarafa ait olduğunu belirtir (Aydınlık/Karanlık/Genel)

        public bool IsUnlocked()
        {
            // Bu mantık MetaProgressionManager tarafından kontrol edilecek
            return false; 
        }
    }
}
