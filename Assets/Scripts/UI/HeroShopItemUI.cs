using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class HeroShopItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image lockedOverlay;
        [SerializeField] private Image highlightFrame;
        [SerializeField] private Button cardButton;

        private HeroData heroData;
        private HeroShopUI shopUI;

        public HeroData HeroData => heroData;

        public void Setup(HeroData data, HeroShopUI shop)
        {
            heroData = data;
            shopUI = shop;

            if (iconImage != null) iconImage.sprite = data.GetIcon();
            if (nameText != null) nameText.text = data.displayName;

            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(OnCardClicked);
            }

            RefreshUI();
        }

        private void OnCardClicked()
        {
            if (heroData == null || shopUI == null) return;
            shopUI.ShowHeroDetail(heroData);
        }

        public void RefreshUI()
        {
            if (heroData == null || MetaProgressionManager.Instance == null) return;

            bool unlocked = MetaProgressionManager.Instance.IsHeroUnlocked(heroData.heroID);
            int level = MetaProgressionManager.Instance.GetHeroLevel(heroData.heroID);

            if (levelText != null)
                levelText.text = unlocked ? $"Lv.{level}" : "Locked";

            if (lockedOverlay != null)
                lockedOverlay.gameObject.SetActive(!unlocked);

            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlightFrame != null)
                highlightFrame.gameObject.SetActive(highlighted);
        }
    }
}
