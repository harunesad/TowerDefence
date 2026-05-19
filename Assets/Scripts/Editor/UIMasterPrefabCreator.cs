using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TowerDefence.UI;
using TowerDefence.Core;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.Grid;

namespace TowerDefence.Editor
{
    public class UIMasterPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/Prefabs/UI";

        public static void CreateTowerUpgradeUIPrefab()
        {
            EnsureDirectory();
            // 1. Root Nesnesini Oluştur
            GameObject root = new GameObject("TowerUpgradeUI", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(TowerUpgradeUI));
            TowerUpgradeUI uiScript = root.GetComponent<TowerUpgradeUI>();
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(300, 400); 
            rootRT.localScale = new Vector3(0.005f, 0.005f, 0.005f); 

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // 2. Main Panel
            GameObject mainPanel = CreateUINode(root.transform, "MainPanel", new Vector2(300, 400));
            mainPanel.SetActive(false); // Başlangıçta gizli
            mainPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            VerticalLayoutGroup vlg = mainPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            uiScript.mainPanel = mainPanel;

            // ... (Kısaltılmış mantık, asıl metodları aşağıya taşıyoruz)
            uiScript.towerNameText = CreateTMP(mainPanel.transform, "TowerName", "Archer Tower", 32);
            uiScript.levelText = CreateTMP(mainPanel.transform, "LevelText", "Level 1", 24);

            GameObject normalGroup = CreateUINode(mainPanel.transform, "NormalUpgradeGroup", new Vector2(260, 100));
            uiScript.normalUpgradeGroup = normalGroup;
            VerticalLayoutGroup normalVlg = normalGroup.AddComponent<VerticalLayoutGroup>();
            normalVlg.childAlignment = TextAnchor.MiddleCenter;
            normalVlg.spacing = 5;
            
            uiScript.upgradeButton = CreateButton(normalGroup.transform, "UpgradeButton", "UPGRADE");
            uiScript.upgradeCostText = CreateTMP(normalGroup.transform, "UpgradeCost", "100", 20);

            GameObject specGroup = CreateUINode(mainPanel.transform, "SpecializationGroup", new Vector2(260, 150));
            uiScript.specializationGroup = specGroup;
            HorizontalLayoutGroup hlg = specGroup.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            GameObject specACont = CreateUINode(specGroup.transform, "SpecA_Container", new Vector2(110, 140));
            VerticalLayoutGroup vlgA = specACont.AddComponent<VerticalLayoutGroup>();
            vlgA.childAlignment = TextAnchor.MiddleCenter;
            vlgA.spacing = 5;
            uiScript.specAIcon = CreateImage(specACont.transform, "Icon", new Vector2(60, 60));
            uiScript.specAButton = CreateButton(specACont.transform, "SpecAButton", "SELECT");
            uiScript.specACostText = CreateTMP(specACont.transform, "Cost", "200", 18);

            GameObject specBCont = CreateUINode(specGroup.transform, "SpecB_Container", new Vector2(110, 140));
            VerticalLayoutGroup vlgB = specBCont.AddComponent<VerticalLayoutGroup>();
            vlgB.childAlignment = TextAnchor.MiddleCenter;
            vlgB.spacing = 5;
            uiScript.specBIcon = CreateImage(specBCont.transform, "Icon", new Vector2(60, 60));
            uiScript.specBButton = CreateButton(specBCont.transform, "SpecBButton", "SELECT");
            uiScript.specBCostText = CreateTMP(specBCont.transform, "Cost", "200", 18);

            uiScript.sellButton = CreateButton(mainPanel.transform, "SellButton", "SELL");
            uiScript.sellButton.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
            uiScript.sellValueText = CreateTMP(mainPanel.transform, "Value", "50", 18);

            // 7. Targeting Priority (NEW)
            GameObject targetGroup = CreateUINode(mainPanel.transform, "TargetingGroup", new Vector2(260, 60));
            HorizontalLayoutGroup targetHlg = targetGroup.AddComponent<HorizontalLayoutGroup>();
            targetHlg.childAlignment = TextAnchor.MiddleCenter;
            targetHlg.spacing = 10;
            
            uiScript.priorityButton = CreateButton(targetGroup.transform, "PriorityButton", "CYCLE");
            uiScript.priorityButton.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f);
            uiScript.priorityText = CreateTMP(targetGroup.transform, "PriorityText", "Target: First", 18);

