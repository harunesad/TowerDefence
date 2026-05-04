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

        [SerializeField] private UnitData[] allUnits;
        private Side lastSide;

        private void Update()
        {
            if (SideController.Instance != null && SideController.Instance.GetPlayerSide() != lastSide)
            {
                Debug.Log("[UNIT-FORCE-REFRESH] Side change detected in Update! " + lastSide + " -> " + SideController.Instance.GetPlayerSide());
                InitializeUI();
            }
        }

        private void OnEnable()
        {
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged += HandleSideChanged;
            
            StopAllCoroutines();
            StartCoroutine(DeferredInit());
        }

        private System.Collections.IEnumerator DeferredInit()
        {
            // Bekle ki SideController vs iyice yerleşsin
            yield return new WaitForSeconds(0.1f);
            InitializeUI();
            
            // Eğer hala (yanlışlıkla) aydınlık gelmişse veya boşsa birkaç kez daha dene
            for (int i = 0; i < 3; i++)
            {
                if (lastSide == Side.Light && SideController.Instance != null && SideController.Instance.GetPlayerSide() != Side.Light)
                {
                    InitializeUI();
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
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
            
            lastSide = SideController.Instance.GetPlayerSide();

            // Konteynerdaki eski butonları temizle
            if (container != null)
            {
                foreach (Transform child in container)
                {
                    if (child != null) Object.Destroy(child.gameObject);
                }
            }

            // Birim verilerini al (Agresif bulma fallback ile)
            List<UnitData> unitsPool = new List<UnitData>();
            if (UnitPlacementManager.Instance != null && UnitPlacementManager.Instance.AllUnits != null && UnitPlacementManager.Instance.AllUnits.Count > 0)
            {
                unitsPool = UnitPlacementManager.Instance.AllUnits;
            }
            else
            {
                unitsPool = new List<UnitData>(Resources.FindObjectsOfTypeAll<UnitData>());
            }

            Side playerSide = lastSide;
            Debug.Log($"[CORE-UNIT] Refreshing bottom bar. Side: {playerSide}. Units: {unitsPool.Count}");

            HashSet<string> addedNames = new HashSet<string>();
            foreach (UnitData unit in unitsPool)
            {
                if (unit == null) continue;
                UnitData finalUnit = null;

                if (unit.side == playerSide) finalUnit = unit;
                else if (unit.enemyCounterpart != null && unit.enemyCounterpart.side == playerSide) finalUnit = unit.enemyCounterpart;

                if (finalUnit != null && !addedNames.Contains(finalUnit.unitName))
                {
                    if (unitButtonPrefab != null && container != null)
                    {
                        GameObject buttonGO = Instantiate(unitButtonPrefab, container);
                        UnitButton unitBtn = buttonGO.GetComponent<UnitButton>();
                        if (unitBtn == null) unitBtn = buttonGO.AddComponent<UnitButton>();
                        unitBtn.Setup(finalUnit);
                        addedNames.Add(finalUnit.unitName);
                    }
                }
            }

            if (addedNames.Count == 0)
            {
                Debug.LogWarning("[CORE-UNIT] No units found for side: " + playerSide);
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
