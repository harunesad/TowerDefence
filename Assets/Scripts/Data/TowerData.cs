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
        public TowerData enemyCounterpart; // Karşı taraftaki karşılığı (Aydınlık <-> Karanlık)
        public Color towerColor = Color.white;

        [Header("Economics")]
        public int cost;
        public int upgradeCost;

        [Header("Stats")]
        public float range;
        public float fireRate;
        public float damage;
        public float health; // Kule dayanıklılığı
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
        public bool isBaseTower = false; // Sadece ana kuleler (Archer, Mage vb.) true olur

        [Header("Aura Tower")]
        public bool isAuraTower = false;
        [Range(1f, 15f)] public float auraRadius = 5f;
        [Range(0f, 1f)] public float auraDamageBonus = 0.25f;   // %25 hasar artışı
        [Range(0f, 1f)] public float auraFireRateBonus = 0.15f; // %15 ateş hızı artışı

    }
}
