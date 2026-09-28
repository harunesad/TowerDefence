using UnityEngine;
using TowerDefence.Core;
using TowerDefence.Data;
using System.Collections.Generic;
using UnityEngine.InputSystem;

using TowerDefence.Combat;
using TowerDefence.UI;

namespace TowerDefence.Grid
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [SerializeField] private List<TowerData> allTowers;
        public List<TowerData> AllTowers => allTowers;

        [Header("Audio")]
        public AudioClip buildSFX;
        public AudioClip menuOpenSFX;

        private TowerSlot activeSlot;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Eğer liste boşsa, Projedeki tüm TowerData'ları bulmaya çalış (Gelişmiş Başlatma)
            if (allTowers == null || allTowers.Count == 0)
            {
                allTowers = new List<TowerData>(Resources.FindObjectsOfTypeAll<TowerData>());
                Debug.Log($"[TOWER-Placement] Fallback LOAD! Discovering towers: {allTowers.Count}");
            }
            else
            {
                Debug.Log($"[TOWER-Placement] Pre-Loaded READY! Towers in list: {allTowers.Count}");
            }
        }

        private void Update()
        {
            // Yeni Input Sistemi'nde hem fare hem dokunmatik için en iyi yöntem: Pointer.current
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                // Rally modunda UI check'ini bypass et, doğrudan HandleMouseClick'e git
                if (isInRallyMode && currentRallyBarracks != null)
                {
                    Debug.Log($"[RALLY] Bypassing UI check in rally mode, calling HandleMouseClick");
                    HandleMouseClick();
                    return;
                }

                bool isOverUI = false;
                string uiName = "None";
                bool isRealUIElement = false;
                var results = new List<UnityEngine.EventSystems.RaycastResult>();

                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                    
                    if (isOverUI)
                    {
                        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                        pointerData.position = Pointer.current.position.ReadValue();
                        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                        if (results.Count > 0)
                        {
                            uiName = results[0].gameObject.name;
                            isRealUIElement = (results[0].gameObject.layer == LayerMask.NameToLayer("UI")) ||
                                              (results[0].module is UnityEngine.UI.GraphicRaycaster);
                        }
                    }
                }
                
                Debug.Log($"[TowerPlacementManager] Click detected! isOverUI: {isOverUI}, Blocking UI: {uiName}, IsRealUI: {isRealUIElement}");
                
                if (isOverUI && isRealUIElement) 
                {
                    bool isInteractive = results.Count > 0 && results[0].gameObject.GetComponentInParent<UnityEngine.UI.Selectable>() != null;
                    bool isIgnorable = false;

                    if (!isInteractive)
                    {
                        isIgnorable = uiName.Contains("MasterPrefab") || 
                                     uiName.Contains("MainPanel") ||
                                     (uiName.Contains("Panel") && !uiName.Contains("Selection") && !uiName.Contains("Upgrade")) ||
                                     uiName.Contains("Label") || uiName.Contains("Text") || uiName.Contains("TMP") || uiName.Contains("TextMesh") ||
                                     uiName.Contains("Background");
                    }

                    if (isIgnorable)
                    {
                        Debug.Log($"[TowerPlacementManager] Ignoring UI catch from {uiName} and proceeding to game click.");
                    }
                    else
                    {
                        Debug.Log($"[RALLY] Click blocked by UI: {uiName}, interactive={isInteractive}. Rally mode active: {isInRallyMode}");
                        return; 
                    }
                }

                if (isInRallyMode)
                {
                    Debug.Log($"[RALLY] Click passed UI check, calling HandleMouseClick. isOverUI={isOverUI}, isRealUIElement={isRealUIElement}");
                }

                HandleMouseClick();
            }
        }

        private BarracksTower currentRallyBarracks;
        private bool isInRallyMode = false;

        public void EnterRallyPlacementMode(BarracksTower barracks)
        {
            isInRallyMode = true;
            currentRallyBarracks = barracks;
            barracks.SetRangeVisible(true);
            Debug.Log($"[RALLY] Entered Rally Mode for {barracks.gameObject.name} at pos {barracks.transform.position}");
            Debug.Log($"[RALLY] isInRallyMode={isInRallyMode}, currentRallyBarracks={currentRallyBarracks?.gameObject.name}");
        }

        private void HandleMouseClick()
        {
            if (Camera.main == null) return;
            Debug.Log("<color=white>[CLICK-TRACE]</color> HandleMouseClick Called!");

            Vector2 mousePos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            int pathLayerMask = LayerMask.GetMask("Path");
            bool hitSomething = false;

            if (isInRallyMode)
            {
                hitSomething = Physics.Raycast(ray, out hit, 150f, pathLayerMask, QueryTriggerInteraction.Collide);
            }
            else
            {
                hitSomething = Physics.Raycast(ray, out hit, 150f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            }
            
            // --- RALLY MODE HANDLING ---
            if (isInRallyMode && currentRallyBarracks != null)
            {
                if (hitSomething)
                {
                    Vector3 clickedPoint = hit.point;
                    clickedPoint.y = 0; // Zemin düzlemi

                    // En yakın yol noktasını matematiksel olarak bul (Uzaklık Sınırı Yok)
                    PathWaypoints[] allPaths = FindObjectsByType<PathWaypoints>(FindObjectsSortMode.None);
                    float minPathDist = float.MaxValue; 
                    Vector3 snappedPos = hit.point;
                    bool pathFound = false;

                    foreach (var path in allPaths)
                    {
                        var wps = path.GetWaypoints();
                        if (wps.Count < 2) continue;

                        for (int i = 0; i < wps.Count - 1; i++)
                        {
                            Vector3 pA = wps[i].position; pA.y = 0;
                            Vector3 pB = wps[i+1].position; pB.y = 0;

                            Vector3 closestPoint = GetClosestPointOnSegment(clickedPoint, pA, pB);
                            float d = Vector3.Distance(clickedPoint, closestPoint);

                            if (d < minPathDist)
                            {
                                minPathDist = d;
                                snappedPos = new Vector3(closestPoint.x, hit.point.y, closestPoint.z);
                                pathFound = true;
                            }
                        }
                    }

                    if (pathFound)
                    {
                        float distToBarracks = Vector3.Distance(currentRallyBarracks.transform.position, snappedPos);

                        // MENZİL DIŞINDAYSA: Sınıra çek (Clamp)
                        if (distToBarracks > currentRallyBarracks.rallyRadius)
                        {
                            Vector3 direction = (snappedPos - currentRallyBarracks.transform.position).normalized;
                            snappedPos = currentRallyBarracks.transform.position + direction * currentRallyBarracks.rallyRadius;
                        }

                        currentRallyBarracks.SetRallyPoint(snappedPos);
                        currentRallyBarracks.SetRangeVisible(false); // İşlem bitince gizle
                        
                        if (VFXManager.Instance != null)
                            VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, snappedPos, Quaternion.identity);

                        isInRallyMode = false;
                        currentRallyBarracks = null;
                        Debug.Log("<color=cyan>[RALLY-DEBUG]</color> <color=green>SUCCESS!</color> Rally Point Set at mathematically snapped pos.");
                    }
                    else
                    {
                        Debug.LogWarning($"<color=cyan>[RALLY-DEBUG]</color> Clicked too far from any path line (>{minPathDist} units). Cancelled.");
                        if (currentRallyBarracks != null) currentRallyBarracks.SetRangeVisible(false);
                        isInRallyMode = false;
                        currentRallyBarracks = null;
                    }
                }
                else
                {
                    Debug.LogWarning("<color=cyan>[RALLY-DEBUG]</color> Raycast hit nothing! Cancelled.");
                    if (currentRallyBarracks != null) currentRallyBarracks.SetRangeVisible(false);
                    isInRallyMode = false;
                    currentRallyBarracks = null;
                }

                return;
            }


            Debug.Log($"[TowerPlacementManager] Click detected on: {(hitSomething ? hit.collider.gameObject.name : "Nothing")}");

            if (hitSomething)
            {
                // ÖNCELİK: Eğer bir büyü seçiliyse kule işlemlerini yapma, büyüyü at
                if (SpellManager.Instance != null && SpellManager.Instance.SelectedSpell != null)
                {
                    SpellManager.Instance.CastSpell(SpellManager.Instance.SelectedSpell, hit.point);
                    SpellManager.Instance.ClearSelectedSpell();
                    return; // İşlemi bitir
                }

                TowerSlot slot = hit.collider.GetComponentInParent<TowerSlot>();
                if (slot == null)
                {
                    // Fallback: Eğer kuleye tıklandıysa kule üzerinden slotu al
                    Tower tower = hit.collider.GetComponentInParent<Tower>();
                    if (tower != null) slot = tower.GetSlot();
                }

                if (slot != null)
                {
                    if (AudioManager.Instance != null && menuOpenSFX != null)
                        AudioManager.Instance.PlaySFX(menuOpenSFX);
                    slot.HandleClick();
                }
                else
                {
                    // Boş bir yere veya slot olmayan bir yere tıklandığında tüm menüleri kapat
                    CloseAllTowerUI();
                }
            }
            else
            {
                // Gökyüzüne veya collider olmayan bir yere tıklandığında da kapat
                if (SpellManager.Instance != null && SpellManager.Instance.SelectedSpell != null)
                {
                    SpellManager.Instance.ClearSelectedSpell();
                }
                CloseAllTowerUI();
            }
        }

        public void StartPlacementAtSlot(TowerSlot slot)
        {
            // Eğer daha önce başka bir slot aktifse onun menüsünü kapatabilirsin
            activeSlot = slot;
        }

        public void SelectTower(TowerData data)
        {
            if (activeSlot == null) return;

            if (CurrencyManager.Instance.TrySpendCurrency(data.side, data.cost))
            {
                activeSlot.PlaceTower(data.prefab, data);
                
                if (VFXManager.Instance != null)
                    VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, activeSlot.GetPlacementPosition(), Quaternion.identity);

                CloseSelection();
            }
            else
            {
                Debug.Log("Not enough currency!");
            }
        }

        public void CloseAllTowerUI()
        {
            // Sahnedeki tüm kule seçim ve yükseltme menülerini kapat
            var selections = FindObjectsByType<UI.TowerSelectionUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var s in selections) s.Hide();

            var upgrades = FindObjectsByType<UI.TowerUpgradeUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var u in upgrades) u.Hide();
        }

        public void CloseSelection()
        {
            activeSlot = null;
        }

        public void ShowUpgradeUI(TowerSlot slot, Combat.Tower tower)
        {
            // Singleton yerine kulenin kendi içindeki (çocuk) UI'ı tetikle
            TowerUpgradeUI ui = tower.GetComponentInChildren<TowerUpgradeUI>(true);
            if (ui != null) ui.Show(slot, tower);
        }
        private Vector3 GetClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
            return a + Mathf.Clamp01(t) * ab;
        }
    }
}
