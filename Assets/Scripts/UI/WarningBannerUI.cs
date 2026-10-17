using System.Collections;
using UnityEngine;
using TMPro;

namespace TowerDefence.UI
{
    public class WarningBannerUI : MonoBehaviour
    {
        public static WarningBannerUI Instance { get; private set; }

        private RectTransform bannerRoot;
        private TextMeshProUGUI warningText;
        private CanvasGroup canvasGroup;
        private Coroutine activeRoutine;
        private TMP_FontAsset cinzelFont;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadFont();
            BuildUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LoadFont()
        {
#if UNITY_EDITOR
            cinzelFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Cinzel-Black SDF.asset");
#endif
        }

        private void BuildUI()
        {
            // Canvas veya parent kontrolü
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                parentCanvas = FindObjectOfType<Canvas>();
                if (parentCanvas != null)
                {
                    transform.SetParent(parentCanvas.transform, false);
                }
            }

            RectTransform rootRT = GetComponent<RectTransform>();
            if (rootRT == null) rootRT = gameObject.AddComponent<RectTransform>();

            // Ekranın üst-orta bölgesinde yerleşecek
            rootRT.anchorMin = new Vector2(0.5f, 0.72f);
            rootRT.anchorMax = new Vector2(0.5f, 0.72f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = new Vector2(700f, 60f);
            rootRT.anchoredPosition = Vector2.zero;
            bannerRoot = rootRT;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Metin Objesi
            GameObject textObj = new GameObject("WarningText", typeof(RectTransform));
            textObj.transform.SetParent(transform, false);
            RectTransform trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            trt.anchoredPosition = Vector2.zero;

            warningText = textObj.AddComponent<TextMeshProUGUI>();
            if (cinzelFont != null) warningText.font = cinzelFont;
            warningText.fontSize = 28f;
            warningText.fontStyle = FontStyles.Bold;
            warningText.alignment = TextAlignmentOptions.Center;
            warningText.color = new Color(1f, 0.25f, 0.2f, 1f); // Kırmızı / Uyarı
            warningText.raycastTarget = false;
            warningText.enableWordWrapping = false;
            warningText.outlineColor = new Color32(0, 0, 0, 255);
            warningText.outlineWidth = 0.25f;
        }

        public static void Show(string message, float duration = 2.0f, Color? textColor = null)
        {
            if (Instance == null)
            {
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null) return;

                GameObject go = new GameObject("WarningBannerUI", typeof(RectTransform), typeof(WarningBannerUI));
                go.transform.SetParent(mainCanvas.transform, false);
            }

            if (Instance != null)
            {
                Instance.DisplayWarning(message, duration, textColor);
            }
        }

        private void DisplayWarning(string message, float duration, Color? textColor)
        {
            if (warningText == null) return;

            transform.SetAsLastSibling(); // Her zaman en önde görünsün
            warningText.text = message;
            warningText.color = textColor ?? new Color(1f, 0.3f, 0.25f, 1f);

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }
            activeRoutine = StartCoroutine(AnimateBanner(duration));
        }

        private IEnumerator AnimateBanner(float stayDuration)
        {
            // Fade In & Scale Pop
            float elapsed = 0f;
            float fadeInTime = 0.15f;
            Vector3 startScale = Vector3.one * 0.85f;
            Vector3 targetScale = Vector3.one;

            while (elapsed < fadeInTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInTime);
                canvasGroup.alpha = t;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = targetScale;

            // Bekleme süresi
            yield return new WaitForSecondsRealtime(stayDuration);

            // Fade Out & Yükselme
            elapsed = 0f;
            float fadeOutTime = 0.35f;
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = new Vector2(0f, 25f);

            while (elapsed < fadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutTime);
                canvasGroup.alpha = 1f - t;
                bannerRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            bannerRoot.anchoredPosition = startPos;
            activeRoutine = null;
        }
    }
}
