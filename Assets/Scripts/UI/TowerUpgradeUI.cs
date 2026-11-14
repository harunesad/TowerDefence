using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Combat;
using TowerDefence.Data;
using TowerDefence.Core;
using TowerDefence.Grid;

namespace TowerDefence.UI
{
    public class TowerUpgradeUI : MonoBehaviour
    {
        private enum RingAction
        {
            None,
            Info,
            Upgrade,
            SpecA,
            SpecB,
            Sell,
            Priority
        }

        [Header("References")]
        public GameObject mainPanel;
        public TextMeshProUGUI towerNameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI statsText;

        [Header("Stats Panel")]
        public GameObject statsPanel;
        
        [Header("Normal Upgrade")]
        public GameObject normalUpgradeGroup;
        public Button upgradeButton;
        public TextMeshProUGUI upgradeCostText;

        [Header("Specialization")]
        public GameObject specializationGroup;
        public Button specAButton;
        public Button specBButton;
        public TextMeshProUGUI specACostText;
        public TextMeshProUGUI specBCostText;
        public Image specAIcon;
        public Image specBIcon;

        [Header("Sell")]
        public Button sellButton;
        public TextMeshProUGUI sellValueText;

        [Header("Targeting")]
        public Button priorityButton;
        public TextMeshProUGUI priorityText;

        [Header("Ring")]
        [SerializeField] private GameObject actionButtonPrefab;
        [SerializeField] private Sprite ringSprite;
        [SerializeField] private Sprite infoIcon;
        [SerializeField] private Sprite upgradeIcon;
        [SerializeField] private Sprite sellIcon;
        [SerializeField] private Sprite priorityFirstIcon;
        [SerializeField] private Sprite priorityLastIcon;
        [SerializeField] private Sprite priorityStrongIcon;
        [SerializeField] private Sprite priorityWeakIcon;
        [SerializeField] private Sprite moveIcon;
        [SerializeField, Range(0.5f, 1f)] private float ringRadiusFactor = 0.927f;

        [Header("Audio")]
        [SerializeField] private AudioClip upgradeSFX;
        [SerializeField] private AudioClip sellSFX;

        private const float RingSide = 540f;
        private static readonly Vector2 IconCenterOffset = new Vector2(0f, 10f);
        private static readonly Vector3 DefaultLocalPosition = new Vector3(0f, 15f, -5f);
        private const float ScreenBottom = 0.28f;
        private const float ScreenTop = 0.82f;
        private static readonly Color AffordableCostColor = new Color(1f, 0.84f, 0.2f, 1f);
        private static readonly Color UnaffordableCostColor = new Color(0.9f, 0.22f, 0.18f, 1f);
        private static readonly Color LabelColor = Color.white;

        private TowerSlot currentSlot;
        private Tower currentTower;
        private RingAction selectedAction = RingAction.None;
        private bool infoVisible;
        private static float lastShowTime = -1f;

        private void Awake()
        {
            if (upgradeSFX == null)
            {
#if UNITY_EDITOR
                upgradeSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/TowerUpgrade.wav");
#endif
            }
            if (sellSFX == null)
            {
#if UNITY_EDITOR
                sellSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/TowerSell.wav");
#endif
            }
        }

        private void OnEnable()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        private void HandleCurrencyChanged(Side side, int amount)
        {
            if (mainPanel != null && mainPanel.activeSelf && currentTower != null)
                RefreshUI();
        }

