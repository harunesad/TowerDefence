using UnityEngine;

namespace TowerDefence.Data
{
    public enum SpellType
    {
        Meteor,        // Alan hasarı + Stun
        Reinforcement, // Yol üzerine geçici asker çağırma
        Freeze,        // Tüm düşmanları dondurma
        GoldBoost      // Anlık altın kazanımı
    }

    [CreateAssetMenu(fileName = "New Spell Data", menuName = "Tower Defence/Spells/Spell Data")]
    public class SpellData : ScriptableObject
    {
        [Header("Basic Info")]
        public string spellID;
        public string spellName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Economics")]
        public int manaCost;        // Büyü enerjisi / Altın maliyeti
        public float cooldown = 10f;

        [Header("Effects")]
        public SpellType spellType;
        public float power = 50f;     // Hasar, iyileştirme veya dondurma süresi
        public float radius = 3f;    // Etki alanı yarıçapı
        
        [Header("Visuals")]
        public GameObject vfxPrefab;
        public AudioClip castSFX;

        [Header("Spawn Settings (For Reinforcements)")]
        public GameObject unitPrefabToSpawn;
        public int unitSpawnCount = 2;
    }
}
