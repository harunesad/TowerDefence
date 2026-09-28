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
        GUILayout.Label("Level Harita Konfigüratörü (Sürükle-Bırak)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "1. Düzenlediğiniz LevelData dosyasını aşağıya sürükleyin.\n" +
            "2. Hiyerarşide açık olan Harita (Map) kök objesini sürükleyin.\n" +
            "3. Sahnedeki Spawner_X, Base_X, WP_X ve Slot_X objelerini sürükleyerek yerleştirin.\n" +
            "4. Kaydet butonuna basın.", 
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
                EditorUtility.DisplayDialog("Hata", "Lütfen hem Level Data dosyasını hem de sahnedeki Harita objesini seçin.", "Tamam");
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

        // 2. Tower Slots'ları Bul
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
            // Eğer "TowerSlots" objesi yoksa, isminde Slot_ geçen her şeyi alalım
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

        // Her Path için WP_'leri topla ve sıraya diz
        for (int i = 0; i < pathRoots.Count; i++)
        {
            LevelPath newLevelPath = new LevelPath();
            newLevelPath.points = new List<Vector3>();
            
            // İsmin sonundaki sayıyı spawnerIndex yapalım (Path_0 -> 0)
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

            // WP_0, WP_1 ismine göre sırala
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

        Debug.Log($"<color=cyan>[Map Configurator]</color> Başarıyla okundu ve {targetLevel.name} içine kaydedildi!\n" +
                  $"Base Sayısı: {newBasePoints.Count} | Kule Slotu: {newTowerSlots.Count} | Yol(Path) Sayısı: {newPaths.Count}");
        
        EditorUtility.DisplayDialog("Başarılı", $"{targetLevel.name} güncellendi!\n" +
                  $"Base Noktaları: {newBasePoints.Count}\nKule Slotları: {newTowerSlots.Count}\nYol Sayısı: {newPaths.Count}", "Tamam");
    }

    private int GetWaypointIndex(string wpName)
    {
        // Örn: "WP_5" veya "WP_0_5" (Eski sistemde alt WP'ler)
        string[] parts = wpName.Split('_');
        if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int val))
        {
            return val;
        }
        return 0;
    }
}
