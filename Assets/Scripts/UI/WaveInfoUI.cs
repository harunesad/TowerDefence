using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Data;
using TowerDefence.Combat;
using System.Collections.Generic;

namespace TowerDefence.UI
{
    public class WaveInfoUI : MonoBehaviour
    {
        [SerializeField] private float panelWidth = 175f;
        [SerializeField] private float panelHeight = 350f;

        private GameObject panel;
        private GameObject content;
        private bool isOpen;
        private GameObject statPopup;
        private UnitData lastPopupUnit;

        private void Start()
        {
            CreateToggleButton();
            CreatePanel();

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.Combat)
                Refresh();
        }

        private void CreateToggleButton()
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -10f);
            rt.sizeDelta = new Vector2(36f, 36f);

            Image bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            bg.raycastTarget = true;

            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(TogglePanel);

            GameObject label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(transform, false);
            RectTransform lrt = label.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(36f, 36f);

            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "W";
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreatePanel()
        {
            panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(transform, false);

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 1f);
            prt.anchorMax = new Vector2(0f, 1f);
            prt.pivot = new Vector2(0f, 1f);
            prt.anchoredPosition = new Vector2(50f, -40f);
            prt.sizeDelta = new Vector2(panelWidth, panelHeight);

            Image pbg = panel.AddComponent<Image>();
            pbg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            pbg.raycastTarget = true;

            GameObject sgo = new GameObject("ScrollRect", typeof(RectTransform));
            sgo.transform.SetParent(panel.transform, false);
            RectTransform srt = sgo.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            ScrollRect sr = sgo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 30f;

            Image maskImg = sgo.AddComponent<Image>();
            maskImg.color = Color.white;
            maskImg.raycastTarget = false;
            Mask mask = sgo.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            sr.viewport = srt;

            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(srt, false);
            content = contentGO;
            RectTransform crt = contentGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(0f, 1f);
            crt.pivot = new Vector2(0f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(panelWidth - 20f, 0f);

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 6f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt;

            panel.SetActive(false);
        }

        public void TogglePanel()
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);
            if (isOpen)
            {
                Refresh();
            }
            else if (statPopup != null)
            {
                Destroy(statPopup);
                statPopup = null;
                lastPopupUnit = null;
            }
        }

        public void Refresh()
        {
            for (int i = content.transform.childCount - 1; i >= 0; i--)
                Destroy(content.transform.GetChild(i).gameObject);

            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level == null || level.waves == null) return;

            int waveIndex = PhaseManager.Instance != null ? PhaseManager.Instance.GetCurrentWaveIndex() : 0;

            AddWaveSection($"Wave {waveIndex + 1} (Current)", waveIndex < level.waves.Count ? level.waves[waveIndex] : null);

            int nextIndex = waveIndex + 1;
            if (nextIndex < level.waves.Count)
                AddWaveSection($"Wave {nextIndex + 1} (Next)", level.waves[nextIndex]);

            RectTransform crt = content.GetComponent<RectTransform>();
            float h = content.GetComponent<VerticalLayoutGroup>().preferredHeight + 20f;
            crt.sizeDelta = new Vector2(crt.sizeDelta.x, Mathf.Max(h, panelHeight - 10f));
        }

        private void AddWaveSection(string header, WaveData wave)
        {
            GameObject headerGO = new GameObject("Header_" + header, typeof(RectTransform));
            headerGO.transform.SetParent(content.transform, false);
            RectTransform hrt = headerGO.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(0f, 24f);

            TextMeshProUGUI htmp = headerGO.AddComponent<TextMeshProUGUI>();
            htmp.text = header;
            htmp.fontSize = 15f;
            htmp.fontStyle = FontStyles.Bold;
            htmp.color = new Color(1f, 0.75f, 0.1f);
            htmp.alignment = TextAlignmentOptions.Left;
            htmp.raycastTarget = false;

            if (wave == null || wave.unitGroups == null) return;

            var merged = new Dictionary<UnitData, int>();
            for (int i = 0; i < wave.unitGroups.Count; i++)
            {
                WaveUnitGroup g = wave.unitGroups[i];
                if (g == null || g.unitData == null) continue;
                if (merged.ContainsKey(g.unitData))
                    merged[g.unitData] += g.count;
                else
                    merged[g.unitData] = g.count;
            }

            GameObject rowGO = new GameObject("UnitsRow", typeof(RectTransform));
            rowGO.transform.SetParent(content.transform, false);
            RectTransform rrt = rowGO.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(0f, 95f);

            LayoutElement rowLe = rowGO.AddComponent<LayoutElement>();
            rowLe.preferredWidth = -1f;
            rowLe.preferredHeight = -1f;
            rowLe.flexibleWidth = 1f;

            GridLayoutGroup glg = rowGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(60f, 95f);
            glg.spacing = new Vector2(6f, 6f);
            glg.padding = new RectOffset(4, 4, 4, 4);
            glg.childAlignment = TextAnchor.UpperLeft;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            foreach (var kvp in merged)
                CreateUnitIcon(rowGO.transform, kvp.Key, kvp.Value);
        }

        private void CreateUnitIcon(Transform parent, UnitData data, int count)
        {
            GameObject iconGO = new GameObject("Unit_" + data.unitName, typeof(RectTransform));
            iconGO.transform.SetParent(parent, false);

            RectTransform rt = iconGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60f, 95f);

            LayoutElement le = iconGO.AddComponent<LayoutElement>();
            le.minWidth = 60f;
            le.minHeight = 95f;
            le.preferredWidth = 60f;
            le.preferredHeight = 95f;

            Image hitImg = iconGO.AddComponent<Image>();
            hitImg.color = new Color(1f, 1f, 1f, 0.02f);
            hitImg.raycastTarget = true;

            Button btn = iconGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = cb;

            UnitData captured = data;
            btn.onClick.AddListener(() => ToggleStatPopup(captured, rt));

            GameObject spriteGO = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
            spriteGO.transform.SetParent(iconGO.transform, false);
            RectTransform srt = spriteGO.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            Image spriteImg = spriteGO.GetComponent<Image>();
            spriteImg.sprite = data.icon;
            spriteImg.preserveAspect = true;
            spriteImg.raycastTarget = false;
            btn.targetGraphic = spriteImg;

            GameObject countGO = new GameObject("Count", typeof(RectTransform));
            countGO.transform.SetParent(iconGO.transform, false);
            RectTransform countRt = countGO.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(1f, 0f);
            countRt.anchorMax = new Vector2(1f, 0f);
            countRt.pivot = new Vector2(1f, 0f);
            countRt.anchoredPosition = new Vector2(2f, 2f);

            TextMeshProUGUI countText = countGO.AddComponent<TextMeshProUGUI>();
            countText.text = $"x{count}";
            countText.fontSize = 13f;
            countText.fontStyle = FontStyles.Bold;
            countText.color = Color.white;
            countText.alignment = TextAlignmentOptions.BottomRight;
            countText.raycastTarget = false;
        }

        private void ToggleStatPopup(UnitData data, RectTransform sourceRt)
        {
            if (data == null) return;

            if (statPopup != null && lastPopupUnit == data)
            {
                Destroy(statPopup);
                statPopup = null;
                lastPopupUnit = null;
                return;
            }

            if (statPopup != null)
            {
                Destroy(statPopup);
                statPopup = null;
            }

            lastPopupUnit = data;

            statPopup = new GameObject("StatPopup", typeof(RectTransform));
            statPopup.transform.SetParent(transform.root, false);

            RectTransform popupRt = statPopup.GetComponent<RectTransform>();
            popupRt.anchorMin = new Vector2(0.5f, 0.5f);
            popupRt.anchorMax = new Vector2(0.5f, 0.5f);
            popupRt.pivot = new Vector2(0.5f, 0.5f);
            popupRt.sizeDelta = new Vector2(220f, 150f);

            float iconH = sourceRt != null ? sourceRt.rect.height : 0f;
            Vector3 popupPos = sourceRt != null
                ? sourceRt.position + new Vector3(0f, iconH * 0.5f + 14f, 0f)
                : transform.position;

            RectTransform canvasRt = transform.root as RectTransform;
            if (canvasRt != null)
            {
                Vector2 canvasSize = canvasRt.rect.size;
                float halfW = popupRt.rect.width * 0.5f;
                float halfH = popupRt.rect.height * 0.5f;

                if (popupPos.y + halfH > canvasRt.position.y + canvasSize.y * 0.5f)
                    popupPos.y = sourceRt.position.y - iconH * 0.5f - 14f;

                popupPos.x = Mathf.Clamp(popupPos.x,
                    canvasRt.position.x - canvasSize.x * 0.5f + halfW,
                    canvasRt.position.x + canvasSize.x * 0.5f - halfW);
            }

            popupRt.position = popupPos;

            Image popupBg = statPopup.AddComponent<Image>();
            popupBg.color = new Color(0.15f, 0.15f, 0.15f, 0.98f);

            Button closeBtn = statPopup.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => { Destroy(statPopup); statPopup = null; });

            float y = -12f;
            float lineH = 24f;

            AddStatText("Title", data.unitName, 16f, FontStyles.Bold, new Vector2(12f, y));
            y -= lineH + 6f;

            AddStatText("Health", $"Health: {data.maxHealth}", 14f, FontStyles.Normal, new Vector2(12f, y));
            y -= lineH;

            AddStatText("Damage", $"Damage: {data.attackDamage}", 14f, FontStyles.Normal, new Vector2(12f, y));
            y -= lineH;

            AddStatText("Speed", $"Speed: {data.moveSpeed}", 14f, FontStyles.Normal, new Vector2(12f, y));
            y -= lineH;

            AddStatText("Range", $"Range: {data.attackRange}", 14f, FontStyles.Normal, new Vector2(12f, y));
        }

        private void AddStatText(string name, string text, float fontSize, FontStyles style, Vector2 offset)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(statPopup.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(200f, 24f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
        }
    }
}
