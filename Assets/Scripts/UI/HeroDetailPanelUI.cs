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

        private void Awake()
        {
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

            if (heroIcon != null) heroIcon.sprite = currentHero.GetIcon();
            if (heroNameText != null) heroNameText.text = currentHero.displayName;
            if (sideText != null) sideText.text = currentHero.side.ToString().ToUpper();

            bool unlocked = MetaProgressionManager.Instance.IsHeroUnlocked(currentHero.heroID);
            int level = MetaProgressionManager.Instance.GetHeroLevel(currentHero.heroID);

            if (levelText != null)
                levelText.text = unlocked ? $"Level {level} / {currentHero.maxUpgradeLevel}" : "Locked";

            MetaProgressionManager.Instance.GetHeroCombatStats(currentHero, out float hp, out float dmg, out float spd, out float rng, out float rate);

            if (healthText != null) healthText.text = $"Health: {hp:0}";
            if (damageText != null) damageText.text = $"Damage: {dmg:0}";
            if (speedText != null) speedText.text = $"Speed: {spd:0.00}";
            if (rangeText != null) rangeText.text = $"Range: {rng:0.0}";
            if (attackRateText != null) attackRateText.text = $"Attack Rate: {rate:0.00}/s";

            if (abilityNameText != null) abilityNameText.text = currentHero.abilityName;
            if (abilityDescText != null)
                abilityDescText.text = $"{currentHero.abilityDescription}\nCooldown: {currentHero.abilityCooldown:0}s";

            int karma = MetaProgressionManager.Instance.GetTotalKarma();

            if (!unlocked)
            {
                if (costText != null) costText.text = $"Unlock Cost: {currentHero.unlockKarmaCost} Karma";
                if (actionLabel != null) actionLabel.text = "UNLOCK HERO";
                if (actionButton != null) actionButton.interactable = karma >= currentHero.unlockKarmaCost;
            }
            else if (level >= currentHero.maxUpgradeLevel)
            {
                if (costText != null) costText.text = "Maximum level reached";
                if (actionLabel != null) actionLabel.text = "MAX LEVEL";
                if (actionButton != null) actionButton.interactable = false;
            }
            else
            {
                if (costText != null) costText.text = $"Upgrade Cost: {currentHero.upgradeKarmaCost} Karma";
                if (actionLabel != null) actionLabel.text = "UPGRADE";
                if (actionButton != null) actionButton.interactable = karma >= currentHero.upgradeKarmaCost;
            }

            shopUI?.RefreshListHighlights();
        }

        private void OnActionClicked()
        {
            if (currentHero == null || MetaProgressionManager.Instance == null) return;

            if (!MetaProgressionManager.Instance.IsHeroUnlocked(currentHero.heroID))
                MetaProgressionManager.Instance.TryUnlockHero(currentHero);
            else
                MetaProgressionManager.Instance.TryUpgradeHero(currentHero);

            RefreshCurrent();
        }
    }
}
