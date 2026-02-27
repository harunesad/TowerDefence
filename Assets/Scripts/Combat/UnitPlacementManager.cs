using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Core;
using System.Collections.Generic;

namespace TowerDefence.Combat
{
    public class UnitPlacementManager : MonoBehaviour
    {
        public static UnitPlacementManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private LayerMask pathLayer;
        [SerializeField] private float maxPathDetectionDistance = 5f;
        
        private GameObject ghostUnit;
        private UnitData currentDraggingUnit;
        private PathWaypoints nearestPath;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void StartDragging(UnitData unitData)
        {
            currentDraggingUnit = unitData;
            
            // Ghost unit oluştur (Birim prefab'ının bir kopyası ama yarı saydam shader verilbilir)
            if (unitData.prefab != null)
            {
                ghostUnit = Instantiate(unitData.prefab);
                // NavMeshAgent'ı kapat ki sürüklerken hareket etmeye çalışmasın
                var agent = ghostUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.enabled = false;
                
                // Çarpışmaları kapat
                var colliders = ghostUnit.GetComponentsInChildren<Collider>();
                foreach (var col in colliders) col.enabled = false;
            }
        }

        private void Update()
        {
            if (currentDraggingUnit == null || ghostUnit == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pathLayer))
            {
                ghostUnit.SetActive(true);
                ghostUnit.transform.position = hit.point;
                
                // Çarpılan nesneden veya üst objesinden yolu al
                nearestPath = hit.collider.GetComponentInParent<PathWaypoints>();
                UpdateGhostVisuals(nearestPath != null);
            }
            else
            {
                // Yol dışındayken ghost unit'i gizle veya kırmızı yap
                ghostUnit.SetActive(false);
                nearestPath = null;
                UpdateGhostVisuals(false);
            }

            // Mouse bırakılınca yerleştir
            if (Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }

        public void StopDragging()
        {
            if (currentDraggingUnit != null && nearestPath != null)
            {
                // Ödeme Yap ve Yerleştir
                if (CurrencyManager.Instance.TrySpendCurrency(currentDraggingUnit.side, currentDraggingUnit.spawnCost))
                {
                    if (Spawner.AllSpawners.Count > 0)
                    {
                        Spawner.AllSpawners[0].ManualSpawnAtPosition(currentDraggingUnit, ghostUnit.transform.position, nearestPath);
                    }
                }
                else
                {
                    Debug.Log("UnitPlacementManager: Not enough currency at the moment of placement!");
                }
            }

            // Temizlik
            if (ghostUnit != null) Destroy(ghostUnit);
            currentDraggingUnit = null;
            nearestPath = null;
        }

        private void UpdateGhostVisuals(bool isValid)
        {
            // İleride buraya malzeme (Material) değiştirme eklenebilir. 
            // Şimdilik sadece debug veya basit renk değişimi.
        }
    }
}
