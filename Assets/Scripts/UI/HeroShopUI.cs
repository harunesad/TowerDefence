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

        public RectTransform BackTarget => backButton != null ? backButton.transform as RectTransform : null;
        public RectTransform BuyTarget => detailPanel != null ? detailPanel.ActionTarget : null;

        bool scrolledDark;

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
                    if (TutorialDirector.Instance != null && TutorialDirector.Instance.BlocksHeroShopBack())
                        return;
                    if (TutorialDirector.Instance != null)
                        TutorialDirector.Instance.OnHeroShopBack();
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
            scrolledDark = false;
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
            {
                karmaText.text = MetaProgressionManager.Instance.GetTotalKarma().ToString();
                CurrencyRowView.Apply(karmaText.transform, MetaProgressionManager.Instance.GetTotalKarma(), 0, false);
            }
            if (crystalText != null)
            {
                crystalText.text = MetaProgressionManager.Instance.GetTotalCrystals().ToString();
                CurrencyRowView.Apply(crystalText.transform, 0, MetaProgressionManager.Instance.GetTotalCrystals(), false);
            }
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

        public RectTransform DarkHeroTarget(string heroId)
        {
            if (string.IsNullOrEmpty(heroId)) return null;
            foreach (HeroShopItemUI item in spawnedItems)
            {
                if (item == null || item.HeroData == null) continue;
                if (item.HeroData.heroID == heroId)
                    return item.transform as RectTransform;
            }
            return null;
        }

        private void LateUpdate()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            bool lesson = tutorial != null && tutorial.Current() == "heroes_tell";
            int page = lesson ? tutorial.HeroShopPage : -1;
            string targetId = lesson ? tutorial.TaughtDarkHeroId() : null;

            if (backButton != null)
                backButton.interactable = !lesson || page >= 2;

            foreach (HeroShopItemUI item in spawnedItems)
            {
                if (item == null) continue;
                bool allowed = !lesson || (page == 0 && item.HeroData != null && item.HeroData.heroID == targetId);
                item.SetCardInteractable(allowed);
            }

            if (!lesson)
            {
                scrolledDark = false;
                return;
            }
            if (page != 0 || scrolledDark) return;
            RectTransform target = DarkHeroTarget(targetId);
            RectTransform content = itemContainer as RectTransform;
            if (target == null || content == null) return;
            Canvas.ForceUpdateCanvases();
            if (Mathf.Abs(target.anchoredPosition.y) < 1f && target.GetSiblingIndex() > 0)
                return;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, -target.anchoredPosition.y);
            scrolledDark = true;
        }
    }
}
