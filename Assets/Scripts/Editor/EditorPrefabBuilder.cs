using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TowerDefence.UI;
using TowerDefence.Combat;
using TowerDefence.Grid;
using TowerDefence.Data;
using UnityEngine.AI;

public class EditorPrefabBuilder : Editor
{
    [MenuItem("Tower Defence/Tools/Generate All Prefabs")]
    public static void GeneratePrefabs()
    {
        string uiFolder = "Assets/Prefabs/UI";
        string gameplayBase = "Assets/Prefabs/Gameplay";
        string unitsFolder = gameplayBase + "/Units";
        string towersFolder = gameplayBase + "/Towers";
        string projectilesFolder = gameplayBase + "/Projectiles";

        EnsureFolders(uiFolder, unitsFolder, towersFolder, projectilesFolder);

        // UI Prefabs
        CreateLevelButtonPrefab(uiFolder);
        CreateSkillNodePrefab(uiFolder);
        CreateTowerButtonPrefab(uiFolder);
        CreateUnitButtonPrefab(uiFolder);
        CreateTowerSelectionPrefab(uiFolder);
        CreateUnitSelectionPrefab(uiFolder);

        // Mermi Varyasyonları (10 Adet)
        CreateProjectileTemplate(projectilesFolder, "Archer_Arrow", Color.white);
        CreateProjectileTemplate(projectilesFolder, "Ballista_Bolt", Color.gray);
        CreateProjectileTemplate(projectilesFolder, "Bone_Projectyle", Color.white);
        CreateProjectileTemplate(projectilesFolder, "Cannon_Ball", Color.black);
        CreateProjectileTemplate(projectilesFolder, "Dark_Pulse", Color.magenta);
        CreateProjectileTemplate(projectilesFolder, "Mage_Bolt", Color.blue);
        CreateProjectileTemplate(projectilesFolder, "Poison_Drip", new Color(0.1f, 0.5f, 0.1f));
        CreateProjectileTemplate(projectilesFolder, "Solar_Beam", Color.yellow);
        CreateProjectileTemplate(projectilesFolder, "Soul_Orb", new Color(0.5f, 0, 0));
        CreateProjectileTemplate(projectilesFolder, "Void_Missile", new Color(0.2f, 0, 0.2f));

        // Kule Varyasyonları (10 Adet)
        CreateTowerTemplate(towersFolder, projectilesFolder, "Archer_Tower", Color.green);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Ballista_Tower", Color.gray);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Bone_Catapult", Color.white);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Cannon_Tower", Color.black);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Dark_Sentry", Color.magenta);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Mage_Tower", Color.blue);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Poison_Spitter", new Color(0.1f, 0.5f, 0.1f));
        CreateTowerTemplate(towersFolder, projectilesFolder, "Solar_Prism", Color.yellow);
        CreateTowerTemplate(towersFolder, projectilesFolder, "Soul_Harvester", new Color(0.5f, 0, 0));
        CreateTowerTemplate(towersFolder, projectilesFolder, "Void_Obelisk", new Color(0.2f, 0, 0.2f));

        // Ünite Varyasyonları (10 Adet)
        CreateUnitTemplate(unitsFolder, "Celestial_Archer", Color.cyan);
        CreateUnitTemplate(unitsFolder, "Holy_Scout", Color.yellow);
        CreateUnitTemplate(unitsFolder, "Iron_Knight", Color.white);
        CreateUnitTemplate(unitsFolder, "Light_Swordsman", Color.white);
        CreateUnitTemplate(unitsFolder, "Shield_Bearer", Color.blue);

        CreateUnitTemplate(unitsFolder, "Abyssal_Behemoth", Color.red);
        CreateUnitTemplate(unitsFolder, "Plague_Runner", new Color(0.3f, 0.4f, 0));
        CreateUnitTemplate(unitsFolder, "Shadow_Stalker", Color.black);
        CreateUnitTemplate(unitsFolder, "Skeleton_Warrior", Color.gray);
        CreateUnitTemplate(unitsFolder, "Wraith", Color.magenta);

        CreateWaypointsPrefab(gameplayBase);
        CreateVFXPrefab(gameplayBase);
        CreateTowerSlotPrefab(gameplayBase, uiFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All Prefabs generated successfully and organized into subfolders!");
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
        foreach (var path in paths)
        {
            string[] folders = path.Split('/');
            string currentPath = "";
            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(currentPath))
                {
                    currentPath = folder;
                }
                else
                {
                    string parent = currentPath;
                    currentPath += "/" + folder;
                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(parent, folder);
                    }
                }
            }
        }
    }

    private static void CreateUnitTemplate(string folder, string name, Color color)
    {
        GameObject root = new GameObject(name, typeof(NavMeshAgent), typeof(Unit), typeof(CapsuleCollider));
        root.GetComponent<CapsuleCollider>().center = new Vector3(0, 1, 0);
        root.GetComponent<CapsuleCollider>().radius = 0.5f;
        root.GetComponent<CapsuleCollider>().height = 2f;
        
        Unit unitScript = root.GetComponent<Unit>();

        // Karşılık gelen UnitData'yı bul
        UnitData data = null;
        string[] guids = AssetDatabase.FindAssets(name + " t:UnitData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            Debug.Log($"Found UnitData for {name} at: {path}");
        }

        if (data != null)
        {
            var serializedUnit = new SerializedObject(unitScript);
            // Unit.cs içinde [SerializeField] private UnitData unitData; olduğunu varsayıyoruz
            var dataProp = serializedUnit.FindProperty("unitData");
            if (dataProp != null)
            {
                dataProp.objectReferenceValue = data;
                serializedUnit.ApplyModifiedProperties();
            }
        }

        GameObject visuals = new GameObject("Visuals", typeof(SpriteRenderer), typeof(Animator));
        visuals.transform.SetParent(root.transform);
        visuals.GetComponent<SpriteRenderer>().color = color;

        string prefabPath = folder + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

        // UnitData'yı güncelle
        if (data != null)
        {
            var serializedData = new SerializedObject(data);
            serializedData.FindProperty("prefab").objectReferenceValue = prefab;
            serializedData.FindProperty("unitName").stringValue = name.Replace("_", " ");
            serializedData.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);
            Debug.Log($"Successfully updated UnitData for {name} with Prefab.");
        }

        GameObject.DestroyImmediate(root);
    }

    private static void CreateTowerTemplate(string folder, string projectileFolder, string name, Color color)
    {
        GameObject root = new GameObject(name, typeof(Tower));
        Tower towerScript = root.GetComponent<Tower>();
        
        // Karşılık gelen TowerData'yı bul (Daha esnek arama)
        TowerData data = null;
        string[] guids = AssetDatabase.FindAssets(name + " t:TowerData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            data = AssetDatabase.LoadAssetAtPath<TowerData>(path);
            Debug.Log($"Found TowerData for {name} at: {path}");
        }
        else
        {
            // Fallback: Eski usul klasik yol
            string dataPath = "Assets/Resources/Data/Towers/" + name + ".asset";
            data = AssetDatabase.LoadAssetAtPath<TowerData>(dataPath);
            if (data != null) Debug.Log($"Found TowerData for {name} at fallback path: {dataPath}");
            else Debug.LogWarning($"Could not find TowerData for {name}! Tried searching by name and at {dataPath}");
        }
        
        if (data != null)
        {
            var serializedTower = new SerializedObject(towerScript);
            serializedTower.FindProperty("towerData").objectReferenceValue = data;
            serializedTower.ApplyModifiedProperties();
        }

        GameObject visuals = new GameObject("Visuals", typeof(SpriteRenderer));
        visuals.transform.SetParent(root.transform);
        visuals.GetComponent<SpriteRenderer>().color = color;

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(root.transform);
        firePoint.transform.localPosition = new Vector3(0, 1, 0);

        string prefabPath = folder + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        
        // TowerData'yı güncelle
        if (data != null)
        {
            var serializedData = new SerializedObject(data);
            serializedData.FindProperty("prefab").objectReferenceValue = prefab;
            serializedData.FindProperty("towerName").stringValue = name.Replace("_", " ");
            
            // İlgili mermi prefabını bul ve bağla
            string projName = GetProjectileNameForTower(name);
            if (!string.IsNullOrEmpty(projName))
            {
                GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projectileFolder + "/" + projName + ".prefab");
                if (projPrefab != null)
                {
                    serializedData.FindProperty("projectilePrefab").objectReferenceValue = projPrefab;
                    Debug.Log($"Linked Projectile {projName} to TowerData {name}");
                }
                else Debug.LogWarning($"Could not find Projectile prefab {projName} at {projectileFolder}");
            }

            serializedData.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);
            Debug.Log($"Successfully updated TowerData for {name} with Prefab and Projectile.");
        }

        GameObject.DestroyImmediate(root);
    }

    private static string GetProjectileNameForTower(string towerName)
    {
        switch (towerName)
        {
            case "Archer_Tower": return "Archer_Arrow";
            case "Ballista_Tower": return "Ballista_Bolt";
            case "Bone_Catapult": return "Bone_Projectyle";
            case "Cannon_Tower": return "Cannon_Ball";
            case "Dark_Sentry": return "Dark_Pulse";
            case "Mage_Tower": return "Mage_Bolt";
            case "Poison_Spitter": return "Poison_Drip";
            case "Solar_Prism": return "Solar_Beam";
            case "Soul_Harvester": return "Soul_Orb";
            case "Void_Obelisk": return "Void_Missile";
            default: return "";
        }
    }

    private static void CreateProjectileTemplate(string folder, string name, Color color)
    {
        GameObject root = new GameObject(name, typeof(Projectile), typeof(SphereCollider), typeof(Rigidbody));
        root.GetComponent<SphereCollider>().isTrigger = true;
        root.GetComponent<Rigidbody>().useGravity = false;
        root.GetComponent<Rigidbody>().isKinematic = true;

        GameObject visuals = new GameObject("Visuals", typeof(SpriteRenderer));
        visuals.transform.SetParent(root.transform);
        visuals.GetComponent<SpriteRenderer>().color = color;
        visuals.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/" + name + ".prefab");
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

    private static void CreateTowerSlotPrefab(string folder, string uiFolder)
    {
        // Önce UI prefabının güncel olduğundan emin olalım
        CreateTowerSelectionPrefab(uiFolder);

        GameObject root = new GameObject("BaseTowerSlotPrefab", typeof(TowerSlot), typeof(BoxCollider));
        root.GetComponent<BoxCollider>().size = new Vector3(2, 0.2f, 2);
        root.GetComponent<BoxCollider>().isTrigger = true;

        GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visuals.name = "Visuals";
        visuals.transform.SetParent(root.transform);
        visuals.transform.localScale = new Vector3(1.8f, 0.1f, 1.8f);
        visuals.transform.localPosition = Vector3.zero;
        
        if (visuals.GetComponent<Collider>()) GameObject.DestroyImmediate(visuals.GetComponent<Collider>());

        // UI Prefabını child olarak ekle (Doğru klasörden)
        string uiPath = uiFolder + "/BaseTowerSelectionPrefab.prefab";
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
