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
        [SerializeField] private float maxPathDetectionDistance = 4f;
        [SerializeField] private List<UnitData> allUnits;
        public List<UnitData> AllUnits => allUnits;
        private GameObject ghostUnit;
        private UnitData currentDraggingUnit;
        private PathWaypoints nearestPath;
        private int targetWaypointIndex = -1;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void StartDragging(UnitData unitData)
        {
            Debug.Log($"[UnitPlacementManager] StartDragging: {unitData.unitName}");
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
            // Sadece UI ve Ignore Raycast katmanlarını yoksay, diğer her şeye çarp (zemin, dekorasyon vb.)
            int layerMask = ~LayerMask.GetMask("UI", "Ignore Raycast");

            if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
            {
                ghostUnit.SetActive(true);
                
                // 1. Spawner bul (oyuncu tarafında olanı)
                bool isPlayer = (currentDraggingUnit.side == Side.Light);
                Spawner correctSpawner = null;
                foreach (var s in Spawner.AllSpawners)
                {
                    if (s.isPlayerSpawner == isPlayer)
                    {
                        correctSpawner = s;
                        break;
                    }
                }

                if (correctSpawner == null && Spawner.AllSpawners.Count > 0)
                    correctSpawner = Spawner.AllSpawners[0];

                nearestPath = null;
                float minPathDist = maxPathDetectionDistance;
                int nearestWpIdx = 0;
                Vector3 snappedPos = hit.point;

                if (correctSpawner != null && correctSpawner.assignedPaths != null)
                {
                    foreach (var path in correctSpawner.assignedPaths)
                    {
                        if (path == null) continue;
                        var wps = path.GetWaypoints();
                        if (wps.Count < 2) continue;

                        for (int i = 0; i < wps.Count - 1; i++)
                        {
                            Vector3 pA = wps[i].position; pA.y = 0;
                            Vector3 pB = wps[i+1].position; pB.y = 0;
                            Vector3 pHit = hit.point; pHit.y = 0;

                            Vector3 closestPointOnSegment = GetClosestPointOnSegment(pHit, pA, pB);
                            float d = Vector3.Distance(pHit, closestPointOnSegment);

                            if (d < minPathDist)
                            {
                                minPathDist = d;
                                nearestPath = path;
                                nearestWpIdx = i;
                                
                                // Orijinal Y eksenini (hit.point.y) koruyarak snap noktasını ayarla
                                snappedPos = new Vector3(closestPointOnSegment.x, hit.point.y, closestPointOnSegment.z);
                            }
                        }
                    }
                }

                if (nearestPath != null)
                {
                    ghostUnit.transform.position = snappedPos;
                    var wps = nearestPath.GetWaypoints();
                    targetWaypointIndex = Mathf.Min(nearestWpIdx + 1, wps.Count - 1);
                    Vector3 lookPos = wps[targetWaypointIndex].position;
                    lookPos.y = snappedPos.y;
                    
                    Vector3 dir = (lookPos - ghostUnit.transform.position);
                    if (dir.sqrMagnitude > 0.001f)
                    {
                        ghostUnit.transform.rotation = Quaternion.LookRotation(dir.normalized);
                    }
                }
                else
                {
                    ghostUnit.SetActive(false);
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
            Debug.Log($"[UnitPlacementManager] StopDragging called. CurrentUnit: {currentDraggingUnit?.unitName}, NearestPath: {nearestPath != null}");
            if (currentDraggingUnit != null && nearestPath != null)
            {
                // Ödeme Yap ve Yerleştir
                if (CurrencyManager.Instance.TrySpendCurrency(currentDraggingUnit.side, currentDraggingUnit.spawnCost))
                {
                    bool isPlayer = (currentDraggingUnit.side == Side.Light);
                    Spawner correctSpawner = null;
                    foreach (var s in Spawner.AllSpawners)
                    {
                        if (s.isPlayerSpawner == isPlayer)
                        {
                            correctSpawner = s;
                            break;
                        }
                    }

                    if (correctSpawner == null && Spawner.AllSpawners.Count > 0)
                        correctSpawner = Spawner.AllSpawners[0];

                    if (correctSpawner != null)
                    {
                        Debug.Log($"[UnitPlacementManager] Spawning unit at {ghostUnit.transform.position} via {correctSpawner.gameObject.name} targeting wp {targetWaypointIndex}");
                        correctSpawner.ManualSpawnAtPosition(currentDraggingUnit, ghostUnit.transform.position, nearestPath, targetWaypointIndex);
                    }
                    else
                    {
                        Debug.LogError("[UnitPlacementManager] No spawners available in the scene!");
                    }
                }
                else
                {
                    Debug.Log("[UnitPlacementManager] Not enough currency at the moment of placement!");
                }
            }
            else
            {
                Debug.Log($"[UnitPlacementManager] Cannot drop. currentUnit={currentDraggingUnit != null}, nearestPath={nearestPath != null}");
            }

            // Temizlik
            if (ghostUnit != null) Destroy(ghostUnit);
            currentDraggingUnit = null;
            nearestPath = null;
            targetWaypointIndex = -1;
        }

        private void UpdateGhostVisuals(bool isValid)
        {
            // İleride buraya malzeme (Material) değiştirme eklenebilir. 
            // Şimdilik sadece debug veya basit renk değişimi.
        }

        private Vector3 GetClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;
            float t = Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab);
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }
    }
}

