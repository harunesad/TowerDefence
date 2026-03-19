using UnityEngine;
using TowerDefence.Core;
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
            if (localUI == null) Debug.LogWarning($"TowerSlot {gameObject.name}: localUI (TowerSelectionUI) not found in children!");
            else localUI.Hide();
        }

        public void HandleClick()
        {
            if (isOccupied || localUI == null) return;
            
            // Başka bir slot menüsü açıksa onu kapat (opsiyonel ama iyi bir pratik)
            if (TowerPlacementManager.Instance != null)
                TowerPlacementManager.Instance.StartPlacementAtSlot(this);

            // Kendi içindeki seçim menüsünü aç
            localUI.ShowForSlot(this);
        }

        public void PlaceTower(GameObject towerPrefab, TowerData data)
        {
            if (isOccupied) return;

            Vector3 spawnPos = transform.position + towerOffset;
            currentTower = Instantiate(towerPrefab, spawnPos, Quaternion.identity);
            
            var towerScript = currentTower.GetComponent<Combat.Tower>();
            if (towerScript != null) towerScript.Initialize(data);

            isOccupied = true;
            
            // İnşa sonrası kendi menüsünü kapat
            if (localUI != null) localUI.Hide();
        }

        public void ClearSlot()
        {
            if (currentTower != null) Destroy(currentTower);
            isOccupied = false;
        }

        public Vector3 GetPlacementPosition() => transform.position + towerOffset;
    }
}
