using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TowerDefence.UI
{
    public class UnitSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject unitButtonPrefab;
        [SerializeField] private Transform container;

        private UnitData[] allUnits;

        private void OnEnable()
        {
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged += HandleSideChanged;
            
            InitializeUI();
        }

        private void OnDisable()
        {
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged -= HandleSideChanged;
        }

        private void HandleSideChanged(Side side)
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (SideController.Instance == null) return;
            
            // Konteynerdaki eski butonları temizle
            foreach (Transform child in container)
            {
                if (child != null) Destroy(child.gameObject);
            }

            // Tüm birim verilerini Resources'tan otomatik yükle
            allUnits = Resources.LoadAll<UnitData>("Data/Units");

            Side playerSide = SideController.Instance.GetPlayerSide();

            foreach (UnitData unit in allUnits)
            {
                if (unit.side != playerSide) continue;

                GameObject buttonGO = Instantiate(unitButtonPrefab, container);
                Button button = buttonGO.GetComponent<Button>();
                
                Image icon = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null) icon.sprite = unit.icon;

                // Maliyet metni varsa güncelle (Opsiyonel: prefabda Text eklenirse aktif olur)
                TMPro.TextMeshProUGUI costText = buttonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (costText != null) costText.text = unit.spawnCost.ToString();

                button.onClick.AddListener(() => OnUnitButtonClicked(unit));
            }
        }

        private void OnUnitButtonClicked(UnitData unit)
        {
            if (UnitPlacementManager.Instance != null && CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(unit.side, unit.spawnCost))
            {
                UnitPlacementManager.Instance.StartDragging(unit);
            }
            else
            {
                Debug.Log("Not enough currency or Manager missing!");
            }
        }

        public void RefreshUI()
        {
            InitializeUI();
        }
    }
}