        private void Start()
        {
            Graphic panelGraphic = mainPanel != null ? mainPanel.GetComponent<Graphic>() : null;
            if (panelGraphic != null) panelGraphic.raycastTarget = false;

            if (upgradeButton != null)
            {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            if (specAButton != null)
            {
            specAButton.onClick.RemoveAllListeners();
            specAButton.onClick.AddListener(OnSpecAClicked);
            }

            if (specBButton != null)
            {
            specBButton.onClick.RemoveAllListeners();
            specBButton.onClick.AddListener(OnSpecBClicked);
            }

            if (sellButton != null)
            {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
            }

            if (priorityButton != null)
            {
                priorityButton.onClick.RemoveAllListeners();
                priorityButton.onClick.AddListener(OnPriorityClicked);
            }

            FitRing();
            HideLegacyChrome();
            if (statsPanel != null) statsPanel.SetActive(false);
            if (specializationGroup != null) specializationGroup.SetActive(false);
        }

        public void Show(TowerSlot slot, Tower tower)
        {
            if (slot == currentSlot && Time.time - lastShowTime < 0.2f) return;
            lastShowTime = Time.time;

            if (currentSlot == slot && mainPanel != null && mainPanel.activeSelf)
            {
                Hide();
                return;
            }

            currentSlot = slot;
            currentTower = tower;
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.Notify("tower_menu");
            selectedAction = RingAction.None;
            infoVisible = false;
            mainPanel.SetActive(true);

            if (currentTower != null)
            currentTower.SetRangeVisible(true);

            transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localPosition = DefaultLocalPosition;

            FitRing();
            HideLegacyChrome();
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (currentTower == null) return;

            EnsureRingButtons();
            RefreshRingButtons();

            if (selectedAction != RingAction.None)
                ShowActionInfo(selectedAction);
            else if (infoVisible)
                ShowActionInfo(RingAction.Info);
            else
                HideActionInfo();

            RefreshHighlight();
        }

        public void OnUpgradeClicked()
        {
            if (currentTower == null || currentTower.CurrentLevel >= 3) return;
            TowerData data = currentTower.GetTowerData();
            if (data == null || CurrencyManager.Instance == null) return;

            if (CurrencyManager.Instance.TrySpendCurrency(data.side, data.upgradeCost))
            {
                if (!currentTower.Upgrade())
                {
                    CurrencyManager.Instance.AddCurrency(data.side, data.upgradeCost);
                    return;
                }
                if (currentSlot != null)
                    CurrencyFloatUI.Show(currentSlot.GetPlacementPosition(), -data.upgradeCost, data.side);
                currentTower.AddInvestedGold(data.upgradeCost);
                currentTower.SetRangeVisible(true);

                if (VFXManager.Instance != null && currentSlot != null)
                    VFXManager.Instance.SpawnVFX(VFXType.UpgradeSparkle, currentSlot.GetPlacementPosition(), Quaternion.identity);

                if (AudioManager.Instance != null && upgradeSFX != null)
                    AudioManager.Instance.PlaySFX(upgradeSFX);

                selectedAction = RingAction.None;
                infoVisible = false;
                RefreshUI();
            }
        }

        public void OnSpecAClicked() => Specialize(0);
        public void OnSpecBClicked() => Specialize(1);

        private void Specialize(int index)
        {
            if (currentTower == null || currentSlot == null) return;
            var specs = currentTower.GetSpecializations();
            if (specs == null || index >= specs.Count) return;

            TowerData specData = specs[index];
            if (specData == null || CurrencyManager.Instance == null) return;
            if (!CurrencyManager.Instance.TrySpendCurrency(specData.side, specData.cost))
                return;

            CurrencyFloatUI.Show(currentSlot.GetPlacementPosition(), -specData.cost, specData.side);
            int invested = currentTower.GetInvestedGold() + specData.cost;
            currentSlot.SpecializeTower(specData, invested);
                
                if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.UpgradeSparkle, currentSlot.GetPlacementPosition(), Quaternion.identity);

                if (AudioManager.Instance != null && upgradeSFX != null)
                    AudioManager.Instance.PlaySFX(upgradeSFX);

                Hide();
        }

        public void OnSellClicked()
        {
            if (BlocksSell()) return;
            if (currentTower == null || currentSlot == null) return;
            TowerData data = currentTower.GetTowerData();
            int refund = currentTower.GetSellRefund();
            if (data != null && CurrencyManager.Instance != null)
                CurrencyManager.Instance.AddCurrency(data.side, refund);
            if (currentSlot != null && data != null)
                CurrencyFloatUI.Show(currentSlot.GetPlacementPosition(), refund, data.side);
            if (AudioManager.Instance != null && sellSFX != null)
                AudioManager.Instance.PlaySFX(sellSFX);
            currentSlot.ClearSlot();
            Hide();
        }

