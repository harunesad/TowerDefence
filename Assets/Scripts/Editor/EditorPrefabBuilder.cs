using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TowerDefence.UI;
using TowerDefence.Combat;
using TowerDefence.Grid;
using UnityEngine.AI;

public class EditorPrefabBuilder : Editor
{
    [MenuItem("Tower Defence/Tools/Generate All Prefabs")]
    public static void GeneratePrefabs()
    {
        string uiFolder = "Assets/Prefabs/UI";
        string gameplayFolder = "Assets/Prefabs/Gameplay";

        EnsureFolders(uiFolder, gameplayFolder);

        // UI Prefabs
        CreateLevelButtonPrefab(uiFolder);
        CreateSkillNodePrefab(uiFolder);
        CreateTowerButtonPrefab(uiFolder);
        CreateUnitButtonPrefab(uiFolder);
        CreateTowerSelectionPrefab(uiFolder);
        CreateUnitSelectionPrefab(uiFolder);

        // Gameplay Prefabs
        CreateUnitPrefab(gameplayFolder);
        CreateTowerPrefab(gameplayFolder);
        CreateProjectilePrefab(gameplayFolder);
        CreateWaypointsPrefab(gameplayFolder);
        CreateVFXPrefab(gameplayFolder);
        CreateTowerSlotPrefab(gameplayFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All Prefabs generated successfully!");
    }

    private static void CreateWaypointsPrefab(string folder)
    {
        GameObject root = new GameObject("BaseWaypointsPrefab", typeof(PathWaypoints));
        for (int i = 0; i < 3; i++)
        {
            GameObject wp = new GameObject("WP_" + i);
            wp.transform.SetParent(root.transform);
            wp.transform.localPosition = new Vector3(i * 5, 0, 0); // Görsel kolaylık için yan yana dizelim
        }

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseWaypointsPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateVFXPrefab(string folder)
    {
        GameObject root = new GameObject("BaseVFXPrefab", typeof(ParticleSystem), typeof(AudioSource));
        // Otomatik yok olma scripti de eklenebilir ama şimdilik iskelet:
        var ps = root.GetComponent<ParticleSystem>().main;
        ps.stopAction = ParticleSystemStopAction.Destroy;

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseVFXPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void EnsureFolders(params string[] paths)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        foreach (var path in paths)
        {
            string folderName = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets/Prefabs", folderName);
        }
    }

    private static void CreateUnitPrefab(string folder)
    {
        GameObject root = new GameObject("BaseUnitPrefab", typeof(NavMeshAgent), typeof(Unit), typeof(CapsuleCollider));
        root.GetComponent<CapsuleCollider>().center = new Vector3(0, 1, 0);
        root.GetComponent<CapsuleCollider>().radius = 0.5f;
        root.GetComponent<CapsuleCollider>().height = 2f;

        GameObject visuals = new GameObject("Visuals", typeof(SpriteRenderer));
        visuals.transform.SetParent(root.transform);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseUnitPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateTowerPrefab(string folder)
    {
        GameObject root = new GameObject("BaseTowerPrefab", typeof(Tower));
        
        GameObject visuals = new GameObject("Visuals", typeof(SpriteRenderer));
        visuals.transform.SetParent(root.transform);

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(root.transform);
        firePoint.transform.localPosition = new Vector3(0, 1, 0);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseTowerPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateProjectilePrefab(string folder)
    {
        GameObject root = new GameObject("BaseProjectilePrefab", typeof(Projectile), typeof(SphereCollider), typeof(Rigidbody));
        root.GetComponent<SphereCollider>().isTrigger = true;
        root.GetComponent<Rigidbody>().useGravity = false;
        root.GetComponent<Rigidbody>().isKinematic = true;

        GameObject visuals = new GameObject("Visuals", typeof(SpriteRenderer));
        visuals.transform.SetParent(root.transform);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseProjectilePrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateLevelButtonPrefab(string folder)
    {
        GameObject root = new GameObject("LevelButtonPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 400);

        // PreviewImage
        GameObject preview = new GameObject("PreviewImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        preview.transform.SetParent(root.transform);
        preview.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        preview.GetComponent<RectTransform>().anchorMax = Vector2.one;
        preview.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        preview.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // LevelName (TMPro)
        GameObject nameText = new GameObject("LevelName", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameText.transform.SetParent(root.transform);
        var txtComp = nameText.GetComponent<TextMeshProUGUI>();
        txtComp.text = "Level Name";
        txtComp.alignment = TextAlignmentOptions.Center;
        txtComp.fontSize = 24;
        nameText.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 50);

        // DifficultyContainer
        GameObject diffContainer = new GameObject("DifficultyContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        diffContainer.transform.SetParent(root.transform);
        for(int i=0; i<3; i++) {
            GameObject skull = new GameObject("Skull_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            skull.transform.SetParent(diffContainer.transform);
            skull.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
        }

        // LockedOverlay
        GameObject locked = new GameObject("LockedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        locked.transform.SetParent(root.transform);
        locked.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        locked.GetComponent<RectTransform>().anchorMax = Vector2.one;
        locked.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        locked.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/LevelButtonPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateSkillNodePrefab(string folder)
    {
        // SkillNodeUI componentini de ekleyelim
        GameObject root = new GameObject("SkillNodePrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(SkillNodeUI));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

        SkillNodeUI nodeScript = root.GetComponent<SkillNodeUI>();

        // Icon
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(root.transform);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

        // CostText
        GameObject costTextObj = new GameObject("CostText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        costTextObj.transform.SetParent(root.transform);
        costTextObj.GetComponent<TextMeshProUGUI>().text = "100";
        costTextObj.GetComponent<TextMeshProUGUI>().fontSize = 18;

        // LockedOverlay
        GameObject locked = new GameObject("LockedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        locked.transform.SetParent(root.transform);
        locked.SetActive(false);

        // PurchasedOverlay
        GameObject purchased = new GameObject("PurchasedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        purchased.transform.SetParent(root.transform);
        purchased.SetActive(false);

        // --- SCRIPT REFERANSLARINI OTOMATİK BAĞLA ---
        var serializedObject = new SerializedObject(nodeScript);
        serializedObject.FindProperty("iconImage").objectReferenceValue = icon.GetComponent<Image>();
        serializedObject.FindProperty("buyButton").objectReferenceValue = root.GetComponent<Button>();
        serializedObject.FindProperty("costText").objectReferenceValue = costTextObj.GetComponent<TextMeshProUGUI>();
        serializedObject.FindProperty("lockedOverlay").objectReferenceValue = locked;
        serializedObject.FindProperty("purchasedOverlay").objectReferenceValue = purchased;
        serializedObject.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/SkillNodePrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateTowerButtonPrefab(string folder)
    {
        GameObject root = new GameObject("TowerButtonPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(root.transform);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/TowerButtonPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateUnitButtonPrefab(string folder)
    {
        GameObject root = new GameObject("UnitButtonPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(root.transform);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/UnitButtonPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateTowerSlotPrefab(string folder)
    {
        // Önce UI prefabının güncel olduğundan emin olalım
        CreateTowerSelectionPrefab(folder);

        GameObject root = new GameObject("BaseTowerSlotPrefab", typeof(TowerSlot), typeof(BoxCollider));
        root.GetComponent<BoxCollider>().size = new Vector3(2, 0.2f, 2);
        root.GetComponent<BoxCollider>().isTrigger = true;

        GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visuals.name = "Visuals";
        visuals.transform.SetParent(root.transform);
        visuals.transform.localScale = new Vector3(1.8f, 0.1f, 1.8f);
        visuals.transform.localPosition = Vector3.zero;
        
        if (visuals.GetComponent<Collider>()) GameObject.DestroyImmediate(visuals.GetComponent<Collider>());

        // UI Prefabını child olarak ekle
        string uiPath = folder + "/BaseTowerSelectionPrefab.prefab";
        GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(uiPath);
        if (uiPrefab != null)
        {
            GameObject uiInstance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab);
            uiInstance.transform.SetParent(root.transform);
            uiInstance.transform.localPosition = new Vector3(0, 2.5f, 0);
        }

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseTowerSlotPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateTowerSelectionPrefab(string folder)
    {
        // Ana Root (World Space Canvas)
        GameObject root = new GameObject("BaseTowerSelectionPrefab", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(TowerSelectionUI));
        
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 150);
        // World Space'de devasa görünmemesi için küçültüyoruz (1 unit = 100 pixel yaklaşık)
        rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Arka Plan Image
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(root.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // Container (Butonların dizileceği yer)
        GameObject container = new GameObject("Container", typeof(RectTransform), typeof(GridLayoutGroup));
        container.transform.SetParent(root.transform);
        container.transform.localPosition = Vector3.zero;
        container.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        container.GetComponent<RectTransform>().anchorMax = Vector2.one;
        container.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
        container.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
        
        var grid = container.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(10, 10);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // Script Referanslarını Bağla
        TowerSelectionUI uiScript = root.GetComponent<TowerSelectionUI>();
        var serializedObject = new SerializedObject(uiScript);
        serializedObject.FindProperty("container").objectReferenceValue = container.transform;
        
        GameObject btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/TowerButtonPrefab.prefab");
        serializedObject.FindProperty("towerButtonPrefab").objectReferenceValue = btnPrefab;
        
        serializedObject.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseTowerSelectionPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateUnitSelectionPrefab(string folder)
    {
        // Ana Root (Panel)
        GameObject root = new GameObject("BaseUnitSelectionPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UnitSelectionUI));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 150);
        root.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // Container (Butonların dizileceği yer)
        GameObject container = new GameObject("Container", typeof(RectTransform), typeof(GridLayoutGroup));
        container.transform.SetParent(root.transform);
        container.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        container.GetComponent<RectTransform>().anchorMax = Vector2.one;
        container.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
        container.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
        
        var grid = container.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(10, 10);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // Script Referanslarını Bağla
        UnitSelectionUI uiScript = root.GetComponent<UnitSelectionUI>();
        var serializedObject = new SerializedObject(uiScript);
        serializedObject.FindProperty("container").objectReferenceValue = container.transform;
        
        GameObject btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/UnitButtonPrefab.prefab");
        serializedObject.FindProperty("unitButtonPrefab").objectReferenceValue = btnPrefab;
        
        serializedObject.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/BaseUnitSelectionPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }
}
