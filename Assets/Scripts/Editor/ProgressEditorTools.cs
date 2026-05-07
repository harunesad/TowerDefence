using UnityEngine;
using UnityEditor;
using TowerDefence.Core;
using System.IO;

public class ProgressEditorTools : Editor
{
    [MenuItem("Tower Defence/Progress/Reset All Save Data")]
    public static void ResetSaveData()
    {
        if (EditorUtility.DisplayDialog("Reset Progress", "Are you sure you want to delete all save data? This cannot be undone.", "Yes", "No"))
        {
            if (Application.isPlaying && MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.ResetSave();
            }
            else
            {
                string path = Path.Combine(Application.persistentDataPath, "meta_progression.json");
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log("Save file deleted manually.");
                }
                else
                {
                    Debug.Log("No save file found to delete.");
                }
            }
        }
    }

    [MenuItem("Tower Defence/Progress/Add 1000 Karma")]
    public static void AddKarma()
    {
        if (Application.isPlaying && MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.AddKarma(1000);
            Debug.Log("Added 1000 Karma for testing.");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "You must be in Play Mode to add Karma!", "OK");
        }
    }

    [MenuItem("Tower Defence/Progress/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
