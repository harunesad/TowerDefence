using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class HeroInfoPanelUI : MonoBehaviour
    {
        private GameObject overlayRef;

        private void OnDestroy()
        {
            if (overlayRef != null) Destroy(overlayRef);
        }

        public void Setup(HeroData data, RectTransform source)
        {
            float panelW = 320;

            // Overlay
            overlayRef = new GameObject("HeroInfoOverlay", typeof(RectTransform), typeof(Image));
            overlayRef.transform.SetParent(transform.parent, false);
            overlayRef.transform.SetAsLastSibling();
            transform.SetAsLastSibling();
            RectTransform oRT = overlayRef.GetComponent<RectTransform>();
            oRT.anchorMin = Vector2.zero;
            oRT.anchorMax = Vector2.one;
            oRT.sizeDelta = Vector2.zero;
            Image oImg = overlayRef.GetComponent<Image>();
            oImg.color = Color.clear;
            oImg.raycastTarget = true;
            var et = overlayRef.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((_) => { Destroy(overlayRef); Destroy(gameObject); });
            et.triggers.Add(entry);

            // Panel
            int level = MetaProgressionManager.Instance != null
                ? MetaProgressionManager.Instance.GetHeroLevel(data.heroID) : 1;

            float iconSize = 64f;
            float y = -10 - iconSize - 8;
            float contentH = 24 + 2; // name + its bottom gap
            contentH += 5 * 18 + 4; // 5 stat rows
            contentH += 2 + 18 + 28; // sep2 + ability name + ability desc
            float panelH = Mathf.Abs(y) + contentH + 12;

            RectTransform rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(panelW, panelH);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Vector3 btnPos = source.position;
            rt.position = new Vector3(btnPos.x, btnPos.y + panelH * 0.5f + 10f, btnPos.z);

            Image bgImg = gameObject.GetComponent<Image>();
            if (bgImg == null) bgImg = gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.06f, 0.1f, 0.95f);
            bgImg.raycastTarget = true;

            var pTrigger = gameObject.AddComponent<EventTrigger>();
            var pEntry = new EventTrigger.Entry();
            pEntry.eventID = EventTriggerType.PointerClick;
            pEntry.callback.AddListener((_) => { Destroy(overlayRef); Destroy(gameObject); });
            pTrigger.triggers.Add(pEntry);

            CreateImage("Border", Vector2.zero, Vector2.zero, Vector2.one, Vector2.zero, new Color(0.2f, 0.2f, 0.3f, 1f), false);
            CreateImage("InnerBg", Vector2.zero, Vector2.zero, Vector2.one, new Vector2(-4, -4), new Color(0.08f, 0.08f, 0.13f, 1f), false);

            GameObject icoBg = new GameObject("IconBg", typeof(RectTransform), typeof(Image));
            icoBg.transform.SetParent(transform, false);
            RectTransform icoBgRT = icoBg.GetComponent<RectTransform>();
            icoBgRT.anchorMin = new Vector2(0.5f, 1f);
            icoBgRT.anchorMax = new Vector2(0.5f, 1f);
            icoBgRT.pivot = new Vector2(0.5f, 1f);
            icoBgRT.anchoredPosition = new Vector2(0, -10);
            icoBgRT.sizeDelta = new Vector2(iconSize, iconSize);
            Image icoBgImg = icoBg.GetComponent<Image>();
            icoBgImg.color = new Color(0.12f, 0.12f, 0.18f, 1f);
            icoBgImg.raycastTarget = false;

            GameObject icoGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icoGO.transform.SetParent(icoBg.transform, false);
            RectTransform icoRT = icoGO.GetComponent<RectTransform>();
            icoRT.anchorMin = Vector2.zero;
            icoRT.anchorMax = Vector2.one;
            icoRT.offsetMin = Vector2.zero;
            icoRT.offsetMax = Vector2.zero;
            Image icoImg = icoGO.GetComponent<Image>();
            icoImg.sprite = data.GetIcon();
            icoImg.preserveAspect = true;
            icoImg.raycastTarget = false;

            CreateText("NameText", $"{data.displayName}  <size=15>Lv.{level}</size>",
                new Vector2(0, y), new Vector2(290, 22),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                18, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));
            y -= 26;

            MetaProgressionManager.Instance.GetHeroCombatStats(data,
                out float hp, out float dmg, out float spd, out float rng, out float rate);

            CreateStatRow("HealthRow", "Health", hp.ToString("0"), new Vector2(0, y), false);
            y -= 18;
            CreateStatRow("DamageRow", "Damage", dmg.ToString("0"), new Vector2(0, y), false);
            y -= 18;
            CreateStatRow("SpeedRow", "Speed", spd.ToString("0.00"), new Vector2(0, y), false);
            y -= 18;
            CreateStatRow("RangeRow", "Range", rng.ToString("0.0"), new Vector2(0, y), false);
            y -= 18;
            CreateStatRow("AtkRateRow", "Attack Rate", rate.ToString("0.00") + "/s", new Vector2(0, y), false);

            y -= 14;
            CreateImage("Sep2", new Vector2(0, y), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(panelW - 40, 1), new Color(0.3f, 0.3f, 0.4f, 0.6f), false);

            y -= 14;
            CreateText("AbilityNameText", data.abilityName,
                new Vector2(0, y), new Vector2(290, 18),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                14, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.6f, 0.8f, 1f));

            y -= 22;
            CreateText("AbilityDescText", $"{data.abilityDescription}  (CD: {data.abilityCooldown}s)",
                new Vector2(0, y), new Vector2(290, 28),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                12, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.8f));
        }

        private GameObject CreateImage(string name, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Color color, bool raycastTarget)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
            return go;
        }

        private void CreateText(string name, string text, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles style, TextAlignmentOptions align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
        }

        private void CreateStatRow(string name, string label, string value, Vector2 offset, bool goldValue)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(transform, false);
            RectTransform rowRT = row.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 1f);
            rowRT.anchorMax = new Vector2(0.5f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = offset;
            rowRT.sizeDelta = new Vector2(290, 16);

            GameObject lGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lGO.transform.SetParent(row.transform, false);
            RectTransform lRT = lGO.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(1, 1);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(16, 0);
            lRT.sizeDelta = new Vector2(160, 16);
            TextMeshProUGUI lTmp = lGO.GetComponent<TextMeshProUGUI>();
            lTmp.text = label;
            lTmp.fontSize = 13;
            lTmp.alignment = TextAlignmentOptions.Left;
            lTmp.color = new Color(0.7f, 0.7f, 0.8f);
            lTmp.raycastTarget = false;

            GameObject vGO = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            vGO.transform.SetParent(row.transform, false);
            RectTransform vRT = vGO.GetComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0, 0);
            vRT.anchorMax = new Vector2(1, 1);
            vRT.pivot = new Vector2(1, 0.5f);
            vRT.anchoredPosition = new Vector2(-16, 0);
            vRT.sizeDelta = new Vector2(120, 16);
            TextMeshProUGUI vTmp = vGO.GetComponent<TextMeshProUGUI>();
            vTmp.text = value;
            vTmp.fontSize = 13;
            vTmp.fontStyle = FontStyles.Bold;
            vTmp.alignment = TextAlignmentOptions.Right;
            vTmp.color = goldValue ? new Color(1f, 0.85f, 0.2f) : Color.white;
            vTmp.raycastTarget = false;
        }
    }
}
