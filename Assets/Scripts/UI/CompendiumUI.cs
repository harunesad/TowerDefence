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
        [Header("Data")]
        [SerializeField] private List<UnitData> allUnits = new List<UnitData>();

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

        private RectTransform GetContentParent()
        {
            if (unitContent != null) return unitContent;
            return heroContent;
        }

        private void EnsureGridLayout()
        {
            RectTransform contentParent = GetContentParent();
            if (contentParent == null) return;

            GridLayoutGroup glg = contentParent.GetComponent<GridLayoutGroup>();
            if (glg == null)
            {
                glg = contentParent.gameObject.AddComponent<GridLayoutGroup>();
                glg.cellSize = new Vector2(240, 240);
                glg.spacing = new Vector2(15, 15);
                glg.padding = new RectOffset(10, 10, 10, 10);
                glg.childAlignment = TextAnchor.UpperCenter;
                glg.startAxis = GridLayoutGroup.Axis.Horizontal;
                glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
                glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                glg.constraintCount = 3;
            }

            ContentSizeFitter csf = contentParent.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = contentParent.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void PopulateCompendium()
        {
            RectTransform contentParent = GetContentParent();
            if (contentParent == null) return;

            // Clear existing
            foreach (Transform t in contentParent) Destroy(t.gameObject);

            HashSet<string> seen = new HashSet<string>();

            // 1. Use serialized allUnits if available
            if (allUnits != null && allUnits.Count > 0)
            {
                foreach (UnitData data in allUnits)
                {
                    if (data == null) continue;
                    if (!seen.Add(data.unitName)) continue;
                    CreateItem(data, contentParent);
                }
                return;
            }

#if UNITY_EDITOR
            // Editor fallback in case serialized list is empty
            string[] unitGuids = UnityEditor.AssetDatabase.FindAssets("t:UnitData", new string[] { "Assets/Data/Units" });
            foreach (string guid in unitGuids)
            {
                UnitData data = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (data == null) continue;
                if (!seen.Add(data.unitName)) continue;
                CreateItem(data, contentParent);
            }
#else
            Debug.LogWarning("[CompendiumUI] allUnits is empty in runtime build.");
#endif
        }

        private void CreateItem(UnitData data, Transform parent)
        {
            if (data == null) return;

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

            item.name = "CompendiumItem_" + data.unitName;

            Image iconImg = item.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameTxt = item.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            Button btn = item.GetComponent<Button>();

            if (iconImg != null)
            {
                iconImg.sprite = data.icon;
                data.compendiumIcon.ApplyTo(iconImg);
            }
            if (nameTxt != null) nameTxt.text = data.unitName;
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowUnitDetail(data));
            }
        }

        private void ShowUnitDetail(UnitData data)
        {
            if (data == null) return;
            if (detailPanel != null) detailPanel.SetActive(true);
            if (AudioManager.Instance != null && pageSFX != null) AudioManager.Instance.PlaySFX(pageSFX);
            if (detailIcon != null) detailIcon.sprite = data.icon;
            if (detailName != null) detailName.text = data.unitName;
            if (detailStats != null)
            {
                detailStats.text = $"{StatIcons.Health} {data.maxHealth}\n{StatIcons.Damage} {data.attackDamage}\n{StatIcons.Move} {data.moveSpeed}\n{StatIcons.Attack} {data.attackRate:F2}/s";
                StatIcons.Set(detailStats, detailStats.text);
            }
            if (detailLore != null)
            {
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
}
