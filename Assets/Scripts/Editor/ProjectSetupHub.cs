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
                if (GUILayout.Button("COMPLETE SYSTEM REPAIR (Data, UI, Prefabs)", GUILayout.Height(40)))
                {
                    // 0. Build/Update all base gameplay prefabs (incorporating new Meshy 3D Models for towers & preserving Mixamo models for units)
                    EditorPrefabBuilder.GeneratePrefabs();

                    // 1. Data & Logic
                    DataAssetGenerator.GenerateAllData(); 
                    
                    // 2. UI Panels (Atomic)
                    UIMasterPrefabCreator.CreateTowerUpgradeUIPrefab();
                    UIMasterPrefabCreator.CreateSkillTreeUIPrefab();
                    UIMasterPrefabCreator.CreateLevelSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateSideSelectionPanelPrefab();
                    UIMasterPrefabCreator.CreateLevelResultUIPrefab();
                    UIMasterPrefabCreator.CreateSpellSlotPrefabs();
                    UIMasterPrefabCreator.CreateUnitButtonPrefab();

                    // 3. Configurations
                    DataAssetGenerator.ConfigureTowerPrefabs();
                    DataAssetGenerator.ConfigureUnitPrefabs();
                    
                    // 4. Master Prefabs (Composite)
                    UIMasterPrefabCreator.CreateCoreEnginePrefab();
                    UIMasterPrefabCreator.CreateMainMenuMaster();
                    UIMasterPrefabCreator.CreateGameplayHUDMaster();
                    
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("✔ ALL SYSTEMS REPAIRED SUCCESSFULLY!");
                }
                GUI.backgroundColor = Color.white;
            });

            // 1. Core Systems Group
            DrawSection("Core Infrastructure", () => {
                if (GUILayout.Button("Generate/Update CORE ENGINE Master Prefab"))
                {
                    UIMasterPrefabCreator.CreateCoreEnginePrefab();
                }
            });

            // 2. Veri Asset Grubu
            DrawSection("Data Asset Generation", () => {
                if (GUILayout.Button("Generate/Update All Data Assets (Towers, Units, Skills, Spells)"))
                {
                    DataAssetGenerator.GenerateAllData(); // Bu zaten içerideydi
                }
                if (GUILayout.Button("REGENERATE ALL LEVELS (New Paths & Teams)"))
                {
                    DataAssetGenerator.GenerateLevels();
                }
                if (GUILayout.Button("Configure All Tower Prefabs (Embedded UI)"))
                {
                    DataAssetGenerator.ConfigureTowerPrefabs();
                }
                if (GUILayout.Button("AUTO-REPAIR MISSING TOWER REFERENCES (Prefabs, Icons)"))
                {
                    DataAssetGenerator.FixMissingTowerReferences();
                }
                if (GUILayout.Button("AUTO-REPAIR MISSING UNIT REFERENCES (Safe-Link)"))
                {
                    DataAssetGenerator.FixMissingUnitReferences();
                }
                if (GUILayout.Button("Configure All Unit Prefabs (Health Bars)"))
                {
                    DataAssetGenerator.ConfigureUnitPrefabs();
                }
            });

            // 2. UI Prefab Grubu
            DrawSection("Standard UI Prefabs", () => {
                if (GUILayout.Button("Create/Update TowerUpgradeUI Prefab"))
                    UIMasterPrefabCreator.CreateTowerUpgradeUIPrefab();
                
                if (GUILayout.Button("Create/Update SkillTree UI Prefab"))
                    UIMasterPrefabCreator.CreateSkillTreeUIPrefab();

                if (GUILayout.Button("Create/Update Level Selection Panel (+ LevelButton)"))
                    UIMasterPrefabCreator.CreateLevelSelectionPanelPrefab();

                if (GUILayout.Button("Create/Update Side Selection Panel"))
                    UIMasterPrefabCreator.CreateSideSelectionPanelPrefab();

                if (GUILayout.Button("Create/Update UnitButton Prefab"))
                    UIMasterPrefabCreator.CreateUnitButtonPrefab();

                if (GUILayout.Button("Create/Update Gameplay HUD Prefab"))
                    UIMasterPrefabCreator.CreateGameplayHUDMaster();

                if (GUILayout.Button("Create/Update Level Result Panel"))
                    UIMasterPrefabCreator.CreateLevelResultUIPrefab();

                if (GUILayout.Button("Create/Update Tower Slot Prefab (Visuals)"))
                    UIMasterPrefabCreator.CreateTowerSlotPrefab();

                if (GUILayout.Button("Initialize Default Level Layout Designs"))
                    UIMasterPrefabCreator.InitializeDefaultLevelLayouts();

                if (GUILayout.Button("GENERATE ALL LEVEL MAPS (Advanced)"))
                    UIMasterPrefabCreator.GenerateAllLevelMaps();

                if (GUILayout.Button("MASSIVE SCALE & MATERIAL POLISH (Gameplay)"))
                    UIMasterPrefabCreator.PolishAllGameplayPrefabs();
            });

            // 3. MASTER PREFABS
            DrawSection("MASTER PREFABS (Everything-as-Prefab)", () => {
                if (GUILayout.Button("Generate/Update MainMenu Master Prefab"))
                    UIMasterPrefabCreator.CreateMainMenuMaster();

                if (GUILayout.Button("Generate/Update Gameplay HUD Master Prefab"))
                    UIMasterPrefabCreator.CreateGameplayHUDMaster();
            });

            // 4. Other Tools
            DrawSection("Spell System Setup", () => {
                if (GUILayout.Button("Create/Update Spell Slot Prefabs"))
                    UIMasterPrefabCreator.CreateSpellSlotPrefabs();
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
