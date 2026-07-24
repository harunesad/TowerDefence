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

                // Unit bileşenini kapat ki sürükleme esnasında saldırmasın/hedef aramasın
                var unitComp = ghostUnit.GetComponent<Unit>();
                if (unitComp != null) unitComp.enabled = false;

                // Sağlık barını da gizle
                var healthBar = ghostUnit.GetComponentInChildren<TowerDefence.UI.HealthBarUI>();
                if (healthBar != null) healthBar.gameObject.SetActive(false);

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
                
                // Tüm PathWaypoints'leri tara (spawner bağımsız)
                bool isPlayer = (currentDraggingUnit.side == SideController.Instance.GetPlayerSide());
                PathWaypoints[] allPaths = FindObjectsByType<PathWaypoints>(FindObjectsSortMode.None);

                nearestPath = null;
                float minPathDist = maxPathDetectionDistance;
                int nearestWpIdx = 0;
                Vector3 snappedPos = hit.point;

                foreach (var path in allPaths)
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
                            snappedPos = new Vector3(closestPointOnSegment.x, hit.point.y, closestPointOnSegment.z);
                        }
                    }
                }

                if (nearestPath != null)
                {
                    var wps = nearestPath.GetWaypoints();
                    if (isPlayer)
                    {
                        // Player unitleri path'te geriye doğru yürüsün (wp[son] → wp[0] = düşmana doğru)
                        targetWaypointIndex = Mathf.Max(nearestWpIdx - 1, 0);
                    }
                    else
                    {
                        targetWaypointIndex = Mathf.Min(nearestWpIdx + 1, wps.Count - 1);
                    }
                    Debug.Log($"[DROP-DEBUG] nearestPath={nearestPath.name}, nearestWpIdx={nearestWpIdx}, targetWpIdx={targetWaypointIndex}, wpCount={wps.Count}");
                    Debug.Log($"[DROP-DEBUG] wp[0]={wps[0].position}, wp[{wps.Count-1}]={wps[wps.Count-1].position}, snappedPos={snappedPos}");

                    ghostUnit.transform.position = snappedPos;
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
                if (CurrencyManager.Instance.TrySpendCurrency(currentDraggingUnit.side, currentDraggingUnit.spawnCost))
                {
                    bool isPlayer = (currentDraggingUnit.side == SideController.Instance.GetPlayerSide());
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
                        var wps = nearestPath.GetWaypoints();
                        Debug.Log($"[DROP-DEBUG] SPAWNING: spawner={correctSpawner.gameObject.name}, isPlayerSpawner={correctSpawner.isPlayerSpawner}");
                        Debug.Log($"[DROP-DEBUG] path={nearestPath.name}, wpCount={wps.Count}, wp[0]={wps[0].position}, wp[{wps.Count-1}]={wps[wps.Count-1].position}");
                        Debug.Log($"[DROP-DEBUG] targetWpIdx={targetWaypointIndex}, ghostPos={ghostUnit.transform.position}, walkBackward={isPlayer}");
                        correctSpawner.ManualSpawnAtPosition(currentDraggingUnit, ghostUnit.transform.position, nearestPath, targetWaypointIndex, isPlayer);
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

