using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class UnitButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMPro.TextMeshProUGUI costText;
        
        private UnitData unitData;
        private UnitInfoPanelUI activePanel;

        private void OnEnable()
        {
        }

        public void Setup(UnitData data)
        {
            unitData = data;
            
            if (iconImage == null) iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (costText == null) costText = GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (iconImage != null) iconImage.sprite = data.icon;
            if (costText != null) costText.text = data.spawnCost.ToString();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (unitData == null) return;

            if (activePanel != null)
            {
                Destroy(activePanel.gameObject);
                activePanel = null;
                return;
            }

            var existing = FindObjectOfType<UnitInfoPanelUI>();
            if (existing != null) Destroy(existing.gameObject);

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject go = new GameObject("UnitInfoPanelUI", typeof(RectTransform), typeof(UnitInfoPanelUI));
            go.transform.SetParent(canvas.transform, false);

            UnitInfoPanelUI panel = go.GetComponent<UnitInfoPanelUI>();
            panel.Setup(unitData, GetComponent<RectTransform>(), false);
            activePanel = panel;

            PositionAboveButton(go.GetComponent<RectTransform>());
        }

        private void PositionAboveButton(RectTransform panelRT)
        {
            Vector3 btnPos = GetComponent<RectTransform>().position;
            float panelH = panelRT.sizeDelta.y;
            panelRT.position = new Vector3(btnPos.x, btnPos.y + panelH * 0.5f + 10f, btnPos.z);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (UnitPlacementManager.Instance != null && CurrencyManager.Instance.CanAfford(unitData.side, unitData.spawnCost))
            {
                UnitPlacementManager.Instance.StartDragging(unitData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.StopDragging();
            }
            if (activePanel != null)
            {
                Destroy(activePanel.gameObject);
                activePanel = null;
            }
        }
    }
}
