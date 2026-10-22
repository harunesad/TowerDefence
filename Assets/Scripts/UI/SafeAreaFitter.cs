using UnityEngine;

namespace TowerDefence.UI
{
    public enum SafeAreaFitMode
    {
        Auto,           // Tam ekran panellerde Anchor, köşelere sabitli öğelerde Offset kullanır
        StretchAnchors, // Doğrudan anchorMin ve anchorMax'i güvenli alana eşitler (Konteyner/Panel için ideal)
        OffsetPosition  // Öğenin konumunu çentik mesafesi kadar içeri kaydırır (Köşe butonları için ideal)
    }

    /// <summary>
    /// Ekranın çentik (notch), kamera deliği ve yuvarlak köşe gibi güvenli alan sınırlarını (Screen.safeArea)
    /// RectTransform üzerine otomatik uygular. Hem tam ekran panellerde hem de köşe butonlarında sorunsuz çalışır.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Header("Fitting Mode")]
        [Tooltip("Uygulama modu: Auto (önerilen), StretchAnchors veya OffsetPosition")]
        [SerializeField] private SafeAreaFitMode fitMode = SafeAreaFitMode.Auto;

        [Header("Safe Area Edges")]
        [Tooltip("Sol kenardaki çentik/kamera boşluğunu uygula")]
        [SerializeField] private bool applyLeft = true;

        [Tooltip("Sağ kenardaki çentik/kamera boşluğunu uygula")]
        [SerializeField] private bool applyRight = true;

        [Tooltip("Üst kenardaki çentik/durum çubuğu boşluğunu uygula")]
        [SerializeField] private bool applyTop = true;

        [Tooltip("Alt kenardaki ev çubuğu (home indicator) boşluğunu uygula")]
        [SerializeField] private bool applyBottom = true;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        // Orijinal pozisyonu sakla (Offset modunda katlanarak kaymayı önlemek için)
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
                // Anchor Modu: Güvenli alan koordinatlarını 0.0 - 1.0 aralığına normalize et
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
                // Offset Modu: Canvas ölçeğine göre piksel farkını hesapla ve içeri ötele
                if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
                float scaleFactor = (rootCanvas != null && rootCanvas.scaleFactor > 0f) ? rootCanvas.scaleFactor : 1f;

                float insetLeft = applyLeft ? (safeArea.xMin / scaleFactor) : 0f;
                float insetRight = applyRight ? ((Screen.width - safeArea.xMax) / scaleFactor) : 0f;
                float insetBottom = applyBottom ? (safeArea.yMin / scaleFactor) : 0f;
                float insetTop = applyTop ? ((Screen.height - safeArea.yMax) / scaleFactor) : 0f;

                Vector2 offset = Vector2.zero;

                // Sol kenara sabitlenmişse içeri (sağa) kaydır
                if (rectTransform.anchorMin.x < 0.5f && rectTransform.anchorMax.x < 0.5f)
                    offset.x += insetLeft;
                // Sağ kenara sabitlenmişse içeri (sola) kaydır
                else if (rectTransform.anchorMin.x > 0.5f && rectTransform.anchorMax.x > 0.5f)
                    offset.x -= insetRight;

                // Alt kenara sabitlenmişse içeri (yukarı) kaydır
                if (rectTransform.anchorMin.y < 0.5f && rectTransform.anchorMax.y < 0.5f)
                    offset.y += insetBottom;
                // Üst kenara sabitlenmişse içeri (aşağı) kaydır
                else if (rectTransform.anchorMin.y > 0.5f && rectTransform.anchorMax.y > 0.5f)
                    offset.y -= insetTop;

                rectTransform.anchoredPosition = initialAnchoredPosition + offset;
            }
        }
    }
}
