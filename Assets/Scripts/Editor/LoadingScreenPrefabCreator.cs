using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace TowerDefence.Editor
{
    public class LoadingScreenPrefabCreator
    {
        [MenuItem("Tower Defence/UI/Create Loading Screen Prefab")]
        public static void CreateLoadingScreenPrefab()
        {
            string prefabPath = "Assets/Prefabs/UI/LoadingScreenPrefab.prefab";

            // === Root Canvas ===
            GameObject root = new GameObject("LoadingScreenPrefab");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Her şeyin üstünde

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            // === Loading Panel (CanvasGroup ile fade kontrolü) ===
            GameObject panel = CreateChild(root.transform, "LoadingPanel");
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            StretchFull(panelRT);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.04f, 0.08f, 1f); // Koyu karanlık arka plan

            CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();

            // === Arka Plan Dekoratif Gradient (Orta'dan dışa) ===
            GameObject vignette = CreateChild(panel.transform, "Vignette");
            StretchFull(vignette.GetComponent<RectTransform>());
            Image vignetteImg = vignette.AddComponent<Image>();
            vignetteImg.color = new Color(0.08f, 0.06f, 0.15f, 0.5f);

            // === Başlık Metni ===
            GameObject titleGo = CreateChild(panel.transform, "TitleText");
            RectTransform titleRT = titleGo.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.65f);
            titleRT.anchorMax = new Vector2(0.5f, 0.65f);
            titleRT.sizeDelta = new Vector2(600, 60);
            TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "LOADING...";
            titleText.fontSize = 42;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.9f, 0.8f, 0.5f, 1f); // Altın tonusu
            titleText.fontStyle = FontStyles.Bold;

            // === Spinner (Dönen ikon) ===
            GameObject spinnerGo = CreateChild(panel.transform, "Spinner");
            RectTransform spinnerRT = spinnerGo.GetComponent<RectTransform>();
            spinnerRT.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRT.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRT.sizeDelta = new Vector2(80, 80);
            Image spinnerImg = spinnerGo.AddComponent<Image>();
            // Varsayılan Unity sprite'ı kullan, sonra özel ikon atanabilir
            spinnerImg.color = new Color(0.9f, 0.75f, 0.3f, 0.9f);

            // Spinner içinde çizgi/segment efekti (basit geometrik gösterim)
            // Bu kısım manuel sprite atanabilir. Şimdilik Knob sprite kullan.
            Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (knobSprite != null) spinnerImg.sprite = knobSprite;

            // === Progress Bar Arka Plan ===
            GameObject progressBg = CreateChild(panel.transform, "ProgressBarBG");
            RectTransform progressBgRT = progressBg.GetComponent<RectTransform>();
            progressBgRT.anchorMin = new Vector2(0.25f, 0.38f);
            progressBgRT.anchorMax = new Vector2(0.75f, 0.38f);
            progressBgRT.sizeDelta = new Vector2(0, 24);
            Image progressBgImg = progressBg.AddComponent<Image>();
            progressBgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            progressBgImg.type = Image.Type.Sliced;

            // === Progress Bar Slider ===
            GameObject sliderGo = CreateChild(panel.transform, "ProgressSlider");
            RectTransform sliderRT = sliderGo.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.25f, 0.38f);
            sliderRT.anchorMax = new Vector2(0.75f, 0.38f);
            sliderRT.sizeDelta = new Vector2(0, 24);

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            // Fill Area
            GameObject fillArea = CreateChild(sliderGo.transform, "Fill Area");
            RectTransform fillAreaRT = fillArea.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;

            // Fill
            GameObject fill = CreateChild(fillArea.transform, "Fill");
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.85f, 0.65f, 0.15f, 1f); // Altın dolu çubuk

            slider.fillRect = fillRT;

            // === Progress Yüzde Metni ===
            GameObject progressTextGo = CreateChild(panel.transform, "ProgressText");
            RectTransform progressTextRT = progressTextGo.GetComponent<RectTransform>();
            progressTextRT.anchorMin = new Vector2(0.5f, 0.33f);
            progressTextRT.anchorMax = new Vector2(0.5f, 0.33f);
            progressTextRT.sizeDelta = new Vector2(200, 40);
            TextMeshProUGUI progressTMP = progressTextGo.AddComponent<TextMeshProUGUI>();
            progressTMP.text = "0%";
            progressTMP.fontSize = 28;
            progressTMP.alignment = TextAlignmentOptions.Center;
            progressTMP.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // === Tips Metni (Alt kısım) ===
            GameObject tipsGo = CreateChild(panel.transform, "TipsText");
            RectTransform tipsRT = tipsGo.GetComponent<RectTransform>();
            tipsRT.anchorMin = new Vector2(0.15f, 0.12f);
            tipsRT.anchorMax = new Vector2(0.85f, 0.12f);
            tipsRT.sizeDelta = new Vector2(0, 60);
            TextMeshProUGUI tipsTMP = tipsGo.AddComponent<TextMeshProUGUI>();
            tipsTMP.text = "Tip: Upgrade your towers to deal more damage!";
            tipsTMP.fontSize = 22;
            tipsTMP.fontStyle = FontStyles.Italic;
            tipsTMP.alignment = TextAlignmentOptions.Center;
            tipsTMP.color = new Color(0.6f, 0.6f, 0.7f, 1f);

            // === SceneLoader Bileşenini Ekle ve Referansları Bağla ===
            TowerDefence.Core.SceneLoader loader = root.AddComponent<TowerDefence.Core.SceneLoader>();

            SerializedObject so = new SerializedObject(loader);
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("progressBar").objectReferenceValue = slider;
            so.FindProperty("progressText").objectReferenceValue = progressTMP;
            so.FindProperty("tipsText").objectReferenceValue = tipsTMP;
            so.FindProperty("spinnerIcon").objectReferenceValue = spinnerRT;
            so.ApplyModifiedPropertiesWithoutUndo();

            // === Prefab Olarak Kaydet ===
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[LoadingScreen] Prefab created at: {prefabPath}");
            EditorUtility.DisplayDialog("Loading Screen Prefab", $"Prefab successfully created at:\n{prefabPath}\n\nAdd it to your Bootstrap scene or _Engine_MasterPrefab.", "OK");
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
