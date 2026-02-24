using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;

using TowerDefence.Combat;

namespace TowerDefence.Grid
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private LayerMask groundLayer;
        
        private TowerData selectedTower;
        private GameObject ghostInstance;
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (selectedTower == null) return;

            UpdateGhostPosition();

            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTower();
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }

        public void SelectTower(TowerData data)
        {
            // Eğer zaten bir şey seçiliyse temizle
            CancelPlacement();

            selectedTower = data;
            
            // Preview/Ghost oluştur (Aslında sanat kısmında shader ile yarı şeffaf yapılacak)
            if (selectedTower.prefab != null)
            {
                ghostInstance = Instantiate(selectedTower.prefab);
                // Ghost üzerindeki kule ve collider bileşenlerini yazılımsal olarak devre dışı bırak
                Behaviour[] behaviours = ghostInstance.GetComponentsInChildren<Behaviour>();
                foreach (var b in behaviours)
                {
                    if (!(b is Tower)) b.enabled = false; 
                }
            }
        }

        private void UpdateGhostPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 gridPos = GridManager.Instance.GetNearestPointOnGrid(hit.point);
                ghostInstance.transform.position = gridPos;

                // Geçerli yer mi görsel geri bildirim (Yazılım kısmı: Renk değişimi mantığı)
                bool canPlace = GridManager.Instance.IsPlaceable(gridPos) && 
                                CurrencyManager.Instance.CanAfford(selectedTower.side, selectedTower.cost);
                
                // Burada ghostInstance materyaline müdahale edilebilir (Kırmızı/Yeşil)
            }
        }

        private void TryPlaceTower()
        {
            Vector3 spawnPos = ghostInstance.transform.position;

            if (GridManager.Instance.IsPlaceable(spawnPos))
            {
                if (CurrencyManager.Instance.TrySpendCurrency(selectedTower.side, selectedTower.cost))
                {
                    GameObject towerGO = Instantiate(selectedTower.prefab, spawnPos, Quaternion.identity);
                    Tower tower = towerGO.GetComponent<Tower>();
                    if (tower != null) tower.Initialize(selectedTower);

                    GridManager.Instance.OccupyCell(spawnPos, true);
                    
                    // Ses ve efekt tetikle
                    if (VFXManager.Instance != null)
                        VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, spawnPos, Quaternion.identity); // Build VFX de eklenebilir

                    // Mobile için yerleştirdikten sonra seçimi iptal edebiliriz veya devam edebiliriz.
                    // Şimdilik iptal edelim.
                    CancelPlacement();
                }
                else
                {
                    Debug.Log("Not enough currency!");
                }
            }
            else
            {
                Debug.Log("Area occupied!");
            }
        }

        public void CancelPlacement()
        {
            selectedTower = null;
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
            }
        }
    }
}
