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
            
            // Mevcut durumu kontrol et (Zaten açıksa toggle yapacak)
            bool isSelectionOpen = !isOccupied && localUI != null && localUI.gameObject.activeSelf;
            var upgradeUIComp = isOccupied && currentTower != null 
                ? currentTower.GetComponentInChildren<UI.TowerUpgradeUI>(true) 
                : null;
            bool isUpgradeOpen = upgradeUIComp != null && upgradeUIComp.gameObject.activeSelf;

            if (isSelectionOpen || isUpgradeOpen)
            {
                // Zaten açıksa, sadece bu slota özel kapatma/toggle işlemini Show metodlarına bırak
                // (Show metodları içerisindeki toggle mantığı çalışacak)
            }
            else
            {
                // Başka bir slot menüsü açıksa hepsini kapat
                if (TowerPlacementManager.Instance != null)
                    TowerPlacementManager.Instance.CloseAllTowerUI();
            }

            if (isOccupied)
            {
                // Yükseltme/Satış menüsünü aç (BarracksTower da Tower'dan türediği için GetComponent<Tower> çalışır)
                Combat.Tower towerComp = currentTower.GetComponent<Combat.Tower>();
                if (TowerPlacementManager.Instance != null && towerComp != null)
                    TowerPlacementManager.Instance.ShowUpgradeUI(this, towerComp);
                return;
            }

            if (localUI == null) return;
            
            // Kendi içindeki seçim menüsünü aç
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
            }

            isOccupied = true;
            
            // İnşa sonrası kendi menüsünü kapat
            if (localUI != null) localUI.Hide();

            // Slot görselini gizle (kule inşa edildi)
            SetSlotVisualsActive(false);

            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.buildSFX != null)
                AudioManager.Instance.PlaySFX(TowerPlacementManager.Instance.buildSFX);
        }

        public void ClearSlot()
        {
            if (currentTower != null) Destroy(currentTower);
            currentTower = null;
            isOccupied = false;

            // Slot görselini tekrar göster (kule yok edildi, yeniden inşa edilebilir)
            SetSlotVisualsActive(true);
        }

        private void SetSlotVisualsActive(bool active)
        {
            Transform visuals = transform.Find("Visuals");
            if (visuals != null) visuals.gameObject.SetActive(active);
        }

        public void SpecializeTower(TowerData newData)
        {
            if (newData == null || newData.prefab == null) return;

            // Mevcut kuleyi temizle
            if (currentTower != null) Destroy(currentTower);
            
            // Yeni kuleyi aynı konuma yerleştir
            Vector3 spawnPos = transform.position + towerOffset;
            currentTower = Instantiate(newData.prefab, spawnPos, Quaternion.identity);
            
            var towerScript = currentTower.GetComponent<Combat.Tower>();
            if (towerScript != null)
            {
                towerScript.Initialize(newData);
                towerScript.SetSlot(this);
            }
        }

        public Vector3 GetPlacementPosition() => transform.position + towerOffset;
    }
}