        public void OnPriorityClicked()
        {
            if (currentTower == null) return;

            if (currentTower is BarracksTower barracks)
            {
                if (TowerPlacementManager.Instance != null)
                {
                    BarracksTower tempBarracks = barracks;
                    Hide(); 
                    TowerPlacementManager.Instance.EnterRallyPlacementMode(tempBarracks);
                }
            }
            else
            {
                currentTower.CycleTargetingPriority();
                RefreshUI();
            }
        }

        public void Hide()
        {
            if (currentTower != null) currentTower.SetRangeVisible(false);
            
            selectedAction = RingAction.None;
            infoVisible = false;
            if (statsPanel != null) statsPanel.SetActive(false);
            if (specializationGroup != null) specializationGroup.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(false);
            currentSlot = null;
            currentTower = null;
        }

        bool BlocksSell()
        {
            if (CampaignManager.Instance == null || CampaignManager.Instance.CurrentDifficultyLevel != 1) return false;
            LevelData level = CampaignManager.Instance.GetCurrentLevel();
            return level != null && level.levelID == "Level1";
        }

        public RectTransform LessonButton(string actionName)
        {
            if (mainPanel == null || !mainPanel.activeInHierarchy || string.IsNullOrEmpty(actionName)) return null;
            Transform root = mainPanel.transform.Find("RingButtons");
            if (root == null) return null;
            return root.Find(actionName) as RectTransform;
        }

