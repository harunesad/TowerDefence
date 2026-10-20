using UnityEngine;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Data;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class CompendiumUI : MonoBehaviour
    {
        [Header("Item Prefab")]
        [SerializeField] private GameObject compendiumItemPrefab;

        [Header("UI Elements")]
        [SerializeField] private RectTransform heroContent;
        [SerializeField] private RectTransform unitContent;
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailStats;
        [SerializeField] private TextMeshProUGUI detailLore;
        [SerializeField] private Button backButton;

        [Header("Audio")]
        [SerializeField] private AudioClip pageSFX;

        private void Start()
        {
            if (pageSFX == null)
            {
#if UNITY_EDITOR
                pageSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/WaveInfo.wav");
#endif
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => {
                    MainMenuController mc = GetComponentInParent<MainMenuController>();
                    if (mc != null) mc.ShowMainMenu();
                });
            }
            
            EnsureGridLayout();
            PopulateCompendium();
            if (detailPanel != null) detailPanel.SetActive(false);
        }

        private void EnsureGridLayout()
        {
            if (heroContent == null) return;

            GridLayoutGroup glg = heroContent.GetComponent<GridLayoutGroup>();
            if (glg == null)
            {
                glg = heroContent.gameObject.AddComponent<GridLayoutGroup>();
                glg.cellSize = new Vector2(240, 240);
                glg.spacing = new Vector2(15, 15);
                glg.padding = new RectOffset(10, 10, 10, 10);
                glg.childAlignment = TextAnchor.UpperCenter;
                glg.startAxis = GridLayoutGroup.Axis.Horizontal;
                glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
                glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 3;
            }

            ContentSizeFitter csf = heroContent.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = heroContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void PopulateCompendium()
        {
#if UNITY_EDITOR
            if (heroContent == null) return;

            // Clear existing
            foreach (Transform t in heroContent) Destroy(t.gameObject);

            HashSet<string> seen = new HashSet<string>();
            string[] unitGuids = UnityEditor.AssetDatabase.FindAssets("t:UnitData", new string[] { "Assets/Data/Units" });
            foreach (string guid in unitGuids)
            {
                UnitData data = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (data == null) continue;
                if (!seen.Add("U:" + data.unitName)) continue;
                CreateItem(data, heroContent); // Use the same vertical list
            }
#else
            Debug.LogWarning("[CompendiumUI] Compendium population is editor-only right now.");
#endif
        }

        private void CreateItem(ScriptableObject data, Transform parent)
        {
            GameObject item = null;
            if (compendiumItemPrefab != null)
            {
                item = Instantiate(compendiumItemPrefab, parent);
            }
#if UNITY_EDITOR
            else
            {
                compendiumItemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CompendiumItem.prefab");
                if (compendiumItemPrefab != null)
                {
                    item = Instantiate(compendiumItemPrefab, parent);
                }
            }
#endif

            if (item == null)
            {
                Debug.LogError("[CompendiumUI] compendiumItemPrefab bulunamadı veya atanmamış!");
                return;
            }

            item.name = "CompendiumItem";

            Image iconImg = item.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameTxt = item.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            Button btn = item.GetComponent<Button>();

            if (data is UnitData ud)
            {
                if (iconImg != null)
                {
                    iconImg.sprite = ud.icon;
                    ud.compendiumIcon.ApplyTo(iconImg);
                }
                if (nameTxt != null) nameTxt.text = ud.unitName;
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ShowUnitDetail(ud));
                }
            }
            else if (data is HeroData hd)
            {
                if (iconImg != null)
                {
                    iconImg.sprite = hd.GetIcon();
                    hd.rosterIcon.ApplyTo(iconImg);
                }
                if (nameTxt != null) nameTxt.text = hd.displayName;
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ShowHeroDetail(hd));
                }
            }
        }

        private void ShowHeroDetail(HeroData data)
        {
            detailPanel.SetActive(true);
            if (AudioManager.Instance != null && pageSFX != null) AudioManager.Instance.PlaySFX(pageSFX);
            if (detailIcon != null) detailIcon.sprite = data.icon;
            detailName.text = data.displayName;
            if (data.unitData != null)
            {
                detailStats.text = $"HP: {data.unitData.maxHealth}\nDMG: {data.unitData.attackDamage}\nSPD: {data.unitData.moveSpeed}";
            }
            detailLore.text = data.abilityDescription;
        }

        private void ShowUnitDetail(UnitData data)
        {
            detailPanel.SetActive(true);
            if (AudioManager.Instance != null && pageSFX != null) AudioManager.Instance.PlaySFX(pageSFX);
            if (detailIcon != null) detailIcon.sprite = data.icon;
            detailName.text = data.unitName;
            detailStats.text = $"HP: {data.maxHealth}\nDMG: {data.attackDamage}\nSPD: {data.moveSpeed}\nATK Rate: {data.attackRate:F2}/s";
            if (!string.IsNullOrEmpty(data.loreDescription))
            {
                detailLore.text = data.loreDescription;
            }
            else
            {
                detailLore.text = data.side == Side.Light ? "A brave defender of the light." : "A corrupted creature of the dark.";
            }
        }
    }
}
