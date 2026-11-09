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
        public AudioClip rallyPointSFX;

        private TowerSlot activeSlot;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (buildSFX == null)
            {
#if UNITY_EDITOR
                buildSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Construction.mp3");
#endif
            }
            if (menuOpenSFX == null)
            {
#if UNITY_EDITOR
                menuOpenSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/MenuOpen.wav");
#endif
            }
            if (rallyPointSFX == null)
            {
#if UNITY_EDITOR
                rallyPointSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/RallyPointPlacement.wav");
#endif
            }

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
                bool isRealUIElement = false;
                var results = new List<UnityEngine.EventSystems.RaycastResult>();

                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                    pointerData.position = Pointer.current.position.ReadValue();
                    pointerData.pointerId = Pointer.current is UnityEngine.InputSystem.Touchscreen
                        ? UnityEngine.InputSystem.Touchscreen.current.primaryTouch.touchId.ReadValue()
                        : -1;
                    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
                    isOverUI = results.Count > 0;
                    if (isOverUI)
                    {
                        uiName = results[0].gameObject.name;
                        isRealUIElement = (results[0].gameObject.layer == LayerMask.NameToLayer("UI")) ||
                                          (results[0].module is UnityEngine.UI.GraphicRaycaster);
                    }
                }
                
                Debug.Log($"[TowerPlacementManager] Click detected! isOverUI: {isOverUI}, Blocking UI: {uiName}, IsRealUI: {isRealUIElement}");
                
                if (isOverUI && isRealUIElement) 
                {
                    GameObject hitObject = results[0].gameObject;
                    if (hitObject.GetComponentInParent<TowerUpgradeUI>() != null || hitObject.GetComponentInParent<TowerSelectionUI>() != null)
                        return;

                    bool isInteractive = results.Count > 0 && hitObject.GetComponentInParent<UnityEngine.UI.Selectable>() != null;
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

            int groundMask = LayerMask.GetMask("Default", "Path");
            bool hitSomething = false;

            if (isInRallyMode)
                hitSomething = Physics.Raycast(ray, out hit, 150f, groundMask, QueryTriggerInteraction.Ignore);
            else
                hitSomething = Physics.Raycast(ray, out hit, 150f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            
            // --- RALLY MODE HANDLING ---
            if (isInRallyMode && currentRallyBarracks != null)
            {
                if (hitSomething)
                {
                    Vector3 clicked = hit.point;
                    Vector3 origin = currentRallyBarracks.transform.position;
                    Vector3 flat = clicked - origin;
                    flat.y = 0f;

                    float radius = currentRallyBarracks.rallyRadius;
                    if (radius > 0.1f && flat.magnitude > radius)
                        clicked = origin + flat.normalized * radius;

                    clicked.y = hit.point.y;
                    currentRallyBarracks.SetRallyPoint(clicked);
                    currentRallyBarracks.SetRangeVisible(false);

                    if (VFXManager.Instance != null)
                        VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, clicked, Quaternion.identity);

                    if (AudioManager.Instance != null && rallyPointSFX != null)
                        AudioManager.Instance.PlaySFX(rallyPointSFX);

                    isInRallyMode = false;
                    currentRallyBarracks = null;
                }
                else
                {
                    Debug.Log("[RALLY] Ground miss. Rally mode stays active.");
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

                if (AudioManager.Instance != null && buildSFX != null)
                    AudioManager.Instance.PlaySFX(buildSFX);

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
