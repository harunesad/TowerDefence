using UnityEngine;
using TowerDefence.UI;
using TowerDefence.Data;

namespace TowerDefence.Grid
{
    public class TowerSlot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector3 towerOffset = new Vector3(0, 0.5f, 0);
        
        private bool isOccupied = false;
        private GameObject currentTower;
        private TowerSelectionUI localUI;

        public bool IsOccupied => isOccupied;

        private void Start()
        {
            localUI = GetComponentInChildren<TowerSelectionUI>(true);
            if (localUI == null) 
            {
                var selectionGO = transform.Find("Visuals/BaseTowerSelectionPrefab");
                if (selectionGO != null) localUI = selectionGO.GetComponent<TowerSelectionUI>();
            }

            if (localUI == null) Debug.LogWarning($"TowerSlot {gameObject.name}: localUI (TowerSelectionUI) not found in children!");
            else 
            {
                localUI.Hide();
            }
        }

        private float lastClickTime = -1f;

        public void HandleClick()
        {
            if (Time.time - lastClickTime < 0.2f) return;
            lastClickTime = Time.time;

            Debug.Log($"TowerSlot {gameObject.name} clicked. Occupied: {isOccupied}");
            
            // Check current state (toggles if already open)
            bool isSelectionOpen = !isOccupied && localUI != null && localUI.gameObject.activeSelf;
            var upgradeUIComp = isOccupied && currentTower != null 
                ? currentTower.GetComponentInChildren<UI.TowerUpgradeUI>(true) 
                : null;
            bool isUpgradeOpen = upgradeUIComp != null && upgradeUIComp.gameObject.activeSelf;

            if (isSelectionOpen || isUpgradeOpen)
            {
                // If already open, leave close/toggle handling to Show methods
                // (Toggle logic inside Show methods will run)
            }
            else
            {
                // Close all if another slot menu is open
                if (TowerPlacementManager.Instance != null)
                    TowerPlacementManager.Instance.CloseAllTowerUI();
            }

            if (isOccupied)
            {
                // Open Upgrade/Sell menu (GetComponent<Tower> works since BarracksTower derives from Tower)
                Combat.Tower towerComp = currentTower.GetComponent<Combat.Tower>();
                if (TowerPlacementManager.Instance != null && towerComp != null)
                    TowerPlacementManager.Instance.ShowUpgradeUI(this, towerComp);
                return;
            }

            if (localUI == null) return;
            
            // Open internal selection menu
            localUI.ShowForSlot(this);
        }

        public void PlaceTower(GameObject towerPrefab, TowerData data)
        {
            if (isOccupied) return;

            Vector3 spawnPos = transform.position + towerOffset;
            currentTower = Instantiate(towerPrefab, spawnPos, Quaternion.identity);
            
            var towerScript = currentTower.GetComponent<Combat.Tower>();
            if (towerScript != null)
            {
                towerScript.Initialize(data);
                towerScript.SetSlot(this);
                towerScript.SetInvestedGold(data.cost);
            }

            isOccupied = true;
            
            // Close menu after construction
            if (localUI != null) localUI.Hide();

            // Hide slot visual (tower constructed)
            SetSlotVisualsActive(false);
        }

        public void ClearSlot()
        {
            if (currentTower != null) Destroy(currentTower);
            currentTower = null;
            isOccupied = false;

            // Show slot visual again (tower destroyed, can be rebuilt)
            SetSlotVisualsActive(true);
        }

        private void SetSlotVisualsActive(bool active)
        {
            Transform visuals = transform.Find("Visuals");
            if (visuals != null) visuals.gameObject.SetActive(active);
        }

        public void SpecializeTower(TowerData newData, int investedGold = -1)
        {
            if (newData == null || newData.prefab == null) return;

            if (currentTower != null) Destroy(currentTower);

            Vector3 spawnPos = transform.position + towerOffset;
            currentTower = Instantiate(newData.prefab, spawnPos, Quaternion.identity);

            var towerScript = currentTower.GetComponent<Combat.Tower>();
            if (towerScript != null)
            {
                towerScript.Initialize(newData);
                towerScript.SetSlot(this);
                towerScript.SetInvestedGold(investedGold >= 0 ? investedGold : newData.cost);
            }
        }

        public Vector3 GetPlacementPosition() => transform.position + towerOffset;
    }
}
