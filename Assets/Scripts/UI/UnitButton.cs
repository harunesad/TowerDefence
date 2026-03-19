using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class UnitButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMPro.TextMeshProUGUI costText;
        
        private UnitData unitData;

        public void Setup(UnitData data)
        {
            unitData = data;
            
            // Bileşenler Inspector'dan atanmamışsa otomatik bulmayı dene
            if (iconImage == null) iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (costText == null) costText = GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (iconImage != null) iconImage.sprite = data.icon;
            if (costText != null) costText.text = data.spawnCost.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Eğer sürükleme yapılmadıysa ve sadece tıklandıysa, varsayılan spawner noktasında doğur
            if (!eventData.dragging)
            {
                if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpendCurrency(unitData.side, unitData.spawnCost))
                {
                    Spawner.SpawnPlayerUnits(unitData);
                    Debug.Log($"Click spawn: {unitData.unitName} at default spawner.");
                }
            }
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
            // Sürükleme işlemi UnitPlacementManager.Update() tarafından mouse takibiyle yapılıyor.
            // Burası event'in tüketilmemesi için boş bırakıldı.
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.StopDragging();
            }
        }
    }
}
