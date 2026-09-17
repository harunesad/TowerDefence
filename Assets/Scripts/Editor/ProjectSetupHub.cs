using UnityEngine;
using UnityEditor;

namespace TowerDefence.Editor
{
    public class ProjectSetupHub : EditorWindow
    {
        [MenuItem("Tower Defence/Project Setup Hub")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetupHub>("Setup Hub");
        }

        private Vector2 scrollPos;

        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Project Initialization & Asset Management", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 1. DATA & LOGIC REPAIR (SAFE)
            DrawSection("DATA & LOGIC (Safe for UI)", () => {
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // Green
                if (GUILayout.Button("REPAIR DATA & GAMEPLAY PREFABS", GUILayout.Height(40)))
                {
                    Debug.Log("Starting DATA REPAIR...");
                    EditorPrefabBuilder.GeneratePrefabs();
                    VFXPrefabGenerator.GenerateVFXPrefabs();
                    DataAssetGenerator.GenerateAllData(); 
                    DataAssetGenerator.ConfigureTowerPrefabs();
                    DataAssetGenerator.ConfigureUnitPrefabs();
                    
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("✔ DATA & PREFABS REPAIRED SUCCESSFULLY! (Your UI changes are untouched)");
                }
                GUI.backgroundColor = Color.white;
            });

            // 1.5. UI STYLING & FONT REPAIR
            DrawSection("UI STYLING & FONTS (Safe)", () => {
                GUI.backgroundColor = new Color(0.5f, 0.8f, 1f); // Mavi (Blue)
                if (GUILayout.Button("REPAIR ALL FONTS (Apply Cinzel)", GUILayout.Height(40)))
                {
                    Debug.Log("Starting FONT REPAIR...");
                    UIFontFixer.FixAllFonts();
                }
                GUI.backgroundColor = Color.white;
            });

            // 2. UI REBUILD (DANGEROUS)
            DrawSection("UI GENERATION (WARNING: Overwrites Manual Changes)", () => {
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // Red
                if (GUILayout.Button("REBUILD ALL UI PREFABS FROM SCRATCH", GUILayout.Height(40)))
                {
                    Debug.Log("Starting UI REBUILD...");
                    UIMasterPrefabCreator.CreateTowerSlotPrefab();
                    UIMasterPrefabCreator.CreateTowerUpgradeUIPrefab();
                    UIMasterPrefabCreator.CreateSkillTreeUIPrefab();
                    UIMasterPrefabCreator.CreateLevelSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateSideSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateHeroShopPanelPrefab();
                    UIMasterPrefabCreator.CreateLevelResultUIPrefab();
                    UIMasterPrefabCreator.CreateSpellSlotPrefabs();
                    UIMasterPrefabCreator.CreateUnitButtonPrefab();
                    UIMasterPrefabCreator.CreateCompendiumPanelPrefab();
                    UIMasterPrefabCreator.CreateSettingsUIPrefab();
                    
                    UIMasterPrefabCreator.CreateCoreEnginePrefab();
                    UIMasterPrefabCreator.CreateMainMenuMaster();
                    UIMasterPrefabCreator.CreateGameplayHUDMaster();

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("✔ ALL UI PREFABS REBUILT FROM CODE.");
                }
                GUI.backgroundColor = Color.white;
            });

            EditorGUILayout.HelpBox("Use 'REPAIR DATA' to fix data/logic without losing your manual UI changes. Use 'REBUILD ALL UI' only if you want to wipe manual UI changes and regenerate from code.", MessageType.Info);

            EditorGUILayout.Space();

            // 1.5. LEVEL MAPS (4 Map grouping + panel rebuild)
            DrawSection("LEVEL SELECTION MAPS", () => {
                GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
                if (GUILayout.Button("GENERATE 4 LEVEL MAPS (Group Levels)", GUILayout.Height(40)))
                {
                    DataAssetGenerator.CreateLevelMapDataAssets();
                    UIMasterPrefabCreator.CreateLevelSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateMainMenuMaster();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("✔ 4 Level Maps generated & LevelSelectionPanel rebuild.");
                }
                GUI.backgroundColor = Color.white;
            });

            GUILayout.EndScrollView();
        }

        private void DrawSection(string title, System.Action content)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label(title, EditorStyles.miniBoldLabel);
            content?.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
