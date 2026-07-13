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

            // 0. MASTER REPAIR (New)
            DrawSection("MASTER SYSTEMS", () => {
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // Yeşil tonlu (Başarılı/Güvenli)
                if (GUILayout.Button("COMPLETE SYSTEM REPAIR (Data, UI, Prefabs)", GUILayout.Height(50)))
                {
                    Debug.Log("Starting COMPLETE SYSTEM REPAIR...");

                    // 0. Build/Update all base gameplay prefabs
                    EditorPrefabBuilder.GeneratePrefabs();

                    // 1. Create Tower Slot Prefabs (Visuals)
                    UIMasterPrefabCreator.CreateTowerSlotPrefab();

                    // 2. Data & Logic
                    DataAssetGenerator.GenerateAllData(); 
                    
                    // 3. UI Panels (Atomic)
                    UIMasterPrefabCreator.CreateTowerUpgradeUIPrefab();
                    UIMasterPrefabCreator.CreateSkillTreeUIPrefab();
                    UIMasterPrefabCreator.CreateLevelSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateSideSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateHeroShopPanelPrefab();
                    UIMasterPrefabCreator.CreateLevelResultUIPrefab();
                    UIMasterPrefabCreator.CreateSpellSlotPrefabs();
                    UIMasterPrefabCreator.CreateUnitButtonPrefab();
                    UIMasterPrefabCreator.CreateTowerSlotPrefab();
                    UIMasterPrefabCreator.CreateCompendiumPanelPrefab();

                    // 4. Configurations
                    DataAssetGenerator.ConfigureTowerPrefabs();
                    DataAssetGenerator.ConfigureUnitPrefabs();
                    
                    // 5. Master Prefabs (Composite)
                    UIMasterPrefabCreator.CreateCoreEnginePrefab();
                    UIMasterPrefabCreator.CreateMainMenuMaster();
                    UIMasterPrefabCreator.CreateGameplayHUDMaster();

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("✔ ALL SYSTEMS REPAIRED SUCCESSFULLY! (Map prefabs are not touched — assign manually)");
                }
                GUI.backgroundColor = Color.white;
            });

            EditorGUILayout.HelpBox("Use the button above to regenerate, link, and repair all Data Assets, Game Prefabs, and UI Systems. Map prefabs are NOT regenerated — assign them manually via LevelData.", MessageType.Info);

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
