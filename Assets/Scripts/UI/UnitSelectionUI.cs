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

            // Birim verilerini UnitPlacementManager listesinden al (Resources.LoadAll yerine)
            if (UnitPlacementManager.Instance != null && UnitPlacementManager.Instance.AllUnits != null)
                allUnits = UnitPlacementManager.Instance.AllUnits.ToArray();
            else
                allUnits = new UnitData[0];

            Side playerSide = SideController.Instance.GetPlayerSide();

            foreach (UnitData unit in allUnits)
            {
                if (unit.side != playerSide) continue;

                GameObject buttonGO = Instantiate(unitButtonPrefab, container);
                UnitButton unitBtn = buttonGO.GetComponent<UnitButton>();
                if (unitBtn == null) unitBtn = buttonGO.AddComponent<UnitButton>();
                unitBtn.Setup(unit);
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
