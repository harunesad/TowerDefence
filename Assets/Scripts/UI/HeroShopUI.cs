using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class HeroShopUI : MonoBehaviour
    {
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private TextMeshProUGUI karmaText;
        [SerializeField] private Button backButton;
        [SerializeField] private HeroDetailPanelUI detailPanel;

        private readonly List<HeroShopItemUI> spawnedItems = new List<HeroShopItemUI>();
        private HeroData selectedHero;

        private void Start()
        {
            if (detailPanel != null)
                detailPanel.BindShop(this);

            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }

            Populate();
        }

        private void OnEnable()
        {
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.OnHeroProgressChanged += OnProgressChanged;
            RefreshKarma();
            Populate();
        }

        private void OnDisable()
        {
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.OnHeroProgressChanged -= OnProgressChanged;
        }

        private void OnProgressChanged()
        {
            RefreshKarma();
            if (selectedHero != null && detailPanel != null)
                detailPanel.Show(selectedHero);
            RefreshListHighlights();
        }

        public void ShowHeroDetail(HeroData hero)
        {
            selectedHero = hero;
            if (detailPanel != null)
                detailPanel.Show(hero);
            RefreshListHighlights();
        }

        public void RefreshListHighlights()
        {
            foreach (var item in spawnedItems)
            {
                if (item == null) continue;
                item.SetHighlighted(selectedHero != null && item.HeroData == selectedHero);
                item.RefreshUI();
            }
        }

        private void RefreshKarma()
        {
            if (karmaText != null && MetaProgressionManager.Instance != null)
                karmaText.text = $"Karma: {MetaProgressionManager.Instance.GetTotalKarma()}";
        }

        public void Populate()
        {
            if (itemContainer == null || itemPrefab == null || MetaProgressionManager.Instance == null) return;

            foreach (Transform child in itemContainer)
                Destroy(child.gameObject);
            spawnedItems.Clear();

            foreach (HeroData hero in MetaProgressionManager.Instance.GetAllHeroesSorted())
            {
                GameObject go = Instantiate(itemPrefab, itemContainer);
                HeroShopItemUI item = go.GetComponent<HeroShopItemUI>();
                if (item != null)
                {
                    item.Setup(hero, this);
                    spawnedItems.Add(item);
                }
            }

            RefreshKarma();
            RefreshListHighlights();
        }
    }
}
