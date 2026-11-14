using UnityEngine;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Core;
using TowerDefence.Grid;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace TowerDefence.UI
{
    public class TowerSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject towerButtonPrefab;
        [SerializeField] private Transform buttonRoot;
        [SerializeField, Range(0.5f, 1f)] private float ringRadiusFactor = 0.927f;
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private TextMeshProUGUI statsText;
        private static readonly Vector2 IconCenterOffset = new Vector2(0f, 10f);

        private TowerData previewedTower;

        private static readonly Color AffordableCostColor = new Color(1f, 0.84f, 0.2f, 1f);
        private static readonly Color UnaffordableCostColor = new Color(0.9f, 0.22f, 0.18f, 1f);

        private TowerSlot currentActiveSlot;

        private void Awake()
        {
            gameObject.SetActive(false); // Başlangıçta gizle
        }

        private void OnEnable()
        {
            // Taraf veya altın değiştiğinde menüyü güncelle
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged += HandleSideChanged;
            
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            if (SideController.Instance != null)
                SideController.Instance.OnSideChanged -= HandleSideChanged;

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        private void HandleCurrencyChanged(Side side, int amount)
        {
            if (gameObject.activeSelf) InitializeUI();
        }

        private void HandleSideChanged(Side side)
        {
            if (gameObject.activeSelf) InitializeUI();
        }

        private void InitializeUI()
        {
            if (buttonRoot == null) return;

            for (int i = buttonRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = buttonRoot.GetChild(i);
                if (child.name == "RowUpper" || child.name == "RowLower" || child.name == "StatsPanel") continue;
                Destroy(child.gameObject);
            }

            Side playerSide = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
            
            // Managers check
            List<TowerData> allTowers = TowerPlacementManager.Instance != null ? TowerPlacementManager.Instance.AllTowers : new List<TowerData>();

            // FALLBACK: Eğer manager boşsa her şeyi bul
            if (allTowers == null || allTowers.Count == 0)
            {
                allTowers = new List<TowerData>(Resources.FindObjectsOfTypeAll<TowerData>());
                Debug.LogWarning($"[CORE-UI] Manager empty. Local search found {allTowers.Count} towers.");
            }

            List<TowerData> buildable = new List<TowerData>();
            HashSet<string> addedNames = new HashSet<string>();

            foreach (TowerData tower in allTowers)
            {
                if (tower == null) continue;
                TowerData chosen = null;

                if (tower.side == playerSide) chosen = tower;
                else if (tower.enemyCounterpart != null && tower.enemyCounterpart.side == playerSide) chosen = tower.enemyCounterpart;

                if (chosen != null && chosen.isBaseTower && addedNames.Add(chosen.towerName))
                    buildable.Add(chosen);
            }

            buildable.Sort((a, b) =>
            {
                int order = a.menuOrder.CompareTo(b.menuOrder);
                return order != 0 ? order : string.CompareOrdinal(a.towerName, b.towerName);
            });

            if (buildable.Count == 0)
            {
                Debug.LogWarning($"[CORE-UI] No TOWERS found for side {playerSide}.");
                return;
            }

            float step = 360f / buildable.Count;
            for (int i = 0; i < buildable.Count; i++)
            {
                float angle = (90f - i * step) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * CurrentRingRadius();
                CreateSelectionButton(buildable[i], pos - IconCenterOffset);
            }

            if (previewedTower != null && !buildable.Contains(previewedTower))
                previewedTower = null;

            if (previewedTower != null)
                ShowStats(previewedTower);
            else
                HideStats();

            RefreshSelectionHighlight();
        }

        private float CurrentRingRadius()
        {
            RectTransform canvasRect = transform as RectTransform;
            if (canvasRect == null) return 148f;
            float side = Mathf.Min(canvasRect.rect.width, canvasRect.rect.height);
            if (side <= 1f) side = Mathf.Min(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y);
            return side * 0.5f * ringRadiusFactor;
        }

        private void CreateSelectionButton(TowerData tower, Vector2 anchoredPosition)
        {
            if (buttonRoot == null || towerButtonPrefab == null || tower == null) return;

            GameObject buttonGO = Instantiate(towerButtonPrefab, buttonRoot);
            buttonGO.name = tower.towerName;
            RectTransform rect = buttonGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPosition;
            }

            Button button = buttonGO.GetComponent<Button>();
            bool lessonLock = TutorialDirector.Instance != null && TutorialDirector.Instance.OnlyArcher() && tower.towerName != "Archer Tower";

            Image iconImg = buttonGO.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null)
            {
                iconImg.sprite = tower.icon;
                iconImg.color = lessonLock ? new Color(0.35f, 0.35f, 0.35f, 0.4f) : Color.white;
            }

            Side side = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : tower.side;
            bool canAfford = CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(side, tower.cost);

            TextMeshProUGUI costText = buttonGO.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
            if (costText != null)
            {
                costText.text = tower.cost.ToString();
                costText.color = lessonLock
                    ? new Color(0.35f, 0.35f, 0.35f, 0.4f)
                    : (canAfford ? AffordableCostColor : UnaffordableCostColor);
            }

            if (button != null)
            {
                button.interactable = !lessonLock;
                TowerData chosen = tower;
                button.onClick.AddListener(() => OnTowerButtonClicked(chosen));
            }
        }

        private static float lastShowTime = -1f;

        public void ShowForSlot(TowerSlot slot)
        {
            if (slot == currentActiveSlot && Time.time - lastShowTime < 0.2f) return;
            lastShowTime = Time.time;

            bool lessonOpen = TutorialDirector.Instance != null && TutorialDirector.Instance.OnlyArcher();
            bool closing = gameObject.activeSelf && currentActiveSlot == slot && !lessonOpen;
            if (!closing) previewedTower = null;

            InitializeUI();

            if (closing)
            {
                Hide();
                return;
            }

            currentActiveSlot = slot;
            gameObject.SetActive(true);

            // Standart Ölçek
            transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localPosition = DefaultLocalPosition;
        }

        private static readonly Vector3 DefaultLocalPosition = new Vector3(0f, 15f, -8f);
        private const float ScreenBottom = 0.28f;
        private const float ScreenTop = 0.82f;

        private void LateUpdate()
        {
            if (!isActiveAndEnabled) return;

            transform.localPosition = DefaultLocalPosition;

            Camera cam = Camera.main;
            RectTransform rect = transform as RectTransform;
            if (cam == null || rect == null) return;

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            bool anyVisible = false;
            for (int i = 0; i < 4; i++)
            {
                Vector3 screen = cam.WorldToScreenPoint(corners[i]);
                if (screen.z <= 0f) continue;
                anyVisible = true;
                minX = Mathf.Min(minX, screen.x);
                maxX = Mathf.Max(maxX, screen.x);
                minY = Mathf.Min(minY, screen.y);
                maxY = Mathf.Max(maxY, screen.y);
            }

            if (!anyVisible) return;

            float shiftX = ScreenShift(minX, maxX, 0f, Screen.width);
            float shiftY = ScreenShift(minY, maxY, Screen.height * ScreenBottom, Screen.height * ScreenTop);
            if (shiftX == 0f && shiftY == 0f) return;

            Plane plane = new Plane(Vector3.up, transform.position);
            Vector3 centerScreen = cam.WorldToScreenPoint(transform.position);
            if (centerScreen.z <= 0f) return;

            Ray from = cam.ScreenPointToRay(centerScreen);
            Ray to = cam.ScreenPointToRay(centerScreen + new Vector3(shiftX, shiftY, 0f));
            if (plane.Raycast(from, out float d0) && plane.Raycast(to, out float d1))
                transform.position += to.GetPoint(d1) - from.GetPoint(d0);
        }

        private static float ScreenShift(float min, float max, float safeMin, float safeMax)
        {
            bool below = min < safeMin;
            bool above = max > safeMax;
            if (below && above)
                return (safeMin + safeMax) * 0.5f - (min + max) * 0.5f;
            if (below) return safeMin - min;
            if (above) return safeMax - max;
            return 0f;
        }

        public void Hide()
        {
            previewedTower = null;
            gameObject.SetActive(false);
            currentActiveSlot = null;
        }

        private void OnTowerButtonClicked(TowerData tower)
        {
            if (tower == null || currentActiveSlot == null) return;
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.OnlyArcher() && tower.towerName != "Archer Tower")
                return;

            if (previewedTower == tower)
            {
                Side side = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : tower.side;
                if (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAfford(side, tower.cost))
                    return;

                if (TowerPlacementManager.Instance != null)
                {
                    TowerPlacementManager.Instance.StartPlacementAtSlot(currentActiveSlot);
                    TowerPlacementManager.Instance.SelectTower(tower);
                    Hide();
                }
                return;
            }

            previewedTower = tower;
            ShowStats(tower);
            RefreshSelectionHighlight();
        }

        private void ShowStats(TowerData tower)
        {
            BindStatsPanel();
            if (statsPanel == null || statsText == null || tower == null) return;

            StatIcons.Set(statsText, BuildStatsText(tower));
            statsPanel.SetActive(true);
        }

        private void HideStats()
        {
            if (statsPanel != null) statsPanel.SetActive(false);
        }

        private string BuildStatsText(TowerData data)
        {
            BarracksTower barracks = data.prefab != null ? data.prefab.GetComponent<BarracksTower>() : null;
            if (barracks != null)
                return $"{data.towerName}\n{StatIcons.Health} {data.health:0}\n{StatIcons.Soldiers} {barracks.soldierCount}\n{StatIcons.Time} {barracks.respawnDelay:0}s";

            string text = $"{data.towerName}\n{StatIcons.Damage} {data.damage:0}\n{StatIcons.Range} {data.range:0.#}\n{StatIcons.Attack} {data.fireRate:0.##}/s\n{StatIcons.Health} {data.health:0}";
            if (data.isArtillery && data.explosionRadius > 0f)
                text += $"\n{StatIcons.Area} {data.explosionRadius:0.#}";
            else if (data.isSlowTower)
                text += $"\n{StatIcons.Slow} {data.effectPower * 100f:0}%";
            else if (data.isAuraTower)
                text += $"\n{StatIcons.Aura} +{data.auraDamageBonus * 100f:0}%";
            return text;
        }

        private void RefreshSelectionHighlight()
        {
            if (buttonRoot == null) return;
            foreach (Transform child in buttonRoot)
            {
                if (child.name == "StatsPanel" || child.name == "RowUpper" || child.name == "RowLower") continue;
                Transform apply = child.Find("Apply");
                if (apply == null) continue;
                apply.gameObject.SetActive(previewedTower != null && child.name == previewedTower.towerName);
            }
        }

        private void BindStatsPanel()
        {
            if (statsPanel == null)
            {
                Transform found = transform.Find("StatsPanel");
                if (found != null) statsPanel = found.gameObject;
            }

            if (statsText == null && statsPanel != null)
                statsText = statsPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        private void OnUnitButtonClicked(UnitData unit)
        {
            // Bu metod artık seçim panelinde (popup) kullanılmıyor, üniteler alt barda.
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpendCurrency(unit.side, unit.spawnCost))
            {
                Spawner.SpawnPlayerUnits(unit);
                Hide();
            }
        }
    }
}
