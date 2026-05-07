using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TowerDefence.UI;
using TowerDefence.Combat;
using TowerDefence.Grid;
using TowerDefence.Data;
using TowerDefence.Core;
using UnityEngine.AI;

public class EditorPrefabBuilder : Editor
{
    [MenuItem("Tower Defence/🚀 Setup/🏗️ Build All Gameplay Prefabs")]
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

        // Counterpart ve Icon Bağlantıları
        LinkAllCounterparts();
        LinkAllIcons();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        
        Debug.Log("✔ Tüm Prefab ve Asset'ler başarıyla oluşturuldu/güncellendi.");
    }

    [MenuItem("Tower Defence/🛠️ Utilities/🔍 Fix Model Settings")]
    public static void FixModelSettings()
    {
        Debug.Log("Checking Model Read/Write settings...");
        string[] modelGuids = AssetDatabase.FindAssets("t:Model");
        int fixedCount = 0;

        foreach (var guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                fixedCount++;
                Debug.Log($"Fixed Read/Write for: {path}");
            }
        }
        
        Debug.Log($"✔ {fixedCount} model başarıyla güncellendi.");
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
        root.GetComponent<NavMeshAgent>().enabled = false; // Devre dışı bırak (Instantiate hatasını önlemek için)
        
        Unit unitScript = root.GetComponent<Unit>();

        // Karşılık gelen UnitData'yı bul (Akıllı Bulma)
        string safeName = name.Replace(" ", "_");
        string dataPath = "Assets/Data/Units/" + safeName + ".asset";
        UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(dataPath);
        
        if (data == null) // Fallback: Arama yap
        {
            string[] guids = AssetDatabase.FindAssets(safeName + " t:UnitData");
            if (guids.Length > 0)
            {
                data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        // Görselleştirme (2D Sprite yerine 3D Model Animasyonlu)
        Apply3DVisualsToUnit(root, name);

        string prefabPath = folder + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

        // TERS BAĞLAMA (Prefab -> Data): Prefab kaydedildikten sonra bağlıyoruz ki kalıcı olsun
        if (data != null && prefab != null)
        {
            Unit unitComp = prefab.GetComponent<Unit>();
            if (unitComp != null)
            {
                SerializedObject so = new SerializedObject(unitComp);
                so.FindProperty("unitData").objectReferenceValue = data;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
            }
        }

        // UnitData'yı güncelle (Data -> Prefab)
        if (data != null)
        {
            data.prefab = prefab;
            data.unitName = name.Replace("_", " ");
            data.side = GetSideByName(name);
            EditorUtility.SetDirty(data);
            Debug.Log($"Successfully updated UnitData for {name} with Prefab and Side: {data.side}");
        }

        GameObject.DestroyImmediate(root);
    }

    private static void CreateTowerTemplate(string folder, string projectileFolder, string name, Color color)
    {
        GameObject root = new GameObject(name, typeof(Tower));
        Tower towerScript = root.GetComponent<Tower>();
        
        // Karşılık gelen TowerData'yı bul (Akıllı Bulma)
        string safeName = name.Replace(" ", "_");
        string dataPath = "Assets/Data/Towers/" + safeName + ".asset";
        TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(dataPath);
        
        if (data == null) // Fallback: Arama yap
        {
            string[] guids = AssetDatabase.FindAssets(safeName + " t:TowerData");
            if (guids.Length > 0)
            {
                data = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        // Görselleştirme (2D Sprite yerine 3D Model Kenney Kit)
        Apply3DVisualsToTower(root, name);

        // --- YENİ: partToRotate alanını geçici olarak scene objesinde ata ---
        Transform weapon = root.transform.Find("Visuals/Weapon");
        if (weapon != null)
        {
            towerScript.transform.Find("Visuals/Weapon"); // Debug için
            // SerializedObject ile bağlamak için root üzerindeki componenti kullanıyoruz
            var soRoot = new SerializedObject(towerScript);
            soRoot.FindProperty("partToRotate").objectReferenceValue = weapon;
            soRoot.ApplyModifiedProperties();
        }

        string prefabPath = folder + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        
        // TERS BAĞLAMA (Prefab -> Data): Prefab kaydedildikten sonra bağlıyoruz ki kalıcı olsun
        if (data != null && prefab != null)
        {
            Tower towerComp = prefab.GetComponent<Tower>();
            if (towerComp != null)
            {
                SerializedObject so = new SerializedObject(towerComp);
                so.FindProperty("towerData").objectReferenceValue = data;
                
                // Rotasyon parçasını da prefab üzerinden tekrar kontrol et/bağla
                Transform prefabWeapon = prefab.transform.Find("Visuals/Weapon");
                if (prefabWeapon != null)
                {
                    so.FindProperty("partToRotate").objectReferenceValue = prefabWeapon;
                }
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
            }
        }

        // TowerData'yı güncelle (Data -> Prefab)
        if (data != null)
        {
            data.prefab = prefab;
            data.towerName = name.Replace("_", " ");
            data.side = GetSideByName(name);
            
            // İlgili mermi prefabını bul ve bağla
            string projName = GetProjectileNameForTower(name);
            if (!string.IsNullOrEmpty(projName))
            {
                GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projectileFolder + "/" + projName + ".prefab");
                if (projPrefab != null)
                {
                    data.projectilePrefab = projPrefab;
                    Debug.Log($"Linked Projectile {projName} to TowerData {name}");
                }
                else Debug.LogWarning($"Could not find Projectile prefab {projName} at {projectileFolder}");
            }

            EditorUtility.SetDirty(data);
            Debug.Log($"Successfully updated TowerData for {name} with Prefab, Projectile and Side: {data.side}");
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

        // Görselleştirme (2D Sprite yerine 3D Model + Trail)
        Apply3DVisualsToProjectile(root, name, color);

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/" + name + ".prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateLevelButtonPrefab(string folder)
    {
        GameObject root = new GameObject("LevelButtonPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 220); // Boyutlar daha uyumlu hale getirildi

        // PreviewImage - Kartın üst kısmını kaplar
        GameObject preview = new GameObject("PreviewImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        preview.transform.SetParent(root.transform);
        RectTransform previewRt = preview.GetComponent<RectTransform>();
        previewRt.anchorMin = new Vector2(0, 0.45f);
        previewRt.anchorMax = Vector2.one;
        previewRt.offsetMin = new Vector2(5, 5);
        previewRt.offsetMax = new Vector2(-5, -5);
        preview.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f); // Hafif gri arka plan

        // LevelName (TMPro) - Orta kısım
        GameObject nameText = new GameObject("LevelName", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameText.transform.SetParent(root.transform);
        var txtComp = nameText.GetComponent<TextMeshProUGUI>();
        txtComp.text = "Level 01";
        txtComp.alignment = TextAlignmentOptions.Center;
        txtComp.fontSize = 18;
        txtComp.color = new Color(0.15f, 0.15f, 0.15f);
        
        RectTransform nameRt = nameText.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0.2f);
        nameRt.anchorMax = new Vector2(1, 0.45f);
        nameRt.offsetMin = new Vector2(5, 0);
        nameRt.offsetMax = new Vector2(-5, 0);

        // DifficultyContainer - En Alt
        GameObject diffContainer = new GameObject("DifficultyContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        diffContainer.transform.SetParent(root.transform);
        var hlg = diffContainer.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 3;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;

        RectTransform diffRt = diffContainer.GetComponent<RectTransform>();
        diffRt.anchorMin = new Vector2(0, 0.05f);
        diffRt.anchorMax = new Vector2(1, 0.2f);
        diffRt.offsetMin = Vector2.zero;
        diffRt.offsetMax = Vector2.zero;

        // Özel skull görselini Assets/Resources/UI/SkullIcon.png yolundan yükle
        Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SkullIcon.png");
        
        // Fallback: Eğer kullanıcı henüz eklemediyse varsayılan bir sprite dene
        if (defaultSprite == null) 
            defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        for(int i=0; i<3; i++) {
            GameObject skull = new GameObject("Skull_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            skull.transform.SetParent(diffContainer.transform);
            var skullImg = skull.GetComponent<Image>();
            skullImg.sprite = defaultSprite; // Varsayılan sprite atandı
            skullImg.color = new Color(0.85f, 0.1f, 0.1f);
            skull.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
        }

        // LockedOverlay
        GameObject locked = new GameObject("LockedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        locked.transform.SetParent(root.transform);
        locked.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        locked.GetComponent<RectTransform>().anchorMax = Vector2.one;
        locked.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
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
        root.GetComponent<Image>().enabled = false; // Background image'ı kapat

        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(root.transform);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
        
        // Target Graphic'i Icon'a bağla
        root.GetComponent<Button>().targetGraphic = icon.GetComponent<Image>();

        // Maliyet Metni (Cost)
        GameObject costGO = new GameObject("Cost", typeof(RectTransform), typeof(CanvasRenderer));
        costGO.transform.SetParent(root.transform);
        var costRt = costGO.GetComponent<RectTransform>();
        costRt.sizeDelta = new Vector2(80, 30);
        costRt.anchoredPosition = new Vector2(0, -25);

        // Not: Projenizde TMP yüklü değilse bu hata verebilir. Modern projelerde standarttır.
        var text = costGO.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "0";
        text.fontSize = 18;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.white; // Beyaz renk (Kullanıcı Talebi)

        PrefabUtility.SaveAsPrefabAsset(root, folder + "/TowerButtonPrefab.prefab");
        GameObject.DestroyImmediate(root);
    }

    private static void CreateUnitButtonPrefab(string folder)
    {
        GameObject root = new GameObject("UnitButtonPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        root.GetComponent<Image>().enabled = false;

        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icon.transform.SetParent(root.transform);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50); // Biraz küçülttük metne yer kalsın
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
        
        // Target Graphic'i Icon'a bağla
        root.GetComponent<Button>().targetGraphic = icon.GetComponent<Image>();

        // Maliyet Metni (Cost)
        GameObject costGO = new GameObject("Cost", typeof(RectTransform), typeof(CanvasRenderer));
        costGO.transform.SetParent(root.transform);
        var costRt = costGO.GetComponent<RectTransform>();
        costRt.sizeDelta = new Vector2(80, 30);
        costRt.anchoredPosition = new Vector2(0, -25);

        var text = costGO.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "0";
        text.fontSize = 18;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.white;

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
        rt.sizeDelta = new Vector2(450, 320); // 6 buton için genişletildi
        // World Space'de ideal boyutta görünmesi için ölçeklendiriyoruz (Kullanıcı Talebi: Daha Büyük)
        rt.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        
        // Arka Plan Image
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(root.transform);
        bg.transform.localScale = Vector3.one; // Ölçek Garantisi
        bg.transform.localPosition = Vector3.zero;
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // Ana Container (Dikey Dizilim)
        GameObject container = new GameObject("Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
        container.transform.SetParent(root.transform);
        container.transform.localScale = Vector3.one;
        container.transform.localPosition = Vector3.zero;
        container.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        container.GetComponent<RectTransform>().anchorMax = Vector2.one;
        container.GetComponent<RectTransform>().offsetMin = new Vector2(15, 15);
        container.GetComponent<RectTransform>().offsetMax = new Vector2(-15, -15);
        
        var vGroup = container.GetComponent<VerticalLayoutGroup>();
        vGroup.childAlignment = TextAnchor.MiddleCenter;
        vGroup.spacing = 15;
        vGroup.childControlHeight = true;
        vGroup.childControlWidth = true;
        vGroup.childForceExpandHeight = false;
        vGroup.childForceExpandWidth = true;

        // Üst Satır (Yatay Dizilim - 3 Buton İçin)
        GameObject rowUpper = new GameObject("RowUpper", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowUpper.transform.SetParent(container.transform);
        rowUpper.transform.localScale = Vector3.one;
        var hGroupUpper = rowUpper.GetComponent<HorizontalLayoutGroup>();
        hGroupUpper.childAlignment = TextAnchor.MiddleCenter;
        hGroupUpper.spacing = 20;
        hGroupUpper.childControlHeight = true;
        hGroupUpper.childControlWidth = false;

        // Alt Satır (Yatay Dizilim - 2 Buton İçin)
        GameObject rowLower = new GameObject("RowLower", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowLower.transform.SetParent(container.transform);
        rowLower.transform.localScale = Vector3.one;
        var hGroupLower = rowLower.GetComponent<HorizontalLayoutGroup>();
        hGroupLower.childAlignment = TextAnchor.MiddleCenter;
        hGroupLower.spacing = 20;
        hGroupLower.childControlHeight = true;
        hGroupLower.childControlWidth = false;

        // Script Referanslarını Bağla
        TowerSelectionUI uiScript = root.GetComponent<TowerSelectionUI>();
        var serializedObject = new SerializedObject(uiScript);
        serializedObject.FindProperty("rowUpper").objectReferenceValue = rowUpper.transform;
        serializedObject.FindProperty("rowLower").objectReferenceValue = rowLower.transform;
        
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
    private static void LinkAllIcons()
    {
        Debug.Log("Linking Icons...");
        
        // Tower Icons
        string[] towerGuids = AssetDatabase.FindAssets("t:TowerData");
        foreach (var guid in towerGuids)
        {
            TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            string iconPath = $"Assets/Data/Icons/{data.name}_Icon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            
            if (iconSprite != null)
            {
                data.icon = iconSprite;
                EditorUtility.SetDirty(data);
                Debug.Log($"Linked Icon for Tower: {data.name}");
            }
        }

        // Unit Icons
        string[] unitGuids = AssetDatabase.FindAssets("t:UnitData");
        foreach (var guid in unitGuids)
        {
            UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            string iconPath = $"Assets/Data/Icons/{data.name}_Icon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            
            if (iconSprite != null)
            {
                data.icon = iconSprite;
                EditorUtility.SetDirty(data);
                Debug.Log($"Linked Icon for Unit: {data.name}");
            }
        }
        
        AssetDatabase.SaveAssets();
    }

    private static void LinkAllCounterparts()
    {
        Debug.Log("Linking Counterparts...");
        
        // Tüm TowerData'ları bul
        string[] towerGuids = AssetDatabase.FindAssets("t:TowerData");
        foreach (var guid in towerGuids)
        {
            TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            string counterpartName = GetCounterpartName(data.name);
            if (!string.IsNullOrEmpty(counterpartName))
            {
                TowerData counterpart = FindDataByName<TowerData>(counterpartName);
                if (counterpart != null)
                {
                    data.enemyCounterpart = counterpart;
                    EditorUtility.SetDirty(data);
                    Debug.Log($"Linked Tower: {data.name} <-> {counterpartName}");
                }
            }
        }

        // Tüm UnitData'ları bul
        string[] unitGuids = AssetDatabase.FindAssets("t:UnitData");
        foreach (var guid in unitGuids)
        {
            UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            string counterpartName = GetCounterpartName(data.name);
            if (!string.IsNullOrEmpty(counterpartName))
            {
                UnitData counterpart = FindDataByName<UnitData>(counterpartName);
                if (counterpart != null)
                {
                    data.enemyCounterpart = counterpart;
                    EditorUtility.SetDirty(data);
                    Debug.Log($"Linked Unit: {data.name} <-> {counterpartName}");
                }
            }
        }
    }

    private static T FindDataByName<T>(string name) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:" + typeof(T).Name);
        if (guids.Length > 0)
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return null;
    }

    private static string GetCounterpartName(string originName)
    {
        switch (originName)
        {
            // Towers
            case "Archer_Tower": return "Bone_Catapult";
            case "Bone_Catapult": return "Archer_Tower";
            case "Ballista_Tower": return "Cannon_Tower";
            case "Cannon_Tower": return "Ballista_Tower";
            case "Mage_Tower": return "Dark_Sentry";
            case "Dark_Sentry": return "Mage_Tower";
            case "Solar_Prism": return "Soul_Harvester";
            case "Soul_Harvester": return "Solar_Prism";
            case "Poison_Spitter": return "Void_Obelisk";
            case "Void_Obelisk": return "Poison_Spitter";
            case "Barracks": return "Graveyard";
            case "Graveyard": return "Barracks";

            // Units
            case "Celestial_Archer": return "Shadow_Stalker";
            case "Shadow_Stalker": return "Celestial_Archer";
            case "Holy_Scout": return "Plague_Runner";
            case "Plague_Runner": return "Holy_Scout";
            case "Iron_Knight": return "Skeleton_Warrior";
            case "Skeleton_Warrior": return "Iron_Knight";
            case "Light_Swordsman": return "Skeleton_Warrior";
            case "Skeleton_Warrior_Ally": return "Light_Swordsman";
            case "Wraith": return "Light_Swordsman";
            case "Shield_Bearer": return "Abyssal_Behemoth";
            case "Abyssal_Behemoth": return "Shield_Bearer";

            default: return "";
        }
    }

    private static void Apply3DVisualsToTower(GameObject root, string towerName)
    {
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.SetParent(root.transform);
        visuals.transform.localPosition = Vector3.zero;

        string basePath = "";
        string weaponPath = "";
        string baseModel = "";
        string weaponModel = "";

        switch (towerName)
        {
            case "Archer_Tower": baseModel = "tower-round-build-a.fbx"; weaponModel = "weapon-ballista.fbx"; break;
            case "Ballista_Tower": baseModel = "tower-round-build-b.fbx"; weaponModel = "weapon-ballista.fbx"; break;
            case "Bone_Catapult": baseModel = "tower-round-build-c.fbx"; weaponModel = "weapon-catapult.fbx"; break;
            case "Cannon_Tower": baseModel = "tower-round-build-d.fbx"; weaponModel = "weapon-cannon.fbx"; break;
            case "Dark_Sentry": baseModel = "tower-round-build-e.fbx"; weaponModel = "weapon-turret.fbx"; break;
            case "Mage_Tower": baseModel = "tower-round-build-f.fbx"; weaponModel = "tower-round-crystals.fbx"; break;
            case "Poison_Spitter": baseModel = "tower-round-crystals.fbx"; weaponModel = "weapon-turret.fbx"; break;
            case "Solar_Prism": baseModel = "tower-square-build-a.fbx"; weaponModel = "detail-crystal-large.fbx"; break;
            case "Soul_Harvester": baseModel = "tower-square-build-b.fbx"; weaponModel = "detail-crystal.fbx"; break;
            case "Void_Obelisk": baseModel = "tower-square-build-c.fbx"; weaponModel = "detail-rocks-large.fbx"; break;
        }

        string fbxFolder = "Assets/kenney_tower-defense-kit/Models/FBX format/";

        if (!string.IsNullOrEmpty(baseModel))
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxFolder + baseModel);
            if (basePrefab != null)
            {
                GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                b.transform.SetParent(visuals.transform);
                b.transform.localPosition = Vector3.zero;
            }
        }

        if (!string.IsNullOrEmpty(weaponModel))
        {
            GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxFolder + weaponModel);
            if (weaponPrefab != null)
            {
                GameObject w = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
                w.name = "Weapon";
                w.transform.SetParent(visuals.transform);
                w.transform.localPosition = new Vector3(0, 1.2f, 0); // Kule üzerine yerleştir

                // FirePoint'i Silaha Bağla (Böylece mermiler silahla birlikte döner)
                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(w.transform);
                firePoint.transform.localPosition = new Vector3(0, 0, 1f); // Silahın biraz önünde
            }
        }
    }

    private static void Apply3DVisualsToUnit(GameObject root, string unitName)
    {
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.SetParent(root.transform);
        visuals.transform.localPosition = Vector3.zero;

        string folderName = "";
        switch (unitName)
        {
            case "Celestial_Archer": folderName = "Erika Archer"; break;
            case "Shadow_Stalker": folderName = "Vampire A Lusth"; break;
            case "Holy_Scout": folderName = "Remy"; break;
            case "Plague_Runner": folderName = "Mutant"; break;
            case "Iron_Knight": folderName = "Vanguard By T. Choonyung"; break;
            case "Skeleton_Warrior": folderName = "Warrok W Kurniawan"; break;
            case "Light_Swordsman": folderName = "Ch10_nonPBR"; break;
            case "Wraith": folderName = "Exo Gray"; break;
            case "Shield_Bearer": folderName = "The Boss"; break;
            case "Abyssal_Behemoth": folderName = "Ely By K.Atienza"; break;
        }

        if (!string.IsNullOrEmpty(folderName))
        {
            string modelPath = $"Assets/Models/Characters/{folderName}/{folderName}.fbx";
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelPrefab != null)
            {
                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
                modelInstance.transform.SetParent(visuals.transform);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity; 
                modelInstance.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

                Animator anim = modelInstance.GetComponent<Animator>();
                if (anim == null) anim = modelInstance.AddComponent<Animator>();
                
                string controllerPath = $"Assets/Models/Characters/{folderName}/{folderName}.controller";
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    anim.runtimeAnimatorController = controller;
                }
            }
        }
    }

    private static void Apply3DVisualsToProjectile(GameObject root, string name, Color color)
    {
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.SetParent(root.transform);
        visuals.transform.localPosition = Vector3.zero;

        string fbxFolder = "Assets/kenney_tower-defense-kit/Models/FBX format/";
        string modelName = "";
        
        Material projectileMat = GetOrCreateProjectileMaterial(name, color);

        switch (name)
        {
            case "Archer_Arrow": modelName = "weapon-ammo-arrow.fbx"; break;
            case "Ballista_Bolt": modelName = "weapon-ammo-arrow.fbx"; break;
            case "Bone_Projectyle": modelName = "weapon-ammo-boulder.fbx"; break;
            case "Cannon_Ball": modelName = "weapon-ammo-cannonball.fbx"; break;
            case "Poison_Drip": modelName = "weapon-ammo-bullet.fbx"; break;
            default: // Büyü mermileri için sphere kullanacağız
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(visuals.transform);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                if (sphere.GetComponent<Collider>()) GameObject.DestroyImmediate(sphere.GetComponent<Collider>());
                
                // Standart materyali renklendir ve parlat
                MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
                mr.sharedMaterial = projectileMat;
                break;
        }

        if (!string.IsNullOrEmpty(modelName))
        {
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxFolder + modelName);
            if (modelPrefab != null)
            {
                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
                modelInstance.transform.SetParent(visuals.transform);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity; 
                modelInstance.transform.localScale = Vector3.one * 1.5f;

                // Orijinal materyalleri koru - Material override kaldırıldı
            }
        }

        // --- Trail Renderer Ekle ---
        TrailRenderer tr = root.AddComponent<TrailRenderer>();
        tr.time = 0.2f;
        tr.startWidth = 0.15f;
        tr.endWidth = 0f;
        tr.sharedMaterial = GetOrCreateTrailMaterial();
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        tr.colorGradient = gradient;
    }

    private static Material GetOrCreateProjectileMaterial(string name, Color color)
    {
        string folder = "Assets/Materials/Projectiles";
        EnsureFolders(folder);

        string path = folder + "/" + name + "_Mat.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(FindProperShader());
            AssetDatabase.CreateAsset(mat, path);
        }
        
        // --- KRİTİK: Shader'ı her zaman güncelle (Eski Standard materyalleri URP'ye çekmek için) ---
        mat.shader = FindProperShader();
        
        // Hem Standard hem URP için renkleri set et
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 1.5f); 
        if (mat.HasProperty("_EmissionMap")) mat.SetColor("_EmissionColor", color * 1.5f);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material GetOrCreateTrailMaterial()
    {
        string folder = "Assets/Materials/Projectiles";
        EnsureFolders(folder);
        string path = folder + "/ProjectileTrail_Mat.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    private static Shader FindProperShader()
    {
        // URP Lit dene
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Sprites/Default");
        return s;
    }

    private static Side GetSideByName(string name)
    {
        string n = name.ToLower();
        // Karanlık birimler ve kuleler
        if (n.Contains("dark") || n.Contains("bone") || n.Contains("poison") || n.Contains("void") || 
            n.Contains("soul") || n.Contains("abyssal") || n.Contains("plague") || n.Contains("shadow") || 
            n.Contains("skeleton") || n.Contains("wraith"))
        {
            return Side.Dark;
        }
        return Side.Light;
    }
}
