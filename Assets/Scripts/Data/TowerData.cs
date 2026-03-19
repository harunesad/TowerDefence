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
        public float explosionRadius;
        public LayerMask targetLayer;

        [Header("Status Effect")]
        public StatusEffectType effectType;
        public float effectDuration;
        public float effectPower;

        [Header("Audio")]
        public AudioClip shootSFX;
    }
}
