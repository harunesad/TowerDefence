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

            int pathLayerId = LayerMask.NameToLayer("Path");
            int pathLayerMask = pathLayerId >= 0 ? 1 << pathLayerId : 0;
            bool hitSomething = false;

            if (isInRallyMode)
            {
                Debug.Log($"[RALLY] Raycasting with pathLayerMask=0x{pathLayerMask:X8}, pathLayerId={pathLayerId}");
                hitSomething = Physics.Raycast(ray, out hit, 150f, pathLayerMask, QueryTriggerInteraction.Collide);
                Debug.Log($"[RALLY] Raycast result: hitSomething={hitSomething}, point={(hitSomething ? hit.point.ToString() : "N/A")}");
            }
            else
            {
                hitSomething = Physics.Raycast(ray, out hit, 150f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            }
            
            // --- RALLY MODE HANDLING ---
            if (isInRallyMode && currentRallyBarracks != null)
            {
                Debug.Log($"[RALLY-DEBUG] Click! hitSomething={hitSomething}, pathLayerId={pathLayerId}");

                if (hitSomething)
                {
                    GameObject hitGO = hit.collider.gameObject;
                    int hitLayer = hitGO.layer;
                    string hitName = hitGO.name.ToLower();
                    Vector3 targetPoint = hit.point;
                    float dist = Vector3.Distance(currentRallyBarracks.transform.position, targetPoint);

                    bool layerMatch = (pathLayerId != -1 && hitLayer == pathLayerId);
                    bool nameMatch = (hitName.Contains("tile") || hitName.Contains("path"));

                    // HER DURUMDA LOGLA: Neyin üzerine tıkladık ve ne kadar uzaktayız?
                    Debug.Log($"<color=cyan>[RALLY-DEBUG]</color> Hit: <b>{hitGO.name}</b> | Layer: {hitLayer} | Dist: {dist:F2} | LayerMatch: {layerMatch} | NameMatch: {nameMatch}");

                    if (layerMatch || nameMatch)
                    {
                        // MENZİL DIŞINDAYSA: Sınıra çek (Clamp)
                        if (dist > currentRallyBarracks.rallyRadius)
                        {
                            Vector3 direction = (targetPoint - currentRallyBarracks.transform.position).normalized;
                            targetPoint = currentRallyBarracks.transform.position + direction * currentRallyBarracks.rallyRadius;
                            
                            Debug.Log($"<color=cyan>[RALLY-DEBUG]</color> <color=orange>CLAMPED!</color> Original: {dist:F1} -> Target: {currentRallyBarracks.rallyRadius:F1}");
                        }

                        currentRallyBarracks.SetRallyPoint(targetPoint);
                        currentRallyBarracks.SetRangeVisible(false); // İşlem bitince gizle
                        
                        if (VFXManager.Instance != null)
                            VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, targetPoint, Quaternion.identity);

                        isInRallyMode = false;
                        currentRallyBarracks = null;
                        Debug.Log("<color=cyan>[RALLY-DEBUG]</color> <color=green>SUCCESS!</color> Rally Point Set.");
                    }
                    else
                    {
                        Debug.LogWarning($"<color=cyan>[RALLY-DEBUG]</color> Invalid Object! <b>{hitGO.name}</b> is not a Path. Cancelled.");
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
    }
}
