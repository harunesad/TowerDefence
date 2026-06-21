using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.UI
{
    public class SideSelectionUI : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button lightSideButton;
        [SerializeField] private Button darkSideButton;
        [SerializeField] private Button backButton;

        [Header("Side Selection")]
        [SerializeField] private GameObject sideSelectionGroup;
        [SerializeField] private GameObject loadoutGroup;

        [Header("Hero Loadout")]
        [SerializeField] private Transform heroItemContainer;
        [SerializeField] private GameObject heroItemPrefab;
        [SerializeField] private TextMeshProUGUI heroLoadoutHint;

        [Header("Spell Loadout")]
        [SerializeField] private Transform spellItemContainer;
        [SerializeField] private GameObject spellItemPrefab;

        [Header("Match")]
        [SerializeField] private Button startMatchButton;
        [SerializeField] private List<SpellData> allPossibleSpells;

        private Side currentSelectedSide;

        private void Start()
        {
            lightSideButton.onClick.AddListener(() => OnSideSelected(Side.Light));
            darkSideButton.onClick.AddListener(() => OnSideSelected(Side.Dark));

            if (startMatchButton != null)
                startMatchButton.onClick.AddListener(StartMatch);

            if (backButton != null)
            {
                backButton.onClick.AddListener(() =>
                {
                    if (loadoutGroup != null && loadoutGroup.activeSelf)
                    {
                        loadoutGroup.SetActive(false);
                        sideSelectionGroup.SetActive(true);
                    }
                    else
                    {
                        MainMenuController mc = GetComponentInParent<MainMenuController>();
                        if (mc != null) mc.ShowLevelSelect();
                    }
                });
            }

            if (loadoutGroup != null) loadoutGroup.SetActive(false);
        }

        private void OnSideSelected(Side side)
        {
            currentSelectedSide = side;
            SideController.Instance.SetPlayerSide(side);

            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.SanitizeEquippedHeroesForSide(side);

            if (sideSelectionGroup != null) sideSelectionGroup.SetActive(false);
            if (loadoutGroup != null)
            {
                loadoutGroup.SetActive(true);
                PopulateLoadout(side);
            }
            else
            {
                StartMatch();
            }
        }

        private void PopulateLoadout(Side side)
        {
            PopulateHeroLoadout(side);
            PopulateSpellLoadout(side);
        }

        private void PopulateHeroLoadout(Side side)
        {
            if (heroItemContainer == null || heroItemPrefab == null) return;

            foreach (Transform child in heroItemContainer)
                Destroy(child.gameObject);

            if (MetaProgressionManager.Instance == null) return;

            foreach (HeroData hero in MetaProgressionManager.Instance.GetAllHeroes())
            {
                if (hero == null || hero.side != side) continue;
                if (!MetaProgressionManager.Instance.IsHeroUnlocked(hero.heroID)) continue;

                GameObject go = Instantiate(heroItemPrefab, heroItemContainer);
                HeroLoadoutItemUI itemUI = go.GetComponent<HeroLoadoutItemUI>();
                if (itemUI != null) itemUI.Setup(hero);
            }

            if (heroLoadoutHint != null)
                heroLoadoutHint.text = "SELECT HEROES (Max 2)";
        }

        private void PopulateSpellLoadout(Side side)
        {
            if (spellItemContainer == null || spellItemPrefab == null) return;

            foreach (Transform child in spellItemContainer)
                Destroy(child.gameObject);

            foreach (SpellData spell in allPossibleSpells)
            {
                if (spell == null) continue;
                if (spell.side != side && spell.side != Side.Neutral) continue;
                if (!MetaProgressionManager.Instance.IsSpellUnlocked(spell.spellID)) continue;

                GameObject go = Instantiate(spellItemPrefab, spellItemContainer);
                SpellLoadoutItemUI itemUI = go.GetComponent<SpellLoadoutItemUI>();
                if (itemUI != null) itemUI.Setup(spell);
            }
        }

        private void StartMatch()
        {
            if (MetaProgressionManager.Instance == null) return;

            if (MetaProgressionManager.Instance.GetEquippedHeroIDs().Count == 0)
            {
                foreach (HeroData hero in MetaProgressionManager.Instance.GetAllHeroes())
                {
                    if (hero == null || hero.side != currentSelectedSide) continue;
                    if (!MetaProgressionManager.Instance.IsHeroUnlocked(hero.heroID)) continue;
                    MetaProgressionManager.Instance.EquipHero(hero.heroID);
                    break;
                }
            }

            if (MetaProgressionManager.Instance.GetEquippedHeroIDs().Count == 0)
            {
                if (heroLoadoutHint != null)
                    heroLoadoutHint.text = "UNLOCK A HERO IN THE HEROES MENU FIRST!";
                return;
            }

            Debug.Log("Starting match with side: " + currentSelectedSide);
            CampaignManager.Instance.LoadSelectedLevel();
        }
    }
}
