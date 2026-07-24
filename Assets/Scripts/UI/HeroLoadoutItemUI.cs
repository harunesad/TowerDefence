using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class HeroLoadoutItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionHighlight;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button button;

        private HeroData heroData;
        private GameObject activePanel;

        public void Setup(HeroData data)
        {
            heroData = data;
            if (iconImage != null) iconImage.sprite = data.GetIcon();
            if (nameText != null) nameText.text = data.displayName;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
            RefreshUI();
        }

        private void OnClick()
        {
            ToggleSelection();

            var existing = FindObjectOfType<HeroInfoPanelUI>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
                activePanel = null;
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject go = new GameObject("HeroInfoPanelUI", typeof(RectTransform), typeof(HeroInfoPanelUI));
            go.transform.SetParent(canvas.transform, false);
            go.GetComponent<HeroInfoPanelUI>().Setup(heroData, GetComponent<RectTransform>());
            activePanel = go;
        }

        private void ToggleSelection()
        {
            if (heroData == null || MetaProgressionManager.Instance == null) return;

            if (MetaProgressionManager.Instance.GetEquippedHeroIDs().Contains(heroData.heroID))
                MetaProgressionManager.Instance.UnequipHero(heroData.heroID);
            else
                MetaProgressionManager.Instance.EquipHero(heroData.heroID);

            RefreshAllLoadoutItems();
        }

        public void RefreshUI()
        {
            if (heroData == null || MetaProgressionManager.Instance == null) return;

            bool isEquipped = MetaProgressionManager.Instance.GetEquippedHeroIDs().Contains(heroData.heroID);
            if (selectionHighlight != null)
                selectionHighlight.gameObject.SetActive(isEquipped);

            if (levelText != null)
            {
                int level = MetaProgressionManager.Instance.GetHeroLevel(heroData.heroID);
                levelText.text = $"Lv.{level}";
            }
        }

        private void RefreshAllLoadoutItems()
        {
            var items = transform.parent.GetComponentsInChildren<HeroLoadoutItemUI>(true);
            foreach (var item in items)
                item.RefreshUI();
        }
    }
}
