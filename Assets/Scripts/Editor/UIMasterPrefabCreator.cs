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
            rootRT.localScale = new Vector3(0.03f, 0.03f, 0.005f); 
            rootRT.localRotation = Quaternion.Euler(65f, 0f, 0f);

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
            root.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            // --- 2D Scroll View (Massive Area) ---
            GameObject scroll = new GameObject("TreeScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scroll.transform.SetParent(root.transform, false);
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            RectTransform scrollRT = scroll.GetComponent<RectTransform>();
            SetStretch(scrollRT);
            scrollRT.offsetMax = new Vector2(0, -100); // Header'ın altı

            // Content (The large canvas)
            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scroll.transform, false);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.sizeDelta = new Vector2(3000, 3000); // 3Kx3K alan
            contentRT.anchoredPosition = Vector2.zero;

            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            sr.content = contentRT;
            sr.horizontal = true;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 50;

            // --- Header (Fixed at top, created AFTER scroll to stay on top) ---
            GameObject header = CreateUINode(root.transform, "Header", new Vector2(0, 100));
            RectTransform hRT = header.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0, 1); hRT.anchorMax = new Vector2(1, 1);
            hRT.pivot = new Vector2(0.5f, 1);
            hRT.anchoredPosition = Vector2.zero;
            header.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            TextMeshProUGUI titleTmp = CreateTMP(header.transform, "Title", "LEGENDARY SKILL TREE", 48, 600, 80);
            titleTmp.color = new Color(1f, 0.85f, 0.2f);
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Geri Butonu
            Button backBtn = CreateButton(header.transform, "BackButton", "← BACK", 180, 60, 24);
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f); backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(110, 0);

            SkillTreeUI uiScript = root.GetComponent<SkillTreeUI>();

            // --- Detail Panel (Fixed at center, created AFTER scroll) ---
            GameObject detail = CreateUINode(root.transform, "DetailPanel", new Vector2(500, 600));
            detail.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            Image detailIcon = CreateImage(detail.transform, "Icon", new Vector2(150, 150));
            detailIcon.rectTransform.anchoredPosition = new Vector2(0, 180);
            
            TextMeshProUGUI detailName = CreateTMP(detail.transform, "Name", "Skill Name", 32, 450, 50);
            detailName.rectTransform.anchoredPosition = new Vector2(0, 80);
            detailName.alignment = TextAlignmentOptions.Center;
            detailName.color = Color.yellow;

            TextMeshProUGUI detailDesc = CreateTMP(detail.transform, "Desc", "Skill description goes here...", 20, 450, 150);
            detailDesc.rectTransform.anchoredPosition = new Vector2(0, -30);
            detailDesc.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI detailCost = CreateTMP(detail.transform, "Cost", "Cost: 500 Karma & 50 Crystals", 22, 450, 40);
            detailCost.rectTransform.anchoredPosition = new Vector2(0, -140);
            detailCost.alignment = TextAlignmentOptions.Center;
            detailCost.color = Color.green;

            Button buyBtn = CreateButton(detail.transform, "BuyButton", "BUY SKILL", 300, 60, 24);
            buyBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -220);

            // Close button for detail panel
            Button closeBtn = CreateButton(detail.transform, "CloseButton", "X", 50, 50, 20);
            closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(220, 270);

            // Feedback Text
            TextMeshProUGUI feedbackTmp = CreateTMP(root.transform, "FeedbackText", "", 24, 600, 40);
            feedbackTmp.rectTransform.anchoredPosition = new Vector2(0, -150);
            feedbackTmp.alignment = TextAlignmentOptions.Center;

            // Link master references
            var so = new SerializedObject(uiScript);
            so.FindProperty("totalKarmaText").objectReferenceValue = null; // Use global TopPanel instead
            so.FindProperty("totalCrystalsText").objectReferenceValue = null;
            so.FindProperty("feedbackText").objectReferenceValue = feedbackTmp;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("contentRect").objectReferenceValue = contentRT;
            
            so.FindProperty("detailPanel").objectReferenceValue = detail;
            so.FindProperty("detailIcon").objectReferenceValue = detailIcon;
            so.FindProperty("detailName").objectReferenceValue = detailName;
            so.FindProperty("detailDesc").objectReferenceValue = detailDesc;
            so.FindProperty("detailCost").objectReferenceValue = detailCost;
            so.FindProperty("detailBuyButton").objectReferenceValue = buyBtn;
            so.FindProperty("detailCloseButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedProperties();

            // --- Generate Nodes ---
            string[] guids = AssetDatabase.FindAssets("t:SkillNodeData", new[] { "Assets/Data/Skills" });
            var nodeUIList = new System.Collections.Generic.List<SkillNodeUI>();

            foreach (string guid in guids)
            {
                SkillNodeData data = AssetDatabase.LoadAssetAtPath<SkillNodeData>(AssetDatabase.GUIDToAssetPath(guid));
                if (data == null) continue;

                GameObject node = new GameObject(data.skillName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(SkillNodeUI));
                node.transform.SetParent(contentRT.transform, false);
                RectTransform nRT = node.GetComponent<RectTransform>();
                nRT.sizeDelta = new Vector2(160, 160);
                nRT.anchoredPosition = Vector2.zero; // Auto layout will handle this at runtime

                node.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 1f);

                // Icon
                Image icon = CreateImage(node.transform, "Icon", new Vector2(80, 80));
                icon.rectTransform.anchoredPosition = new Vector2(0, 15);
                icon.color = data.side == Side.Light ? new Color(1, 0.9f, 0.5f) : new Color(0.6f, 0.4f, 0.8f);

                // Name
                TextMeshProUGUI nameTmp = CreateTMP(node.transform, "Name", data.skillName, 14, 150, 40);
                nameTmp.rectTransform.anchoredPosition = new Vector2(0, -45);
                nameTmp.alignment = TextAlignmentOptions.Center;

                // Cost
                TextMeshProUGUI costTmp = CreateTMP(node.transform, "Cost", $"{data.karmaCost} K", 12, 100, 20);
                costTmp.rectTransform.anchoredPosition = new Vector2(0, -60);
                costTmp.color = Color.green;
                costTmp.alignment = TextAlignmentOptions.Center;

                // Type Label
                TextMeshProUGUI typeTmp = CreateTMP(node.transform, "TypeText", "PASSIVE", 10, 100, 20);
                typeTmp.rectTransform.anchoredPosition = new Vector2(0, 60);
                typeTmp.alignment = TextAlignmentOptions.Center;

                // Overlays
                Image locked = CreateImage(node.transform, "LockedOverlay", new Vector2(140, 140));
                locked.color = new Color(0, 0, 0, 0.7f);
                locked.gameObject.SetActive(false);

                Image purchased = CreateImage(node.transform, "PurchasedOverlay", new Vector2(140, 140));
                purchased.color = new Color(0, 1, 0, 0.3f);
                purchased.gameObject.SetActive(false);

                // Link SkillNodeUI
                SkillNodeUI snUI = node.GetComponent<SkillNodeUI>();
                var snSo = new SerializedObject(snUI);
                snSo.FindProperty("skillData").objectReferenceValue = data;
                snSo.FindProperty("iconImage").objectReferenceValue = icon;
                snSo.FindProperty("buyButton").objectReferenceValue = node.GetComponent<Button>();
                snSo.FindProperty("costText").objectReferenceValue = costTmp;
                snSo.FindProperty("typeText").objectReferenceValue = typeTmp;
                snSo.FindProperty("lockedOverlay").objectReferenceValue = locked;
                snSo.FindProperty("purchasedOverlay").objectReferenceValue = purchased;
                snSo.ApplyModifiedProperties();

                nodeUIList.Add(snUI);
            }

            // Listeyi SkillTreeUI'ya bağla
            var uiSo = new SerializedObject(uiScript);
            var nodesProp = uiSo.FindProperty("allNodes");
            nodesProp.arraySize = nodeUIList.Count;
            for (int i = 0; i < nodeUIList.Count; i++)
                nodesProp.GetArrayElementAtIndex(i).objectReferenceValue = nodeUIList[i];
            uiSo.ApplyModifiedProperties();

            SaveAndCleanup(root, "SkillTreePanel");
        }

        public static void CreateCompendiumPanelPrefab()
        {
            EnsureDirectory();
            GameObject root = new GameObject("CompendiumPanel", typeof(RectTransform), typeof(Image), typeof(CompendiumUI));
            SetStretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);

            // Title
            TextMeshProUGUI title = CreateTMP(root.transform, "Title", "CHARACTER COMPENDIUM", 42, 600, 80);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1);
            title.rectTransform.anchoredPosition = new Vector2(0, -60);

            // --- Left Side: Vertical Scroll List ---
            GameObject listRoot = CreateUINode(root.transform, "ListRoot", new Vector2(800, 0));
            RectTransform listRT = listRoot.GetComponent<RectTransform>();
            listRT.anchorMin = new Vector2(0, 0);
            listRT.anchorMax = new Vector2(0, 1);
            listRT.pivot = new Vector2(0, 0.5f);
            listRT.offsetMin = new Vector2(50, 50);
            listRT.offsetMax = new Vector2(850, -120); 

            GameObject scroll = new GameObject("VerticalScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scroll.transform.SetParent(listRoot.transform, false);
            SetStretch(scroll.GetComponent<RectTransform>());
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scroll.transform, false);
            SetStretch(viewport.GetComponent<RectTransform>());

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            GridLayoutGroup glg = content.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(280, 240);
            glg.spacing = new Vector2(15, 15);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2; // Width is 800 now, so 2 columns of 280 (560) + spacing will fit nicely
            
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            sr.content = contentRT;
            sr.viewport = viewport.GetComponent<RectTransform>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 35;

            // --- Right Side: Detail Panel ---
            GameObject detail = CreateUINode(root.transform, "DetailPanel", new Vector2(0, 0));
            RectTransform detailRT = detail.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0, 0);
            detailRT.anchorMax = new Vector2(1, 1);
            detailRT.offsetMin = new Vector2(900, 50);
            detailRT.offsetMax = new Vector2(-50, -120);
            detail.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            Image detailIcon = null;
            
            TextMeshProUGUI detailName = CreateTMP(detail.transform, "Name", "Select a character", 36, 500, 60);
            detailName.rectTransform.anchoredPosition = new Vector2(0, 50);
            detailName.rectTransform.localScale = new Vector3(2, 2, 2);
            detailName.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI detailStats = CreateTMP(detail.transform, "Stats", "", 24, 500, 120);
            detailStats.rectTransform.anchoredPosition = new Vector2(0, -100);
            detailStats.rectTransform.localScale = new Vector3(2, 2, 2);
            detailStats.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI detailLore = CreateTMP(detail.transform, "Lore", "", 20, 550, 200);
            detailLore.rectTransform.anchoredPosition = new Vector2(0, -350);
            detailLore.rectTransform.localScale = new Vector3(2, 2, 2);
            detailLore.alignment = TextAlignmentOptions.Center;

            // Back Button
            Button backBtn = CreateButton(root.transform, "BackButton", "← BACK", 180, 60, 24);
            backBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            backBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, -50);

            // Link UI
            CompendiumUI ui = root.GetComponent<CompendiumUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("heroContent").objectReferenceValue = content.transform; // Artık tek bir content var
            so.FindProperty("unitContent").objectReferenceValue = null;
            so.FindProperty("detailPanel").objectReferenceValue = detail;
            so.FindProperty("detailIcon").objectReferenceValue = detailIcon;
            so.FindProperty("detailName").objectReferenceValue = detailName;
            so.FindProperty("detailStats").objectReferenceValue = detailStats;
            so.FindProperty("detailLore").objectReferenceValue = detailLore;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.ApplyModifiedProperties();

            SaveAndCleanup(root, "CompendiumPanel");
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
            GameObject btnContainer = CreateUINode(mainMenuPanel.transform, "ButtonContainer", new Vector2(450, 550));
            btnContainer.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f); // Subtle dark box
            VerticalLayoutGroup vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 15;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;

            // 3c. Buttons
            Button playBtn  = CreateButton(btnContainer.transform, "PlayButton",      "PLAY",  400, 80, 36);
            Button skillBtn = CreateButton(btnContainer.transform, "SkillTreeButton", "SKILL TREE",400, 80, 36);
            Button heroBtn  = CreateButton(btnContainer.transform, "HeroesButton",    "HEROES",    400, 80, 36);
            Button compBtn  = CreateButton(btnContainer.transform, "CompendiumButton", "COMPENDIUM", 400, 80, 36);
            Button quitBtn  = CreateButton(btnContainer.transform, "QuitButton",      "QUIT",        400, 80, 36);

            // 3d. Daily Reward Button (On Main Menu)
            Button dailyBtn = CreateButton(mainMenuPanel.transform, "DailyRewardButton", "DAILY REWARD", 250, 80, 20);
            RectTransform dailyRT = dailyBtn.GetComponent<RectTransform>();
            dailyRT.anchorMin = new Vector2(1, 0);
            dailyRT.anchorMax = new Vector2(1, 0);
            dailyRT.anchoredPosition = new Vector2(-160, 100);
            dailyBtn.GetComponent<Image>().color = new Color(0.15f, 0.4f, 0.15f); // Darker green
            
            TextMeshProUGUI timerText = CreateTMP(dailyBtn.transform, "Timer", "Available!", 16, 250, 30);
            timerText.rectTransform.anchorMin = new Vector2(0.5f, 0);
            timerText.rectTransform.anchorMax = new Vector2(0.5f, 0);
            timerText.rectTransform.anchoredPosition = new Vector2(0, -25); // Slightly below the button text, inside button area
            timerText.color = Color.green;

            // 4. Panels
            GameObject levelPanel = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/LevelSelectionPanel.prefab", false);
            GameObject skillPanel = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/SkillTreePanel.prefab",   false);
            GameObject heroPanel  = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/HeroShopPanel.prefab",    false);
            GameObject sidePanel  = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/SideSelectionPanel.prefab", false);
            GameObject compPanel  = AddPanelToMaster(root.transform, "Assets/Prefabs/UI/CompendiumPanel.prefab",   false);

            // 4b. Daily Reward Panel (30-Day Calendar)
            GameObject drBackdrop = CreateUINode(root.transform, "DailyRewardBackdrop", new Vector2(4000, 4000));
            drBackdrop.SetActive(false);
            drBackdrop.AddComponent<Image>().color = new Color(0, 0, 0, 1f);
            drBackdrop.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            GameObject drPanel = CreateUINode(root.transform, "DailyRewardPanel", new Vector2(1400, 850));
            drPanel.SetActive(false);
            drPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);
            
            TextMeshProUGUI drTitle = CreateTMP(drPanel.transform, "Title", "DAILY REWARD", 48, 600, 60);
            drTitle.rectTransform.anchorMin = new Vector2(0.5f, 1);
            drTitle.rectTransform.anchorMax = new Vector2(0.5f, 1);
            drTitle.rectTransform.anchoredPosition = new Vector2(0, -60);

            // 30-Day Grid
            GameObject gridGO = CreateUINode(drPanel.transform, "Grid", new Vector2(1300, 550));
            gridGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(160, 100);
            glg.spacing = new Vector2(15, 15);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 7;

            // Info texts
            TextMeshProUGUI drTimer = CreateTMP(drPanel.transform, "TimerText", "CLAIM NOW!", 22, 400, 40);
            drTimer.rectTransform.anchoredPosition = new Vector2(0, -260);
            drTimer.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI drInfo = CreateTMP(drPanel.transform, "Info", "Day 1 reward is ready!", 18, 500, 30);
            drInfo.rectTransform.anchoredPosition = new Vector2(0, -290);
            drInfo.alignment = TextAlignmentOptions.Center;
            
            Button claimBtn = CreateButton(drPanel.transform, "ClaimButton", "CLAIM", 200, 55, 24);
            claimBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -320);
            claimBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f);
            
            Button drCloseBtn = CreateButton(drPanel.transform, "CloseButton", "X", 50, 50, 20);
            drCloseBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            drCloseBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            drCloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, -30);

            // DailyRewardUI component
            DailyRewardUI drUI = drPanel.AddComponent<DailyRewardUI>();
            var drSO = new UnityEditor.SerializedObject(drUI);
            drSO.FindProperty("gridContainer").objectReferenceValue = gridGO.transform;
            drSO.FindProperty("backdrop").objectReferenceValue = drBackdrop;
            drSO.FindProperty("timerText").objectReferenceValue = drTimer;
            drSO.FindProperty("infoText").objectReferenceValue = drInfo;
            drSO.FindProperty("claimButton").objectReferenceValue = claimBtn;
            drSO.FindProperty("closeButton").objectReferenceValue = drCloseBtn;
            drSO.ApplyModifiedProperties();

            // 5. Top Currency Panel
            GameObject topPanel = CreateUINode(root.transform, "TopPanel", new Vector2(0, 100));
            RectTransform topRT = topPanel.GetComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 1); topRT.anchorMax = new Vector2(1, 1);
            topRT.offsetMin = Vector2.zero; topRT.offsetMax = Vector2.zero;
            topRT.anchoredPosition = new Vector2(0, -60);

            // Semi-transparent background for currency
            GameObject curBg = CreateUINode(topPanel.transform, "CurrencyBG", new Vector2(500, 70));
            curBg.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            RectTransform curBgRT = curBg.GetComponent<RectTransform>();
            curBgRT.anchorMin = new Vector2(1, 0.5f); curBgRT.anchorMax = new Vector2(1, 0.5f);
            curBgRT.anchoredPosition = new Vector2(-280, 0);

            GameObject currencyCont = CreateUINode(curBg.transform, "CurrencyContainer", new Vector2(480, 60));
            HorizontalLayoutGroup curHlg = currencyCont.AddComponent<HorizontalLayoutGroup>();
            curHlg.childAlignment = TextAnchor.MiddleCenter;
            curHlg.spacing = 30;
            curHlg.childControlWidth = true; curHlg.childControlHeight = true;
            curHlg.childForceExpandWidth = false;
            SetStretch(currencyCont.GetComponent<RectTransform>());

            // Karma Box
            GameObject karmaBox = CreateUINode(currencyCont.transform, "Karma", new Vector2(200, 50));
            HorizontalLayoutGroup kHlg = karmaBox.AddComponent<HorizontalLayoutGroup>();
            kHlg.spacing = 5; kHlg.childAlignment = TextAnchor.MiddleRight;
            
            TextMeshProUGUI kLabel = CreateTMP(karmaBox.transform, "Label", "KARMA:", 18, 80, 40);
            kLabel.color = Color.gray;
            TextMeshProUGUI karmaVal = CreateTMP(karmaBox.transform, "Value", "0", 24, 100, 40);
            karmaVal.alignment = TextAlignmentOptions.Left;
            karmaVal.color = Color.yellow;

            // Crystal Box
            GameObject crystalBox = CreateUINode(currencyCont.transform, "Crystals", new Vector2(220, 50));
            HorizontalLayoutGroup cHlg = crystalBox.AddComponent<HorizontalLayoutGroup>();
            cHlg.spacing = 5; cHlg.childAlignment = TextAnchor.MiddleLeft;
            
            TextMeshProUGUI cLabel = CreateTMP(crystalBox.transform, "Label", "CRYSTALS:", 18, 100, 40);
            cLabel.color = Color.gray;
            TextMeshProUGUI crystalVal = CreateTMP(crystalBox.transform, "Value", "0", 24, 100, 40);
            crystalVal.alignment = TextAlignmentOptions.Left;
            crystalVal.color = Color.cyan;

            // 6. MainMenuController bağlantıları
            MainMenuController mc = root.AddComponent<MainMenuController>();
            var so = new UnityEditor.SerializedObject(mc);
            so.FindProperty("mainMenuPanel").objectReferenceValue    = mainMenuPanel;
            so.FindProperty("levelSelectPanel").objectReferenceValue = levelPanel;
            so.FindProperty("skillTreePanel").objectReferenceValue   = skillPanel;
            so.FindProperty("heroShopPanel").objectReferenceValue    = heroPanel;
            so.FindProperty("sideSelectionPanel").objectReferenceValue = sidePanel;
            so.FindProperty("compendiumPanel").objectReferenceValue  = compPanel;
            
            so.FindProperty("karmaText").objectReferenceValue = karmaVal;
            so.FindProperty("crystalText").objectReferenceValue = crystalVal;
            so.FindProperty("dailyRewardPanel").objectReferenceValue = drPanel;
            so.FindProperty("dailyRewardBackdrop").objectReferenceValue = drBackdrop;
            so.FindProperty("dailyRewardButton").objectReferenceValue = dailyBtn;
            so.FindProperty("dailyRewardTimerText").objectReferenceValue = timerText;

            so.FindProperty("playButton").objectReferenceValue       = playBtn;
            so.FindProperty("skillTreeButton").objectReferenceValue  = skillBtn;
            so.FindProperty("heroesButton").objectReferenceValue     = heroBtn;
            so.FindProperty("compendiumButton").objectReferenceValue = compBtn;
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

            // Scroll View Root
            GameObject scrollView = CreateUINode(root.transform, "Scroll View", new Vector2(1200, 650));
            scrollView.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            
            // Viewport
            GameObject viewport = CreateUINode(scrollView.transform, "Viewport", new Vector2(1200, 650));
            SetStretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            
            // Level Container (Grid) - Now inside Viewport
            GameObject container = CreateUINode(viewport.transform, "LevelContainer", new Vector2(1200, 650));
            SetStretch(container.GetComponent<RectTransform>());
            
            // Set ScrollRect references
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = container.GetComponent<RectTransform>();
            
            GridLayoutGroup glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(240, 280);
            glg.spacing = new Vector2(40, 40);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;

            // Make the container expand based on its children so scrolling works
            ContentSizeFitter csf = container.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.MinSize;

            // Ensure the content starts at the top
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.pivot = new Vector2(0.5f, 1f); // Set pivot to top center
            containerRT.anchorMin = new Vector2(0f, 1f); // Anchor to top left
            containerRT.anchorMax = new Vector2(1f, 1f); // Anchor to top right
            containerRT.anchoredPosition = Vector2.zero; // Reset position to top

            // Back Button
            Button backBtn = CreateButton(root.transform, "BackButton", "← BACK", 180, 60, 22);
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 1);
            backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.anchoredPosition = new Vector2(30, -20);

            // Difficulty Selection Row
            GameObject diffRow = CreateUINode(root.transform, "DifficultySelectionRow", new Vector2(600, 60));
            RectTransform diffRowRT = diffRow.GetComponent<RectTransform>();
            diffRowRT.anchorMin = new Vector2(0.5f, 1f);
            diffRowRT.anchorMax = new Vector2(0.5f, 1f);
            diffRowRT.anchoredPosition = new Vector2(0, -170); // Below Title

            HorizontalLayoutGroup hlg = diffRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            Button btnNormal = CreateButton(diffRow.transform, "BtnDiffNormal", "NORMAL", 180, 50, 18);
            Button btnHard = CreateButton(diffRow.transform, "BtnDiffHard", "HARD", 180, 50, 18);
            Button btnExpert = CreateButton(diffRow.transform, "BtnDiffExpert", "EXPERT", 180, 50, 18);

            // Link LevelSelectionUI references
            LevelSelectionUI ui = root.GetComponent<LevelSelectionUI>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("container").objectReferenceValue = container.transform;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("btnDiffNormal").objectReferenceValue = btnNormal;
            so.FindProperty("btnDiffHard").objectReferenceValue = btnHard;
            so.FindProperty("btnDiffExpert").objectReferenceValue = btnExpert;
            if (levelBtnPrefab != null)
                so.FindProperty("levelButtonPrefab").objectReferenceValue = levelBtnPrefab;

            // Tüm LevelData ScriptableObject'leri otomatik bul ve listeye ekle
            string[] guids = AssetDatabase.FindAssets("t:LevelData");
            
            // Level'ları topla ve isme göre (doğal sayısal sıralama ile) sırala
            System.Collections.Generic.List<LevelData> allLevels = new System.Collections.Generic.List<LevelData>();
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                AutoAssignOrPlaceholder(levelData);
                allLevels.Add(levelData);
            }

            // Doğal sıralama için gelişmiş sayısal ayrıştırma (Regex ile)
            allLevels.Sort((a, b) => {
                string nameA = a.levelName != null ? a.levelName : a.name;
                string nameB = b.levelName != null ? b.levelName : b.name;
                
                // İsimlerin içinden sadece sayıları çek
                int numA = ExtractLevelNumber(nameA);
                int numB = ExtractLevelNumber(nameB);
                
                // Eğer ikisinde de sayı bulunduysa sayısal karşılaştır
                if (numA != -1 && numB != -1)
                {
                    return numA.CompareTo(numB);
                }
                
                // Sayı yoksa normal string karşılaştırmasına dön
                return string.Compare(nameA, nameB);
            });

            var levelsProp = so.FindProperty("levels");
            levelsProp.arraySize = allLevels.Count;
            for (int i = 0; i < allLevels.Count; i++)
            {
                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = allLevels[i];
            }
            Debug.Log($"✔ LevelSelectionPanel: {allLevels.Count} LevelData otomatik yüklendi ve sıralandı.");
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH + "/LevelSelectionPanel.prefab");
            GameObject.DestroyImmediate(root);
        }

        // Metnin içindeki ilk sayıyı bulur ve döndürür. (Örn: "Level 10" -> 10)
        private static int ExtractLevelNumber(string text)
        {
            if (string.IsNullOrEmpty(text)) return -1;
            
            var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int result))
            {
                return result;
            }
            return -1;
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
            nameTxt.enableAutoSizing = true;
            nameTxt.fontSizeMin = 12;
            nameTxt.fontSizeMax = 28;

            // Preview Image alanı
            GameObject preview = new GameObject("PreviewImage", typeof(RectTransform), typeof(Image));
            preview.transform.SetParent(btn.transform, false);
            RectTransform previewRT = preview.GetComponent<RectTransform>();
            previewRT.anchorMin = new Vector2(0.1f, 0.2f);
            previewRT.anchorMax = new Vector2(0.9f, 0.85f);
            previewRT.offsetMin = new Vector2(0, -20); // bottom = -20
            previewRT.offsetMax = new Vector2(0, 10);   // top = 10
            previewRT.localScale = new Vector3(1f, 0.5f, 1f); // scale Y = 0.5
            preview.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);

            // Zorluk Container (3 yıldız)
            GameObject diffContainer = new GameObject("DifficultyContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            diffContainer.transform.SetParent(btn.transform, false);
            RectTransform diffRT = diffContainer.GetComponent<RectTransform>();
            diffRT.anchorMin = new Vector2(0.5f, 0);
            diffRT.anchorMax = new Vector2(0.5f, 0);
            diffRT.sizeDelta = new Vector2(120, 30);
            diffRT.anchoredPosition = new Vector2(0, 30);
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

            // --- Phase 2: Hero + Spell Loadout ---
            GameObject loadoutGroup = CreateUINode(root.transform, "LoadoutGroup", new Vector2(1100, 700));
            loadoutGroup.SetActive(false);

            TextMeshProUGUI loadoutTitle = CreateTMP(loadoutGroup.transform, "Title", "PREPARE FOR BATTLE", 42, 900, 60);
            loadoutTitle.rectTransform.anchoredPosition = new Vector2(0, 300);

            // Hero section
            TextMeshProUGUI heroTitle = CreateTMP(loadoutGroup.transform, "HeroTitle", "SELECT HEROES (Max 2)", 28, 700, 40);
            heroTitle.rectTransform.anchoredPosition = new Vector2(0, 240);

            TextMeshProUGUI heroHint = CreateTMP(loadoutGroup.transform, "HeroHint", "SELECT HEROES (Max 2)", 20, 700, 30);
            heroHint.rectTransform.anchoredPosition = new Vector2(0, 205);

            GameObject heroScroll = new GameObject("HeroScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            heroScroll.transform.SetParent(loadoutGroup.transform, false);
            heroScroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.25f);
            RectTransform heroScrollRT = heroScroll.GetComponent<RectTransform>();
            heroScrollRT.sizeDelta = new Vector2(900, 130);
            heroScrollRT.anchoredPosition = new Vector2(0, 130);

            GameObject heroContent = CreateUINode(heroScroll.transform, "HeroContent", new Vector2(850, 110));
            HorizontalLayoutGroup heroHlg = heroContent.AddComponent<HorizontalLayoutGroup>();
            heroHlg.spacing = 15;
            heroHlg.childAlignment = TextAnchor.MiddleLeft;
            ContentSizeFitter heroCsf = heroContent.AddComponent<ContentSizeFitter>();
            heroCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            heroScroll.GetComponent<ScrollRect>().content = heroContent.GetComponent<RectTransform>();
            heroScroll.GetComponent<ScrollRect>().horizontal = true;

            // Unit section
            TextMeshProUGUI unitTitle = CreateTMP(loadoutGroup.transform, "UnitTitle", "SELECT UNITS (Exactly 5)", 28, 700, 40);
            unitTitle.rectTransform.anchoredPosition = new Vector2(0, 20);

            TextMeshProUGUI unitHint = CreateTMP(loadoutGroup.transform, "UnitHint", "SELECT EXACTLY 5 UNITS! (0/5)", 20, 700, 30);
            unitHint.rectTransform.anchoredPosition = new Vector2(0, -15);
            unitHint.color = Color.red;

            GameObject unitScroll = new GameObject("UnitScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            unitScroll.transform.SetParent(loadoutGroup.transform, false);
            unitScroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.25f);
            RectTransform unitScrollRT = unitScroll.GetComponent<RectTransform>();
            unitScrollRT.sizeDelta = new Vector2(900, 130);
            unitScrollRT.anchoredPosition = new Vector2(0, -90);

            GameObject unitContent = CreateUINode(unitScroll.transform, "UnitContent", new Vector2(850, 110));
            HorizontalLayoutGroup unitHlg = unitContent.AddComponent<HorizontalLayoutGroup>();
            unitHlg.spacing = 15;
            unitHlg.childAlignment = TextAnchor.MiddleLeft;
            ContentSizeFitter unitCsf = unitContent.AddComponent<ContentSizeFitter>();
            unitCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            unitScroll.GetComponent<ScrollRect>().content = unitContent.GetComponent<RectTransform>();
            unitScroll.GetComponent<ScrollRect>().horizontal = true;

            // Spell section
            TextMeshProUGUI spellTitle = CreateTMP(loadoutGroup.transform, "SpellTitle", "EQUIP SPELLS (Max 3)", 28, 700, 40);
            spellTitle.rectTransform.anchoredPosition = new Vector2(0, -190);

            GameObject spellScroll = new GameObject("SpellScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            spellScroll.transform.SetParent(loadoutGroup.transform, false);
            spellScroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.25f);
            RectTransform spellScrollRT = spellScroll.GetComponent<RectTransform>();
            spellScrollRT.sizeDelta = new Vector2(900, 150);
            spellScrollRT.anchoredPosition = new Vector2(0, -290);

            GameObject spellContent = CreateUINode(spellScroll.transform, "SpellContent", new Vector2(850, 140));
            GridLayoutGroup glg = spellContent.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(120, 150);
            glg.spacing = new Vector2(20, 20);
            glg.childAlignment = TextAnchor.UpperCenter;
            spellScroll.GetComponent<ScrollRect>().content = spellContent.GetComponent<RectTransform>();

            Button startMatchBtn = CreateButton(loadoutGroup.transform, "StartMatchButton", "START BATTLE", 350, 90, 32);
            startMatchBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -420);
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
            so.FindProperty("loadoutGroup").objectReferenceValue       = loadoutGroup;
            so.FindProperty("heroItemContainer").objectReferenceValue  = heroContent.transform;
            so.FindProperty("heroLoadoutHint").objectReferenceValue    = heroHint;
            so.FindProperty("unitItemContainer").objectReferenceValue  = unitContent.transform;
            so.FindProperty("unitLoadoutHint").objectReferenceValue    = unitHint;
            so.FindProperty("spellItemContainer").objectReferenceValue = spellContent.transform;
            so.FindProperty("startMatchButton").objectReferenceValue   = startMatchBtn;

            GameObject heroItemPrefab = CreateHeroLoadoutItemPrefab();
            so.FindProperty("heroItemPrefab").objectReferenceValue = heroItemPrefab;

            GameObject unitLoadoutPrefab = CreateUnitLoadoutItemPrefab();
            so.FindProperty("unitItemPrefab").objectReferenceValue = unitLoadoutPrefab;

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

            // Auto-Populate allPossibleUnits (from Assets/Data/Units only)
            string[] unitGuids = AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/Data/Units" });
            var allUnitsProp = so.FindProperty("allPossibleUnits");
            allUnitsProp.ClearArray();
            for (int i = 0; i < unitGuids.Length; i++)
            {
                allUnitsProp.InsertArrayElementAtIndex(i);
                allUnitsProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(unitGuids[i]));
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

        private static GameObject CreateHeroLoadoutItemPrefab()
        {
            EnsureDirectory();
            GameObject root = new GameObject("HeroLoadoutItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HeroLoadoutItemUI));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 110);
            root.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Image icon = CreateImage(root.transform, "Icon", new Vector2(70, 70));
            icon.rectTransform.anchoredPosition = new Vector2(0, 10);

            Image highlight = CreateImage(root.transform, "Highlight", new Vector2(120, 110));
            highlight.color = new Color(0.2f, 1f, 0.3f, 0.45f);
            highlight.gameObject.SetActive(false);

            TextMeshProUGUI nameText = CreateTMP(root.transform, "Name", "Hero", 14, 110, 24);
            nameText.rectTransform.anchoredPosition = new Vector2(0, -30);

            TextMeshProUGUI levelText = CreateTMP(root.transform, "Level", "Lv.1", 12, 110, 20);
            levelText.rectTransform.anchoredPosition = new Vector2(0, -48);

            HeroLoadoutItemUI ui = root.GetComponent<HeroLoadoutItemUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("selectionHighlight").objectReferenceValue = highlight;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("button").objectReferenceValue = root.GetComponent<Button>();
            so.ApplyModifiedProperties();

            string path = PREFAB_PATH + "/HeroLoadoutItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateUnitLoadoutItemPrefab()
        {
            EnsureDirectory();
            GameObject root = new GameObject("UnitLoadoutItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UnitLoadoutItemUI));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 110);
            root.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Image icon = CreateImage(root.transform, "Icon", new Vector2(70, 70));
            icon.rectTransform.anchoredPosition = new Vector2(0, 10);

            Image highlight = CreateImage(root.transform, "Highlight", new Vector2(100, 110));
            highlight.color = new Color(0.2f, 1f, 0.3f, 0.45f);
            highlight.gameObject.SetActive(false);

            TextMeshProUGUI nameText = CreateTMP(root.transform, "Name", "Unit", 14, 100, 24);
            nameText.rectTransform.anchoredPosition = new Vector2(0, -35);

            UnitLoadoutItemUI ui = root.GetComponent<UnitLoadoutItemUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("selectionHighlight").objectReferenceValue = highlight;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("button").objectReferenceValue = root.GetComponent<Button>();
            so.ApplyModifiedProperties();

            string path = PREFAB_PATH + "/UnitLoadoutItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject.DestroyImmediate(root);
            return prefab;
        }

        public static void CreateHeroShopPanelPrefab()
        {
            EnsureDirectory();

            GameObject itemPrefab = CreateHeroShopItemPrefabAsset();

            GameObject root = new GameObject("HeroShopPanel", typeof(RectTransform), typeof(Image), typeof(HeroShopUI));
            root.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.97f);
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            TextMeshProUGUI title = CreateTMP(root.transform, "Title", "HERO ROSTER", 56, 900, 80);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0, -40);
            title.color = new Color(1f, 0.85f, 0.2f);

            // Left: hero list
            GameObject scroll = new GameObject("HeroScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scroll.transform.SetParent(root.transform, false);
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            RectTransform scrollRT = scroll.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(0.5f, 1);
            scrollRT.pivot = new Vector2(0, 1);
            scrollRT.offsetMin = new Vector2(40, 50);
            scrollRT.offsetMax = new Vector2(-20, -130);
            scroll.GetComponent<Image>().type = Image.Type.Sliced;
            scroll.AddComponent<Mask>();

            GameObject content = CreateUINode(scroll.transform, "Content", new Vector2(500, 1000));
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(250, 210);
            glg.spacing = new Vector2(20, 20);
            glg.padding = new RectOffset(20, 20, 20, 20);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
            glg.childAlignment = TextAnchor.UpperCenter;

            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            sr.content = content.GetComponent<RectTransform>();
            sr.vertical = true;
            sr.horizontal = false;

            // Right: detail panel
            GameObject detailRoot = CreateUINode(root.transform, "DetailPanel", new Vector2(480, 560));
            RectTransform detailRT = detailRoot.GetComponent<RectTransform>();
            detailRT.anchorMin = new Vector2(0.5f, 0);
            detailRT.anchorMax = new Vector2(1, 1);
            detailRT.pivot = new Vector2(0, 1);
            detailRT.offsetMin = new Vector2(20, 50);
            detailRT.offsetMax = new Vector2(-40, -130);
            Image detailBg = detailRoot.AddComponent<Image>();
            detailBg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);
            detailRoot.AddComponent<HeroDetailPanelUI>();
            
            // Vertical Layout Group - Pozisyonu artık bu component yönetir
            VerticalLayoutGroup vlg = detailRoot.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 8;
            vlg.padding = new RectOffset(20, 20, 30, 20);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            TextMeshProUGUI detailName = CreateTMP(detailRoot.transform, "HeroName", "Hero Name", 60, 400, 60);
            TextMeshProUGUI detailSide = CreateTMP(detailRoot.transform, "Side", "LIGHT", 28, 300, 40);
            TextMeshProUGUI detailLevel = CreateTMP(detailRoot.transform, "Level", "Level 1/5", 28, 300, 40);
            TextMeshProUGUI healthText = CreateTMP(detailRoot.transform, "Health", "Health: 500", 24, 400, 40);
            TextMeshProUGUI damageText = CreateTMP(detailRoot.transform, "Damage", "Damage: 40", 24, 400, 40);
            TextMeshProUGUI speedText = CreateTMP(detailRoot.transform, "Speed", "Speed: 1.0", 24, 400, 40);
            TextMeshProUGUI rangeText = CreateTMP(detailRoot.transform, "Range", "Range: 1.8", 24, 400, 40);
            TextMeshProUGUI attackRateText = CreateTMP(detailRoot.transform, "AttackRate", "Attack Rate: 1.0/s", 24, 400, 40);
            TextMeshProUGUI abilityName = CreateTMP(detailRoot.transform, "AbilityName", "Ability", 32, 450, 50);
            TextMeshProUGUI abilityDesc = CreateTMP(detailRoot.transform, "AbilityDesc", "Description", 24, 450, 120);
            TextMeshProUGUI costText = CreateTMP(detailRoot.transform, "Cost", "500 Karma", 28, 300, 40);

            Button actionBtn = CreateButton(detailRoot.transform, "ActionButton", "UNLOCK HERO", 280, 60, 26);
            Button closeBtn = CreateButton(detailRoot.transform, "CloseButton", "CLOSE", 140, 50, 22);

            HeroDetailPanelUI detailUI = detailRoot.GetComponent<HeroDetailPanelUI>();
            var detailSo = new SerializedObject(detailUI);
            detailSo.FindProperty("panelRoot").objectReferenceValue = detailRoot;
            detailSo.FindProperty("heroIcon").objectReferenceValue = null;
            detailSo.FindProperty("heroNameText").objectReferenceValue = detailName;
            detailSo.FindProperty("sideText").objectReferenceValue = detailSide;
            detailSo.FindProperty("levelText").objectReferenceValue = detailLevel;
            detailSo.FindProperty("healthText").objectReferenceValue = healthText;
            detailSo.FindProperty("damageText").objectReferenceValue = damageText;
            detailSo.FindProperty("speedText").objectReferenceValue = speedText;
            detailSo.FindProperty("rangeText").objectReferenceValue = rangeText;
            detailSo.FindProperty("attackRateText").objectReferenceValue = attackRateText;
            detailSo.FindProperty("abilityNameText").objectReferenceValue = abilityName;
            detailSo.FindProperty("abilityDescText").objectReferenceValue = abilityDesc;
            detailSo.FindProperty("costText").objectReferenceValue = costText;
            detailSo.FindProperty("actionButton").objectReferenceValue = actionBtn;
            detailSo.FindProperty("actionLabel").objectReferenceValue = actionBtn.GetComponentInChildren<TextMeshProUGUI>();
            detailSo.FindProperty("closeButton").objectReferenceValue = closeBtn;
            detailSo.ApplyModifiedProperties();
            detailRoot.SetActive(false);

            Button backBtn = CreateButton(root.transform, "BackButton", "← BACK", 180, 60, 22);
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 1);
            backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.anchoredPosition = new Vector2(30, -20);

            HeroShopUI shopUI = root.GetComponent<HeroShopUI>();
            var so = new SerializedObject(shopUI);
            so.FindProperty("itemContainer").objectReferenceValue = content.transform;
            so.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
            so.FindProperty("karmaText").objectReferenceValue = null;
            so.FindProperty("crystalText").objectReferenceValue = null;
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("detailPanel").objectReferenceValue = detailUI;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH + "/HeroShopPanel.prefab");
            GameObject.DestroyImmediate(root);
        }

        private static GameObject CreateHeroShopItemPrefabAsset()
        {
            EnsureDirectory();
            GameObject root = new GameObject("HeroShopItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HeroShopItemUI));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(260, 220);
            rootRT.anchorMin = new Vector2(0.5f, 0.5f);
            rootRT.anchorMax = new Vector2(0.5f, 0.5f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            root.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            Image iconBg = CreateImage(root.transform, "IconBg", new Vector2(140, 140));
            iconBg.rectTransform.anchoredPosition = new Vector2(0, 10);
            iconBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            Image icon = CreateImage(iconBg.transform, "Icon", new Vector2(130, 130));
            icon.rectTransform.anchorMin = Vector2.zero;
            icon.rectTransform.anchorMax = Vector2.one;
            icon.rectTransform.offsetMin = Vector2.zero;
            icon.rectTransform.offsetMax = Vector2.zero;
            icon.preserveAspect = true;

            TextMeshProUGUI nameText = CreateTMP(root.transform, "Name", "Hero", 22, 240, 30);
            nameText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            nameText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            nameText.rectTransform.pivot = new Vector2(0.5f, 1f);
            nameText.rectTransform.anchoredPosition = new Vector2(0, -150);
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = new Color(1f, 0.85f, 0.4f);

            TextMeshProUGUI levelText = CreateTMP(root.transform, "Level", "Lv.1", 18, 240, 24);
            levelText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            levelText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            levelText.rectTransform.pivot = new Vector2(0.5f, 1f);
            levelText.rectTransform.anchoredPosition = new Vector2(0, -180);
            levelText.alignment = TextAlignmentOptions.Center;

            Image lockedOverlay = CreateImage(root.transform, "LockedOverlay", new Vector2(260, 220));
            lockedOverlay.color = new Color(0, 0, 0, 0.55f);

            Image highlightFrame = CreateImage(root.transform, "Highlight", new Vector2(260, 220));
            highlightFrame.color = new Color(1f, 0.85f, 0.2f, 0.35f);
            highlightFrame.gameObject.SetActive(false);

            HeroShopItemUI ui = root.GetComponent<HeroShopItemUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("levelText").objectReferenceValue = levelText;
            so.FindProperty("lockedOverlay").objectReferenceValue = lockedOverlay;
            so.FindProperty("highlightFrame").objectReferenceValue = highlightFrame;
            so.FindProperty("cardButton").objectReferenceValue = root.GetComponent<Button>();
            so.ApplyModifiedProperties();

            string path = PREFAB_PATH + "/HeroShopItem.prefab";
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

            // --- YENİ: Hero Panel ve Butonları (Sol Alt, UnitPanel'ın Üstü) ---
            GameObject heroPanel = new GameObject("HeroPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            heroPanel.transform.SetParent(root.transform);
            RectTransform heroRT = heroPanel.GetComponent<RectTransform>();
            heroRT.anchorMin = new Vector2(0, 0);
            heroRT.anchorMax = new Vector2(0, 0);
            heroRT.pivot = new Vector2(0, 0);
            heroRT.anchoredPosition = new Vector2(30, 160); // UnitPanel'in (y=30, h=120) hemen üzerinde
            heroRT.sizeDelta = new Vector2(250, 100);

            HorizontalLayoutGroup heroHlg = heroPanel.GetComponent<HorizontalLayoutGroup>();
            heroHlg.childAlignment = TextAnchor.MiddleLeft;
            heroHlg.spacing = 15;

            // Hero 1 Butonu
            GameObject hero1Obj = new GameObject("HeroButton_1", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HeroButtonUI));
            hero1Obj.transform.SetParent(heroPanel.transform, false);
            hero1Obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            hero1Obj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            GameObject hero1Icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            hero1Icon.transform.SetParent(hero1Obj.transform, false);
            hero1Icon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            hero1Icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            // Health Slider (Alt Kısımda)
            GameObject hero1SliderObj = new GameObject("HealthSlider", typeof(RectTransform), typeof(Slider));
            hero1SliderObj.transform.SetParent(hero1Obj.transform, false);
            RectTransform h1SliderRT = hero1SliderObj.GetComponent<RectTransform>();
            h1SliderRT.anchorMin = new Vector2(0, 0);
            h1SliderRT.anchorMax = new Vector2(1, 0);
            h1SliderRT.pivot = new Vector2(0.5f, 0);
            h1SliderRT.anchoredPosition = new Vector2(0, 5);
            h1SliderRT.sizeDelta = new Vector2(-10, 10);

            GameObject h1Background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            h1Background.transform.SetParent(hero1SliderObj.transform, false);
            h1Background.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            h1Background.GetComponent<RectTransform>().anchorMax = Vector2.one;
            h1Background.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            h1Background.GetComponent<Image>().color = Color.gray;

            GameObject h1FillArea = new GameObject("Fill Area", typeof(RectTransform));
            h1FillArea.transform.SetParent(hero1SliderObj.transform, false);
            h1FillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            h1FillArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
            h1FillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject h1Fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            h1Fill.transform.SetParent(h1FillArea.transform, false);
            h1Fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            h1Fill.GetComponent<Image>().color = Color.green;

            Slider h1Slider = hero1SliderObj.GetComponent<Slider>();
            h1Slider.fillRect = h1Fill.GetComponent<RectTransform>();
            h1Slider.targetGraphic = h1Fill.GetComponent<Image>();
            h1Slider.value = 1f;

            // Respawn Overlay
            GameObject hero1Overlay = new GameObject("RespawnOverlay", typeof(RectTransform), typeof(Image));
            hero1Overlay.transform.SetParent(hero1Obj.transform, false);
            hero1Overlay.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            hero1Overlay.GetComponent<RectTransform>().anchorMax = Vector2.one;
            hero1Overlay.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            Image h1OverlayImg = hero1Overlay.GetComponent<Image>();
            h1OverlayImg.color = new Color(0, 0, 0, 0.7f);
            h1OverlayImg.type = Image.Type.Filled;
            h1OverlayImg.fillMethod = Image.FillMethod.Radial360;

            // Timer Text
            TextMeshProUGUI hero1Timer = CreateTMP(hero1Obj.transform, "TimerText", "30s", 20, 100, 30);
            hero1Timer.rectTransform.anchoredPosition = Vector2.zero;

            Image hero1SelectionFrame = CreateImage(hero1Obj.transform, "SelectionFrame", new Vector2(100, 100));
            hero1SelectionFrame.color = new Color(0.2f, 1f, 0.3f, 0.7f);
            hero1SelectionFrame.gameObject.SetActive(false);

            HeroButtonUI heroUI1 = hero1Obj.GetComponent<HeroButtonUI>();
            var h1So = new SerializedObject(heroUI1);
            h1So.FindProperty("iconImage").objectReferenceValue = hero1Icon.GetComponent<Image>();
            h1So.FindProperty("healthSlider").objectReferenceValue = h1Slider;
            h1So.FindProperty("respawnOverlay").objectReferenceValue = h1OverlayImg;
            h1So.FindProperty("timerText").objectReferenceValue = hero1Timer;
            h1So.FindProperty("selectButton").objectReferenceValue = hero1Obj.GetComponent<Button>();
            h1So.FindProperty("selectionFrame").objectReferenceValue = hero1SelectionFrame;
            h1So.ApplyModifiedProperties();

            // Hero 2 Butonu
            GameObject hero2Obj = new GameObject("HeroButton_2", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HeroButtonUI));
            hero2Obj.transform.SetParent(heroPanel.transform, false);
            hero2Obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            hero2Obj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            GameObject hero2Icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            hero2Icon.transform.SetParent(hero2Obj.transform, false);
            hero2Icon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            hero2Icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            // Health Slider (Alt Kısımda)
            GameObject hero2SliderObj = new GameObject("HealthSlider", typeof(RectTransform), typeof(Slider));
            hero2SliderObj.transform.SetParent(hero2Obj.transform, false);
            RectTransform h2SliderRT = hero2SliderObj.GetComponent<RectTransform>();
            h2SliderRT.anchorMin = new Vector2(0, 0);
            h2SliderRT.anchorMax = new Vector2(1, 0);
            h2SliderRT.pivot = new Vector2(0.5f, 0);
            h2SliderRT.anchoredPosition = new Vector2(0, 5);
            h2SliderRT.sizeDelta = new Vector2(-10, 10);

            GameObject h2Background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            h2Background.transform.SetParent(hero2SliderObj.transform, false);
            h2Background.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            h2Background.GetComponent<RectTransform>().anchorMax = Vector2.one;
            h2Background.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            h2Background.GetComponent<Image>().color = Color.gray;

            GameObject h2FillArea = new GameObject("Fill Area", typeof(RectTransform));
            h2FillArea.transform.SetParent(hero2SliderObj.transform, false);
            h2FillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            h2FillArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
            h2FillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject h2Fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            h2Fill.transform.SetParent(h2FillArea.transform, false);
            h2Fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            h2Fill.GetComponent<Image>().color = Color.green;

            Slider h2Slider = hero2SliderObj.GetComponent<Slider>();
            h2Slider.fillRect = h2Fill.GetComponent<RectTransform>();
            h2Slider.targetGraphic = h2Fill.GetComponent<Image>();
            h2Slider.value = 1f;

            // Respawn Overlay
            GameObject hero2Overlay = new GameObject("RespawnOverlay", typeof(RectTransform), typeof(Image));
            hero2Overlay.transform.SetParent(hero2Obj.transform, false);
            hero2Overlay.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            hero2Overlay.GetComponent<RectTransform>().anchorMax = Vector2.one;
            hero2Overlay.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            Image h2OverlayImg = hero2Overlay.GetComponent<Image>();
            h2OverlayImg.color = new Color(0, 0, 0, 0.7f);
            h2OverlayImg.type = Image.Type.Filled;
            h2OverlayImg.fillMethod = Image.FillMethod.Radial360;

            // Timer Text
            TextMeshProUGUI hero2Timer = CreateTMP(hero2Obj.transform, "TimerText", "30s", 20, 100, 30);
            hero2Timer.rectTransform.anchoredPosition = Vector2.zero;

            Image hero2SelectionFrame = CreateImage(hero2Obj.transform, "SelectionFrame", new Vector2(100, 100));
            hero2SelectionFrame.color = new Color(0.2f, 1f, 0.3f, 0.7f);
            hero2SelectionFrame.gameObject.SetActive(false);

            HeroButtonUI heroUI2 = hero2Obj.GetComponent<HeroButtonUI>();
            var h2So = new SerializedObject(heroUI2);
            h2So.FindProperty("iconImage").objectReferenceValue = hero2Icon.GetComponent<Image>();
            h2So.FindProperty("healthSlider").objectReferenceValue = h2Slider;
            h2So.FindProperty("respawnOverlay").objectReferenceValue = h2OverlayImg;
            h2So.FindProperty("timerText").objectReferenceValue = hero2Timer;
            h2So.FindProperty("selectButton").objectReferenceValue = hero2Obj.GetComponent<Button>();
            h2So.FindProperty("selectionFrame").objectReferenceValue = hero2SelectionFrame;
            h2So.ApplyModifiedProperties();

            // HUDController'a Hero butonlarını bağla
            var hudHeroSo = new SerializedObject(hud);
            hudHeroSo.FindProperty("heroButton1").objectReferenceValue = heroUI1;
            hudHeroSo.FindProperty("heroButton2").objectReferenceValue = heroUI2;
            hudHeroSo.ApplyModifiedProperties();

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
            root.AddComponent<DailyRewardManager>();
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
            root.AddComponent<HeroManager>(); // Hero sistemi

            // --- HeroManager.allHeroes listesini doldur ---
            HeroManager hm = root.GetComponent<HeroManager>();
            var hmSo = new SerializedObject(hm);
            var heroGuids = AssetDatabase.FindAssets("t:HeroData", new[] { "Assets/Data/Heroes" });
            var heroProp = hmSo.FindProperty("allHeroes");
            heroProp.ClearArray();
            int hIndex = 0;
            foreach (var guid in heroGuids)
            {
                var hData = AssetDatabase.LoadAssetAtPath<HeroData>(AssetDatabase.GUIDToAssetPath(guid));
                if (hData == null) continue;
                heroProp.InsertArrayElementAtIndex(hIndex);
                heroProp.GetArrayElementAtIndex(hIndex).objectReferenceValue = hData;
                hIndex++;
            }
            hmSo.ApplyModifiedProperties();

            // --- MetaProgressionManager.allAvailableHeroes ---
            MetaProgressionManager mp = root.GetComponent<MetaProgressionManager>();
            var mpSo = new SerializedObject(mp);
            var mpHeroProp = mpSo.FindProperty("allAvailableHeroes");
            mpHeroProp.ClearArray();
            for (int i = 0; i < hIndex; i++)
            {
                mpHeroProp.InsertArrayElementAtIndex(i);
                mpHeroProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    heroProp.GetArrayElementAtIndex(i).objectReferenceValue;
            }
            mpSo.ApplyModifiedProperties();

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
            
            // Doğal sıralama için gelişmiş sayısal ayrıştırma (Regex ile)
            levelDataList.Sort((a, b) => {
                string nameA = a.levelName != null ? a.levelName : a.name;
                string nameB = b.levelName != null ? b.levelName : b.name;
                
                int numA = ExtractLevelNumber(nameA);
                int numB = ExtractLevelNumber(nameB);
                
                if (numA != -1 && numB != -1) return numA.CompareTo(numB);
                return string.Compare(nameA, nameB);
            });

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
                
                // Kule Slotları (Yol boyunca - Sadece özel konumlar yoksa)
                if (data.customSlotPositions == null || data.customSlotPositions.Count == 0)
                {
                    CreateTowerSlotsAlongPath(root.transform, points, data.towerSlotCount / levelPaths.Count);
                }
            }

            // Kule Slotları (Özel Konumlar)
            if (data.customSlotPositions != null && data.customSlotPositions.Count > 0)
            {
                string slotPath = "Assets/Prefabs/Gameplay/BaseTowerSlotPrefab.prefab";
                GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPath);
                if (slotPrefab == null)
                {
                    CreateTowerSlotPrefab();
                    slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(slotPath);
                }
                if (slotPrefab != null)
                {
                    foreach (Vector3 customPos in data.customSlotPositions)
                    {
                        CreateSingleSlotAt(root.transform, slotPrefab, customPos);
                    }
                }
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
            
            // orthographicSize artık kod tarafından değiştirilmiyor, editördeki değer korunuyor.
            if (!cam.orthographic) cam.fieldOfView = 45;

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

            string numStr = data.levelID.Replace("Level", "");
            string safeName = data.levelName.Replace(" ", "_").Replace("'", "").Replace(":", "");
            
            // Daha güvenilir bir arama yöntemi: Doğrudan klasördeki tüm haritaları alıp string kontrolü yapmak
            string[] mapGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Maps" });
            foreach (string guid in mapGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains($"Level_{numStr}__") || path.Contains($"{safeName}_Map"))
                {
                    data.mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    break;
                }
            }
            
            // Eğer hala bulunamadıysa bile ARTIK PLACEHOLDER (Pembe Küre) OLUŞTURMA!
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
                if (data == null) continue;

                string[] mapGuids = new string[0];
                string safeName = GetSafeFilename(data.levelName);
                mapGuids = AssetDatabase.FindAssets($"{safeName}_Map t:GameObject");

                if (mapGuids.Length == 0)
                {
                    string numStr = data.levelID.Replace("Level", "");
                    if (int.TryParse(numStr, out int num))
                    {
                        mapGuids = AssetDatabase.FindAssets($"Level_{num}__ t:GameObject");
                    }
                }

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
