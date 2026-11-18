using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.Data
{
    [System.Serializable]
    public struct UIIconSettings
    {
        [Tooltip("Icon size (Width, Height). Default prefab size used if (0,0).")]
        public Vector2 size;
        [Tooltip("Icon scale multiplier (Default 1).")]
        public float scale;
        [Tooltip("Icon position offset (X, Y).")]
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
        public UnitData enemyCounterpart; // Counterpart on the opposing side (Light <-> Dark)
        [TextArea(2, 3)]
        [Tooltip("Short lore/intro text shown in Compendium and info screen.")]
        public string loreDescription;

        [Header("Compendium Icon Settings")]
        [Tooltip("Icon settings in Compendium screen.")]
        public UIIconSettings compendiumIcon = new UIIconSettings { scale = 1f };

        [Header("Prepare For Battle (Loadout) Icon Settings")]
        [Tooltip("Icon settings on Prepare for Battle screen.")]
        public UIIconSettings loadoutIcon = new UIIconSettings { scale = 1f };

        [Header("In-Game HUD Icon Settings")]
        [Tooltip("Icon settings on in-game unit summon button.")]
        public UIIconSettings hudIcon = new UIIconSettings { scale = 1f };

        [Header("Wave Info Icon Settings")]
        [Tooltip("Icon settings in top-left Wave Info panel.")]
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

        public int LeakLives
        {
            get
            {
                if (spawnCost >= 500 || maxHealth >= 3000f) return 5;
                if (maxHealth >= 800f) return 3;
                if (spawnCost >= 250) return 2;
                return 1;
            }
        }

        public GameObject projectilePrefab;
        public GameObject breathVfxPrefab;
        public float breathVfxScale = 0.55f;

        [Header("Audio")]
        public AudioClip spawnSFX;
        public AudioClip deathSFX;
        public AudioClip attackSFX;
    }
}
