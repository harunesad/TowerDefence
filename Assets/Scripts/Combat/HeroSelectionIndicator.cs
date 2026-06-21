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
        private static readonly Color PassiveColor = new Color(0.15f, 0.75f, 0.25f, 0.35f);
        private static readonly Color ActiveColor = new Color(0.2f, 1f, 0.35f, 0.85f);

        private GameObject ringObject;
        private Renderer ringRenderer;
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
            float scale = state == HeroSelectionState.Active ? 2.5f : 2.0f;
            ringObject.transform.localScale = new Vector3(scale, 0.02f, scale);

            if (ringRenderer != null)
                ringRenderer.material.color = state == HeroSelectionState.Active ? ActiveColor : PassiveColor;
        }

        private void EnsureRing()
        {
            if (ringObject != null) return;

            ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringObject.name = "SelectionRing";
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            ringObject.transform.localScale = new Vector3(2f, 0.02f, 2f);

            var col = ringObject.GetComponent<Collider>();
            if (col != null) Destroy(col);

            ringRenderer = ringObject.GetComponent<Renderer>();
            if (ringRenderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                // Dynamic URP Lit Transparent setup
                mat.SetFloat("_Surface", 1f); // Transparent
                mat.SetFloat("_Blend", 0f); // Alpha blend
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                mat.color = PassiveColor;
                ringRenderer.material = mat;
            }

            ringObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (ringObject != null)
                Destroy(ringObject);
        }
    }
}
