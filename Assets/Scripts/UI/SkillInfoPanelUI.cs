using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class SkillInfoPanelUI : MonoBehaviour
    {
        private GameObject overlayRef;

        private void OnDestroy()
        {
            if (overlayRef != null) Destroy(overlayRef);
        }

        public void Setup(SkillNodeData data, RectTransform source)
        {
            float panelW = 300;

            overlayRef = new GameObject("SkillInfoOverlay", typeof(RectTransform), typeof(Image));
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

            float iconSize = 64f;
            float y = -10 - iconSize - 8;
            float descLineCount = string.IsNullOrEmpty(data.description) ? 1 : Mathf.Max(1, data.description.Length / 40 + 1);
            float contentH = 24 + 2 + 16 + 2 + descLineCount * 16 + 4 + 16 + 12;
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
            icoImg.sprite = data.icon;
            icoImg.preserveAspect = true;
            icoImg.raycastTarget = false;

            CreateText("NameText", data.skillName, new Vector2(0, y), new Vector2(260, 22),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 18, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));
            y -= 26;

            bool isActive = data.upgradeType == UpgradeType.UnlockSpell;
            string sideStr = data.side.ToString().ToUpper();
            string typeStr = $"{sideStr} {(isActive ? "ACTIVE" : "PASSIVE")}";
            Color typeColor = isActive ? new Color(1f, 0.8f, 0.2f) : Color.white;
            CreateText("TypeText", typeStr, new Vector2(0, y), new Vector2(260, 16),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 12, FontStyles.Bold, TextAlignmentOptions.Center, typeColor);
            y -= 18;

            CreateText("DescText", data.description, new Vector2(0, y), new Vector2(260, descLineCount * 16),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 13, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.8f));
            y -= descLineCount * 16 + 4;

            string costStr;
            if (data.crystalCost > 0 && data.karmaCost == 0)
                costStr = $"{data.crystalCost} Crystals";
            else if (data.crystalCost > 0)
                costStr = $"{data.karmaCost} Karma + {data.crystalCost} Crystals";
            else
                costStr = $"{data.karmaCost} Karma";
            CreateText("CostText", "Cost: " + costStr, new Vector2(0, y), new Vector2(260, 16),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 12, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.6f, 1f, 0.6f));
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
    }
}
