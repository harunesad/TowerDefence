using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class HeroDetailPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image heroIcon;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI sideText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI attackRateText;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionLabel;
        [SerializeField] private Button closeButton;

        private HeroData currentHero;
        private HeroShopUI shopUI;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSFX;

        private void Awake()
        {
            if (unlockSFX == null)
            {
#if UNITY_EDITOR
                unlockSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/HeroUnlock.mp3");
#endif
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionClicked);

            Hide();
        }

        private void OnEnable()
        {
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.OnHeroProgressChanged += RefreshCurrent;
        }

        private void OnDisable()
        {
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.OnHeroProgressChanged -= RefreshCurrent;
        }

        public void BindShop(HeroShopUI shop) => shopUI = shop;

        public void Show(HeroData hero)
        {
            currentHero = hero;
            if (panelRoot != null) panelRoot.SetActive(true);
            
            // Hero icon'ı info panelde gösterme - kullanıcı istediği gibi gizle
            // Sprite'ı null yap + toggle state'i koru (Repair sonrası da gizli kalması için)
            if (heroIcon != null)
            {
                heroIcon.sprite = null;
                heroIcon.enabled = false; // Image component'ını devre dışı bırak
            }
            
            RefreshCurrent();
        }

        public void Hide()
        {
            currentHero = null;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void RefreshCurrent()
        {
            if (currentHero == null || MetaProgressionManager.Instance == null) return;

            // Hero icon'ı info panelde gösterme - kullanıcı istediği gibi gizle
            // Icon sprite ve enabled state'i koru (Repair sonrası da gizli kalması için)
            // if (heroIcon != null) heroIcon.sprite = currentHero.GetIcon();  // Commented out
            // if (heroIcon != null) heroIcon.enabled = true;  // Commented out
            
            if (heroNameText != null) heroNameText.text = currentHero.displayName;
            if (sideText != null) sideText.text = currentHero.side.ToString().ToUpper();

            bool unlocked = MetaProgressionManager.Instance.IsHeroUnlocked(currentHero.heroID);
            int level = MetaProgressionManager.Instance.GetHeroLevel(currentHero.heroID);

            if (levelText != null)
                levelText.text = unlocked ? $"Level {level} / {currentHero.maxUpgradeLevel}" : "Locked";

            MetaProgressionManager.Instance.GetHeroCombatStats(currentHero, out float hp, out float dmg, out float spd, out float rng, out float rate);

            if (healthText != null) StatIcons.Set(healthText, $"{StatIcons.Health} {hp:0}");
            if (damageText != null) StatIcons.Set(damageText, $"{StatIcons.Damage} {dmg:0}");
            if (speedText != null) StatIcons.Set(speedText, $"{StatIcons.Move} {spd:0.00}");
            if (rangeText != null) StatIcons.Set(rangeText, $"{StatIcons.Range} {rng:0.0}");
            if (attackRateText != null) StatIcons.Set(attackRateText, $"{StatIcons.Attack} {rate:0.00}/s");

            if (abilityNameText != null) abilityNameText.text = $"Ability: {currentHero.abilityName}";
            if (abilityDescText != null)
                abilityDescText.text = $"{currentHero.abilityDescription}\n{StatIcons.Time} {currentHero.abilityCooldown:0}s";
                StatIcons.Set(abilityDescText, abilityDescText.text);

            int karma = MetaProgressionManager.Instance.GetTotalKarma();
            int crystals = MetaProgressionManager.Instance.GetTotalCrystals();

            if (!unlocked)
            {
                ShowHeroCost(currentHero.unlockKarmaCost, currentHero.unlockCrystalCost);
                if (actionLabel != null) actionLabel.text = "UNLOCK HERO";
                if (actionButton != null) actionButton.interactable = karma >= currentHero.unlockKarmaCost && crystals >= currentHero.unlockCrystalCost;
            }
            else if (level >= currentHero.maxUpgradeLevel)
            {
                ShowHeroCost(0, 0);
                if (costText != null) costText.text = "Maximum level reached";
                if (actionLabel != null) actionLabel.text = "MAX LEVEL";
                if (actionButton != null) actionButton.interactable = false;
            }
            else
            {
                ShowHeroCost(currentHero.upgradeKarmaCost, currentHero.upgradeCrystalCost);
                if (actionLabel != null) actionLabel.text = "UPGRADE";
                if (actionButton != null) actionButton.interactable = karma >= currentHero.upgradeKarmaCost && crystals >= currentHero.upgradeCrystalCost;
            }

            shopUI?.RefreshListHighlights();
        }

        private void OnActionClicked()
        {
            if (currentHero == null || MetaProgressionManager.Instance == null) return;

            bool success = false;
            if (!MetaProgressionManager.Instance.IsHeroUnlocked(currentHero.heroID))
                success = MetaProgressionManager.Instance.TryUnlockHero(currentHero);
            else
                success = MetaProgressionManager.Instance.TryUpgradeHero(currentHero);

            if (success && AudioManager.Instance != null && unlockSFX != null)
            {
                AudioManager.Instance.PlaySFX(unlockSFX);
            }

            RefreshCurrent();
        }

        private void ShowHeroCost(int karma, int crystals)
        {
            if (costText == null) return;
            Transform row = costText.transform.Find("CurrencyRow");
            if (row != null) row.gameObject.SetActive(karma > 0 || crystals > 0);
            costText.text = "";
            CurrencyRowView.Apply(costText.transform, karma, crystals, false);
        }
    }
}
