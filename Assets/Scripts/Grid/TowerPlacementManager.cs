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

            // If list is empty, attempt to find all TowerData in project (Advanced Init)
            if (allTowers == null || allTowers.Count == 0)
            {
                allTowers = new List<TowerData>(Resources.FindObjectsOfTypeAll<TowerData>());
            }
        }

        private void Update()
        {
            // In New Input System, best method for mouse and touch: Pointer.current
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

                    if (!isIgnorable)
                        return;
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
        }

        private void HandleMouseClick()
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            int groundMask = LayerMask.GetMask("Default", "Path");
            bool hitSomething = false;

            if (isInRallyMode)
                hitSomething = Physics.Raycast(ray, out hit, 150f, groundMask, QueryTriggerInteraction.Ignore);
            else
                hitSomething = RaycastIgnoringTowerObstacle(ray, out hit);
            
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

                return;
            }



            if (hitSomething)
            {
                // PRIORITY: If a spell is selected, cast spell instead of tower actions
                if (SpellManager.Instance != null && SpellManager.Instance.SelectedSpell != null)
                {
                    if (TutorialDirector.Instance != null && TutorialDirector.Instance.WaitsBossCast())
                    {
                        if (!TutorialDirector.Instance.BossCastHits(hit.point))
                            return;
                        SpellManager.Instance.CastSpell(SpellManager.Instance.SelectedSpell, hit.point);
                        SpellManager.Instance.ClearSelectedSpell();
                        TutorialDirector.Instance.OnBossSpellCast();
                        return;
                    }
                    SpellManager.Instance.CastSpell(SpellManager.Instance.SelectedSpell, hit.point);
                    SpellManager.Instance.ClearSelectedSpell();
                    return;
                }

                TowerSlot slot = hit.collider.GetComponentInParent<TowerSlot>();
                if (slot == null)
                {
                    // Fallback: If tower clicked, retrieve slot via tower
                    Tower tower = hit.collider.GetComponentInParent<Tower>();
                    if (tower != null) slot = tower.GetSlot();
                }

                if (slot != null)
                {
                    if (AudioManager.Instance != null && menuOpenSFX != null)
                        AudioManager.Instance.PlaySFX(menuOpenSFX);
                    slot.HandleClick();
                }
                else if (TutorialDirector.Instance == null || !TutorialDirector.Instance.OnlyArcher())
                {
                    // Close all menus when clicking empty ground or non-slot area
                    CloseAllTowerUI();
                }
            }
            else
            {
                // Close when clicking sky or area without collider
                if (SpellManager.Instance != null && SpellManager.Instance.SelectedSpell != null)
                {
                    SpellManager.Instance.ClearSelectedSpell();
                }
                if (TutorialDirector.Instance == null || !TutorialDirector.Instance.OnlyArcher())
                    CloseAllTowerUI();
            }
        }

        bool RaycastIgnoringTowerObstacle(Ray ray, out RaycastHit chosen)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, 150f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            float nearest = float.MaxValue;
            chosen = default;
            bool found = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider == null || collider.GetComponentInParent<TowerObstacle>() != null) continue;
                if (hits[i].distance >= nearest) continue;
                nearest = hits[i].distance;
                chosen = hits[i];
                found = true;
            }
            return found;
        }

        public void StartPlacementAtSlot(TowerSlot slot)
        {
            // Can close menu of previously active slot
            activeSlot = slot;
        }

        public void SelectTower(TowerData data)
        {
            if (activeSlot == null) return;

            if (CurrencyManager.Instance.TrySpendCurrency(data.side, data.cost))
            {
                Vector3 builtAt = activeSlot.GetPlacementPosition();
                activeSlot.PlaceTower(data.prefab, data);
                CurrencyFloatUI.Show(builtAt, -data.cost, data.side);
                
                if (VFXManager.Instance != null)
                    VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, activeSlot.GetPlacementPosition(), Quaternion.identity);

                if (AudioManager.Instance != null && buildSFX != null)
                    AudioManager.Instance.PlaySFX(buildSFX);

                if (TutorialDirector.Instance != null)
                {
                    string levelId = CampaignManager.Instance != null && CampaignManager.Instance.GetCurrentLevel() != null
                        ? CampaignManager.Instance.GetCurrentLevel().levelID
                        : null;
                    if (levelId == "Level2") TutorialDirector.Instance.Notify("place_tower_2");
                    else TutorialDirector.Instance.Notify("place_tower");
                }

                CloseSelection();
            }
        }

        public void CloseAllTowerUI()
        {
            // Close all tower selection and upgrade menus in scene
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
            // Trigger tower's child UI directly instead of singleton
            TowerUpgradeUI ui = tower.GetComponentInChildren<TowerUpgradeUI>(true);
            if (ui != null) ui.Show(slot, tower);
        }
    }
}
