using UnityEngine;

namespace TowerDefence.UI
{
    public enum SafeAreaFitMode
    {
        Auto,           // Uses Anchor for full screen panels, Offset for corner-anchored elements
        StretchAnchors, // Directly matches anchorMin and anchorMax to safe area (ideal for Container/Panel)
        OffsetPosition  // Shifts element position inward by notch distance (ideal for corner buttons)
    }

    /// <summary>
    /// Automatically applies screen safe area boundaries (Screen.safeArea) such as notches, camera cutouts, and rounded corners
    /// to the RectTransform. Works seamlessly for both fullscreen panels and corner buttons.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Header("Fitting Mode")]
        [Tooltip("Application mode: Auto (recommended), StretchAnchors, or OffsetPosition")]
        [SerializeField] private SafeAreaFitMode fitMode = SafeAreaFitMode.Auto;

        [Header("Safe Area Edges")]
        [Tooltip("Apply notch/camera cutout inset on the left edge")]
        [SerializeField] private bool applyLeft = true;

        [Tooltip("Apply notch/camera cutout inset on the right edge")]
        [SerializeField] private bool applyRight = true;

        [Tooltip("Apply notch/status bar inset on the top edge")]
        [SerializeField] private bool applyTop = true;

        [Tooltip("Apply home indicator inset on the bottom edge")]
        [SerializeField] private bool applyBottom = true;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        // Store original position (to prevent cumulative drifting in Offset mode)
        private Vector2 initialAnchoredPosition;
        private bool hasInitializedPos;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>();
            if (!hasInitializedPos)
            {
                initialAnchoredPosition = rectTransform.anchoredPosition;
                hasInitializedPos = true;
            }
            ApplySafeArea();
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
            if (lastSafeArea != Screen.safeArea ||
                lastScreenSize != currentScreenSize ||
                lastOrientation != Screen.orientation)
            {
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;

            bool isFullScreen = Mathf.Approximately(rectTransform.anchorMin.x, 0f) &&
                               Mathf.Approximately(rectTransform.anchorMin.y, 0f) &&
                               Mathf.Approximately(rectTransform.anchorMax.x, 1f) &&
                               Mathf.Approximately(rectTransform.anchorMax.y, 1f);

            bool shouldUseAnchors = (fitMode == SafeAreaFitMode.StretchAnchors) ||
                                    (fitMode == SafeAreaFitMode.Auto && isFullScreen);

            if (shouldUseAnchors)
            {
                // Anchor Mode: Normalize safe area coordinates to 0.0 - 1.0 range
                Vector2 min = safeArea.position;
                Vector2 max = safeArea.position + safeArea.size;

                float normMinX = applyLeft ? (min.x / Screen.width) : 0f;
                float normMinY = applyBottom ? (min.y / Screen.height) : 0f;
                float normMaxX = applyRight ? (max.x / Screen.width) : 1f;
                float normMaxY = applyTop ? (max.y / Screen.height) : 1f;

                rectTransform.anchorMin = new Vector2(normMinX, normMinY);
                rectTransform.anchorMax = new Vector2(normMaxX, normMaxY);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            else
            {
                // Offset Mode: Calculate pixel difference based on canvas scale and shift inward
                if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
                float scaleFactor = (rootCanvas != null && rootCanvas.scaleFactor > 0f) ? rootCanvas.scaleFactor : 1f;

                float insetLeft = applyLeft ? (safeArea.xMin / scaleFactor) : 0f;
                float insetRight = applyRight ? ((Screen.width - safeArea.xMax) / scaleFactor) : 0f;
                float insetBottom = applyBottom ? (safeArea.yMin / scaleFactor) : 0f;
                float insetTop = applyTop ? ((Screen.height - safeArea.yMax) / scaleFactor) : 0f;

                Vector2 offset = Vector2.zero;

                // Shift inward (right) if anchored to left edge
                if (rectTransform.anchorMin.x < 0.5f && rectTransform.anchorMax.x < 0.5f)
                    offset.x += insetLeft;
                // Shift inward (left) if anchored to right edge
                else if (rectTransform.anchorMin.x > 0.5f && rectTransform.anchorMax.x > 0.5f)
                    offset.x -= insetRight;

                // Shift inward (up) if anchored to bottom edge
                if (rectTransform.anchorMin.y < 0.5f && rectTransform.anchorMax.y < 0.5f)
                    offset.y += insetBottom;
                // Shift inward (down) if anchored to top edge
                else if (rectTransform.anchorMin.y > 0.5f && rectTransform.anchorMax.y > 0.5f)
                    offset.y -= insetTop;

                rectTransform.anchoredPosition = initialAnchoredPosition + offset;
            }
        }
    }
}
