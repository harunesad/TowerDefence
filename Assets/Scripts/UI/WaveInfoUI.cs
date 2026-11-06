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
        [SerializeField] private GameObject unitsRowPrefab;
        [SerializeField] private AudioClip toggleSFX;

        private GameObject blocker;
        private GameObject panel;
        private GameObject content;
        private bool isOpen;
        private GameObject statPopup;
        private UnitData lastPopupUnit;

        public void SetUnitsRowPrefab(GameObject prefab)
        {
            unitsRowPrefab = prefab;
        }

        private void Start()
        {
            if (unitsRowPrefab == null)
            {
#if UNITY_EDITOR
                unitsRowPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WaveInfoUnitsRow.prefab");
#endif
            }
            if (toggleSFX == null)
            {
#if UNITY_EDITOR
                toggleSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/WaveInfo.wav");
#endif
            }
            CreateToggleButton();
            CreatePanel();

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;

            if (blocker != null)
                Destroy(blocker);
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
            rt.anchoredPosition = new Vector2(100f, -30f);
            rt.sizeDelta = new Vector2(60f, 60f);

            Image bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            bg.raycastTarget = true;

            Button btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(TogglePanel);

            GameObject label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(transform, false);
            RectTransform lrt = label.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(60f, 60f);

            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "W";
            tmp.fontSize = 30f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreatePanel()
        {
            // Arka plan engelleyici (dışarı tıklandığında paneli kapatır)
            Canvas canvas = GetComponentInParent<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : transform.parent;

            blocker = new GameObject("WaveInfoBlocker", typeof(RectTransform));
            blocker.transform.SetParent(canvasTransform, false);
            RectTransform brt = blocker.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;

            Image bImg = blocker.AddComponent<Image>();
            bImg.color = new Color(0, 0, 0, 0.001f); // Görünmez ama dış tıklamaları yakalar
            bImg.raycastTarget = true;

            Button bBtn = blocker.AddComponent<Button>();
            bBtn.transition = Selectable.Transition.None;
            bBtn.onClick.AddListener(ClosePanel);

            blocker.SetActive(false);

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
            SetPanelOpen(!isOpen);
        }

        public void ClosePanel()
        {
            SetPanelOpen(false);
        }

        public void SetPanelOpen(bool open)
        {
            if (isOpen == open) return;
            isOpen = open;

            if (blocker != null)
            {
                if (isOpen)
                {
                    // Blocker'ı panelin arkasında ama diğer UI elemanlarının önünde olacak şekilde sırala
                    blocker.transform.SetSiblingIndex(Mathf.Max(0, transform.GetSiblingIndex()));
                }
                blocker.SetActive(isOpen);
            }

            if (panel != null)
            {
                panel.SetActive(isOpen);
                if (isOpen)
                {
                    transform.SetAsLastSibling();
                }
            }

            if (AudioManager.Instance != null && toggleSFX != null)
            {
                AudioManager.Instance.PlaySFX(toggleSFX);
            }

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

            GameObject rowGO;
            if (unitsRowPrefab != null)
            {
                rowGO = Instantiate(unitsRowPrefab, content.transform, false);
                rowGO.name = "UnitsRow";
                // Prefab içindeki şablon/placeholder çocukları temizle
                for (int i = rowGO.transform.childCount - 1; i >= 0; i--)
                    Destroy(rowGO.transform.GetChild(i).gameObject);
            }
            else
            {
                rowGO = new GameObject("UnitsRow", typeof(RectTransform));
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
            }

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
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = Vector2.zero;
            srt.sizeDelta = new Vector2(60f, 96f);
            Image spriteImg = spriteGO.GetComponent<Image>();
            spriteImg.sprite = data.icon;
            spriteImg.preserveAspect = true;
            spriteImg.raycastTarget = false;
            btn.targetGraphic = spriteImg;

            // UnitData'daki bağımsız Wave Info ayarlarını uygula
            data.waveInfoIcon.ApplyTo(spriteImg);

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

            Canvas canvas = GetComponentInParent<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : transform.parent;

            statPopup = new GameObject("StatPopup", typeof(RectTransform));
            statPopup.transform.SetParent(canvasTransform, false);
            statPopup.transform.SetAsLastSibling();

            RectTransform popupRt = statPopup.GetComponent<RectTransform>();
            popupRt.anchorMin = new Vector2(0.5f, 0.5f);
            popupRt.anchorMax = new Vector2(0.5f, 0.5f);
            popupRt.pivot = new Vector2(0.5f, 0.5f);
            popupRt.sizeDelta = new Vector2(250f, 180f);

            float iconH = sourceRt != null ? sourceRt.rect.height : 0f;
            Vector3 popupPos = sourceRt != null
                ? sourceRt.position + new Vector3(0f, iconH * 0.5f + 14f, 0f)
                : transform.position;

            RectTransform canvasRt = canvasTransform as RectTransform;
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
            popupBg.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);
            popupBg.raycastTarget = true;

            // Panelin içine tıklanınca kapanmasını engelle
            Button popupBlocker = statPopup.AddComponent<Button>();
            popupBlocker.transition = Selectable.Transition.None;

            // Zorluk katsayılarını hesapla (1: Normal, 2: Hard (+50%), 3: Expert (+100%))
            int diffLevel = CampaignManager.Instance != null ? CampaignManager.Instance.CurrentDifficultyLevel : 1;
            string diffName = "Normal";
            float diffMult = 1f;
            if (diffLevel == 2)
            {
                diffName = "Hard";
                diffMult = 1.5f;
            }
            else if (diffLevel >= 3)
            {
                diffName = "Expert";
                diffMult = 2.0f;
            }

            float finalHealth = data.maxHealth * diffMult;
            float finalDamage = data.attackDamage * diffMult;
            float finalSpeed = data.moveSpeed;
            float finalRange = data.attackRange;

            float y = -12f;
            float lineH = 24f;

            AddStatText("Title", data.unitName, 16f, FontStyles.Bold, new Vector2(12f, y), Color.white);
            y -= lineH;

            Color diffColor = diffLevel == 1 ? new Color(0.4f, 0.85f, 0.4f) : (diffLevel == 2 ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.35f, 0.35f));
            AddStatText("Difficulty", $"Difficulty: {diffName} (x{diffMult:F1})", 13f, FontStyles.Bold, new Vector2(12f, y), diffColor);
            y -= lineH + 4f;

            AddStatText("Health", $"Health: {Mathf.RoundToInt(finalHealth)}", 14f, FontStyles.Normal, new Vector2(12f, y), Color.white);
            y -= lineH;

            AddStatText("Damage", $"Damage: {Mathf.RoundToInt(finalDamage)}", 14f, FontStyles.Normal, new Vector2(12f, y), Color.white);
            y -= lineH;

            AddStatText("Speed", $"Speed: {finalSpeed:F1}", 14f, FontStyles.Normal, new Vector2(12f, y), Color.white);
            y -= lineH;

            AddStatText("Range", $"Range: {finalRange:F1}", 14f, FontStyles.Normal, new Vector2(12f, y), Color.white);
        }

        private void AddStatText(string name, string text, float fontSize, FontStyles style, Vector2 offset, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(statPopup.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(230f, 24f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;
        }
    }
}
