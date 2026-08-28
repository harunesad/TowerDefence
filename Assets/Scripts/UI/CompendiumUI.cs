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
        [Header("UI Elements")]
        [SerializeField] private RectTransform heroContent;
        [SerializeField] private RectTransform unitContent;
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailStats;
        [SerializeField] private TextMeshProUGUI detailLore;
        [SerializeField] private Button backButton;

        private void Start()
        {
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

            LayoutGroup anyLg = heroContent.GetComponent<LayoutGroup>();
            if (anyLg != null && anyLg is not GridLayoutGroup)
                DestroyImmediate(anyLg);

            GridLayoutGroup glg = heroContent.GetComponent<GridLayoutGroup>();
            if (glg == null) glg = heroContent.gameObject.AddComponent<GridLayoutGroup>();
            if (glg == null) return;

            glg.cellSize = new Vector2(280, 240);
            glg.spacing = new Vector2(15, 15);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

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
            // Create a square-ish item (Card look)
            GameObject item = new GameObject("CompendiumItem", typeof(RectTransform), typeof(Image), typeof(Button));
            item.transform.SetParent(parent, false);
            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(280, 240);
            
            Image bg = item.GetComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            
            Button btn = item.GetComponent<Button>();

            // Icon on top
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(item.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 1f);
            iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.pivot = new Vector2(0.5f, 1f);
            iconRT.sizeDelta = new Vector2(100, 160);
            iconRT.anchoredPosition = new Vector2(0, -18);
            Image iconImg = iconObj.GetComponent<Image>();

            // Name text at the bottom
            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(item.transform, false);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0);
            nameRT.pivot = new Vector2(0.5f, 0);
            nameRT.sizeDelta = new Vector2(0, 75);
            nameRT.anchoredPosition = new Vector2(0, 10);
            
            TextMeshProUGUI nameTxt = nameObj.GetComponent<TextMeshProUGUI>();
            nameTxt.fontSize = 16;
            nameTxt.alignment = TextAlignmentOptions.Center;
            nameTxt.color = Color.white;
            nameTxt.enableWordWrapping = true;

            if (data is UnitData ud)
            {
                iconImg.sprite = ud.icon;
                nameTxt.text = ud.unitName;
                btn.onClick.AddListener(() => ShowUnitDetail(ud));
            }
        }

        private void ShowHeroDetail(HeroData data)
        {
            detailPanel.SetActive(true);
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
            if (detailIcon != null) detailIcon.sprite = data.icon;
            detailName.text = data.unitName;
            detailStats.text = $"HP: {data.maxHealth}\nDMG: {data.attackDamage}\nSPD: {data.moveSpeed}\nATK Rate: {data.attackRate:F2}/s";
            detailLore.text = data.side == Side.Light ? "A brave defender of the light." : "A corrupted creature of the dark.";
        }
    }
}
