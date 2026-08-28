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
        [SerializeField] private TextMeshProUGUI crystalText; // Yeni: Kristal göstergesi
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

            if (itemContainer != null)
            {
                RectTransform rt = itemContainer.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Scroll view'un düzgün çalışması için Content pivotu her zaman üstte (1) olmalı
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    // Y pozisyonunu sıfırlayarak en üste yasla
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);
                }
            }

            Populate();
        }

        private void OnEnable()
        {
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.OnHeroProgressChanged += OnProgressChanged;
            
            // Panelin her açılışında scrollu en başa al
            if (itemContainer != null)
            {
                RectTransform rt = itemContainer.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);
                }
            }

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
            if (MetaProgressionManager.Instance == null) return;
            if (karmaText != null)
                karmaText.text = $"Karma: {MetaProgressionManager.Instance.GetTotalKarma()}";
            if (crystalText != null)
                crystalText.text = $"Crystals: {MetaProgressionManager.Instance.GetTotalCrystals()}";
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
