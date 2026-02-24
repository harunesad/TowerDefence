using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Grid;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class TowerSelectionUI : MonoBehaviour
    {
        [SerializeField] private List<TowerData> availableTowers;
        [SerializeField] private GameObject towerButtonPrefab;
        [SerializeField] private Transform container;

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Side playerSide = SideController.Instance.GetPlayerSide();

            foreach (TowerData tower in availableTowers)
            {
                // Taraf kontrolü: Sadece oyuncunun tarafına ait kuleleri göster
                if (tower.side != playerSide) continue;

                GameObject buttonGO = Instantiate(towerButtonPrefab, container);
                Button button = buttonGO.GetComponent<Button>();
                
                // Butonun simgesini ayarla
                Image icon = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null) icon.sprite = tower.icon;

                button.onClick.AddListener(() => OnTowerButtonClicked(tower));
            }
        }

        private void OnTowerButtonClicked(TowerData tower)
        {
            TowerPlacementManager.Instance.SelectTower(tower);
            Debug.Log($"UI: {tower.towerName} seçildi.");
        }
    }
}
