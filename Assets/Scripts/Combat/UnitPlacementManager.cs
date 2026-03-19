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
        [SerializeField] private List<UnitData> allUnits;
        public List<UnitData> AllUnits => allUnits;
        
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

                UpdateGhostPosition(); // İlk karede ghost'u mouse'a ışınla
            }
        }

        private void UpdateGhostPosition()
        {
            if (ghostUnit == null) return;

            Vector2 mousePos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
                mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            else if (UnityEngine.InputSystem.Pointer.current != null)
                mousePos = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
#else
            mousePos = Input.mousePosition;
#endif

            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pathLayer))
            {
                ghostUnit.SetActive(true);
                ghostUnit.transform.position = hit.point;
                nearestPath = hit.collider.GetComponentInParent<PathWaypoints>();
                
                // Ghost unit'in yolun gidiş yönüne bakmasını sağla
                if (nearestPath != null && nearestPath.GetWaypoints().Count > 0)
                {
                    var wps = nearestPath.GetWaypoints();
                    float minDistance = float.PositiveInfinity;
                    int nearestIdx = 0;

                    for (int i = 0; i < wps.Count; i++)
                    {
                        float dist = Vector3.Distance(ghostUnit.transform.position, wps[i].position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            nearestIdx = i;
                        }
                    }

                    // Bir sonraki noktayı hedef al (Unit.cs SetPathAtNearestWaypoint mantığıyla aynı)
                    int targetIdx = Mathf.Min(nearestIdx + 1, wps.Count - 1);
                    Vector3 lookPos = wps[targetIdx].position;
                    
                    Vector3 dir = (lookPos - ghostUnit.transform.position);
                    dir.y = 0;
                    if (dir.sqrMagnitude > 0.001f)
                    {
                        ghostUnit.transform.rotation = Quaternion.LookRotation(dir.normalized);
                    }
                }

                UpdateGhostVisuals(nearestPath != null);
            }
            else
            {
                ghostUnit.SetActive(false);
                nearestPath = null;
                UpdateGhostVisuals(false);
            }
        }

        private void Update()
        {
            if (currentDraggingUnit == null || ghostUnit == null) return;
            UpdateGhostPosition();
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
