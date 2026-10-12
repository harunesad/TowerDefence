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
        public UnitData enemyCounterpart; // Karşı taraftaki karşılığı (Aydınlık <-> Karanlık)

        [Header("UI Icon Settings")]
        [Tooltip("İkon boyutu (Genişlik, Yükseklik). (0,0) bırakılırsa prefab varsayılanı kullanılır.")]
        public Vector2 iconSize = Vector2.zero;
        [Tooltip("İkon ölçek çarpanı (Varsayılan 1).")]
        public float iconScale = 1f;
        [Tooltip("İkon pozisyon ofseti (X, Y).")]
        public Vector2 iconOffset = Vector2.zero;

        [Header("Economics")]
        public int spawnCost;
        public int killReward;

        [Header("Stats")]
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackRange;
        public float attackRate;
        public GameObject projectilePrefab;

        [Header("Audio")]
        public AudioClip spawnSFX;
        public AudioClip deathSFX;
    }
}
