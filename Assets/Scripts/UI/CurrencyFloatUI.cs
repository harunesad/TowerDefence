using TMPro;
using UnityEngine;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class CurrencyFloatUI : MonoBehaviour
    {
        const float Life = 0.9f;
        const float Rise = 72f;

        Vector3 worldPosition;
        Canvas canvas;
        RectTransform rect;
        TextMeshProUGUI label;
        float elapsed;
        float baseAlpha = 1f;

        public static void Show(Vector3 worldPosition, int amount, Side side)
        {
            if (amount == 0) return;
            Canvas canvas = FindCanvas();
            Camera cam = Camera.main;
            if (canvas == null || cam == null) return;
            if (cam.WorldToScreenPoint(worldPosition).z <= 0f) return;

            GameObject go = new GameObject("CurrencyFloat", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            CurrencyFloatUI floater = go.AddComponent<CurrencyFloatUI>();
            floater.Begin(canvas, worldPosition + Vector3.up * 1.2f, amount, side);
        }

        void Begin(Canvas host, Vector3 world, int amount, Side side)
        {
            canvas = host;
            worldPosition = world;
            rect = transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(220f, 48f);

            label = gameObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 32f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.outlineColor = new Color32(0, 0, 0, 220);
            label.outlineWidth = 0.22f;
            string sign = amount > 0 ? "+" : "-";
            StatIcons.Set(label, sign + Mathf.Abs(amount) + " " + StatIcons.Price(side));
            label.color = amount > 0
                ? new Color(1f, 0.86f, 0.25f, 1f)
                : new Color(1f, 0.86f, 0.25f, 0.62f);
            baseAlpha = label.color.a;
            transform.SetAsLastSibling();
            Place(0f);
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Life);
            Place(Rise * t);
            if (label != null)
            {
                Color color = label.color;
                color.a = t < 0.45f ? baseAlpha : Mathf.Lerp(baseAlpha, 0f, (t - 0.45f) / 0.55f);
                label.color = color;
            }
            if (t >= 1f) Destroy(gameObject);
        }

        void Place(float rise)
        {
            if (rect == null || canvas == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(worldPosition);
            if (screen.z <= 0f) return;
            RectTransform canvasRect = canvas.transform as RectTransform;
            Camera eventCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, eventCam, out Vector2 local))
                rect.anchoredPosition = local + new Vector2(0f, rise);
        }

        static Canvas FindCanvas()
        {
            HUDController hud = FindAnyObjectByType<HUDController>();
            if (hud != null)
            {
                Canvas hudCanvas = hud.GetComponentInParent<Canvas>();
                if (hudCanvas != null) return hudCanvas;
            }

            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null && canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                    return canvases[i];
            }
            return canvases.Length > 0 ? canvases[0] : null;
        }
    }
}