        private void LateUpdate()
        {
            if (mainPanel == null || !mainPanel.activeSelf) return;

            Transform sell = FindRingButton(RingAction.Sell);
            if (sell != null)
            {
                bool allowSell = !BlocksSell()
                    && (TutorialDirector.Instance == null || TutorialDirector.Instance.Current() != "tower_menu");
                Button sellButtonOnRing = sell.GetComponent<Button>();
                if (sellButtonOnRing != null) sellButtonOnRing.interactable = allowSell;
            }

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

        private void FitRing()
        {
            RectTransform canvas = transform as RectTransform;
            if (canvas != null)
                canvas.sizeDelta = new Vector2(RingSide, RingSide);

            if (mainPanel == null) return;

            RectTransform panel = mainPanel.GetComponent<RectTransform>();
            if (panel != null)
            {
                panel.localScale = Vector3.one;
                panel.anchorMin = Vector2.zero;
                panel.anchorMax = Vector2.one;
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;
                panel.pivot = new Vector2(0.5f, 0.5f);
            }

            Image image = mainPanel.GetComponent<Image>();
            if (image != null)
            {
                if (ringSprite != null) image.sprite = ringSprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.raycastTarget = false;
            }

        }

        private void HideLegacyChrome()
        {
            if (mainPanel == null) return;
            foreach (Transform child in mainPanel.transform)
            {
                if (child.name == "StatsPanel" || child.name == "RingButtons")
                    continue;
                child.gameObject.SetActive(false);
            }
        }

        private void EnsureRingButtons()
        {
            Transform root = RingRoot();
            if (root == null || actionButtonPrefab == null) return;

            bool specs = HasSpecs();
            if (RingIsCurrent(root, specs))
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
                DestroyImmediate(root.GetChild(i).gameObject);

            CreateRingButton(RingAction.Info);
            if (specs)
            {
                CreateRingButton(RingAction.SpecA);
                CreateRingButton(RingAction.SpecB);
            }
            else
                CreateRingButton(RingAction.Upgrade);
            CreateRingButton(RingAction.Sell);
            CreateRingButton(RingAction.Priority);
            LayoutRingButtons();

            if (selectedAction != RingAction.None && FindRingButton(selectedAction) == null)
                selectedAction = RingAction.None;
        }

        private static bool RingIsCurrent(Transform root, bool specs)
        {
            if (specs)
                return root.childCount == 5
                    && root.Find("SpecA") != null
                    && root.Find("SpecB") != null
                    && root.Find("Upgrade") == null;
            return root.childCount == 4 && root.Find("Upgrade") != null && root.Find("SpecA") == null;
        }

        private Transform RingRoot()
        {
            if (mainPanel == null) return null;
            Transform existing = mainPanel.transform.Find("RingButtons");
            if (existing != null) return existing;

            GameObject go = new GameObject("RingButtons", typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(mainPanel.transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.SetAsLastSibling();
            return rect;
        }

        private void CreateRingButton(RingAction action)
        {
            Transform root = RingRoot();
            if (root == null || actionButtonPrefab == null) return;

            GameObject buttonGO = Instantiate(actionButtonPrefab, root);
            buttonGO.name = action.ToString();
            RectTransform rect = buttonGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }

            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                RingAction chosen = action;
                button.interactable = true;
                button.onClick.AddListener(() => OnRingAction(chosen));
            }

            Transform apply = buttonGO.transform.Find("Apply");
            if (apply != null) apply.gameObject.SetActive(false);
        }

        private void LayoutRingButtons()
        {
            Transform root = mainPanel != null ? mainPanel.transform.Find("RingButtons") : null;
            if (root == null || root.childCount == 0) return;

            float step = 360f / root.childCount;
            float radius = RingSide * 0.5f * ringRadiusFactor;
            for (int i = 0; i < root.childCount; i++)
            {
                float angle = (90f - i * step) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius - IconCenterOffset;
                RectTransform rect = root.GetChild(i) as RectTransform;
                if (rect == null) continue;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = pos;
            }
        }

        private void RefreshRingButtons()
        {
            if (currentTower == null) return;
            TowerData data = currentTower.GetTowerData();
            int level = currentTower.CurrentLevel;

            SetButtonVisual(RingAction.Info, infoIcon, "", false, false);

            if (HasSpecs())
            {
                var specs = currentTower.GetSpecializations();
                SetButtonVisual(RingAction.SpecA, specs[0].icon, specs[0].cost.ToString(), CanAfford(specs[0].side, specs[0].cost), true);
                SetButtonVisual(RingAction.SpecB, specs[1].icon, specs[1].cost.ToString(), CanAfford(specs[1].side, specs[1].cost), true);
            }
            else if (level < 3 && data != null)
                SetButtonVisual(RingAction.Upgrade, upgradeIcon, data.upgradeCost.ToString(), CanAfford(data.side, data.upgradeCost), true);
            else
                SetButtonVisual(RingAction.Upgrade, upgradeIcon, "", true, false);

            SetButtonVisual(RingAction.Sell, sellIcon, currentTower.GetSellRefund().ToString(), true, true);

            if (currentTower is BarracksTower)
                SetButtonVisual(RingAction.Priority, moveIcon, "Move", true, false);
            else
            {
                TargetingPriority mode = currentTower.GetTargetingPriority();
                SetButtonVisual(RingAction.Priority, PriorityIcon(mode), PriorityLabel(mode), true, false);
            }
        }

        private void SetButtonVisual(RingAction action, Sprite icon, string label, bool affordable, bool isPrice)
        {
            Transform button = FindRingButton(action);
            if (button == null) return;

            Image iconImg = button.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null)
            {
                iconImg.sprite = icon;
                iconImg.enabled = icon != null;
                iconImg.color = Color.white;
            }

            TextMeshProUGUI cost = button.Find("Cost")?.GetComponent<TextMeshProUGUI>();
            if (cost != null)
            {
                cost.text = label ?? "";
                cost.color = isPrice ? (affordable ? AffordableCostColor : UnaffordableCostColor) : LabelColor;
            }

            Button uiButton = button.GetComponent<Button>();
            if (uiButton != null) uiButton.interactable = true;
        }

