using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    [System.Serializable]
    public struct UIIconSettings
    {
        [Tooltip("İkon boyutu (Genişlik, Yükseklik). (0,0) bırakılırsa varsayılan prefab boyutu kullanılır.")]
        public Vector2 size;
        [Tooltip("İkon ölçek çarpanı (Varsayılan 1).")]
        public float scale;
        [Tooltip("İkon pozisyon ofseti (X, Y).")]
        public Vector2 offset;

        public void ApplyTo(UnityEngine.UI.Image img)
        {
            if (img == null) return;
            img.preserveAspect = true;
            RectTransform rt = img.rectTransform;
            if (size != Vector2.zero)
            {
                rt.sizeDelta = size;
            }
            float s = (scale > 0.001f) ? scale : 1f;
            if (s != 1f)
            {
                rt.localScale = Vector3.one * s;
            }
            rt.anchoredPosition = offset;
        }
    }

    [CreateAssetMenu(fileName = "New Unit Data", menuName = "Tower Defence/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("General Info")]
        public string unitName;
        public Side side;
        public GameObject prefab;
        public Sprite icon;
        public UnitData enemyCounterpart; // Karşı taraftaki karşılığı (Aydınlık <-> Karanlık)
        [TextArea(2, 3)]
        [Tooltip("Compendium ve bilgi ekranında gösterilecek kısa tanıtım/lore metni.")]
        public string loreDescription;

        [Header("Compendium Icon Settings")]
        [Tooltip("Kütüphane / Compendium ekranındaki ikon ayarları.")]
        public UIIconSettings compendiumIcon = new UIIconSettings { scale = 1f };

        [Header("Prepare For Battle (Loadout) Icon Settings")]
        [Tooltip("Savaş Hazırlık (Prepare for Battle) ekranındaki ikon ayarları.")]
        public UIIconSettings loadoutIcon = new UIIconSettings { scale = 1f };

        [Header("In-Game HUD Icon Settings")]
        [Tooltip("Oyun içi birim çağırma butonundaki ikon ayarları.")]
        public UIIconSettings hudIcon = new UIIconSettings { scale = 1f };

        [Header("Wave Info Icon Settings")]
        [Tooltip("Oyun içi sol üst Wave Info panelindeki ikon ayarları.")]
        public UIIconSettings waveInfoIcon = new UIIconSettings { scale = 1f };

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
