using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    public enum SpellType
    {
        Thunderstrike, // Anlık alan hasarı
        Reinforcement, // Yol üzerine geçici asker çağırma
        Freeze,        // Tüm düşmanları dondurma
        GoldBoost,     // Anlık altın kazanımı
        Shield,        // Kulelere geçici dokunulmazlık
        Buff           // Kulelere geçici saldırı hızı artışı
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
        public int manaCost;        // Büyü enerjisi / Altın maliyeti
        public float cooldown = 10f;

        [Header("Effects")]
        public SpellType spellType;
        public float power = 50f;     // Hasar, iyileştirme veya buff çarpanı
        public float radius = 3f;    // Etki alanı yarıçapı
        public float duration = 5f;  // Buff, dondurma veya kalkan süresi
        
        [Header("Visuals")]
        public VFXType spellVFXType;
        public GameObject spellVFXPrefab; // Doğrudan instantiate edilecek VFX prefab'ı
        public AudioClip castSFX;

        [Header("Spawn Settings (For Reinforcements)")]
        public GameObject unitPrefabToSpawn;
        public int unitSpawnCount = 2;
    }
}
