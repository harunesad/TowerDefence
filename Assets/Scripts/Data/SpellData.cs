using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    public enum SpellType
    {
        Thunderstrike, // Instant area-of-effect damage
        Reinforcement, // Summons temporary soldiers onto the path
        Freeze,        // Freezes all enemies
        GoldBoost,     // Instant gold gain
        Shield,        // Temporary invulnerability for towers
        Buff           // Temporary attack speed increase for towers
    }

    [CreateAssetMenu(fileName = "New Spell Data", menuName = "Tower Defence/Spells/Spell Data")]
    public class SpellData : ScriptableObject
    {
        [Header("Basic Info")]
        public string spellID;
        public string spellName;
        public Side side;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Economics")]
        public int manaCost;        // Mana / Gold cost
        public float cooldown = 10f;
        public bool isOneTimeUse = false; // Can it be used only once per game?

        [Header("Effects")]
        public SpellType spellType;
        public float power = 50f;     // Damage, heal, or buff multiplier
        public float radius = 3f;    // Radius of effect area
        public float duration = 5f;  // Duration for buff, freeze, or shield
        
        [Header("Visuals")]
        public VFXType spellVFXType;
        public GameObject spellVFXPrefab; // VFX prefab to be directly instantiated
        public AudioClip castSFX;

        [Header("Spawn Settings (For Reinforcements)")]
        public GameObject unitPrefabToSpawn;
        public int unitSpawnCount = 2;
    }
}
