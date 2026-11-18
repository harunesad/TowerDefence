using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TowerDefence.Data;

public class LevelMapConfigurator : EditorWindow
{
    private LevelData targetLevel;
    private GameObject mapRoot;

    [MenuItem("Tools/Tower Defence/Map Configurator")]
    public static void ShowWindow()
    {
        GetWindow<LevelMapConfigurator>("Map Configurator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Map Configurator (Drag & Drop)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "1. Drag the LevelData asset you want to edit below.\n" +
            "2. Drag the active Map root object from the hierarchy.\n" +
            "3. Drag and position Spawner_X, Base_X, WP_X, and Slot_X objects in scene.\n" +
            "4. Click Save.", 
            MessageType.Info);

        EditorGUILayout.Space();

        targetLevel = (LevelData)EditorGUILayout.ObjectField("Hedef Level Data", targetLevel, typeof(LevelData), false);
        mapRoot = (GameObject)EditorGUILayout.ObjectField("Sahnedeki Harita (Root)", mapRoot, typeof(GameObject), true);

        EditorGUILayout.Space();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Sahnedeki Objelerden Oku ve LEVELDATA'YA KAYDET", GUILayout.Height(50)))
        {
            if (targetLevel == null || mapRoot == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Level Data asset and the Map object from scene.", "OK");
                return;
            }

            ParseAndSave();
        }
        GUI.backgroundColor = Color.white;
    }

    private void ParseAndSave()
    {
        List<Vector3> newTowerSlots = new List<Vector3>();
        List<Vector3> newBasePoints = new List<Vector3>();
        List<LevelPath> newPaths = new List<LevelPath>();

        // 1. Base Points'i Bul
        Transform[] allTransforms = mapRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name.StartsWith("Base_"))
            {
                newBasePoints.Add(t.position);
            }
        }

        // 2. Tower Slots'lari Bul
        Transform towerSlotsRoot = null;
        foreach (Transform t in allTransforms)
        {
            if (t.name == "TowerSlots")
            {
                towerSlotsRoot = t;
                break;
            }
        }

        if (towerSlotsRoot != null)
        {
            foreach (Transform slot in towerSlotsRoot)
            {
                if (slot.name.StartsWith("Slot_"))
                {
                    newTowerSlots.Add(slot.position);
                }
            }
        }
        else
        {
            // Eger "TowerSlots" objesi yoksa, isminde Slot_ gecen her seyi alalim
            foreach (Transform t in allTransforms)
            {
                if (t.name.StartsWith("Slot_"))
                {
                    newTowerSlots.Add(t.position);
                }
            }
        }

        // 3. Path'leri (Waypoints) Bul
        // Path_0, Path_1 gibi ana objeleri bul
        List<Transform> pathRoots = new List<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.name.StartsWith("Path_"))
            {
                pathRoots.Add(t);
            }
        }

        // Her Path icin WP_'leri topla ve siraya diz
        for (int i = 0; i < pathRoots.Count; i++)
        {
            LevelPath newLevelPath = new LevelPath();
            newLevelPath.points = new List<Vector3>();
            
            // ismin sonundaki sayiyi spawnerIndex yapalim (Path_0 -> 0)
            int spawnerIndex = 0;
            string[] parts = pathRoots[i].name.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int parsedIndex))
            {
                spawnerIndex = parsedIndex;
            }
            newLevelPath.spawnerIndex = spawnerIndex;

            // WP_'leri topla
            List<Transform> waypoints = new List<Transform>();
            foreach (Transform child in pathRoots[i])
            {
                if (child.name.StartsWith("WP_"))
                {
                    waypoints.Add(child);
                }
            }

            // WP_0, WP_1 ismine gore sirala
            waypoints.Sort((a, b) => 
            {
                int aIndex = GetWaypointIndex(a.name);
                int bIndex = GetWaypointIndex(b.name);
                return aIndex.CompareTo(bIndex);
            });

            foreach (Transform wp in waypoints)
            {
                newLevelPath.points.Add(wp.position);
            }

            newPaths.Add(newLevelPath);
        }

        // Verileri LevelData'ya aktar
        targetLevel.customSlotPositions = newTowerSlots;
        targetLevel.basePoints = newBasePoints;
        targetLevel.paths = newPaths;

        EditorUtility.SetDirty(targetLevel);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=cyan>[Map Configurator]</color> Successfully read and saved into {targetLevel.name}!\n" +
                  $"Base Count: {newBasePoints.Count} | Tower Slots: {newTowerSlots.Count} | Path Count: {newPaths.Count}");
        
        EditorUtility.DisplayDialog("Success", $"{targetLevel.name} updated!\n" +
                  $"Base Points: {newBasePoints.Count}\nTower Slots: {newTowerSlots.Count}\nPaths: {newPaths.Count}", "OK");
    }

    private int GetWaypointIndex(string wpName)
    {
        // orn: "WP_5" veya "WP_0_5" (Eski sistemde alt WP'ler)
        string[] parts = wpName.Split('_');
        if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int val))
        {
            return val;
        }
        return 0;
    }
}
