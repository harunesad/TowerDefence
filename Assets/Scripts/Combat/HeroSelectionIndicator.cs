using UnityEngine;

namespace TowerDefence.Combat
{
    public enum HeroSelectionState
    {
        Hidden,
        Passive,
        Active
    }

    /// <summary>Her zaman görünen seçim halkası — pasif (soluk) / aktif (parlak).</summary>
    public class HeroSelectionIndicator : MonoBehaviour
    {
        private static readonly Color PassiveColor = new Color(0.02f, 0.04f, 0.18f, 0.35f);
        private static readonly Color ActiveColor = new Color(0.03f, 0.07f, 0.3f, 0.85f);

        public Sprite cursorSprite;
        private GameObject ringObject;
        private SpriteRenderer ringRenderer;
        private HeroSelectionState currentState = HeroSelectionState.Hidden;

        public HeroSelectionState CurrentState => currentState;

        public void SetState(HeroSelectionState state)
        {
            currentState = state;
            EnsureRing();

            if (ringObject == null) return;

            if (state == HeroSelectionState.Hidden)
            {
                ringObject.SetActive(false);
                return;
            }

            ringObject.SetActive(true);
            float scale = state == HeroSelectionState.Active ? 1.8f : 1.5f;
            ringObject.transform.localScale = new Vector3(scale, scale, 1f);

            if (ringRenderer != null)
                ringRenderer.color = state == HeroSelectionState.Active ? ActiveColor : PassiveColor;
        }

        private static Sprite cachedRingSprite;

        private void EnsureRing()
        {
            if (ringObject != null) return;

            ringObject = new GameObject("SelectionRing");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            ringObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Yere paralel olsun
            ringObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            ringRenderer = ringObject.AddComponent<SpriteRenderer>();
            
            if (cachedRingSprite == null)
                cachedRingSprite = CreateRingSprite();
                
            ringRenderer.sprite = cachedRingSprite;
            ringRenderer.color = PassiveColor;
            
            ringObject.SetActive(false);
        }

        private static Sprite CreateRingSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerRadius = size / 2f - 4f;
            float innerRadius = size / 2f - 12f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    // Yumuşak kenarlar (anti-aliasing) için
                    float alpha = 0f;
                    if (dist <= outerRadius && dist >= innerRadius) alpha = 1f;
                    else if (dist > outerRadius && dist < outerRadius + 2f) alpha = 1f - (dist - outerRadius) / 2f;
                    else if (dist < innerRadius && dist > innerRadius - 2f) alpha = 1f - (innerRadius - dist) / 2f;

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void OnDestroy()
        {
            if (ringObject != null)
                Destroy(ringObject);
        }
    }
}
