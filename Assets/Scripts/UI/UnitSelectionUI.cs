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

        private List<UnitButton> spawnedButtons = new List<UnitButton>();

        private void OnEnable()
        {
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged += HandleSideChanged;
            
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnWaveChanged += HandleWaveChanged;

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
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnWaveChanged -= HandleWaveChanged;
        }

        private void HandleSideChanged(Side side)
        {
            InitializeUI();
        }

        private void HandleWaveChanged(int currentWave, int totalWaves)
        {
            UpdateButtonsInteractability();
        }

        private void UpdateButtonsInteractability()
        {
            if (PhaseManager.Instance == null || CampaignManager.Instance == null || CampaignManager.Instance.GetCurrentLevel() == null) 
                return;

            int totalWaves = CampaignManager.Instance.GetCurrentLevel().waves.Count;
            bool isBossWave = PhaseManager.Instance.GetCurrentWaveIndex() >= totalWaves - 1;

            foreach (var btn in spawnedButtons)
            {
                if (btn == null || btn.UnitData == null) continue;
                
                bool isBoss = SideSelectionUI.BossNames.Contains(btn.UnitData.unitName);
                
                // Normal wavelerde sadece normal unit, boss wavede sadece boss
                bool shouldBeActive = (isBossWave && isBoss) || (!isBossWave && !isBoss);
                btn.SetInteractable(shouldBeActive);
            }
        }

        private void InitializeUI()
        {
            if (SideController.Instance == null) return;
            
            lastSide = SideController.Instance.GetPlayerSide();

            if (container != null)
            {
                foreach (Transform child in container)
                {
                    if (child != null) Object.Destroy(child.gameObject);
                }
            }
            spawnedButtons.Clear();

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
            List<string> equippedUnitNames = MetaProgressionManager.Instance != null ? MetaProgressionManager.Instance.GetEquippedUnitNames() : new List<string>();

            foreach (UnitData unit in unitsPool)
            {
                if (unit == null) continue;
                UnitData finalUnit = null;

                if (unit.side == playerSide && equippedUnitNames.Contains(unit.unitName)) 
                    finalUnit = unit;
                else if (unit.enemyCounterpart != null && unit.enemyCounterpart.side == playerSide && equippedUnitNames.Contains(unit.enemyCounterpart.unitName)) 
                    finalUnit = unit.enemyCounterpart;

                if (finalUnit != null && !addedNames.Contains(finalUnit.unitName))
                {
                    addedNames.Add(finalUnit.unitName);
                    GameObject btnGO = Object.Instantiate(unitButtonPrefab, container);
                    UnitButton ub = btnGO.GetComponent<UnitButton>();
                    if (ub == null) ub = btnGO.AddComponent<UnitButton>();
                    
                    ub.Setup(finalUnit);
                    spawnedButtons.Add(ub);
                }
            }

            if (addedNames.Count == 0)
            {
                Debug.LogWarning("[CORE-UNIT] No units found for side: " + playerSide);
            }

            // Butonları oluşturduktan sonra wave durumuna göre kontrol et
            UpdateButtonsInteractability();
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
