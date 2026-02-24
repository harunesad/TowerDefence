using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TowerDefence.UI;

public class EditorPrefabBuilder : Editor
{
    [MenuItem("Tower Defence/Tools/Generate UI Prefabs")]
    public static void GeneratePrefabs()
    {
        string folderPath = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        CreateLevelButtonPrefab(folderPath);
        CreateSkillNodePrefab(folderPath);
        CreateTowerButtonPrefab(folderPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("UI Prefabs generated successfully in " + folderPath);
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
}