        private void OnRingAction(RingAction action)
        {
            if (currentTower == null) return;
            if (action == RingAction.Sell && BlocksSell()) return;

            if (action == RingAction.Info)
            {
                selectedAction = RingAction.None;
                infoVisible = true;
                ShowActionInfo(RingAction.Info);
                RefreshHighlight();
                return;
            }

            if (action == RingAction.Priority)
            {
                if (currentTower is BarracksTower)
                {
                    selectedAction = RingAction.None;
                    infoVisible = false;
                    HideActionInfo();
                    OnPriorityClicked();
                    return;
                }

                if (selectedAction == RingAction.Priority)
                {
                    selectedAction = RingAction.None;
                    HideActionInfo();
                }
                currentTower.CycleTargetingPriority();
                RefreshRingButtons();
                RefreshHighlight();
                return;
            }

            if (selectedAction == action)
            {
                Confirm(action);
                return;
            }

            infoVisible = false;
            selectedAction = action;
            ShowActionInfo(action);
            RefreshHighlight();
        }

        private void Confirm(RingAction action)
        {
            switch (action)
            {
                case RingAction.Upgrade:
                    OnUpgradeClicked();
                    break;
                case RingAction.SpecA:
                    Specialize(0);
                    break;
                case RingAction.SpecB:
                    Specialize(1);
                    break;
                case RingAction.Sell:
                    OnSellClicked();
                    break;
                case RingAction.Priority:
                    OnPriorityClicked();
                    break;
            }
        }

        private void ShowActionInfo(RingAction action)
        {
            if (statsPanel == null || currentTower == null) return;
            if (statsText == null)
            {
                Transform body = statsPanel.transform.Find("StatsText");
                if (body != null) statsText = body.GetComponent<TextMeshProUGUI>();
            }
            if (statsText == null) return;

            StatIcons.Set(statsText, BuildActionText(action));
            statsPanel.SetActive(true);
            statsPanel.transform.SetAsLastSibling();
        }

        private void HideActionInfo()
        {
            if (statsPanel != null) statsPanel.SetActive(false);
        }

        private string BuildActionText(RingAction action)
        {
            TowerData data = currentTower.GetTowerData();
            string name = data != null ? data.towerName : currentTower.name;

            switch (action)
            {
                case RingAction.Info:
                    return BuildInfoText(name, data);
                case RingAction.Upgrade:
                    return BuildUpgradeText(name, data);
                case RingAction.SpecA:
                    return BuildSpecText(0);
                case RingAction.SpecB:
                    return BuildSpecText(1);
                case RingAction.Sell:
                    return $"{name}\nSell\n{StatIcons.Gold} {currentTower.GetSellRefund()}";
                case RingAction.Priority:
                    if (currentTower is BarracksTower)
                        return $"{name}\nMove\nChoose a rally point";
                    return $"{name}\nTarget: {PriorityLabel(currentTower.GetTargetingPriority())}";
                default:
                    return name;
            }
        }

        private string BuildInfoText(string name, TowerData data)
        {
            string text = $"{name}\nLv {currentTower.CurrentLevel}\n{StatIcons.Health} {currentTower.GetCurrentHealth():0}/{currentTower.GetMaxHealth():0}";

            if (currentTower is BarracksTower barracks)
            {
                text += $"\n{StatIcons.Soldiers} {barracks.soldierCount}\n{StatIcons.Time} {barracks.respawnDelay:0}s";
                return text;
            }

            text += $"\n{StatIcons.Damage} {currentTower.GetCurrentDamage():0}\n{StatIcons.Range} {currentTower.GetCurrentRange():0.#}\n{StatIcons.Attack} {currentTower.GetCurrentFireRate():0.##}/s";
            if (data == null) return text;
            if (data.isArtillery && data.explosionRadius > 0f)
                text += $"\n{StatIcons.Area} {data.explosionRadius:0.#}";
            else if (data.isSlowTower)
                text += $"\n{StatIcons.Slow} {data.effectPower * 100f:0}%";
            else if (data.isAuraTower)
                text += $"\n{StatIcons.Aura} +{data.auraDamageBonus * 100f:0}%";
            return text;
        }

