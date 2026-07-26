using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TowerDefence.Data;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class HeroLoadoutItemUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectionHighlight;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button button;

        private HeroData heroData;
        private GameObject activePanel;
        private float lastClickTime;
        private const float DoubleClickThreshold = 0.3f;

        public void Setup(HeroData data)
        {
            heroData = data;
            if (iconImage != null) iconImage.sprite = data.GetIcon();
            if (nameText != null) nameText.text = data.displayName;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
            RefreshUI();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            float timeSinceLastClick = Time.unscaledTime - lastClickTime;
            lastClickTime = Time.unscaledTime;

            if (timeSinceLastClick < DoubleClickThreshold)
            {
                CancelInvoke(nameof(ExecuteSingleClick));
                ShowInfoPanel();
            }
            else
            {
                Invoke(nameof(ExecuteSingleClick), DoubleClickThreshold);
            }
        }

        private void ExecuteSingleClick()
        {
            ToggleSelection();
        }

        private void ShowInfoPanel()
        {
            if (activePanel != null)
            {
                Destroy(activePanel);
                activePanel = null;
                return;
            }

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

        private void OnDisable()
        {
            CancelInvoke(nameof(ExecuteSingleClick));
        }
    }
}
