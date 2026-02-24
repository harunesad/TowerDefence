using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    [CreateAssetMenu(fileName = "New Unit Data", menuName = "Tower Defence/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("General Info")]
        public string unitName;
        public Side side;
        public GameObject prefab;
        public Sprite icon;

        [Header("Economics")]
        public int spawnCost;

        [Header("Stats")]
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackRange;
        public float attackRate;

        [Header("Audio")]
        public AudioClip spawnSFX;
        public AudioClip deathSFX;
    }
}