        private string BuildUpgradeText(string name, TowerData data)
        {
            if (currentTower.CurrentLevel >= 3 || data == null)
                return $"{name}\nLv {currentTower.CurrentLevel}";

            int next = currentTower.CurrentLevel + 1;
            float nextDamage = currentTower.GetCurrentDamage() / Mathf.Max(1, currentTower.CurrentLevel) * next;
            float rangeLevel = 1f + (currentTower.CurrentLevel - 1) * 0.2f;
            float nextRange = currentTower.GetCurrentRange() / Mathf.Max(0.01f, rangeLevel) * (1f + (next - 1) * 0.2f);
            return $"{name}\n{StatIcons.Gold} {data.upgradeCost}\nLv {currentTower.CurrentLevel} > {next}\n{StatIcons.Damage} {nextDamage:0}\n{StatIcons.Range} {nextRange:0.#}";
        }

        private string BuildSpecText(int index)
        {
            var specs = currentTower.GetSpecializations();
            if (specs == null || index < 0 || index >= specs.Count || specs[index] == null) return "";
            TowerData spec = specs[index];
            BarracksTower barracks = spec.prefab != null ? spec.prefab.GetComponent<BarracksTower>() : null;
            if (barracks != null)
                return $"{spec.towerName}\n{StatIcons.Gold} {spec.cost}\n{StatIcons.Health} {spec.health:0}\n{StatIcons.Soldiers} {barracks.soldierCount}\n{StatIcons.Time} {barracks.respawnDelay:0}s";

            string text = $"{spec.towerName}\n{StatIcons.Gold} {spec.cost}\n{StatIcons.Damage} {spec.damage:0}\n{StatIcons.Range} {spec.range:0.#}\n{StatIcons.Attack} {spec.fireRate:0.##}/s\n{StatIcons.Health} {spec.health:0}";
            if (spec.isArtillery && spec.explosionRadius > 0f)
                text += $"\n{StatIcons.Area} {spec.explosionRadius:0.#}";
            else if (spec.isSlowTower)
                text += $"\n{StatIcons.Slow} {spec.effectPower * 100f:0}%";
            else if (spec.isAuraTower)
                text += $"\n{StatIcons.Aura} +{spec.auraDamageBonus * 100f:0}%";
            return text;
        }

        private void RefreshHighlight()
        {
            Transform root = mainPanel != null ? mainPanel.transform.Find("RingButtons") : null;
            if (root == null) return;
            foreach (Transform child in root)
            {
                Transform apply = child.Find("Apply");
                if (apply == null) continue;
                apply.gameObject.SetActive(selectedAction != RingAction.None && child.name == selectedAction.ToString());
            }
        }

        private bool HasSpecs()
        {
            if (currentTower == null || currentTower.CurrentLevel < 3) return false;
            var specs = currentTower.GetSpecializations();
            return specs != null && specs.Count >= 2;
        }

        private bool CanAfford(Side side, int cost)
        {
            return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(side, cost);
        }

        private Sprite PriorityIcon(TargetingPriority priority)
        {
            switch (priority)
            {
                case TargetingPriority.Last:
                    return priorityLastIcon != null ? priorityLastIcon : priorityFirstIcon;
                case TargetingPriority.Strongest:
                    return priorityStrongIcon != null ? priorityStrongIcon : priorityFirstIcon;
                case TargetingPriority.Weakest:
                    return priorityWeakIcon != null ? priorityWeakIcon : priorityLastIcon;
                default:
                    return priorityFirstIcon;
            }
        }

        private static string PriorityLabel(TargetingPriority priority)
        {
            switch (priority)
            {
                case TargetingPriority.Last: return "Last";
                case TargetingPriority.Strongest: return "Strong";
                case TargetingPriority.Weakest: return "Weak";
                default: return "First";
            }
        }

        private Transform FindRingButton(RingAction action)
        {
            Transform root = mainPanel != null ? mainPanel.transform.Find("RingButtons") : null;
            return root != null ? root.Find(action.ToString()) : null;
        }
    }
}
