using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Combat;

namespace TowerDefence.UI
{
    public class HeroButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] internal Image iconImage;
        [SerializeField] internal Slider healthSlider;
        [SerializeField] internal Image respawnOverlay;
        [SerializeField] internal TextMeshProUGUI timerText;
        [SerializeField] internal Image selectionFrame;

        [Header("Button Behavior")]
        [SerializeField] private Button mainButton;

        private int slotIndex = -1;
        private HeroUnit linkedHero;

        private static HeroStatsPanel cachedStatsPanel;

        private GameObject abilityRoot;
        private Image abilityIconImage;
        private TextMeshProUGUI abilityTimerText;
        private Image abilityCooldownOverlay;

        private void Awake()
        {
            EnsureAbilityUI();
            if (gameObject.name.Contains("_1")) slotIndex = 0;
            else if (gameObject.name.Contains("_2")) slotIndex = 1;
            else slotIndex = transform.GetSiblingIndex();

            EnsureClickHandler();
        }

        private void EnsureClickHandler()
        {
            if (mainButton == null)
                mainButton = GetComponent<Button>();
            if (mainButton == null)
                mainButton = gameObject.AddComponent<Button>();
            mainButton.transition = Selectable.Transition.None;
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(OnHeroButtonClicked);
        }

        private void OnHeroButtonClicked()
        {
            HeroStatsPanel panel = GetStatsPanel();
            if (panel == null) return;

            var data = HeroManager.Instance != null
                ? HeroManager.Instance.GetEquippedHeroData(slotIndex)
                : null;
            if (data == null) return;

            RectTransform selfRT = GetComponent<RectTransform>();
            if (panel.IsVisible && cachedStatsPanel == panel &&
                ReferenceEquals(selfRT, panel.transform as RectTransform))
            {
                panel.Hide();
                return;
            }
            cachedStatsPanel = panel;
            panel.Show(selfRT, data, linkedHero);
        }

        private HeroStatsPanel GetStatsPanel()
        {
            if (cachedStatsPanel != null && cachedStatsPanel.gameObject != null)
                return cachedStatsPanel;
            HeroStatsPanel[] all = FindObjectsByType<HeroStatsPanel>(FindObjectsSortMode.None);
            if (all != null && all.Length > 0)
                return cachedStatsPanel = all[0];
            return null;
        }

        public static HeroButtonUI Create(RectTransform parent, int slot)
        {
            GameObject obj = new GameObject($"HeroButton_{slot + 1}", typeof(RectTransform), typeof(Image), typeof(HeroButtonUI));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            obj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(obj.transform, false);
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

            GameObject sliderObj = new GameObject("HealthSlider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(obj.transform, false);
            RectTransform sRT = sliderObj.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0, 0);
            sRT.anchorMax = new Vector2(1, 0);
            sRT.pivot = new Vector2(0.5f, 0);
            sRT.anchoredPosition = new Vector2(0, 5);
            sRT.sizeDelta = new Vector2(-10, 10);

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderObj.transform, false);
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            bg.GetComponent<Image>().color = Color.gray;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            fillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            fillArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
            fillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = Color.green;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.value = 1f;

            GameObject overlayObj = new GameObject("RespawnOverlay", typeof(RectTransform), typeof(Image));
            overlayObj.transform.SetParent(obj.transform, false);
            overlayObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            overlayObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
            overlayObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            Image overlayImg = overlayObj.GetComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);
            overlayImg.type = Image.Type.Filled;
            overlayImg.fillMethod = Image.FillMethod.Radial360;

            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(obj.transform, false);
            RectTransform timerRT = timerObj.AddComponent<RectTransform>();
            timerRT.sizeDelta = new Vector2(100, 30);
            timerRT.anchoredPosition = Vector2.zero;
            TextMeshProUGUI timerTMP = timerObj.AddComponent<TextMeshProUGUI>();
            timerTMP.alignment = TextAlignmentOptions.Center;
            timerTMP.fontSize = 20;
            timerTMP.text = "";

            GameObject frameObj = new GameObject("SelectionFrame", typeof(RectTransform), typeof(Image));
            frameObj.transform.SetParent(obj.transform, false);
            frameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            frameObj.GetComponent<Image>().color = new Color(0.2f, 1f, 0.3f, 0.7f);
            frameObj.SetActive(false);

            HeroButtonUI ui = obj.GetComponent<HeroButtonUI>();
            ui.iconImage = iconObj.GetComponent<Image>();
            ui.healthSlider = slider;
            ui.respawnOverlay = overlayImg;
            ui.timerText = timerTMP;
            ui.selectionFrame = frameObj.GetComponent<Image>();

            return ui;
        }

        private void Start()
        {
            if (HeroManager.Instance == null) return;
            RefreshHeroData();
            if (!HasEquippedHero())
            {
                SetEmpty(true);
                return;
            }
            var hero = HeroManager.Instance.GetActiveHero(slotIndex);
            if (hero != null && !hero.IsDead)
                OnHeroSpawned(slotIndex, hero);
        }

        public void RefreshFromManager()
        {
            RefreshHeroData();
            SetEmpty(!HasEquippedHero());
        }

        private bool HasEquippedHero()
        {
            return HeroManager.Instance != null &&
                   HeroManager.Instance.GetEquippedHeroData(slotIndex) != null;
        }

        private void RefreshHeroData()
        {
            if (HeroManager.Instance == null) return;

            var data = HeroManager.Instance.GetEquippedHeroData(slotIndex);
            if (data != null)
            {
                if (iconImage != null)
                    iconImage.sprite = data.GetIcon();
                if (abilityIconImage != null)
                    abilityIconImage.sprite = data.GetIcon();
            }
        }

        private void OnEnable()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroSpawned += OnHeroSpawned;
                HeroManager.Instance.OnHeroDied += OnHeroDied;
                HeroManager.Instance.OnHeroRespawnTimerUpdated += OnRespawnTimerUpdated;
                HeroManager.Instance.OnHeroSelectionChanged += OnHeroSelectionChanged;
            }
        }

        private void OnDisable()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroSpawned -= OnHeroSpawned;
                HeroManager.Instance.OnHeroDied -= OnHeroDied;
                HeroManager.Instance.OnHeroRespawnTimerUpdated -= OnRespawnTimerUpdated;
                HeroManager.Instance.OnHeroSelectionChanged -= OnHeroSelectionChanged;
            }
        }

        private void Update()
        {
            if (linkedHero != null && !linkedHero.IsDead && healthSlider != null)
            {
                healthSlider.gameObject.SetActive(true);
                float maxHp = linkedHero.GetMaxHealth();
                healthSlider.value = maxHp > 0 ? linkedHero.GetHealth() / maxHp : 0f;
            }

            UpdateAbilityUI();
        }

        private void EnsureAbilityUI()
        {
            if (abilityRoot != null) return;

            abilityRoot = new GameObject("AbilityUI", typeof(RectTransform));
            abilityRoot.transform.SetParent(transform, false);
            RectTransform rootRT = abilityRoot.GetComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(1, 0.5f);
            rootRT.anchorMax = new Vector2(1, 0.5f);
            rootRT.pivot = new Vector2(0, 0.5f);
            rootRT.anchoredPosition = new Vector2(10, 0);
            rootRT.sizeDelta = new Vector2(70, 70);

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(abilityRoot.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.sizeDelta = Vector2.zero;
            abilityIconImage = iconObj.GetComponent<Image>();
            abilityIconImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            GameObject overlayObj = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
            overlayObj.transform.SetParent(abilityRoot.transform, false);
            RectTransform overlayRT = overlayObj.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.sizeDelta = Vector2.zero;
            abilityCooldownOverlay = overlayObj.GetComponent<Image>();
            abilityCooldownOverlay.color = new Color(0, 0, 0, 0.6f);
            abilityCooldownOverlay.type = Image.Type.Filled;
            abilityCooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            abilityCooldownOverlay.raycastTarget = false;

            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(abilityRoot.transform, false);
            RectTransform timerRT = timerObj.AddComponent<RectTransform>();
            timerRT.anchorMin = Vector2.zero;
            timerRT.anchorMax = Vector2.one;
            timerRT.sizeDelta = Vector2.zero;
            abilityTimerText = timerObj.AddComponent<TextMeshProUGUI>();
            abilityTimerText.alignment = TextAlignmentOptions.Center;
            abilityTimerText.fontSize = 16;
            abilityTimerText.fontStyle = FontStyles.Bold;
            abilityTimerText.color = Color.white;
            abilityTimerText.raycastTarget = false;

            Button abilityBtn = iconObj.AddComponent<Button>();
            abilityBtn.onClick.AddListener(OnAbilityClicked);
        }

        private void OnAbilityClicked()
        {
            if (linkedHero != null && !linkedHero.IsDead)
            {
                linkedHero.CastAbility();
            }
        }

        private void UpdateAbilityUI()
        {
            if (abilityRoot == null) return;

            if (linkedHero == null || linkedHero.IsDead)
            {
                abilityRoot.SetActive(false);
                return;
            }

            abilityRoot.SetActive(true);

            float current = linkedHero.AbilityCooldownRemaining;
            float max = linkedHero.AbilityCooldownMax;

            if (current <= 0f)
            {
                abilityCooldownOverlay.fillAmount = 0f;
                abilityCooldownOverlay.color = new Color(0, 0, 0, 0.2f);
                abilityTimerText.text = "READY";
                abilityTimerText.color = new Color(0.2f, 1f, 0.3f);
                abilityIconImage.color = Color.white;
            }
            else
            {
                abilityCooldownOverlay.fillAmount = max > 0 ? current / max : 0f;
                abilityCooldownOverlay.color = new Color(0, 0, 0, 0.6f);
                abilityTimerText.text = $"{Mathf.CeilToInt(current)}s";
                abilityTimerText.color = Color.white;
                abilityIconImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        private void OnHeroSpawned(int index, HeroUnit hero)
        {
            if (index != slotIndex) return;
            linkedHero = hero;
            if (respawnOverlay != null) respawnOverlay.gameObject.SetActive(false);
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (healthSlider != null) healthSlider.gameObject.SetActive(true);
            RefreshHeroData();
        }

        private void OnHeroDied(int index)
        {
            if (index != slotIndex) return;
            linkedHero = null;
            if (respawnOverlay != null) respawnOverlay.gameObject.SetActive(true);
            if (timerText != null) timerText.gameObject.SetActive(true);
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
            UpdateSelectionFrame(false);
        }

        private void OnRespawnTimerUpdated(int index, float current, float max)
        {
            if (index != slotIndex) return;
            if (respawnOverlay != null) respawnOverlay.fillAmount = current / max;
            if (timerText != null) timerText.text = $"{Mathf.CeilToInt(current)}s";
        }

        private void OnHeroSelectionChanged(HeroUnit hero)
        {
            UpdateSelectionFrame(linkedHero != null && hero == linkedHero);
        }

        private void UpdateSelectionFrame(bool active)
        {
            if (selectionFrame != null)
                selectionFrame.gameObject.SetActive(active);
        }

        private void SetEmpty(bool empty)
        {
            gameObject.SetActive(!empty);
            if (empty)
            {
                linkedHero = null;
                if (healthSlider != null) healthSlider.gameObject.SetActive(false);
                if (respawnOverlay != null) respawnOverlay.gameObject.SetActive(false);
                if (timerText != null) timerText.gameObject.SetActive(false);
                if (abilityRoot != null) abilityRoot.SetActive(false);
                UpdateSelectionFrame(false);
            }
        }
    }
}
