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
            // Yeni Input Sistemi kontrolü
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool isOverUI = false;
                string uiName = "None";

                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                    
                    if (isOverUI)
                    {
                        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                        pointerData.position = Mouse.current.position.ReadValue();
                        var results = new List<UnityEngine.EventSystems.RaycastResult>();
                        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                        if (results.Count > 0) uiName = results[0].gameObject.name;
                    }
                }
                
                Debug.Log($"[TowerPlacementManager] Click detected! isOverUI: {isOverUI}, Blocking UI: {uiName}");
                
                if (isOverUI) 
                {
                    // Şeffaf ama Raycast Target'ı açık kalmış GameplayHUD_MasterPrefab gibi kapsayıcıları veya metinleri görmezden gel
                    // AYRICA: Kulelerin kendi üzerindeki (World Space) menülerin tıklamayı bloklamasını engelle (Archer_Tower, TowerUpgradeUI vb.)
                    bool isIgnorable = uiName.Contains("MasterPrefab") || 
                                     (uiName.Contains("Panel") && !uiName.Contains("Selection") && !uiName.Contains("Upgrade")) ||
                                     uiName.Contains("Label") || uiName.Contains("Text") || uiName.Contains("TMP") || uiName.Contains("TextMesh") ||
                                     uiName.Contains("_Tower") || uiName.Contains("TowerUpgradeUI") || uiName.Contains("SelectionPrefab") ||
                                     uiName.Contains("BaseTowerSlotPrefab") || uiName.Contains("Background");

                    if (isIgnorable)
                    {
                        Debug.Log($"[TowerPlacementManager] Ignoring UI catch from {uiName} and proceeding to game click.");
                    }
                    else return; 
                }

                HandleMouseClick();
            }
        }

        private void HandleMouseClick()
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, 150f);
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
