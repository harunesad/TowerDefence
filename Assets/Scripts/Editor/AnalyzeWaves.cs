using UnityEngine;
using UnityEditor;
using System.Linq;
using TowerDefence.Data;

public class AnalyzeWaves {
    public static void Run() {
        string[] levelGuids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Levels" });
        string[] bosses = new string[] { "Abyssal_Behemoth", "Blood_Mage", "Bone_Dragon", "Death_Knight", "Demon_King", "Shadow_Leviathan" };
        
        foreach (var guid in levelGuids) {
            LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid));
            if (levelData == null || levelData.waves == null) continue;
            
            for (int i = 0; i < levelData.waves.Count; i++) {
                var wave = levelData.waves[i];
                if (wave == null || wave.unitGroups == null) continue;
                
                bool hasBoss = wave.unitGroups.Any(g => g.unitData != null && bosses.Contains(g.unitData.name));
                if (hasBoss && i != levelData.waves.Count - 1) {
                    Debug.LogError($"Level {levelData.name} Wave {i+1} has a BOSS but is NOT the last wave (Total {levelData.waves.Count})!");
                }
            }
        }
        Debug.Log("Analysis complete.");
    }
}
