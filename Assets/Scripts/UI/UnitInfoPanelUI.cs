using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class UnitInfoPanelUI : MonoBehaviour
    {
        private GameObject overlayRef;

        private void OnDestroy()
        {
            if (overlayRef != null) Destroy(overlayRef);
        }

        public void Setup(UnitData data, RectTransform source, bool showCost = true)
        {
            float panelW = 300;
            int statCount = showCost ? 4 : 3;
            float iconArea = 10 + 64 + 8;
            float panelH = iconArea + 14 + 2 + statCount * 20 + 12;

            // Overlay (en üste, panelin arkasında kalacak)
            overlayRef = new GameObject("UnitInfoOverlay", typeof(RectTransform), typeof(Image));
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

            float iconSize = 64f;
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

            float y = -10 - iconSize - 8;
            CreateText("NameText", data.unitName, new Vector2(0, y), new Vector2(260, 22),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 18, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));
            y -= 26;

            CreateStatRow("HealthRow", "Health", data.maxHealth.ToString("0"), new Vector2(0, y), false);
            y -= 20;
            CreateStatRow("AttackRow", "Attack", data.attackDamage.ToString("0"), new Vector2(0, y), false);
            y -= 20;
            CreateStatRow("AtkSpeedRow", "Attack Speed", data.attackRate.ToString("0.00") + "/s", new Vector2(0, y), false);
            if (showCost)
            {
                y -= 20;
                CreateStatRow("CostRow", "Cost", data.spawnCost.ToString(), new Vector2(0, y), true);
            }
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
            rowRT.sizeDelta = new Vector2(260, 18);

            GameObject lGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lGO.transform.SetParent(row.transform, false);
            RectTransform lRT = lGO.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0);
            lRT.anchorMax = new Vector2(1, 1);
            lRT.pivot = new Vector2(0, 0.5f);
            lRT.anchoredPosition = new Vector2(16, 0);
            lRT.sizeDelta = new Vector2(140, 18);
            TextMeshProUGUI lTmp = lGO.GetComponent<TextMeshProUGUI>();
            lTmp.text = label;
            lTmp.fontSize = 14;
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
            vRT.sizeDelta = new Vector2(100, 18);
            TextMeshProUGUI vTmp = vGO.GetComponent<TextMeshProUGUI>();
            vTmp.text = value;
            vTmp.fontSize = 14;
            vTmp.fontStyle = FontStyles.Bold;
            vTmp.alignment = TextAlignmentOptions.Right;
            vTmp.color = goldValue ? new Color(1f, 0.85f, 0.2f) : Color.white;
            vTmp.raycastTarget = false;
        }
    }
}
