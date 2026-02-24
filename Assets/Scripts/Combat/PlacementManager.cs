using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Core;
using TowerDefence.Grid;

namespace TowerDefence.Combat
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        [SerializeField] private LayerMask groundLayer;
        private TowerData selectedTower;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SelectTower(TowerData tower)
        {
            selectedTower = tower;
            Debug.Log($"Tower Selected: {tower.towerName}");
        }

        private void Update()
        {
            if (selectedTower == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTower();
            }
        }

        private void TryPlaceTower()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 gridPos = GridManager.Instance.GetNearestPointOnGrid(hit.point);

                if (GridManager.Instance.IsPlaceable(gridPos))
                {
                    if (CurrencyManager.Instance.TrySpendCurrency(selectedTower.side, selectedTower.cost))
                    {
                        Instantiate(selectedTower.prefab, gridPos, Quaternion.identity);
                        Debug.Log("Tower Placed!");
                        // selectedTower = null; // Sürekli yerleştirme için kapalı tutulabilir
                    }
                }
            }
        }
    }
}