            SaveAndCleanup(root, "TowerUpgradeUI");
        }

        public static void CreateSkillTreeUIPrefab()
        {
            EnsureDirectory();

            // Panel kökü — tam ekran
            GameObject root = new GameObject("SkillTreePanel", typeof(RectTransform), typeof(Image), typeof(SkillTreeUI));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.12f, 0.97f);

            // --- Header ---
            GameObject header = CreateUINode(root.transform, "Header", new Vector2(0, 80));
            RectTransform hRT = header.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1); hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot     = new Vector2(0.5f, 1);
            hRT.anchoredPosition = Vector2.zero;
            hRT.sizeDelta = new Vector2(0, 80);
            HorizontalLayoutGroup hHlg = header.AddComponent<HorizontalLayoutGroup>();
            hHlg.childAlignment = TextAnchor.MiddleCenter;
            hHlg.padding = new RectOffset(20, 20, 10, 10);
            hHlg.spacing = 40;
            hHlg.childControlWidth = false;
            hHlg.childForceExpandWidth = false;

            TextMeshProUGUI titleTmp = CreateTMP(header.transform, "Title", "SKILL TREE", 42, 460, 60);
            titleTmp.color = new Color(1f, 0.85f, 0.2f);

            TextMeshProUGUI karmaTmp = CreateTMP(header.transform, "KarmaText", "Karma: 0", 32, 280, 60);
            karmaTmp.color = new Color(0.6f, 1f, 0.6f);

            // --- Tabs: Side Selection ---
            GameObject sideTabs = CreateUINode(root.transform, "SideTabs", new Vector2(800, 60));
            RectTransform sideRT = sideTabs.GetComponent<RectTransform>();
            sideRT.anchorMin = new Vector2(0.5f, 1f);
            sideRT.anchorMax = new Vector2(0.5f, 1f);
            sideRT.pivot = new Vector2(0.5f, 1f);
            sideRT.anchoredPosition = new Vector2(0, -100); // Header'ın (400->-50) altında

            HorizontalLayoutGroup sHlg = sideTabs.AddComponent<HorizontalLayoutGroup>();
            sHlg.childAlignment = TextAnchor.MiddleCenter;
            sHlg.spacing = 20;

            Button btnLight = CreateButton(sideTabs.transform, "LightTab", "LIGHT", 180, 50, 18);
            Button btnDark  = CreateButton(sideTabs.transform, "DarkTab", "DARK", 180, 50, 18);
            Button btnNeut  = CreateButton(sideTabs.transform, "NeutralTab", "GENERAL", 180, 50, 18);

            // --- Tabs: Category Selection ---
            GameObject catTabs = CreateUINode(root.transform, "CategoryTabs", new Vector2(600, 50));
            RectTransform catRT = catTabs.GetComponent<RectTransform>();
            catRT.anchorMin = new Vector2(0.5f, 1f);
            catRT.anchorMax = new Vector2(0.5f, 1f);
            catRT.pivot = new Vector2(0.5f, 1f);
            catRT.anchoredPosition = new Vector2(0, -170); // SideTabs'ın altında

            HorizontalLayoutGroup cHlg = catTabs.AddComponent<HorizontalLayoutGroup>();
            cHlg.childAlignment = TextAnchor.MiddleCenter;
            cHlg.spacing = 15;

            Button btnPassives = CreateButton(catTabs.transform, "PassivesTab", "PASSIVES", 200, 45, 16);
            Button btnSpells   = CreateButton(catTabs.transform, "SpellsTab", "ACTIVE SPELLS", 200, 45, 16);

            SkillTreeUI uiScript = root.GetComponent<SkillTreeUI>();

            // Geri Butonu (sol üst)
            Button backBtn = CreateButton(root.transform, "BackButton", "← BACK", 160, 55, 22);
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 1); backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.anchoredPosition = new Vector2(20, -12);

            // Feedback Text
            TextMeshProUGUI feedbackTmp = CreateTMP(root.transform, "FeedbackText", "", 24, 600, 40);
            feedbackTmp.rectTransform.anchoredPosition = new Vector2(0, -320);
            feedbackTmp.alignment = TMPro.TextAlignmentOptions.Center;

            // Link master references
            var so = new UnityEditor.SerializedObject(uiScript);
            so.FindProperty("totalKarmaText").objectReferenceValue = karmaTmp;
            so.FindProperty("feedbackText").objectReferenceValue   = feedbackTmp;
            so.FindProperty("backButton").objectReferenceValue     = backBtn;
            
            so.FindProperty("btnLight").objectReferenceValue       = btnLight;
            so.FindProperty("btnDark").objectReferenceValue        = btnDark;
            so.FindProperty("btnNeutral").objectReferenceValue     = btnNeut;
            so.FindProperty("btnPassives").objectReferenceValue    = btnPassives;
            so.FindProperty("btnSpells").objectReferenceValue      = btnSpells;
            
            so.ApplyModifiedProperties();

            // --- Scroll View ---
            GameObject scroll = new GameObject("NodesScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scroll.transform.SetParent(root.transform, false);
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            RectTransform scrollRT = scroll.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(10, 10);
            scrollRT.offsetMax = new Vector2(-10, -230); // Sekmelerin altında başlasın

            // Content (grid içindeki node'lar burada)
            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(scroll.transform, false);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            GridLayoutGroup glg = content.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(160, 200);
            glg.spacing  = new Vector2(20, 20);
            glg.padding  = new RectOffset(20, 20, 20, 20);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;

            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            sr.content   = contentRT;
            sr.horizontal = false;
            sr.vertical   = true;
            sr.scrollSensitivity = 30;

            // --- Tüm SkillNodeData'ları otomatik bul ve node oluştur ---
            string[] guids = AssetDatabase.FindAssets("t:SkillNodeData", new[] { "Assets/Data/Skills" });
            var nodeUIList = new System.Collections.Generic.List<SkillNodeUI>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SkillNodeData data = AssetDatabase.LoadAssetAtPath<SkillNodeData>(path);
                if (data == null) continue;

                // Her SkillNodeData için bir node UI nesnesi oluştur
                GameObject node = new GameObject(data.skillName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(SkillNodeUI));
                node.transform.SetParent(content.transform, false);
                node.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.28f, 1f);

                // Icon
                GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(node.transform, false);
                RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.1f, 0.35f);
                iconRT.anchorMax = new Vector2(0.9f, 0.9f);
                iconRT.offsetMin = Vector2.zero; iconRT.offsetMax = Vector2.zero;
                iconGO.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.6f);

                // Skill Adı
                TextMeshProUGUI nameTmp = CreateTMP(node.transform, "SkillName", data.skillName, 18, 140, 30);
                nameTmp.rectTransform.anchorMin = new Vector2(0, 0.22f);
                nameTmp.rectTransform.anchorMax = new Vector2(1, 0.35f);
                nameTmp.rectTransform.offsetMin = Vector2.zero;
                nameTmp.rectTransform.offsetMax = Vector2.zero;
                nameTmp.alignment = TMPro.TextAlignmentOptions.Center;

                // Karma Maliyet
                TextMeshProUGUI costTmp = CreateTMP(node.transform, "CostText", $"{data.karmaCost} K", 16, 100, 24);
                costTmp.rectTransform.anchorMin = new Vector2(0, 0);
                costTmp.rectTransform.anchorMax = new Vector2(1, 0.22f);
                costTmp.rectTransform.offsetMin = Vector2.zero;
                costTmp.rectTransform.offsetMax = Vector2.zero;
                costTmp.color = new Color(0.6f, 1f, 0.6f);
                costTmp.alignment = TMPro.TextAlignmentOptions.Center;

                // Locked Overlay
                GameObject lockedGO = new GameObject("LockedOverlay", typeof(RectTransform), typeof(Image));
                lockedGO.transform.SetParent(node.transform, false);
                SetStretch(lockedGO.GetComponent<RectTransform>());
                lockedGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
                lockedGO.SetActive(false);

                // Purchased Overlay
                GameObject purchasedGO = new GameObject("PurchasedOverlay", typeof(RectTransform), typeof(Image));
                purchasedGO.transform.SetParent(node.transform, false);
                SetStretch(purchasedGO.GetComponent<RectTransform>());
                purchasedGO.GetComponent<Image>().color = new Color(0.1f, 0.8f, 0.1f, 0.55f);
                purchasedGO.SetActive(false);

                // Type Label (Passive/Active)
                TextMeshProUGUI typeTmp = CreateTMP(node.transform, "TypeText", "PASSIVE", 12, 100, 20);
                typeTmp.rectTransform.anchorMin = new Vector2(0, 0.9f);
                typeTmp.rectTransform.anchorMax = new Vector2(1, 1);
                typeTmp.rectTransform.offsetMin = new Vector2(5, 0);
                typeTmp.rectTransform.offsetMax = new Vector2(-5, -2);
                typeTmp.alignment = TMPro.TextAlignmentOptions.Left;
                typeTmp.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.Italic;

                // SkillNodeUI referanslarını bağla
                SkillNodeUI nodeUI = node.GetComponent<SkillNodeUI>();
                var nodeSO = new UnityEditor.SerializedObject(nodeUI);
                nodeSO.FindProperty("skillData").objectReferenceValue        = data;
                nodeSO.FindProperty("iconImage").objectReferenceValue        = iconGO.GetComponent<Image>();
                nodeSO.FindProperty("buyButton").objectReferenceValue        = node.GetComponent<Button>();
                nodeSO.FindProperty("costText").objectReferenceValue         = costTmp;
                nodeSO.FindProperty("lockedOverlay").objectReferenceValue    = lockedGO.GetComponent<Image>();
                nodeSO.FindProperty("purchasedOverlay").objectReferenceValue = purchasedGO.GetComponent<Image>();
                nodeSO.FindProperty("typeText").objectReferenceValue         = typeTmp;
                nodeSO.ApplyModifiedProperties();
                nodeUIList.Add(nodeUI);
            }

            // SkillTreeUI referanslarını bağla
            SkillTreeUI treeUI = root.GetComponent<SkillTreeUI>();
            var treeSO = new UnityEditor.SerializedObject(treeUI);
            treeSO.FindProperty("totalKarmaText").objectReferenceValue = karmaTmp;
            treeSO.FindProperty("feedbackText").objectReferenceValue   = feedbackTmp;
            treeSO.FindProperty("backButton").objectReferenceValue     = backBtn;
            var nodesProp = treeSO.FindProperty("allNodes");
            nodesProp.arraySize = nodeUIList.Count;
            for (int i = 0; i < nodeUIList.Count; i++)
                nodesProp.GetArrayElementAtIndex(i).objectReferenceValue = nodeUIList[i];
            treeSO.ApplyModifiedProperties();

            Debug.Log($"✔ SkillTreePanel: {nodeUIList.Count} skill node oluşturuldu.");
            SaveAndCleanup(root, "SkillTreePanel");
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }


        public static void CreateMainMenuMaster()
        {
            EnsureDirectory();

            // 1. Root Canvas
            GameObject root = CreateBaseCanvas("MainMenu_MasterPrefab");
            
            // 2. Background Image — her zaman görünür, asla kapatılmaz
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(root.transform, false);
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);

            // 3. MainMenuPanel — Başlık + Butonların hepsini sarar; MainMenuController bunu açıp kapatır
            GameObject mainMenuPanel = CreateUINode(root.transform, "MainMenuPanel", Vector2.zero);
            RectTransform mmRT = mainMenuPanel.GetComponent<RectTransform>();
            mmRT.anchorMin = Vector2.zero;
            mmRT.anchorMax = Vector2.one;
            mmRT.offsetMin = Vector2.zero;
            mmRT.offsetMax = Vector2.zero;

            // 3a. Title Text
            TextMeshProUGUI title = CreateTMP(mainMenuPanel.transform, "GameTitle", "TOWER DEFENCE: EPIC BATTLE", 72, 1400, 150);
            title.rectTransform.anchoredPosition = new Vector2(0, 300);
            title.color = new Color(1f, 0.8f, 0.2f);

            // 3b. Button Container
            GameObject btnContainer = CreateUINode(mainMenuPanel.transform, "ButtonContainer", new Vector2(400, 500));
            VerticalLayoutGroup vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 20;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            // 3c. Buttons
            Button playBtn  = CreateButton(btnContainer.transform, "PlayButton",      "PLAY",  400, 80, 36);
            Button skillBtn = CreateButton(btnContainer.transform, "SkillTreeButton", "SKILL TREE",400, 80, 36);
            CreateButton(btnContainer.transform, "OptionsButton", "SETTINGS", 400, 80, 36);
            Button quitBtn  = CreateButton(btnContainer.transform, "QuitButton",      "QUIT",        400, 80, 36);

            // 4. Sub-Panels — başlangıçta kapalı
            GameObject skillPanel = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/SkillTreePanel.prefab",      false);
            GameObject levelPanel = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/LevelSelectionPanel.prefab", false);
            GameObject sidePanel  = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/SideSelectionPanel.prefab",  false);

            // 5. MainMenuController bağlantıları
            MainMenuController mc = root.AddComponent<MainMenuController>();
            var so = new UnityEditor.SerializedObject(mc);
            so.FindProperty("mainMenuPanel").objectReferenceValue    = mainMenuPanel;
            so.FindProperty("levelSelectPanel").objectReferenceValue = levelPanel;
            so.FindProperty("skillTreePanel").objectReferenceValue   = skillPanel;
            so.FindProperty("sideSelectionPanel").objectReferenceValue = sidePanel;
            so.FindProperty("playButton").objectReferenceValue       = playBtn;
            so.FindProperty("skillTreeButton").objectReferenceValue  = skillBtn;
            so.FindProperty("quitButton").objectReferenceValue       = quitBtn;
            so.ApplyModifiedProperties();

            SaveAndCleanup(root, "MainMenu_MasterPrefab");
        }

        public static void CreateLevelSelectionPanelPrefab()
        {
            EnsureDirectory();

            // --- Level Button Prefab'ı önce oluştur ---
            GameObject levelBtnPrefab = CreateLevelButtonPrefabAsset();

            // --- Level Selection Panel ---
            GameObject root = new GameObject("LevelSelectionPanel", typeof(RectTransform), typeof(Image), typeof(LevelSelectionUI));
            root.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.97f);
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Title
            TextMeshProUGUI titleTMP = CreateTMP(root.transform, "Title", "LEVEL SELECTION", 64, 900, 100);
            titleTMP.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            titleTMP.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            titleTMP.rectTransform.anchoredPosition = new Vector2(0, -80);

            // Level Container (Grid)
            GameObject container = CreateUINode(root.transform, "LevelContainer", new Vector2(1200, 650));
            container.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
            GridLayoutGroup glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(240, 320);
            glg.spacing = new Vector2(40, 40);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;

            // Back Button
            Button backBtn = CreateButton(root.transform, "BackButton", "← BACK", 180, 60, 22);
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 1);
            backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.anchoredPosition = new Vector2(30, -20);

            // Link LevelSelectionUI references
            LevelSelectionUI ui = root.GetComponent<LevelSelectionUI>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("container").objectReferenceValue = container.transform;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            if (levelBtnPrefab != null)
                so.FindProperty("levelButtonPrefab").objectReferenceValue = levelBtnPrefab;

            // Tüm LevelData ScriptableObject'leri otomatik bul ve listeye ekle
            string[] guids = AssetDatabase.FindAssets("t:LevelData");
            var levelsProp = so.FindProperty("levels");
            levelsProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                
                // OTOMASYON: Harita yoksa bul veya placeholder oluştur
                AutoAssignOrPlaceholder(levelData);

                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = levelData;
            }
            Debug.Log($"✔ LevelSelectionPanel: {guids.Length} LevelData otomatik yüklendi ve haritaları kontrol edildi.");
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH + "/LevelSelectionPanel.prefab");
            GameObject.DestroyImmediate(root);
        }

        // Level butonunu ayrı asset olarak kaydet ve döndür
        private static GameObject CreateLevelButtonPrefabAsset()
        {
            GameObject btn = new GameObject("LevelButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btn.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.22f, 1f);

            // Level ismi
            TextMeshProUGUI nameTxt = CreateTMP(btn.transform, "LevelName", "Level 1", 28, 200, 40);
            nameTxt.rectTransform.anchorMin = new Vector2(0, 1);
            nameTxt.rectTransform.anchorMax = new Vector2(1, 1);
            nameTxt.rectTransform.offsetMin = new Vector2(10, -50);
            nameTxt.rectTransform.offsetMax = new Vector2(-10, -10);
            nameTxt.alignment = TMPro.TextAlignmentOptions.Center;

            // Preview Image alanı
            GameObject preview = new GameObject("PreviewImage", typeof(RectTransform), typeof(Image));
            preview.transform.SetParent(btn.transform, false);
            RectTransform previewRT = preview.GetComponent<RectTransform>();
            previewRT.anchorMin = new Vector2(0.1f, 0.2f);
            previewRT.anchorMax = new Vector2(0.9f, 0.85f);
            previewRT.offsetMin = Vector2.zero;
            previewRT.offsetMax = Vector2.zero;
            preview.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

            // Zorluk Container (3 yıldız)
            GameObject diffContainer = new GameObject("DifficultyContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            diffContainer.transform.SetParent(btn.transform, false);
            RectTransform diffRT = diffContainer.GetComponent<RectTransform>();
            diffRT.anchorMin = new Vector2(0.5f, 0);
            diffRT.anchorMax = new Vector2(0.5f, 0);
            diffRT.sizeDelta = new Vector2(120, 30);
            diffRT.anchoredPosition = new Vector2(0, 15);
            HorizontalLayoutGroup dhlg = diffContainer.GetComponent<HorizontalLayoutGroup>();
            dhlg.childAlignment = TextAnchor.MiddleCenter;
            dhlg.spacing = 5;
            for (int i = 0; i < 3; i++)
            {
                GameObject star = new GameObject("Star_" + i, typeof(RectTransform), typeof(Image));
                star.transform.SetParent(diffContainer.transform, false);
                star.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 25);
                star.GetComponent<Image>().color = new Color(1f, 0.85f, 0.1f);
            }

            // Locked Overlay
            GameObject locked = new GameObject("LockedOverlay", typeof(RectTransform), typeof(Image));
            locked.transform.SetParent(btn.transform, false);
            RectTransform lockedRT = locked.GetComponent<RectTransform>();
            lockedRT.anchorMin = Vector2.zero;
            lockedRT.anchorMax = Vector2.one;
            lockedRT.offsetMin = Vector2.zero;
            lockedRT.offsetMax = Vector2.zero;
            locked.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);

            string path = PREFAB_PATH + "/LevelButton.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(btn, path);
            GameObject.DestroyImmediate(btn);
            return savedPrefab;
        }

        public static void CreateSideSelectionPanelPrefab()
        {
            EnsureDirectory();

            GameObject root = new GameObject("SideSelectionPanel", typeof(RectTransform), typeof(Image), typeof(SideSelectionUI));
            root.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.97f);
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Title
            TextMeshProUGUI titleTMP = CreateTMP(root.transform, "Title", "CHOOSE SIDE", 72, 800, 120);
            titleTMP.rectTransform.anchoredPosition = new Vector2(0, 280);
            titleTMP.color = new Color(1f, 0.85f, 0.2f);

            // Subtitle
            TextMeshProUGUI subTMP = CreateTMP(root.transform, "Subtitle", "LIGHT OR DARK?", 36, 700, 60);
            subTMP.rectTransform.anchoredPosition = new Vector2(0, 200);
            subTMP.color = new Color(0.8f, 0.8f, 0.8f);

            // Button Container
            GameObject btnRow = CreateUINode(root.transform, "SideButtonContainer", new Vector2(900, 100));
            btnRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            HorizontalLayoutGroup hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            // --- Phase 1: Side Selection ---
            GameObject sideGroup = CreateUINode(root.transform, "SideSelectionGroup", new Vector2(800, 400));
            VerticalLayoutGroup sideVlg = sideGroup.AddComponent<VerticalLayoutGroup>();
            sideVlg.childAlignment = TextAnchor.MiddleCenter;
            sideVlg.spacing = 30;

            TextMeshProUGUI sideTitle = CreateTMP(sideGroup.transform, "Title", "CHOOSE YOUR SIDE", 48, 600, 70);
            sideTitle.color = Color.white;

            GameObject sideBtnRow = CreateUINode(sideGroup.transform, "ButtonRow", new Vector2(800, 200));
            HorizontalLayoutGroup rowHlg = sideBtnRow.AddComponent<HorizontalLayoutGroup>();
            rowHlg.childAlignment = TextAnchor.MiddleCenter;
            rowHlg.spacing = 50;

            Button lightBtn = CreateButton(sideBtnRow.transform, "LightSideButton", "LIGHT SIDE", 300, 100, 28);
            lightBtn.GetComponent<Image>().color = new Color(0.9f, 0.9f, 1f);
            
            Button darkBtn = CreateButton(sideBtnRow.transform, "DarkSideButton", "DARK SIDE", 300, 100, 28);
            darkBtn.GetComponent<Image>().color = new Color(0.25f, 0.1f, 0.4f);

            // --- Phase 2: Spell Loadout ---
            GameObject loadoutGroup = CreateUINode(root.transform, "SpellLoadoutGroup", new Vector2(1000, 600));
            loadoutGroup.SetActive(false); // Başlangıçta gizli
            
            TextMeshProUGUI loadoutTitle = CreateTMP(loadoutGroup.transform, "Title", "EQUIP YOUR SPELLS (Max 3)", 42, 700, 60);
            loadoutTitle.rectTransform.anchoredPosition = new Vector2(0, 250);

            GameObject scroll = new GameObject("SpellScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scroll.transform.SetParent(loadoutGroup.transform, false);
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            RectTransform scrollRT = scroll.GetComponent<RectTransform>();
            scrollRT.sizeDelta = new Vector2(900, 350);
            scrollRT.anchoredPosition = new Vector2(0, 20);

            GameObject content = CreateUINode(scroll.transform, "Content", new Vector2(850, 300));
            GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(120, 150);
            glg.spacing = new Vector2(20, 20);
            glg.childAlignment = TextAnchor.UpperCenter;

            scroll.GetComponent<ScrollRect>().content = content.GetComponent<RectTransform>();

            Button startMatchBtn = CreateButton(loadoutGroup.transform, "StartMatchButton", "START BATTLE", 350, 90, 32);
            startMatchBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -220);
            startMatchBtn.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);

            // Back Button
            Button backBtn = CreateButton(root.transform, "BackButton", "← BACK", 180, 60, 22);
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 1);
            backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.anchoredPosition = new Vector2(30, -20);

            // Link SideSelectionUI
            SideSelectionUI ui = root.GetComponent<SideSelectionUI>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("lightSideButton").objectReferenceValue = lightBtn;
            so.FindProperty("darkSideButton").objectReferenceValue  = darkBtn;
            so.FindProperty("backButton").objectReferenceValue      = backBtn;
            
            so.FindProperty("sideSelectionGroup").objectReferenceValue = sideGroup;
            so.FindProperty("spellLoadoutGroup").objectReferenceValue   = loadoutGroup;
            so.FindProperty("spellItemContainer").objectReferenceValue  = content.transform;
            so.FindProperty("startMatchButton").objectReferenceValue    = startMatchBtn;
            
            // Spell Item Prefab
            GameObject itemPrefab = CreateSpellLoadoutItemPrefab();
            so.FindProperty("spellItemPrefab").objectReferenceValue = itemPrefab;

            // Auto-Populate allPossibleSpells
            string[] spellGuids = AssetDatabase.FindAssets("t:SpellData", new[] { "Assets/Data/Spells" });
            var allSpellsProp = so.FindProperty("allPossibleSpells");
            allSpellsProp.ClearArray();
            for (int i = 0; i < spellGuids.Length; i++)
            {
                allSpellsProp.InsertArrayElementAtIndex(i);
                allSpellsProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SpellData>(AssetDatabase.GUIDToAssetPath(spellGuids[i]));
            }

            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH + "/SideSelectionPanel.prefab");
            GameObject.DestroyImmediate(root);
        }

        private static GameObject CreateSpellLoadoutItemPrefab()
        {
            EnsureDirectory();
            GameObject root = new GameObject("SpellLoadoutItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(SpellLoadoutItemUI));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(120, 150);
            root.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Icon
            Image icon = CreateImage(root.transform, "Icon", new Vector2(100, 100));
            icon.rectTransform.anchoredPosition = new Vector2(0, 20);

            // Highlight (Seçili olduğunu gösteren çerçeve)
            Image highlight = CreateImage(root.transform, "Highlight", new Vector2(120, 150));
            highlight.color = new Color(1f, 0.85f, 0f, 0.5f); // Altın sarısı yarı saydam
            highlight.gameObject.SetActive(false);

            SpellLoadoutItemUI ui = root.GetComponent<SpellLoadoutItemUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("selectionHighlight").objectReferenceValue = highlight;
            so.FindProperty("button").objectReferenceValue = root.GetComponent<Button>();
            so.ApplyModifiedProperties();

            string path = PREFAB_PATH + "/SpellLoadoutItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject.DestroyImmediate(root);
            return prefab;
        }

        public static void CreateGameplayHUDMaster()
        {
            EnsureDirectory();

            // 1. Root Canvas
            GameObject root = CreateBaseCanvas("GameplayHUD_MasterPrefab");

            // 1. Stats Panel (Top Center)
            GameObject statsPanel = new GameObject("StatsPanel", typeof(RectTransform), typeof(Image));
            statsPanel.transform.SetParent(root.transform);
            RectTransform statsRT = statsPanel.GetComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0.5f, 1);
            statsRT.anchorMax = new Vector2(0.5f, 1);
            statsRT.pivot = new Vector2(0.5f, 1);
            statsRT.anchoredPosition = new Vector2(0, -10);
            statsRT.sizeDelta = new Vector2(800, 60);
            statsPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            HorizontalLayoutGroup statsHlg = statsPanel.AddComponent<HorizontalLayoutGroup>();
            statsHlg.childAlignment = TextAnchor.MiddleCenter;
            statsHlg.spacing = 50;

            TextMeshProUGUI livesText = CreateTMP(statsPanel.transform, "LivesText", "Lives: 20/20", 24, 150, 40);
            TextMeshProUGUI currencyText = CreateTMP(statsPanel.transform, "CurrencyText", "Gold: 100", 24, 150, 40);
            TextMeshProUGUI timerText = CreateTMP(statsPanel.transform, "TimerText", "Time: 30s", 24, 150, 40);
            TextMeshProUGUI phaseText = CreateTMP(statsPanel.transform, "PhaseText", "PREPARATION", 24, 150, 40);
            phaseText.color = Color.yellow;

            Button skipButton = CreateButton(statsPanel.transform, "SkipButton", ">>", 60, 40, 20);

            // 2. HUDController'ı köke ekle ve bağla
            HUDController hud = root.AddComponent<HUDController>();
            var hudSo = new UnityEditor.SerializedObject(hud);
            hudSo.FindProperty("livesText").objectReferenceValue = livesText;
            hudSo.FindProperty("currencyText").objectReferenceValue = currencyText;
            hudSo.FindProperty("timerText").objectReferenceValue = timerText;
            hudSo.FindProperty("phaseText").objectReferenceValue = phaseText;
            hudSo.FindProperty("skipPrepButton").objectReferenceValue = skipButton;
            // Sona doğru unitSelectionUI'yı bağlayacağız
            hudSo.ApplyModifiedProperties();

            // 3. Spell Selection Panel (Bottom Right)
            GameObject spellPanel = new GameObject("SpellPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Image), typeof(SpellSelectionUI));
            spellPanel.transform.SetParent(root.transform);
            RectTransform spellRT = spellPanel.GetComponent<RectTransform>();
            spellRT.anchorMin = new Vector2(1, 0); // Sağ alt
            spellRT.anchorMax = new Vector2(1, 0);
            spellRT.pivot = new Vector2(1, 0);
            spellRT.anchoredPosition = new Vector2(-30, 30);
            spellRT.sizeDelta = new Vector2(400, 100);
            spellPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);

            HorizontalLayoutGroup hlg = spellPanel.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 20;

            // SpellSelectionUI Konfigürasyonu
            SpellSelectionUI ssUI = spellPanel.GetComponent<SpellSelectionUI>();
            var ssSo = new SerializedObject(ssUI);
            ssSo.FindProperty("spellContainer").objectReferenceValue = spellPanel.transform;
            
            string spellBtnPath = PREFAB_PATH + "/SpellButton_Template.prefab";
            GameObject spellBtnPfb = AssetDatabase.LoadAssetAtPath<GameObject>(spellBtnPath);
            if (spellBtnPfb == null) {
                CreateSpellSlotPrefabs();
                spellBtnPfb = AssetDatabase.LoadAssetAtPath<GameObject>(spellBtnPath);
            }
            ssSo.FindProperty("spellButtonPrefab").objectReferenceValue = spellBtnPfb;
            
            // Büyü listelerini otomatik doldur
            string[] sGuids = AssetDatabase.FindAssets("t:SpellData", new[] { "Assets/Data/Spells" });
            var lightList = ssSo.FindProperty("lightSpells");
            var darkList = ssSo.FindProperty("darkSpells");
            lightList.ClearArray();
            darkList.ClearArray();
            
            foreach (var guid in sGuids)
            {
                var sData = AssetDatabase.LoadAssetAtPath<SpellData>(AssetDatabase.GUIDToAssetPath(guid));
                if (sData == null) continue;
                if (sData.side == Side.Light) {
                    lightList.InsertArrayElementAtIndex(lightList.arraySize);
                    lightList.GetArrayElementAtIndex(lightList.arraySize - 1).objectReferenceValue = sData;
                } else {
                    darkList.InsertArrayElementAtIndex(darkList.arraySize);
                    darkList.GetArrayElementAtIndex(darkList.arraySize - 1).objectReferenceValue = sData;
                }
            }
            ssSo.ApplyModifiedProperties();

            // 4. Unit Selection Panel (New)
            GameObject unitPanel = new GameObject("UnitPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            unitPanel.transform.SetParent(root.transform);
            RectTransform unitRT = unitPanel.GetComponent<RectTransform>();
            unitRT.anchorMin = new Vector2(0, 0);
            unitRT.anchorMax = new Vector2(0, 0);
            unitRT.pivot = new Vector2(0, 0);
            unitRT.anchoredPosition = new Vector2(30, 30);
            unitRT.sizeDelta = new Vector2(800, 120);

            HorizontalLayoutGroup unitHlg = unitPanel.GetComponent<HorizontalLayoutGroup>();
            unitHlg.childAlignment = TextAnchor.MiddleLeft;
            unitHlg.spacing = 15;

            // --- YENİ: Dinamik UnitSelectionUI Ekle ---
            UnitSelectionUI unitUI = unitPanel.AddComponent<UnitSelectionUI>();
            var unitSo = new SerializedObject(unitUI);
            unitSo.FindProperty("container").objectReferenceValue = unitPanel.transform;
            
            string unitBtnPath = PREFAB_PATH + "/UnitButton_Template.prefab";
            GameObject unitBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(unitBtnPath);
            if (unitBtnPrefab == null) {
                CreateUnitButtonPrefab(); // Yoksa oluştur
                unitBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(unitBtnPath);
            }
            unitSo.FindProperty("unitButtonPrefab").objectReferenceValue = unitBtnPrefab;

            // --- YENİ: unitUI.allUnits Listesini Doldur ---
            var unitGuids = AssetDatabase.FindAssets("t:UnitData");
            var allUnitsProp = unitSo.FindProperty("allUnits");
            allUnitsProp.ClearArray();
            int uIndex = 0;
            foreach (var guid in unitGuids)
            {
                var uData = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
                if (uData == null) continue;
                allUnitsProp.InsertArrayElementAtIndex(uIndex);
                allUnitsProp.GetArrayElementAtIndex(uIndex).objectReferenceValue = uData;
                uIndex++;
            }

            unitSo.ApplyModifiedProperties();

            // HUDController bağlantısını yap
            var hudUpdateSo = new SerializedObject(hud);
            hudUpdateSo.FindProperty("unitSelectionUI").objectReferenceValue = unitUI;
            hudUpdateSo.ApplyModifiedProperties();

            // Slotları oluştur (Sadece Spell için, Unit artık dinamik)
            CreateSpellSlotsInPanel(spellPanel.transform);

            // 5. Hız Kontrol Butonları (Sağ üst köşe)
            GameObject speedPanel = new GameObject("SpeedPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Image));
            speedPanel.transform.SetParent(root.transform);
            speedPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            RectTransform speedRT = speedPanel.GetComponent<RectTransform>();
            speedRT.anchorMin = new Vector2(1f, 1f); // Sağ üst
            speedRT.anchorMax = new Vector2(1f, 1f);
            speedRT.pivot     = new Vector2(1f, 1f);
            speedRT.anchoredPosition = new Vector2(-20f, -20f);
            speedRT.sizeDelta = new Vector2(240f, 60f);
            HorizontalLayoutGroup speedHlg = speedPanel.GetComponent<HorizontalLayoutGroup>();
            speedHlg.childAlignment = TextAnchor.MiddleCenter;
            speedHlg.spacing = 8;
            speedHlg.padding = new RectOffset(10, 10, 8, 8);
            speedHlg.childControlWidth  = true;
            speedHlg.childControlHeight = true;
            speedHlg.childForceExpandWidth  = true;
            speedHlg.childForceExpandHeight = true;

            Button speedX1Btn = CreateButton(speedPanel.transform, "SpeedX1Button", "x1", 65, 44, 22);
            Button speedX2Btn = CreateButton(speedPanel.transform, "SpeedX2Button", "x2", 65, 44, 22);
            Button speedX3Btn = CreateButton(speedPanel.transform, "SpeedX3Button", "x3", 65, 44, 22);

            // Aktif buton sarı, pasifler koyu görünsün
            speedX1Btn.GetComponent<Image>().color = new Color(1f,   0.75f, 0.1f); // Varsayılan x1 aktif
            speedX2Btn.GetComponent<Image>().color = new Color(0.2f, 0.2f,  0.2f);
            speedX3Btn.GetComponent<Image>().color = new Color(0.2f, 0.2f,  0.2f);

            // 5.b Duraklat/Başlat Butonu (Hız butonlarının hemen altında)
            GameObject pausePanel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            pausePanel.transform.SetParent(root.transform);
            pausePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            RectTransform pauseRT = pausePanel.GetComponent<RectTransform>();
            pauseRT.anchorMin = new Vector2(1f, 1f); // Sağ üst
            pauseRT.anchorMax = new Vector2(1f, 1f);
            pauseRT.pivot     = new Vector2(1f, 1f);
            pauseRT.anchoredPosition = new Vector2(-20f, -90f);
            pauseRT.sizeDelta = new Vector2(240f, 50f);

            Button pauseBtn = CreateButton(pausePanel.transform, "PauseButton", "PAUSE", 220, 36, 18);
            pauseBtn.GetComponent<Image>().color = new Color(0.2f, 0.2f,  0.2f);

            // HUDController'a hız buton referanslarını bağla
            var hudSpeedSo = new SerializedObject(hud);
            hudSpeedSo.FindProperty("speedX1Button").objectReferenceValue = speedX1Btn;
            hudSpeedSo.FindProperty("speedX2Button").objectReferenceValue = speedX2Btn;
            hudSpeedSo.FindProperty("speedX3Button").objectReferenceValue = speedX3Btn;
            hudSpeedSo.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            hudSpeedSo.ApplyModifiedProperties();

            // 6. Level Result Panel (Phase 4) - BAŞLAĞIÇTA GİZLİ
            GameObject resultObj = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/LevelResultPanel.prefab", false);
            if (resultObj != null)
            {
                var resultUI = resultObj.GetComponent<LevelResultUI>();
                var finalSo = new SerializedObject(hud);
                finalSo.FindProperty("levelResultUI").objectReferenceValue = resultUI;
                finalSo.ApplyModifiedProperties();
            }

            SaveAndCleanup(root, "GameplayHUD_MasterPrefab");
        }

        public static void CreateLevelResultUIPrefab()
        {
            EnsureDirectory();

            GameObject root = new GameObject("LevelResultPanel", typeof(RectTransform), typeof(Image), typeof(LevelResultUI));
            root.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f); // Karartma arka planı
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Title
            TextMeshProUGUI title = CreateTMP(root.transform, "Title", "VICTORY!", 120, 800, 150);
            title.rectTransform.anchoredPosition = new Vector2(0, 300);

            // Star Container
            GameObject starContainer = CreateUINode(root.transform, "StarContainer", new Vector2(600, 200));
            starContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
            HorizontalLayoutGroup hlg = starContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 40;

            Image[] starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject star = new GameObject("Star_" + i, typeof(RectTransform), typeof(Image));
                star.transform.SetParent(starContainer.transform, false);
                star.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
                starImages[i] = star.GetComponent<Image>();
                starImages[i].color = Color.gray;
            }

            // Buttons
            GameObject btnContainer = CreateUINode(root.transform, "ButtonContainer", new Vector2(800, 150));
            btnContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
            HorizontalLayoutGroup btnHlg = btnContainer.AddComponent<HorizontalLayoutGroup>();
            btnHlg.childAlignment = TextAnchor.MiddleCenter;
            btnHlg.spacing = 50;

            Button menuBtn = CreateButton(btnContainer.transform, "MenuButton", "MAIN MENU", 300, 80, 28);
            Button nextBtn = CreateButton(btnContainer.transform, "NextButton", "NEXT", 300, 80, 28);

            // Link LevelResultUI
            LevelResultUI ui = root.GetComponent<LevelResultUI>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("titleText").objectReferenceValue = title;
            
            var starProp = so.FindProperty("starImages");
            starProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                starProp.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];

            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.FindProperty("nextLevelButton").objectReferenceValue = nextBtn;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH + "/LevelResultPanel.prefab");
            GameObject.DestroyImmediate(root);
        }

        public static void CreateCoreEnginePrefab()
        {
            GameObject root = new GameObject("_Engine_MasterPrefab");
            
            // Tüm Manager'ları ekle (Tüm Scriptable Singleton veya MonoBehaviour tabanlı sistemler)
            root.AddComponent<GameManager>();
            root.AddComponent<PhaseManager>();
            root.AddComponent<CurrencyManager>();
            root.AddComponent<SpellManager>();
            root.AddComponent<MetaProgressionManager>();
            root.AddComponent<VFXManager>();
            root.AddComponent<AudioManager>();
            root.AddComponent<SideController>();
            root.AddComponent<CampaignManager>();
            root.AddComponent<SaveManager>();
            root.AddComponent<LivesManager>();
            root.AddComponent<ScreenShake>();
            root.AddComponent<TowerPlacementManager>();
            root.AddComponent<UnitPlacementManager>();
            root.AddComponent<GameSpeedManager>(); // x1 / x2 / x3 Oyun hızı yöneticisi

            // --- YENİ: TowerPlacementManager.allTowers Listesini Doldur ---
            TowerPlacementManager tpm = root.GetComponent<TowerPlacementManager>();
            var tpmSo = new SerializedObject(tpm);
            var towerGuids = AssetDatabase.FindAssets("t:TowerData");
            var towersProp = tpmSo.FindProperty("allTowers");
            towersProp.ClearArray();
            int tIndex = 0;
            foreach (var guid in towerGuids)
            {
                var tData = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
                if (tData == null) continue;
                towersProp.InsertArrayElementAtIndex(tIndex);
                towersProp.GetArrayElementAtIndex(tIndex).objectReferenceValue = tData;
                tIndex++;
            }
            tpmSo.ApplyModifiedProperties();

            // --- YENİ: UnitPlacementManager.allUnits Listesini Doldur ---
            UnitPlacementManager upm = root.GetComponent<UnitPlacementManager>();
            var upmSo = new SerializedObject(upm);
            var unitGuids = AssetDatabase.FindAssets("t:UnitData");
            var allUnitsProp = upmSo.FindProperty("allUnits");
            allUnitsProp.ClearArray();
            int uIndex = 0;
            foreach (var guid in unitGuids)
            {
                var uData = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
                if (uData == null) continue;
                allUnitsProp.InsertArrayElementAtIndex(uIndex);
                allUnitsProp.GetArrayElementAtIndex(uIndex).objectReferenceValue = uData;
                uIndex++;
            }
            upmSo.ApplyModifiedProperties();

            // Auto-populate CampaignManager.allLevels
            CampaignManager cm = root.GetComponent<CampaignManager>();
            var so = new UnityEditor.SerializedObject(cm);
            var levelsProp = so.FindProperty("allLevels");
            string[] guids = AssetDatabase.FindAssets("t:LevelData");
            
            // Verileri yükle ve isme göre sırala
            var levelDataList = new System.Collections.Generic.List<LevelData>();
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                levelDataList.Add(AssetDatabase.LoadAssetAtPath<LevelData>(assetPath));
            }
            levelDataList.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));

            levelsProp.arraySize = levelDataList.Count;
            for (int i = 0; i < levelDataList.Count; i++)
            {
                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = levelDataList[i];
            }
            so.ApplyModifiedProperties();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Core"))
            {
                System.IO.Directory.CreateDirectory("Assets/Prefabs/Core");
                AssetDatabase.Refresh();
            }

            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Core/_Engine_MasterPrefab.prefab");
            Debug.Log("✔ Core Engine Master Prefab created at Assets/Prefabs/Core/_Engine_MasterPrefab.prefab");
            GameObject.DestroyImmediate(root);
        }

        public static void CreateSpellSlotPrefabs()
        {
            EnsureDirectory();
            
            GameObject slot = new GameObject("SpellButton_Template", typeof(RectTransform), typeof(Image), typeof(Button), typeof(SpellButtonUI));
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            slot.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Icon
            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(slot.transform, false);
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);

            // Cooldown Overlay
            GameObject overlay = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(slot.transform, false);
            overlay.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            overlay.GetComponent<RectTransform>().anchorMax = Vector2.one;
            overlay.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            Image overlayImg = overlay.GetComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.6f);
            overlayImg.type = Image.Type.Filled;
            overlayImg.fillMethod = Image.FillMethod.Radial360;

            // Cost Text
            TextMeshProUGUI costText = CreateTMP(slot.transform, "CostText", "100", 18, 100, 30);
            costText.rectTransform.anchoredPosition = new Vector2(0, -50);

            // Referansları Bağla
            SpellButtonUI ui = slot.GetComponent<SpellButtonUI>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("iconImage").objectReferenceValue = icon.GetComponent<Image>();
            so.FindProperty("cooldownOverlay").objectReferenceValue = overlayImg;
            so.FindProperty("costText").objectReferenceValue = costText;
            so.FindProperty("button").objectReferenceValue = slot.GetComponent<Button>();
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(slot, PREFAB_PATH + "/SpellButton_Template.prefab");
            GameObject.DestroyImmediate(slot);
        }

        public static void CreateUnitButtonPrefab()
        {
            EnsureDirectory();
            
            GameObject slot = new GameObject("UnitButton_Template", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UnitButton));
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 120);
            slot.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // Icon
            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(slot.transform, false);
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);

            // Cost Text
            TextMeshProUGUI costText = CreateTMP(slot.transform, "CostText", "50", 22, 100, 30);
            costText.rectTransform.anchoredPosition = new Vector2(0, -35); // Biraz yukarı aldık
            costText.color = new Color(1f, 0.95f, 0.6f);
            costText.fontStyle = FontStyles.Bold;

            // Referansları Bağla
            UnitButton ui = slot.GetComponent<UnitButton>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("iconImage").objectReferenceValue = icon.GetComponent<Image>();
            so.FindProperty("costText").objectReferenceValue = costText;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(slot, PREFAB_PATH + "/UnitButton_Template.prefab");
            GameObject.DestroyImmediate(slot);
        }

        private static GameObject CreateUINode(Transform parent, string name, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return go;
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, int fontSize, float width = 200f, float height = 40f)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.raycastTarget = false; // Metinler tıklamayı engellemesin
            
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, float width = 100f, float height = 35f, int fontSize = 16)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = Color.white;
            
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            
            TextMeshProUGUI tmp = CreateTMP(go.transform, "Label", label, fontSize, width, height);
            tmp.color = Color.black;
            tmp.rectTransform.anchorMin = Vector2.zero;
            tmp.rectTransform.anchorMax = Vector2.one;
            tmp.rectTransform.offsetMin = Vector2.zero;
            tmp.rectTransform.offsetMax = Vector2.zero;
            
            return go.GetComponent<Button>();
        }

        private static Image CreateImage(Transform parent, string name, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false; // Görseller varsayılan olarak tıklamayı engellemesin
            return img;
        }

        private static void CreateSpellSlotsInPanel(Transform parent)
        {
            string slotPath = PREFAB_PATH + "/SpellButton_Template.prefab";
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPath);
            if (slotPrefab == null) return;

            string[] spellGuids = AssetDatabase.FindAssets("t:SpellData");
            foreach (string guid in spellGuids)
            {
                SpellData data = AssetDatabase.LoadAssetAtPath<SpellData>(AssetDatabase.GUIDToAssetPath(guid));
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, parent);
                go.name = "Spell_" + data.spellName;
                go.GetComponent<SpellButtonUI>().Setup(data);
            }
        }

        private static void CreateUnitSlotsInPanel(Transform parent)
        {
            string slotPath = PREFAB_PATH + "/UnitButton_Template.prefab";
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPath);
            if (slotPrefab == null) return;

            string[] unitGuids = AssetDatabase.FindAssets("t:UnitData");
            foreach (string guid in unitGuids)
            {
                UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
                
                // Sadece temel birimleri göster (Counterpart olanları değil, çünkü onlar düşman tarafıdır)
                // Veya data.side kontrolü yap. Şimdilik "Neutral" veya "Light" (Oyuncu tarafı varsayımı)
                // DAHA İYİSİ: UnitButton.cs içinde OnEnable'da kendini kapatabilir eğer tarafı uymuyorsa.
                // Şimdilik kalabalığı önlemek için sadece belirli bir tarafı ekleyelim (Örn: Light)
                if (data.side == Side.Dark) continue; 

                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, parent);
                go.name = "Unit_" + data.unitName;
                go.GetComponent<UnitButton>().Setup(data);
            }
        }

        public static void CreateTowerSlotPrefab()
        {
            EnsureDirectory();

            GameObject root = new GameObject("BaseTowerSlotPrefab", typeof(TowerSlot), typeof(BoxCollider));
            root.GetComponent<BoxCollider>().size = new Vector3(2, 0.2f, 2);
            root.GetComponent<BoxCollider>().isTrigger = true;

            // Disk Visual (Dashed brackets yerine daha modern)
            GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visuals.name = "Visuals";
            visuals.transform.SetParent(root.transform);
            visuals.transform.localScale = new Vector3(3f, 0.05f, 3f);
            visuals.transform.localPosition = new Vector3(0, 0.05f, 0);
            
            // KRİTİK: Görselin kendi collider'ını sil ki ana collider'ı (scriptin olduğu) engellemesin!
            var childCol = visuals.GetComponent<Collider>();
            if (childCol != null) Object.DestroyImmediate(childCol);
            
            Renderer rend = visuals.GetComponent<Renderer>();
            visuals.transform.localScale = new Vector3(3f, 1f, 3f); // Biraz daha dar ve yassı
            
            BoxCollider col = root.GetComponent<BoxCollider>();
            if (col != null) 
            {
                col.size = new Vector3(3f, 0.5f, 3f); 
                col.center = new Vector3(0, 0.25f, 0); // Yerde kalmaması için hafif yukarıda
                col.isTrigger = false; // Trigger olunca bazen Raycast atlayabiliyor, fiziksel olsun
            }
            
            Material slotMat = GetPersistentMaterial("BaseTowerSlot_Mat");
            slotMat.color = new Color(0, 1f, 1f, 0.8f); 
            if (slotMat.HasProperty("_BaseColor")) slotMat.SetColor("_BaseColor", new Color(0, 1f, 1f, 0.8f));
            if (slotMat.HasProperty("_EmissionColor")) 
            {
                slotMat.EnableKeyword("_EMISSION");
                slotMat.SetColor("_EmissionColor", new Color(0, 0.4f, 0.8f) * 2f);
            }
            
            rend.sharedMaterial = slotMat;
            EditorUtility.SetDirty(slotMat);

            // Selection UI (Embedded)
            string uiPath = PREFAB_PATH + "/BaseTowerSelectionPrefab.prefab";
            GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(uiPath);
            if (uiPrefab != null)
            {
                GameObject uiInstance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab);
                uiInstance.transform.SetParent(root.transform);
                uiInstance.transform.localPosition = new Vector3(0, 2, 0);
            }

            // TowerSlot component configuration
            TowerSlot slot = root.GetComponent<TowerSlot>();
            var so = new SerializedObject(slot);
            so.FindProperty("towerOffset").vector3Value = new Vector3(0, 0.5f, 0);
            so.ApplyModifiedProperties();

            // Ensure Gameplay directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Gameplay"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Prefabs", "Gameplay");
            }

            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Gameplay/BaseTowerSlotPrefab.prefab");
            Object.DestroyImmediate(root);
        }

        public static void InitializeDefaultLevelDesign(LevelData data, int levelIndex)
        {
            if (data == null) return;
            
            data.paths = new List<LevelPath>();
            LevelPath p = new LevelPath();
            p.spawnerIndex = 0;

            switch (levelIndex)
            {
                case 1: // Forest - Zig Zag
                    data.theme = LevelTheme.Forest;
                    p.points.AddRange(new Vector3[] { new Vector3(-50, 0, 20), new Vector3(25, 0, 20), new Vector3(-25, 0, 0), new Vector3(50, 0, 0) });
                    data.towerSlotCount = 10;
                    break;
                case 2: // Desert - U-Turn
                    data.theme = LevelTheme.Desert;
                    p.points.AddRange(new Vector3[] { new Vector3(-30, 0, 30), new Vector3(-30, 0, -10), new Vector3(30, 0, -10), new Vector3(30, 0, 30) });
                    data.towerSlotCount = 12;
                    break;
                case 3: // Snow - Long S
                    data.theme = LevelTheme.Snow;
                    p.points.AddRange(new Vector3[] { new Vector3(-50, 0, 40), new Vector3(50, 0, 20), new Vector3(-50, 0, 0), new Vector3(50, 0, -20) });
                    data.towerSlotCount = 15;
                    break;
            }
            data.paths.Add(p);
            EditorUtility.SetDirty(data);
        }

        public static void InitializeDefaultLevelLayouts()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                
                if (path.Contains("Level1")) InitializeDefaultLevelDesign(data, 1);
                else if (path.Contains("Level2")) InitializeDefaultLevelDesign(data, 2);
                else if (path.Contains("Level3")) InitializeDefaultLevelDesign(data, 3);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("✔ Level designs initialized. Now click 'GENERATE ALL LEVEL MAPS' to apply.");
        }

        [MenuItem("Tools/TD Setup/REGENERATE ALL LEVEL MAPS")]
        public static void GenerateAllLevelMaps()
        {
            // Eski haritaları temizle (Pembe kalıntıları önlemek için)
            if (AssetDatabase.IsValidFolder("Assets/Maps"))
            {
                AssetDatabase.DeleteAsset("Assets/Maps");
                AssetDatabase.CreateFolder("Assets", "Maps");
            }

            // Önce TÜM PREFABLARI (Üniteler, Kuleler, Mermiler) Cilala ve Büyüt
            PolishAllGameplayPrefabs();

            string[] guids = AssetDatabase.FindAssets("t:LevelData");
            foreach (string guid in guids)
            {
                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid));
                GenerateAdvancedLevelMap(data);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // YENİ: Haritalar oluşturulduktan sonra otomatik olarak LevelData'ya bağla
            AutoAssignMapsToLevelData();

            Debug.Log("✔ All Level Maps regenerated and materials persisted.");
        }

        public static void GenerateAdvancedLevelMap(LevelData data)
        {
            if (data == null) return;
            EnsureDirectory();

            string safeName = GetSafeFilename(data.levelName);
            string mapName = safeName + "_Map";
            GameObject root = new GameObject(mapName);
            
            // 1. Zemin (Sonsuzluk Hissi)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(50, 1, 50);
            
            Renderer groundRend = ground.GetComponent<Renderer>();
            Material groundMat = new Material(GetURPLitShader());
            groundMat.color = GetThemeColor(data.theme);
            groundRend.sharedMaterial = groundMat;

            // 2. Yol, Waypointler ve Spawnerlar (ÇOKLU YOL DESTEĞİ)
            List<LevelPath> levelPaths = data.paths;
            if (levelPaths == null || levelPaths.Count == 0)
            {
                // Fallback (Tek Yol)
                levelPaths = new List<LevelPath> {
                    new LevelPath {
                        spawnerIndex = 0,
                        points = new List<Vector3> {
                            new Vector3(-15, 0, 15),
                            new Vector3(0, 0, 15),
                            new Vector3(0, 0, -15),
                            new Vector3(15, 0, -15)
                        }
                    }
                };
            }

            Vector3 commonBasePoint = Vector3.zero;
            List<PathWaypoints> allEnemyPaths = new List<PathWaypoints>();
            placedPathTiles = new HashSet<string>(); // Reset cache for this map

            foreach (var pathData in levelPaths)
            {
                List<Vector3> points = pathData.points;
                if (points == null || points.Count < 2) continue;

                // Her yol için tek bir görsel hat (Artık paralel Lane'ler yok, gerçekçilik için)
                GameObject laneGo = new GameObject($"Path_{pathData.spawnerIndex}", typeof(PathWaypoints));
                laneGo.transform.SetParent(root.transform);
                PathWaypoints waypoints = laneGo.GetComponent<PathWaypoints>();
                allEnemyPaths.Add(waypoints);

                var pathSo = new SerializedObject(waypoints);
                var wpProp = pathSo.FindProperty("waypoints");
                wpProp.ClearArray();

                for (int i = 0; i < points.Count; i++)
                {
                    GameObject wp = new GameObject($"WP_{pathData.spawnerIndex}_{i}");
                    wp.transform.SetParent(laneGo.transform);
                    wp.transform.position = points[i];
                    wpProp.InsertArrayElementAtIndex(i);
                    wpProp.GetArrayElementAtIndex(i).objectReferenceValue = wp.transform;
                }
                pathSo.ApplyModifiedProperties();

                // Yol Görselleştirme
                BuildVisualPath(root.transform, points, data.theme);

                // Spawner (Bu yolun başında)
                CreateSpawnerAt(root.transform, points[0], pathData.spawnerIndex, false, new List<PathWaypoints> { waypoints });
                
                // Kule Slotları (Yol boyunca)
                CreateTowerSlotsAlongPath(root.transform, points, data.towerSlotCount / levelPaths.Count);
            }

            // 3. Multi-Base & Manual Spawner
            foreach (var bPos in data.basePoints)
            {
                CreateBaseAt(root.transform, bPos, Side.Light);
            }

            // İlk giriş noktasına düşman kalesi (Sembolik)
            if (levelPaths.Count > 0)
                CreateBaseAt(root.transform, levelPaths[0].points[0], Side.Dark);

            // Player Manual Spawner (Birinci üssün oradan çıkış yap)
            Vector3 playerSpawnOrigin = data.basePoints.Count > 0 ? data.basePoints[0] : Vector3.zero;
            List<PathWaypoints> playerManualPaths = new List<PathWaypoints>();
            
            if (levelPaths.Count > 0)
            {
                var protoPoints = levelPaths[0].points;
                GameObject pLaneGo = new GameObject("PlayerManualPath", typeof(PathWaypoints));
                pLaneGo.transform.SetParent(root.transform);
                PathWaypoints pWaypoints = pLaneGo.GetComponent<PathWaypoints>();
                playerManualPaths.Add(pWaypoints);

                var pPathSo = new SerializedObject(pWaypoints);
                var pWpProp = pPathSo.FindProperty("waypoints");
                pWpProp.ClearArray();

                for (int i = 0; i < protoPoints.Count; i++)
                {
                    int invIdx = protoPoints.Count - 1 - i;
                    GameObject wp = new GameObject("P_WP_" + i);
                    wp.transform.SetParent(pLaneGo.transform);
                    wp.transform.position = protoPoints[invIdx];
                    pWpProp.InsertArrayElementAtIndex(i);
                    pWpProp.GetArrayElementAtIndex(i).objectReferenceValue = wp.transform;
                }
                pPathSo.ApplyModifiedProperties();
            }

            CreateSpawnerAt(root.transform, playerSpawnOrigin, -1, true, playerManualPaths); // Player Manual


            // 4. Core Systems (EĞER YOKSA EKLE VEYA GÜNCELLE)
            GameObject enginePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Core/_Engine_MasterPrefab.prefab");
            if (enginePrefab != null)
            {
                Transform engineT = root.transform.Find("_Engine_MasterPrefab");
                if (engineT == null)
                {
                    GameObject engine = (GameObject)PrefabUtility.InstantiatePrefab(enginePrefab, root.transform);
                    engine.name = "_Engine_MasterPrefab";
                }
                else
                {
                    // Varsa bile içine scripti zorla ekle (Eğer prefabda unutulduysa)
                    if (engineT.GetComponent<TowerPlacementManager>() == null)
                        engineT.gameObject.AddComponent<TowerPlacementManager>();
                }
            }

            // 5. Tower Slots (Yol bazlı üretim artık döngü içinde yapılıyor, burası sadece dekorasyon öncesi temiz alan bırakır)

            // 6. Dekorasyon
            CreateDecorations(root.transform, data.theme);

            // Kaydet
            string prefabPath = "Assets/Maps/" + mapName + ".prefab";
            if (!AssetDatabase.IsValidFolder("Assets/Maps")) AssetDatabase.CreateFolder("Assets", "Maps");
            AssetDatabase.CreateAsset(groundMat, "Assets/Maps/" + mapName + "_Ground.mat");

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            data.mapPrefab = prefab;
            EditorUtility.SetDirty(data);
            
            AutoSetupCamera(root.transform);
            Object.DestroyImmediate(root);
        }

        private static void AutoSetupCamera(Transform mapTransform)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;

            // Harita sınırlarını PathWaypoints ile tam oynanabilir alana göre hesapla
            Bounds bounds = new Bounds();
            bool first = true;
            PathWaypoints[] allPaths = mapTransform.GetComponentsInChildren<PathWaypoints>();
            
            foreach (var path in allPaths)
            {
                if (path.GetWaypoints() == null) continue;
                foreach (var wp in path.GetWaypoints())
                {
                    if (wp == null) continue;
                    if (first) { bounds = new Bounds(wp.position, Vector3.zero); first = false; }
                    else bounds.Encapsulate(wp.position);
                }
            }

            if (first) return;

            Vector3 center = bounds.center;
            float maxDim = Mathf.Max(bounds.size.x, bounds.size.z);
            
            // Dinamik yükseklik ve UI Ofseti (Matematiksel Merkezleme)
            float camAngle = 65f;
            float height = Mathf.Max(40, maxDim * 0.8f); // Yükseklik çarpanı azaltıldı (Kamera yaklaştırıldı)
            
            // Fokus noktasını tam merkeze getirmek için gereken Z ofseti:
            // uiOffsetZ = -height / Tan(angle)
            float uiOffsetZ = -height / Mathf.Tan(camAngle * Mathf.Deg2Rad); 

            cam.transform.position = new Vector3(center.x, height, center.z + uiOffsetZ);
            cam.transform.rotation = Quaternion.Euler(camAngle, 0, 0); 
            
            if (cam.orthographic) cam.orthographicSize = maxDim * 0.55f;
            else cam.fieldOfView = 45;

            // PhysicsRaycaster ekle
            if (cam.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
                cam.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
        }

        private static void BuildVisualPath(Transform parent, List<Vector3> points, LevelTheme theme)
        {
            string tileName = GetThemePathTile(theme);
            GameObject tilePrefab = LoadKenneyModel(tileName);
            if (tilePrefab == null) return;

            // Z-fighting ve mükerrer çizimi engellemek için koordinat hafızası (Static olmalı ki tüm pathler için ortak olsun)
            if (placedPathTiles == null) placedPathTiles = new HashSet<string>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[i + 1];
                float dist = Vector3.Distance(start, end);
                Vector3 dir = (end - start).normalized;

                // Daha sık ve temiz bir dizilim için (Step: 1.5f)
                for (float d = 0; d < dist; d += 1.5f)
                {
                    Vector3 pos = start + dir * d;
                    
                    // Koordinatı 0.5f hassasiyetle stringe çevir (Cache anahtarı)
                    string key = $"{Mathf.Round(pos.x * 2) / 2f}_{Mathf.Round(pos.z * 2) / 2f}";
                    if (placedPathTiles.Contains(key)) continue;

                    GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, parent);
                    tile.transform.position = pos + Vector3.up * 0.05f; // Zeminden hafif yukarıda (Z-fighting koruması)
                    tile.transform.forward = dir;
                    tile.transform.localScale = new Vector3(6.0f, 2.0f, 2.5f); // Genişlik ve uzunluk biraz artırıldı
                    
                    // YENİ: Katman ataması ve Collider kontrolü (Tüm alt objeler dahil)
                    tile.layer = LayerMask.NameToLayer("Path");
                    if (tile.GetComponentInChildren<Collider>() == null)
                    {
                        var col = tile.AddComponent<BoxCollider>();
                        col.size = new Vector3(1, 0.5f, 1);
                        col.center = new Vector3(0, 0.25f, 0);
                    }
                    
                    // Tüm çocukları da aynı katmana al
                    foreach (Transform child in tile.GetComponentsInChildren<Transform>(true))
                    {
                        child.gameObject.layer = tile.layer;
                    }

                    FixMaterialRecursive(tile);
                    placedPathTiles.Add(key);
                }
            }
        }

        private static HashSet<string> placedPathTiles;

        private static void CreateTowerSlotsAlongPath(Transform parent, List<Vector3> points, int count)
        {
            string slotPath = "Assets/Prefabs/Gameplay/BaseTowerSlotPrefab.prefab";
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPath);
            if (slotPrefab == null) {
                CreateTowerSlotPrefab();
                slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPath);
            }
            if (slotPrefab == null) return;

            float totalDist = 0;
            for (int i = 0; i < points.Count - 1; i++) totalDist += Vector3.Distance(points[i], points[i+1]);

            List<Vector3> placedPositions = new List<Vector3>();
            float currentD = 8.0f; // Başlangıç noktasından biraz uzaklaş
            int placedCount = 0;
            int safetyCounter = 0;

            // Her adımda "Güvenli Bölge" kontrolü yaparak ilerle
            while (placedCount < count && currentD < totalDist - 8.0f && safetyCounter < 1000)
            {
                safetyCounter++;
                Vector3 pos = GetPointOnPath(points, currentD);
                Vector3 dir = GetDirectionOnPath(points, currentD);
                Vector3 sideDir = Vector3.Cross(dir, Vector3.up).normalized;

                // Sırayla sağa ve sola koy (Ofset 9 birim - artık daha yakın!)
                float sideSign = (placedCount % 2 == 0) ? 1 : -1;
                Vector3 candidate = pos + sideDir * sideSign * 9f;

                if (IsPositionSafe(candidate, points, placedPositions))
                {
                    CreateSingleSlotAt(parent, slotPrefab, candidate);
                    placedPositions.Add(candidate);
                    placedCount++;
                    currentD += 10f; // Minimum 10 birim ileri atla
                }
                else
                {
                    currentD += 2f; 
                }
            }
        }

        private static bool IsPositionSafe(Vector3 pos, List<Vector3> pathPoints, List<Vector3> otherSlots)
        {
            // 0. Alt UI Paneli koruması - Daha esnek hale getirildi
            if (pos.z < -25.0f) return false;

            // 1. Yolun herhangi bir parçasına (segment) olan uzaklık (7 birim güvenli mesafe)
            float minDistToPath = float.MaxValue;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                float d = DistancePointToSegment(pos, pathPoints[i], pathPoints[i+1]);
                if (d < minDistToPath) minDistToPath = d;
            }

            if (minDistToPath < 7.0f) return false; // Yüzde 80 yakınlaşma

            // 2. Diğer slotlardan uzaklık kontrolü (9 birim minimum ara)
            foreach (var s in otherSlots)
            {
                if (Vector3.Distance(pos, s) < 9f) return false;
            }

            return true;
        }

        private static float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;
            float t = Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab);
            t = Mathf.Clamp01(t);
            Vector3 closest = a + t * ab;
            return Vector3.Distance(p, closest);
        }

        private static void CreateSingleSlotAt(Transform parent, GameObject prefab, Vector3 position)
        {
            GameObject slot = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            slot.transform.position = position;
            FixMaterialRecursive(slot);
        }

        private static Vector3 GetPointOnPath(List<Vector3> points, float d)
        {
            float acc = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                float dist = Vector3.Distance(points[i], points[i+1]);
                if (acc + dist >= d)
                {
                    float t = (d - acc) / dist;
                    return Vector3.Lerp(points[i], points[i+1], t);
                }
                acc += dist;
            }
            return points[points.Count - 1];
        }

        private static Vector3 GetDirectionOnPath(List<Vector3> points, float d)
        {
            float acc = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                float dist = Vector3.Distance(points[i], points[i+1]);
                if (acc + dist >= d) return (points[i+1] - points[i]).normalized;
                acc += dist;
            }
            return Vector3.forward;
        }

        private static void CreateSpawnerAt(Transform parent, Vector3 pos, int spawnerIndex, bool isPlayer, List<PathWaypoints> paths)
        {
            GameObject spawner = new GameObject(isPlayer ? "PlayerSpawner" : (spawnerIndex == 1 ? "EnemySpawner_Dark" : "AlliedSpawner_Light"), typeof(Spawner));
            spawner.transform.SetParent(parent);
            spawner.transform.position = pos;
            
            Spawner script = spawner.GetComponent<Spawner>();
            var so = new SerializedObject(script);
            so.FindProperty("isPlayerSpawner").boolValue = isPlayer;
            so.FindProperty("spawnerIndex").intValue = spawnerIndex;
            so.FindProperty("spawnPoint").objectReferenceValue = spawner.transform;
            
            var pathsProp = so.FindProperty("assignedPaths");
            if (pathsProp != null) {
                pathsProp.ClearArray();
                for (int i = 0; i < paths.Count; i++) {
                    pathsProp.InsertArrayElementAtIndex(i);
                    pathsProp.GetArrayElementAtIndex(i).objectReferenceValue = paths[i];
                }
            }
            so.ApplyModifiedProperties();
        }

        private static void CreateBaseAt(Transform parent, Vector3 pos, Side side)
        {
            GameObject baseObj = new GameObject("Base_" + side, typeof(TowerDefence.Combat.Base));
            baseObj.transform.SetParent(parent);
            baseObj.transform.position = pos + Vector3.up * 0.5f;

            // Görsel (Kenney Tower)
            GameObject visual = LoadKenneyModel("tower-square.fbx");
            if (visual != null) {
                GameObject v = (GameObject)PrefabUtility.InstantiatePrefab(visual, baseObj.transform);
                v.transform.localPosition = Vector3.zero;
                v.transform.localScale = Vector3.one * 5f; // Base de büyük olsun
                FixMaterialRecursive(v);
            }
        }

        private static void CreateDecorations(Transform parent, LevelTheme theme)
        {
            string[] items = GetThemeDecorations(theme);
            for (int i = 0; i < 30; i++)
            {
                string itemName = items[Random.Range(0, items.Length)];
                GameObject prefab = LoadKenneyModel(itemName);
                if (prefab == null) continue;

                Vector3 pos = new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40));
                // Yoldan uzak olsun (Basit kontrol)
                if (pos.magnitude < 10) continue;

                GameObject decor = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                decor.transform.position = pos;
                decor.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                decor.transform.localScale = Vector3.one * Random.Range(3f, 6f);
                
                FixMaterialRecursive(decor);
            }
        }

        private static Shader GetURPLitShader()
        {
            // Shader arama - Son çare: "Lit" ismini içeren herhangi bir URP shader'ı
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("URP/Lit");
            
            if (s == null)
            {
                // Mevcut pipeline'dan al
                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
                    s = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultShader;
            }

            if (s == null)
            {
                // Projedeki herhangi bir URP materyalinin shader'ını çal
                string[] guids = AssetDatabase.FindAssets("t:Material");
                foreach (string g in guids)
                {
                    Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                    if (m != null && m.shader != null && m.shader.name.Contains("Universal Render Pipeline"))
                    {
                        s = m.shader;
                        break;
                    }
                }
            }
            
            if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (s == null) s = Shader.Find("Standard"); // Felaket senaryosu

            return s;
        }

        private static void FixMaterialRecursive(GameObject go)
        {
            if (go == null) return;
            Renderer rend = go.GetComponent<Renderer>();
            Shader urpShader = GetURPLitShader();

            if (rend != null)
            {
                Material[] mats = rend.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    // EĞER SLOT BOŞSA (NONE) VEYA BOZUKSA
                    bool isBroken = mats[i] == null || 
                                   mats[i].shader == null || 
                                   mats[i].shader.name.Contains("Standard") || 
                                   mats[i].shader.name.Contains("Default") || 
                                   mats[i].shader.name.Contains("Error");

                    if (isBroken)
                    {
                        Color oldColor = (mats[i] != null && mats[i].HasProperty("_Color")) ? mats[i].color : Color.white;
                        Texture oldTex = (mats[i] != null && mats[i].HasProperty("_MainTex")) ? mats[i].mainTexture : null;
                        
                        // Her seferinde yeni materyal oluşturmak yerine kalıcı bir "Fix" materyali kullanalım
                        mats[i] = GetPersistentMaterial("URP_Fixed_Mat");
                        
                        if (oldColor != Color.white) {
                            // Eğer özel bir rengi varsa, ona özel materyal yap (Nadir durum)
                            Material uniqueMat = new Material(urpShader);
                            uniqueMat.color = oldColor;
                            if (oldTex != null) uniqueMat.mainTexture = oldTex;
                            mats[i] = uniqueMat;
                        }

                        changed = true;
                    }
                }
                if (changed) rend.sharedMaterials = mats;
            }

            foreach (Transform child in go.transform)
                FixMaterialRecursive(child.gameObject);
        }

        private static Material GetPersistentMaterial(string name)
        {
            string path = $"Assets/Materials/{name}.mat";
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(GetURPLitShader());
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        public static void PolishAllGameplayPrefabs()
        {
            Shader urpShader = GetURPLitShader();
            Debug.Log($"[Polish] Using Shader: {urpShader.name}");

            // 1. Üniteleri düzelt ve büyüt
            string[] unitGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Gameplay/Units" });
            foreach (string guid in unitGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                root.transform.localScale = new Vector3(6f, 6f, 6f);
                foreach(var rend in root.GetComponentsInChildren<Renderer>())
                    rend.transform.localScale = Vector3.one;
                FixMaterialRecursive(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }

            // 2. Kule slotlarını baştan yap
            CreateTowerSlotPrefab();

            // 3. Kuleleri düzelt
            string[] towerGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Gameplay/Towers" });
            foreach (string guid in towerGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                root.transform.localScale = new Vector3(5f, 5f, 5f);
                FixMaterialRecursive(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }

            // 4. Mermileri düzelt
            string[] projGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Gameplay/Projectiles" });
            foreach (string guid in projGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                FixMaterialRecursive(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            Debug.Log("✔ MASSIVE POLISH COMPLETE: All gameplay assets are now huge and URP-compatible.");
        }

        public static void FixAllUnitMaterials()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Gameplay/Units" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                FixMaterialRecursive(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            Debug.Log("✔ Fixed materials for all units in Assets/Prefabs/Gameplay/Units");
        }

        private static GameObject LoadKenneyModel(string fileName)
        {
            string path = "Assets/kenney_tower-defense-kit/Models/FBX format/" + fileName;
            if (!fileName.EndsWith(".fbx")) path += ".fbx";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static Color GetThemeColor(LevelTheme theme)
        {
            switch (theme)
            {
                case LevelTheme.Forest: return new Color(0.1f, 0.4f, 0.1f);
                case LevelTheme.Desert: return new Color(0.8f, 0.6f, 0.2f);
                case LevelTheme.Snow: return Color.white;
                case LevelTheme.Underworld: return new Color(0.1f, 0, 0.1f);
                default: return Color.gray;
            }
        }

        private static string GetThemePathTile(LevelTheme theme)
        {
            switch (theme)
            {
                case LevelTheme.Snow: return "snow-tile-straight.fbx";
                default: return "tile-straight.fbx";
            }
        }

        private static string[] GetThemeDecorations(LevelTheme theme)
        {
            switch (theme)
            {
                case LevelTheme.Forest: return new string[] { "detail-tree.fbx", "detail-tree-large.fbx", "detail-rocks.fbx" };
                case LevelTheme.Desert: return new string[] { "detail-rocks-large.fbx", "detail-dirt.fbx" };
                case LevelTheme.Snow: return new string[] { "snow-detail-tree.fbx", "snow-detail-crystal.fbx" };
                case LevelTheme.Underworld: return new string[] { "detail-crystal-large.fbx", "detail-crystal.fbx" };
                default: return new string[] { "detail-rocks.fbx" };
            }
        }

        private static GameObject CreateBaseCanvas(string name)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas c = root.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler cs = root.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            
            return root;
        }

        private static GameObject AddPanelToMaster(Transform master, string prefabPath, bool active)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, master);
                instance.SetActive(active);
                return instance;
            }
            return null;
        }

        private static void AutoAssignOrPlaceholder(LevelData data)
        {
            if (data == null) return;
            if (data.mapPrefab != null) return; // Zaten atanmış

            // 1. Mevcut haritayı ara (Gelişmiş Filtreleme)
            string[] mapGuids = AssetDatabase.FindAssets($"{data.levelID}_Map t:GameObject");
            
            // Fallback: Sayısal önek (1._Level_Map vb. + Offset desteği)
            if (mapGuids.Length == 0)
            {
                string numStr = data.levelID.Replace("Level", "");
                if (int.TryParse(numStr, out int num))
                {
                    mapGuids = AssetDatabase.FindAssets($"{num}._Level_Map t:GameObject");
                    if (mapGuids.Length == 0 && num > 1)
                        mapGuids = AssetDatabase.FindAssets($"{num - 1}._Level_Map t:GameObject");
                }
            }

            // Fallback: Test Level veya Bölüm Adı
            if (mapGuids.Length == 0)
            {
                if (data.levelID == "Level1" || data.name.ToLower().Contains("test"))
                    mapGuids = AssetDatabase.FindAssets("Test_Level_Map t:GameObject");
                
                if (mapGuids.Length == 0)
                    mapGuids = AssetDatabase.FindAssets($"{data.name}_Map t:GameObject");
            }

            if (mapGuids.Length > 0)
            {
                string mapPath = AssetDatabase.GUIDToAssetPath(mapGuids[0]);
                data.mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPath);
            }
            else
            {
                // 2. Bulunamadıysa Placeholder oluştur
                CreatePlaceholderMap(data.levelID);
                string mapPath = $"Assets/Maps/{data.levelID}_Map.prefab";
                data.mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPath);
            }
            
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        public static void AutoAssignMapsToLevelData()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData");
            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (data == null || data.mapPrefab != null) continue;

                // levelID veya name üzerinden harita ara (Gelişmiş)
                string[] mapGuids = AssetDatabase.FindAssets($"{data.levelID}_Map t:GameObject");
                
                if (mapGuids.Length == 0)
                {
                    string numStr = data.levelID.Replace("Level", "");
                    if (int.TryParse(numStr, out int num))
                    {
                        mapGuids = AssetDatabase.FindAssets($"{num}._Level_Map t:GameObject");
                        if (mapGuids.Length == 0 && num > 1)
                            mapGuids = AssetDatabase.FindAssets($"{num - 1}._Level_Map t:GameObject");
                    }
                }

                if (mapGuids.Length == 0 && (data.levelID == "Level1" || data.name.ToLower().Contains("test")))
                    mapGuids = AssetDatabase.FindAssets("Test_Level_Map t:GameObject");

                if (mapGuids.Length == 0) 
                    mapGuids = AssetDatabase.FindAssets($"{data.name}_Map t:GameObject");

                if (mapGuids.Length > 0)
                {
                    string mapPath = AssetDatabase.GUIDToAssetPath(mapGuids[0]);
                    data.mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPath);
                    EditorUtility.SetDirty(data);
                    count++;
                }
            }
            if (count > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"✔ {count} LevelData için harita otomatik bağlandı.");
            }
        }

        public static void CreatePlaceholderMap(string levelID)
        {
            string mapPath = $"Assets/Maps/{levelID}_Map.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(mapPath) != null) return;

            GameObject root = new GameObject(levelID + "_Map");
            
            // Ground (Large)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(200, 0.1f, 200); // Sonsuzluk hissi için büyük
            ground.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.18f);
            ground.layer = LayerMask.NameToLayer("Default");

            // Decorative Horizon (Optional)
            GameObject horizon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            horizon.name = "HorizonSphere";
            horizon.transform.SetParent(root.transform);
            horizon.transform.localScale = new Vector3(1000, 1000, 1000);
            horizon.transform.position = new Vector3(0, -500, 0);
            horizon.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.12f);
            var sphereCol = horizon.GetComponent<Collider>();
            if (sphereCol != null) Object.DestroyImmediate(sphereCol);

            // Visual Path (Simplified)
            GameObject pathVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathVisual.name = "Path_Visual";
            pathVisual.transform.SetParent(root.transform);
            pathVisual.transform.localScale = new Vector3(8, 0.11f, 18); // Görsel olarak daha geniş
            pathVisual.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.45f);
            pathVisual.layer = LayerMask.NameToLayer("Path");
            pathVisual.GetComponent<Collider>().isTrigger = true;

            // Multiple Lanes (Paths)
            List<PathWaypoints> lanePaths = new List<PathWaypoints>();
            string[] laneNames = { "Path_Left", "Path_Center", "Path_Right" };
            float[] laneOffsets = { -2.5f, 0f, 2.5f };

            for (int i = 0; i < laneNames.Length; i++)
            {
                GameObject laneGo = new GameObject(laneNames[i]);
                laneGo.transform.SetParent(root.transform);
                PathWaypoints laneComp = laneGo.AddComponent<PathWaypoints>();
                lanePaths.Add(laneComp);

                // Add Waypoints for this lane
                List<Transform> wps = new List<Transform>();
                Vector3[] points = {
                    new Vector3(laneOffsets[i], 0.1f, -8),
                    new Vector3(laneOffsets[i] + 4, 0.1f, -3),
                    new Vector3(laneOffsets[i] - 4, 0.1f, 3),
                    new Vector3(laneOffsets[i], 0.1f, 8)
                };

                for (int pIdx = 0; pIdx < points.Length; pIdx++)
                {
                    GameObject wp = new GameObject($"WP_{pIdx}");
                    wp.transform.SetParent(laneGo.transform);
                    wp.transform.position = points[pIdx];
                    wps.Add(wp.transform);
                }

                var wpSo = new SerializedObject(laneComp);
                var wpProp = wpSo.FindProperty("waypoints");
                wpProp.ClearArray();
                for (int p = 0; p < wps.Count; p++)
                {
                    wpProp.InsertArrayElementAtIndex(p);
                    wpProp.GetArrayElementAtIndex(p).objectReferenceValue = wps[p];
                }
                wpSo.ApplyModifiedProperties();
            }

            // Kulvar Spawnerları (Start end)
            for (int l = 0; l < 3; l++)
            {
                GameObject spGo = new GameObject("Spawner_Lane_" + l);
                spGo.transform.SetParent(root.transform);
                spGo.transform.position = new Vector3(laneOffsets[l], 0.5f, -8);
                var sc = spGo.AddComponent<Spawner>();
                sc.spawnerIndex = l;
                
                var so = new SerializedObject(sc);
                var pProp = so.FindProperty("assignedPaths");
                pProp.ClearArray();
                pProp.InsertArrayElementAtIndex(0);
                pProp.GetArrayElementAtIndex(0).objectReferenceValue = lanePaths[l];
                so.ApplyModifiedProperties();
                
                so.FindProperty("spawnPoint").objectReferenceValue = spGo.transform;
                so.ApplyModifiedProperties();
            }

            // Player Spawner (Fallback)
            GameObject pSpawner = new GameObject("PlayerSpawner");
            pSpawner.transform.SetParent(root.transform);
            pSpawner.transform.position = new Vector3(-5, 0.5f, 0);
            var pSpawnComp = pSpawner.AddComponent<Spawner>();
            pSpawnComp.isPlayerSpawner = true;
            
            var pSpawnSo = new UnityEditor.SerializedObject(pSpawnComp);
            pSpawnSo.FindProperty("spawnPoint").objectReferenceValue = pSpawner.transform;
            pSpawnSo.ApplyModifiedProperties();

            // Base
            GameObject bse = new GameObject("Base");
            bse.transform.SetParent(root.transform);
            bse.transform.position = new Vector3(0, 0.5f, 8);
            bse.AddComponent<Base>();
            // Görsel
            GameObject bseVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bseVisual.transform.SetParent(bse.transform);
            bseVisual.transform.localPosition = Vector3.zero;
            bseVisual.transform.localScale = new Vector3(1, 0.5f, 1);

            if (!AssetDatabase.IsValidFolder("Assets/Maps")) System.IO.Directory.CreateDirectory("Assets/Maps");
            PrefabUtility.SaveAsPrefabAsset(root, mapPath);
            GameObject.DestroyImmediate(root);
            Debug.Log($"✔ Placeholder Map created: {mapPath}");
            AssetDatabase.Refresh();
        }

        private static void SaveAndCleanup(GameObject root, string fileName)
        {
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH + "/" + fileName + ".prefab");
            Debug.Log($"✔ Master Prefab saved: {PREFAB_PATH}/{fileName}.prefab");
            GameObject.DestroyImmediate(root);
        }

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_PATH))
            {
                System.IO.Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }
        }

        private static string GetSafeFilename(string name)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            string safeName = string.Join("_", name.Split(invalidChars, System.StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "_");
            return safeName;
        }
    }
}
