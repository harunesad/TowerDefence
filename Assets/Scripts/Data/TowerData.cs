using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Combat;

namespace TowerDefence.Data
{
    [CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defence/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("General Info")]
        public string towerName;
        public Side side;
        public GameObject prefab;
        public GameObject projectilePrefab;
        public Sprite icon;
        public TowerData enemyCounterpart; // Counterpart on opposing side (Light <-> Dark)
        public Color towerColor = Color.white;

        [Header("Economics")]
        public int cost;
        public int upgradeCost;

        [Header("Stats")]
        public float range;
        public float fireRate;
        public float damage;
        public float health; // Tower durability / health
        public float explosionRadius;
        public LayerMask targetLayer;

        [Header("Status Effect")]
        public StatusEffectType effectType;
        public float effectDuration;
        public float effectPower;

        [Header("Specialization")]
        public System.Collections.Generic.List<TowerData> specializations;

        [Header("Audio")]
        public AudioClip shootSFX;

        [Header("Meta")]
        public bool isBaseTower = false; // Only base towers (Archer, Mage, etc.) are true
        [Tooltip("Clockwise index on build ring. 0 is top.")]
        public int menuOrder;

        [Header("Artillery")]
        public bool isArtillery = false; // Artillery/mortar towers: projectile arches balistically

        [Header("Beam Tower")]
        public bool isBeamTower = false; // Beam tower: does not fire projectiles, continuously burns target with a laser

        [Header("Slow Tower")]
        public bool isSlowTower = false; // Area slow tower: continuously slows surrounding enemy units

        [Header("Aura Tower")]
        public bool isAuraTower = false;
        [Range(1f, 15f)] public float auraRadius = 5f;
        [Range(0f, 1f)] public float auraDamageBonus = 0.25f;   // +25% damage boost
        [Range(0f, 1f)] public float auraFireRateBonus = 0.15f; // +15% fire rate boost

        public float AuraLevelScale(int level)
        {
            return 1f + Mathf.Max(0, level - 1) * 0.2f;
        }

    }
}
