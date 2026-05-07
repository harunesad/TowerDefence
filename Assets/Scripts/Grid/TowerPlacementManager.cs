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
                bool isOverUI = false;
                string uiName = "None";

                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                    
                    if (isOverUI)
                    {
                        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                        pointerData.position = Pointer.current.position.ReadValue();
                        var results = new List<UnityEngine.EventSystems.RaycastResult>();
                        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                        if (results.Count > 0) uiName = results[0].gameObject.name;
                    }
                }
                
                Debug.Log($"[TowerPlacementManager] Click detected! isOverUI: {isOverUI}, Blocking UI: {uiName}");
                
                if (isOverUI) 
                {
                    // Şeffaf ama Raycast Target'ı açık kalmış GameplayHUD_MasterPrefab gibi kapsayıcıları veya metinleri görmezden gel
                    // AYRICA: Dünyadaki objelerin (tile, path) PhysicsRaycaster yüzünden UI gibi algılanmasını engelle
                    bool isIgnorable = uiName.Contains("MasterPrefab") || 
                                     uiName.Contains("MainPanel") || uiName.Contains("Clone") ||
                                     uiName.Contains("tile") || uiName.Contains("path") || // DÜZELTME: Yolları engel olarak görme!
                                     (uiName.Contains("Panel") && !uiName.Contains("Selection") && !uiName.Contains("Upgrade")) ||
                                     uiName.Contains("Label") || uiName.Contains("Text") || uiName.Contains("TMP") || uiName.Contains("TextMesh") ||
                                     uiName.Contains("_Tower") || uiName.Contains("TowerUpgradeUI") || uiName.Contains("SelectionPrefab") ||
                                     uiName.Contains("BaseTowerSlotPrefab") || uiName.Contains("Background") ||
                                     uiName.Contains("Barracks") || uiName.Contains("Graveyard");

                    if (isIgnorable)
                    {
                        Debug.Log($"[TowerPlacementManager] Ignoring UI catch from {uiName} and proceeding to game click.");
                    }
                    else return; 
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
            barracks.SetRangeVisible(true); // Rally modunda menzili göster
            Debug.Log("[Rally] Entered Rally Mode for " + barracks.gameObject.name);
        }

        private void HandleMouseClick()
        {
            if (Camera.main == null) return;
            Debug.Log("<color=white>[CLICK-TRACE]</color> HandleMouseClick Called!");

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            int pathLayerMask = 1 << LayerMask.NameToLayer("Path");
            bool hitSomething = false;

            if (isInRallyMode)
            {
                // Rally modunda sadece yolu gör (Kuleleri ve üniteleri delip geç)
                hitSomething = Physics.Raycast(ray, out hit, 150f, pathLayerMask, QueryTriggerInteraction.Collide);
            }
            else
            {
                // Normal modda her şeyi gör
                hitSomething = Physics.Raycast(ray, out hit, 150f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            }
            
            // --- RALLY MODE HANDLING ---
            if (isInRallyMode && currentRallyBarracks != null)
            {
                int pathLayer = LayerMask.NameToLayer("Path");
                Debug.Log($"<color=cyan>[RALLY-DEBUG]</color> Click Attempt! Hit Something: {hitSomething}");
                
                if (hitSomething)
                {
                    GameObject hitGO = hit.collider.gameObject;
                    int hitLayer = hitGO.layer;
                    string hitName = hitGO.name.ToLower();
                    Vector3 targetPoint = hit.point;
                    float dist = Vector3.Distance(currentRallyBarracks.transform.position, targetPoint);

                    bool layerMatch = (pathLayer != -1 && hitLayer == pathLayer);
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
