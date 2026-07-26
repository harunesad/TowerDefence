using UnityEngine;
using UnityEditor;
using TowerDefence.Data;
using TowerDefence.Combat;
using TowerDefence.UI;
using TowerDefence.Core;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public class DataAssetGenerator : Editor
{
    public static void GenerateAllData()
    {
        const string towerPath = "Assets/Data/Towers";
        const string unitPath = "Assets/Data/Units";

        EnsureDirectory(towerPath);
        EnsureDirectory(unitPath);

        // --- PREFAB ENSURE ---
        if (!System.IO.File.Exists($"{towerPath.Replace("Data", "Prefabs/Gameplay")}/Barracks.prefab"))
            AssetDatabase.CopyAsset($"{towerPath.Replace("Data", "Prefabs/Gameplay")}/Archer_Tower.prefab", $"{towerPath.Replace("Data", "Prefabs/Gameplay")}/Barracks.prefab");
        if (!System.IO.File.Exists($"{towerPath.Replace("Data", "Prefabs/Gameplay")}/Graveyard.prefab"))
            AssetDatabase.CopyAsset($"{towerPath.Replace("Data", "Prefabs/Gameplay")}/Dark_Sentry.prefab", $"{towerPath.Replace("Data", "Prefabs/Gameplay")}/Graveyard.prefab");

        // --- TOWERS (5 Light, 5 Dark) ---
        // Light Towers
        CreateTower(towerPath, "Archer Tower", Side.Light, 100, 10, 1.5f, 12, 0, true);
        CreateTower(towerPath, "Cannon Tower", Side.Light, 250, 40, 0.5f, 12, 3, true);
        CreateTower(towerPath, "Mage Tower", Side.Light, 200, 20, 0.8f, 11, 0, true, StatusEffectType.Slow, 2f, 0.5f);
        CreateTower(towerPath, "Ballista Tower", Side.Light, 350, 150, 0.3f, 17, 0, true); 
        CreateTower(towerPath, "Barracks", Side.Light, 300, 0, 0f, 40f, 0, true); 
        CreateTower(towerPath, "Graveyard", Side.Dark, 300, 0, 0f, 40f, 0, true); 

        // Dark Towers
        CreateTower(towerPath, "Dark Sentry", Side.Dark, 80, 8, 2.0f, 11, 0, true);
        CreateTower(towerPath, "Void Obelisk", Side.Dark, 380, 0, 0f, 15, 0, true); 
        CreateTower(towerPath, "Poison Spitter", Side.Dark, 220, 15, 1.0f, 11, 0, true, StatusEffectType.Poison, 5f, 2f);
        CreateTower(towerPath, "Bone Catapult", Side.Dark, 320, 60, 0.5f, 18, 4, true); 
        CreateTower(towerPath, "Soul Harvester", Side.Dark, 150, 12, 3.0f, 11, 0, true);
        CreateTower(towerPath, "Graveyard", Side.Dark, 300, 0, 0f, 28f, 0, true); 

        // --- UNITS (25 Light, 25 Dark) ---
        string[] lightUnitNames = new string[] {
            "Light Swordsman", "Novice Archer", "Spearman", "Scout", "Militia Defender", // 0-4
            "Holy Knight", "Crossbowman", "Cleric of the Dawn", "Cavalry Rider", "Battle Mage", "Griffin Tamer", "Shieldmaiden", // 5-11
            "Sun Priestess", "Pegasus Knight", "Arcane Sorcerer", "Light Golem", "Celestial Blade", "Dwarven Cannoneer", "Elven Ranger", // 12-18
            "Archangel", "Holy Colossus", "Grand Paladin", "Phoenix Summoner", "Avatar of Light", "Dragon of the Sun" // 19-24
        };

        string[] darkUnitNames = new string[] {
            "Goblin Grunt", "Skeleton Warrior", "Orc Marauder", "Cultist Initiate", "Zombie Shambler",
            "Dark Knight", "Skeleton Archer", "Necromancer", "Gargoyle", "Orc Berserker", "Shadow Assassin", "Spider Rider",
            "Vampire Lord", "Lich", "Bone Golem", "Succubus", "Wraith", "Troll Brute", "Dark Elf Sniper",
            "Demon King", "Bone Dragon", "Abyssal Behemoth", "Death Knight Commander", "Blood Mage", "Shadow Leviathan"
        };

        UnitData[] lightUnits = new UnitData[25];
        UnitData[] darkUnits = new UnitData[25];

        // Local helper for Archetype stats
        void AssignStats(string name, int tier, out float hp, out float dmg, out float spd, out float range, out float rate)
        {
            string lower = name.ToLower();
            bool isBoss = tier == 4;
            bool isTank = lower.Contains("defender") || lower.Contains("bearer") || lower.Contains("golem") || lower.Contains("brute") || lower.Contains("behemoth") || lower.Contains("colossus");
            bool isAssassin = lower.Contains("scout") || lower.Contains("assassin") || lower.Contains("runner") || lower.Contains("spider") || lower.Contains("rider");
            bool isRanger = lower.Contains("archer") || lower.Contains("crossbow") || lower.Contains("sniper") || lower.Contains("ranger") || lower.Contains("cannoneer");
            bool isMage = lower.Contains("mage") || lower.Contains("cleric") || lower.Contains("priest") || lower.Contains("sorcerer") || lower.Contains("necromancer") || lower.Contains("lich") || lower.Contains("cultist") || lower.Contains("summoner") || lower.Contains("wraith");

            // Base Tier Multipliers
            float baseHp = tier == 1 ? 100 : tier == 2 ? 220 : tier == 3 ? 500 : 1200;
            float baseDmg = tier == 1 ? 12 : tier == 2 ? 30 : tier == 3 ? 75 : 150;

            if (isBoss)
            {
                hp = baseHp * 1.5f; dmg = baseDmg * 1.2f; spd = 0.8f; range = 2.0f; rate = 1.2f;
                if (isMage || lower.Contains("dragon") || lower.Contains("avatar") || lower.Contains("leviathan")) range = 6.0f;
            }
            else if (isTank)
            {
                hp = baseHp * 1.8f; dmg = baseDmg * 0.7f; spd = 0.6f; range = 1.2f; rate = 1.5f;
            }
            else if (isAssassin)
            {
                hp = baseHp * 0.7f; dmg = baseDmg * 1.4f; spd = 1.4f; range = 1.2f; rate = 0.6f;
            }
            else if (isRanger)
            {
                hp = baseHp * 0.8f; dmg = baseDmg * 1.1f; spd = 1.0f; range = 5.0f; rate = 1.0f;
            }
            else if (isMage)
            {
                hp = baseHp * 0.7f; dmg = baseDmg * 1.3f; spd = 0.8f; range = 5.0f; rate = 1.5f;
            }
            else // Fighter
            {
                hp = baseHp; dmg = baseDmg; spd = 1.0f; range = 1.5f; rate = 1.0f;
            }
        }

        for (int i = 0; i < 25; i++)
        {
            int tier = (i < 5) ? 1 : (i < 12) ? 2 : (i < 19) ? 3 : 4;
            int cost = tier == 1 ? 50 : tier == 2 ? 120 : tier == 3 ? 250 : 500;
            int reward = tier == 1 ? 20 : tier == 2 ? 50 : tier == 3 ? 100 : 200;
            
            AssignStats(lightUnitNames[i], tier, out float lHp, out float lDmg, out float lSpd, out float lRange, out float lRate);
            AssignStats(darkUnitNames[i], tier, out float dHp, out float dDmg, out float dSpd, out float dRange, out float dRate);

            lightUnits[i] = CreateUnit(unitPath, lightUnitNames[i], Side.Light, cost, reward, lHp, lSpd, lDmg, lRange, lRate);
            darkUnits[i] = CreateUnit(unitPath, darkUnitNames[i], Side.Dark, cost, reward, dHp, dSpd, dDmg, dRange, dRate);

            // Counterparts
            lightUnits[i].enemyCounterpart = darkUnits[i];
            darkUnits[i].enemyCounterpart = lightUnits[i];

            EditorUtility.SetDirty(lightUnits[i]);
            EditorUtility.SetDirty(darkUnits[i]);
        }

        // --- TAMİR VE YAPILANDIRMA ZİNCİRİ (Chain Repair) ---
        LinkEliteTowerSpecializations(towerPath);
        LinkTowerCounterparts(towerPath);
        FixMissingTowerReferences();
        FixMissingUnitReferences();
        ConfigureAuraTowers(towerPath);
        
        ConfigureTowerPrefabs(towerPath);
        ConfigureUnitPrefabs(unitPath);
        GenerateHeroes(); // YENİ: Kahramanları üret
        FixHeroReferences(); // Kahraman ikonları ve karşılıklarını düzelt
        
        GenerateDefaultSpells(); // Büyüleri önce üret (Yetenekler bunlara bağlı)
        LinkMetaSkills("Assets/Data/Skills");
        PopulateManagerSkills("Assets/Data/Skills");
        
        // --- ELITE TOWER GENERATION ---
        GenerateEliteTowerPrefabs(towerPath);

        // --- UI REGENERATION (Kritik: Yeni yetenekleri UI'ya ekler) ---
        TowerDefence.Editor.UIMasterPrefabCreator.CreateSkillTreeUIPrefab();
        TowerDefence.Editor.UIMasterPrefabCreator.CreateSideSelectionPanelPrefab();
        TowerDefence.Editor.UIMasterPrefabCreator.CreateHeroShopPanelPrefab();
        TowerDefence.Editor.UIMasterPrefabCreator.CreateGameplayHUDMaster();

        LinkUnitProjectiles();
        GenerateLevels(); 

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✔ ALL DATA, PREFABS AND UI GENERATED, LINKED, AND REPAIRED SUCCESSFULLY!");
    }

    public static void GenerateHeroes()
    {
        string heroDataPath = "Assets/Data/Heroes";
        string heroPrefabPath = "Assets/Prefabs/Gameplay/Heroes";

        EnsureDirectory(heroDataPath);
        EnsureDirectory(heroPrefabPath);

        var heroDefs = new (string id, string name, Side side, string sourceUnit, float hp, float spd, float dmg, float rng, float rate, int unlockCost, bool starter, HeroAbilityType ability, string abName, string abDesc, float abCd, float abPow, float abRad)[]
        {
            ("Hero_Light_Arthur",  "Arthur Pendragon",  Side.Light, "Light_Swordsman",   500, 1.0f,  40, 1.8f, 1.0f, 0,   true,  HeroAbilityType.RallyHeal,     "Rally Heal",      "Restores 15% of max health.", 14f, 1f, 0f),
            ("Hero_Light_Paladin", "Iron Paladin",      Side.Light, "Grand_Paladin",       650, 0.85f, 35, 1.6f, 0.9f, 600, false, HeroAbilityType.HolyShield,     "Holy Shield",     "Heals when health drops below 35%.", 16f, 1f, 0f),
            ("Hero_Light_Archon",  "Celestial Archon",  Side.Light, "Elven_Ranger",  420, 1.1f,  45, 2.2f, 1.1f, 750, false, HeroAbilityType.ArrowRain,      "Arrow Rain",      "Damages all enemies in an area.", 12f, 1f, 4f),
            ("Hero_Light_Scout",   "Holy Scout",        Side.Light, "Scout",        380, 1.25f, 32, 1.7f, 1.3f, 550, false, HeroAbilityType.SwiftStrike,    "Swift Strike",    "Deals a burst of bonus damage.", 10f, 1f, 0f),
            ("Hero_Light_Bulwark", "Shield Bearer",     Side.Light, "Shieldmaiden",     720, 0.8f,  30, 1.5f, 0.85f,650, false, HeroAbilityType.FortifyTaunt,   "Fortify",         "Taunts nearby enemies.", 18f, 1f, 5f),
            ("Hero_Light_Solar",   "Sun Knight",        Side.Light, "Holy_Knight",       480, 1.0f,  38, 1.9f, 1.0f, 800, false, HeroAbilityType.SolarSmite,     "Solar Smite",     "Holy explosion around the hero.", 13f, 1f, 3.5f),
            ("Hero_Dark_Vampire",  "Crimson Count",     Side.Dark,  "Vampire_Lord",    450, 1.2f,  35, 1.5f, 1.2f, 600, false, HeroAbilityType.LifeDrain,      "Life Drain",      "Steals health from the target.", 11f, 1f, 0f),
            ("Hero_Dark_Reaper",   "Soul Reaper",       Side.Dark,  "Wraith",            380, 1.3f,  42, 1.7f, 1.3f, 750, false, HeroAbilityType.SoulExecute,    "Soul Execute",    "Executes wounded enemies.", 15f, 1f, 0f),
            ("Hero_Dark_Behemoth", "Abyssal Lord",      Side.Dark,  "Abyssal_Behemoth",  700, 0.75f, 50, 1.4f, 0.8f, 900, false, HeroAbilityType.GroundSlam,     "Ground Slam",     "Slams the ground for heavy AoE damage.", 14f, 1f, 4f),
            ("Hero_Dark_Plague",   "Plague Herald",     Side.Dark,  "Zombie_Shambler",     400, 1.15f, 36, 1.6f, 1.15f,550, false, HeroAbilityType.PlagueCloud,    "Plague Cloud",    "Poisons enemies in an area.", 12f, 1f, 4f),
            ("Hero_Dark_Bone",     "Bone Commander",    Side.Dark,  "Skeleton_Warrior",  520, 0.95f, 34, 1.5f, 1.0f, 650, false, HeroAbilityType.BoneArmor,      "Bone Armor",      "Reinforces the hero with bone plating.", 16f, 1f, 0f),
            ("Hero_Dark_Stalker",  "Night Stalker",     Side.Dark,  "Shadow_Assassin",    430, 1.35f, 40, 1.6f, 1.25f,800, false, HeroAbilityType.ShadowStep,     "Shadow Step",     "Teleports behind the target.", 13f, 1f, 0f),
        };

        foreach (var def in heroDefs)
        {
            string safeName = def.name.Replace(" ", "_");
            string sourcePfb = $"Assets/Prefabs/Gameplay/Units/{def.sourceUnit}.prefab";
            string targetPfb = $"{heroPrefabPath}/{safeName}.prefab";

            if (!System.IO.File.Exists(targetPfb) && System.IO.File.Exists(sourcePfb))
                AssetDatabase.CopyAsset(sourcePfb, targetPfb);
        }

        AssetDatabase.Refresh();

        foreach (var def in heroDefs)
        {
            string safeName = def.name.Replace(" ", "_");
            string targetPfb = $"{heroPrefabPath}/{safeName}.prefab";

            UnitData unitData = CreateHeroAsset(heroDataPath, def.name, def.side, 0, 0,
                def.hp, def.spd, def.dmg, def.rng, def.rate, targetPfb);

            ConfigureHeroPrefab(targetPfb, unitData, def.name);

            CreateHeroDataAsset(heroDataPath, def.id, def.name, def.side, unitData,
                def.unlockCost, def.starter, def.ability, def.abName, def.abDesc, def.abCd, def.abPow, def.abRad);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✔ Heroes (UnitData + HeroData) generated successfully!");
    }

    private static HeroData CreateHeroDataAsset(string path, string heroID, string displayName, Side side,
        UnitData unitData, int unlockCost, bool isStarter, HeroAbilityType abilityType, string abilityName,
        string abilityDescription, float abilityCooldown, float abilityPower, float abilityRadius)
    {
        string safeName = displayName.Replace(" ", "_");
        string assetPath = $"{path}/{safeName}_HeroData.asset";

        HeroData data = AssetDatabase.LoadAssetAtPath<HeroData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<HeroData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.heroID = heroID;
        data.displayName = displayName;
        data.side = side;
        data.unitData = unitData;
        data.icon = unitData != null ? unitData.icon : null;
        data.unlockKarmaCost = unlockCost;
        data.isStarterHero = isStarter;
        data.maxUpgradeLevel = 5;
        data.upgradeKarmaCost = 150;
        data.healthBonusPerLevel = 0.15f;
        data.damageBonusPerLevel = 0.10f;
        data.abilityType = abilityType;
        data.abilityName = abilityName;
        data.abilityDescription = abilityDescription;
        data.abilityCooldown = abilityCooldown;
        data.abilityPower = abilityPower;
        data.abilityRadius = abilityRadius;

        EditorUtility.SetDirty(data);
        return data;
    }

    public static Sprite FindIcon(string iconName)
    {
        string trimmedIconName = iconName.Trim();
        string cleanIconName = trimmedIconName.Replace("_Icon", "").Replace("_", "").ToLower();
        string iconsFolder = "Assets/Data/Icons";
        
        if (!System.IO.Directory.Exists(iconsFolder))
        {
            Debug.LogError($"[FindIcon] Icons folder not found: {iconsFolder}");
            return null;
        }

        string[] files = System.IO.Directory.GetFiles(iconsFolder, "*.*", System.IO.SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string ext = System.IO.Path.GetExtension(file).ToLower();
            if (ext == ".meta") continue;

            string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(file);
            string nameWithoutExtTrimmed = nameWithoutExt.Trim();
            string cleanFileName = nameWithoutExtTrimmed.Replace("_Icon", "").Replace("_", "").ToLower();

            if (nameWithoutExtTrimmed.Equals(trimmedIconName, System.StringComparison.OrdinalIgnoreCase) ||
                cleanFileName.Equals(cleanIconName, System.StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = file.Replace("\\", "/");
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(relativePath);
                Sprite firstSprite = null;

                foreach (var asset in assets)
                {
                    if (asset is Sprite s)
                    {
                        firstSprite = s;
                        break;
                    }
                }

                if (firstSprite != null)
                {
                    return firstSprite;
                }

                // Sprite bulunamadıysa (Multiple ama dilimlenmemiş veya Unity bug'ı) zorla reimport edelim.
                Debug.LogWarning($"[FindIcon] Found file {relativePath} for {iconName} but NO Sprite sub-asset exists. Forcing reimport...");
                TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    if (importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                    }
                    importer.SaveAndReimport();
                    
                    // Reimport sonrası tekrar yüklemeyi dene
                    assets = AssetDatabase.LoadAllAssetsAtPath(relativePath);
                    foreach (var asset in assets)
                    {
                        if (asset is Sprite s)
                        {
                            Debug.Log($"[FindIcon] Successfully generated Sprite for {iconName} after forcing reimport!");
                            return s;
                        }
                    }
                }
                Debug.LogError($"[FindIcon] Failed to get Sprite from {relativePath} even after reimport!");
            }
        }
        return null; // Gereksiz log kirliliğini önlemek için buradaki uyarı logunu kaldırdık.
    }

    private static UnitData CreateHeroAsset(string path, string name, Side side, int cost, int reward, float health, float speed, float damage, float range, float rate, string prefabPath)
    {
        string safeName = name.Replace(" ", "_");
        string assetPath = $"{path}/{safeName}.asset";

        UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<UnitData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.unitName = name;
        data.side = side;
        data.spawnCost = cost;
        data.killReward = reward;
        data.maxHealth = health;
        data.moveSpeed = speed;
        data.attackDamage = damage;
        data.attackRange = range;
        data.attackRate = rate;

        data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        data.icon = FindIcon($"{safeName}_Icon");
        if (data.icon == null)
        {
            // Fallback to source icons if hero-specific ones don't exist
            string fallbackIconName = (side == Side.Light) ? "Light_Swordsman_Icon" : "Shadow_Stalker_Icon";
            data.icon = FindIcon(fallbackIconName);
        }

        // Heroes don't have enemy counterparts
        data.enemyCounterpart = null;

        if (name == "Celestial Archon")
        {
            data.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/Projectiles/Arrow.prefab");
        }
        else if (name == "Plague Herald")
        {
            data.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/Projectiles/DarkOrb.prefab");
        }
        else
        {
            data.projectilePrefab = null;
        }

        EditorUtility.SetDirty(data);
        return data;
    }

    private static void ConfigureHeroPrefab(string prefabPath, UnitData data, string heroName)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null) return;

        // --- Custom Model Integration ---
        string pascalName = heroName.Replace(" ", "");
        string sideStr = (data.side == Side.Light) ? "Light" : "Dark";
        string fbxPath = $"Assets/Models/Heroes/{sideStr}/{pascalName}/{pascalName}.fbx";
        string animPath = $"Assets/Models/Heroes/{sideStr}/{pascalName}/{pascalName}Animator.controller";

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        RuntimeAnimatorController animCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animPath);

        if (fbxAsset != null)
        {
            Transform visuals = root.transform.Find("Visuals");
            if (visuals != null)
            {
                // Visuals altındaki her şeyi (eski modeli) sil
                for (int i = visuals.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(visuals.GetChild(i).gameObject, true);
                }
            }
            else
            {
                // Eğer Visuals yoksa, yarat (Normalde Unit'lerde Visuals her zaman olur)
                GameObject visObj = new GameObject("Visuals");
                visObj.transform.SetParent(root.transform, false);
                visuals = visObj.transform;
            }

            // Instantiate new model
            GameObject newModel = PrefabUtility.InstantiatePrefab(fbxAsset) as GameObject;
            newModel.transform.SetParent(visuals, false);
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;

            // Set Animator Controller (EXACTLY like Apply3DVisualsToUnit does it!)
            Animator anim = newModel.GetComponent<Animator>();
            if (anim == null) anim = newModel.AddComponent<Animator>();
            
            // Try to find the Animator Controller in the hero's model folder (same way as EditorPrefabBuilder)
            string modelDir = System.IO.Path.GetDirectoryName(fbxPath).Replace("\\", "/");
            string[] animGuids = AssetDatabase.FindAssets($"t:RuntimeAnimatorController", new[] { modelDir });

            if (animGuids.Length > 0)
            {
                string controllerPath = AssetDatabase.GUIDToAssetPath(animGuids[0]);
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    anim.runtimeAnimatorController = controller;
                }
            }

            // --- Attach Weapons to Hero Model ---
            GetWeaponForUnit(heroName, out string rWeapon, out string lWeapon);
            AttachWeaponToBone(newModel, heroName, "RightHand", rWeapon);
            AttachWeaponToBone(newModel, heroName, "LeftHand", lWeapon);

            // --- Fire Point Setup ---
            EditorPrefabBuilder.EnsureRangedUnitFirePoints(root, heroName);
            
            // Special case for Cleric of the Dawn - set exact fire point position
            if (heroName.Contains("Cleric"))
            {
                // Find fire point and set exact position
                Transform firePoint = FindFirstFirePoint(root.transform);
                if (firePoint != null)
                {
                    firePoint.localPosition = new Vector3(-0.108f, 1.118f, 0.005f);
                }
            }
        }

        // Set Layer
        int targetLayer = (data.side == Side.Light) ? 6 : 7;
        SetLayerRecursive(root, targetLayer);

        // Replace Unit/Soldier component with HeroUnit
        Unit existingUnit = root.GetComponent<Unit>();
        if (existingUnit != null)
        {
            DestroyImmediate(existingUnit, true);
        }

        HeroUnit heroComp = root.AddComponent<HeroUnit>();

        // Wire UnitData reference
        SerializedObject so = new SerializedObject(heroComp);
        so.FindProperty("unitData").objectReferenceValue = data;
        
        Transform fp = FindFirstFirePoint(root.transform);
        if (fp != null)
        {
            so.FindProperty("firePoint").objectReferenceValue = fp;
        }
        
        so.ApplyModifiedProperties();

        // 2. Health Bar UI Setup (Same as normal unit setup but offset for hero visual distinction if desired)
        Transform hbTransform = root.transform.Find("HealthBarCanvas");
        HealthBarUI hbScript = null;
        float targetY = 6.0f; // Above unit head
        Sprite emptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prefabs/UI/Empty.png");

        if (hbTransform == null)
        {
            GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(HealthBarUI));
            canvasGo.transform.SetParent(root.transform);
            canvasGo.transform.localPosition = new Vector3(0, targetY, 0); 
            canvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1.8f, 0.25f); // Slightly larger for Heroes
            
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            hbScript = canvasGo.GetComponent<HealthBarUI>();

            // Background
            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform);
            bgGo.transform.localPosition = Vector3.zero;
            bgGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1.8f, 0.25f);
            Image bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = emptySprite;
            bgImg.color = Color.red; // Background is always red

            // Fill
            GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(bgGo.transform);
            fillGo.transform.localPosition = Vector3.zero;
            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(1.8f, 0.25f);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = emptySprite;
            fillImg.color = Color.yellow; // Yellow color to differentiate Hero health bar
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;

            // HealthBarUI Link
            var hbSo = new SerializedObject(hbScript);
            hbSo.FindProperty("fillImage").objectReferenceValue = fillImg;
            hbSo.FindProperty("container").objectReferenceValue = canvasGo;
            hbSo.ApplyModifiedProperties();
        }
        else
        {
            hbTransform.localPosition = new Vector3(0, targetY, 0);
            hbScript = hbTransform.GetComponent<HealthBarUI>();
            
            // Update existing colors/sprites
            Transform bg = hbTransform.Find("Background");
            if (bg != null)
            {
                Image bgImg = bg.GetComponent<Image>();
                bgImg.sprite = emptySprite;
                bgImg.color = Color.red;
                
                Transform fill = bg.Find("Fill");
                if (fill != null)
                {
                    Image fillImg = fill.GetComponent<Image>();
                    fillImg.sprite = emptySprite;
                    fillImg.color = Color.yellow;
                }
            }
        }

        // Link Hero to its HealthBar
        var heroSo = new SerializedObject(heroComp);
        heroSo.FindProperty("healthBar").objectReferenceValue = hbScript;
        heroSo.ApplyModifiedProperties();

        FixVisualModelGroundOffset(root);

        // Final Hero Transform Setup (Requirement: Scale 1.5, Y 0.2)
        root.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        // We set local position Y to 0.2 in the prefab itself
        root.transform.localPosition = new Vector3(root.transform.localPosition.x, 0.2f, root.transform.localPosition.z);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"✔ Configured Hero Prefab: {prefabPath}");
    }

    private static void FixVisualModelGroundOffset(GameObject root)
    {
        Transform visuals = root.transform.Find("Visuals");
        if (visuals == null) return;

        foreach (Transform child in visuals)
        {
            Vector3 lp = child.localPosition;
            child.localPosition = new Vector3(lp.x, 0f, lp.z);
        }
    }
    
    // Helper method to find first fire point in hierarchy
    private static Transform FindFirstFirePoint(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "FirePoint")
                return child;
            Transform found = FindFirstFirePoint(child);
            if (found != null) return found;
        }
        return null;
    }
    
    // Replicate GetWeaponForUnit and AttachWeaponToBone from EditorPrefabBuilder since they're private there
    private static void GetWeaponForUnit(string unitName, out string rightWeapon, out string leftWeapon)
    {
        rightWeapon = ""; leftWeapon = "";
        string n = unitName.Replace(" ", "").Replace("_", "");
        
        switch (n)
        {
            case "LightSwordsman": rightWeapon = "BasicBroadsword"; leftWeapon = "HeavyTowerShield"; break;
            case "NoviceArcher": leftWeapon = "LightBow"; break;
            case "Spearman": rightWeapon = "IronSpear"; break;
            case "Scout": rightWeapon = "DualShortDaggers"; leftWeapon = "DualShortDaggers"; break;
            case "MilitiaDefender": rightWeapon = "OneHandedHammer"; leftWeapon = "HeavyTowerShield"; break;
            case "HolyKnight": rightWeapon = "HolyGreatsword"; break;
            case "Crossbowman": rightWeapon = "HeavyMechanicalCrossbow"; break;
            case "ClericoftheDawn": rightWeapon = "GlowingSunMace"; break;
            case "CavalryRider": rightWeapon = "KnightsLance"; break;
            case "BattleMage": rightWeapon = "MageStaff"; break;
            case "GriffinTamer": rightWeapon = "BeastmasterWhip"; break;
            case "Shieldmaiden": rightWeapon = "SilverBattleaxe"; leftWeapon = "RoundShield"; break;
            case "SunPriestess": rightWeapon = "MageStaff"; break;
            case "PegasusKnight": rightWeapon = "IronSpear"; break;
            case "CelestialBlade": rightWeapon = "BasicBroadsword"; leftWeapon = "BasicBroadsword"; break;
            case "DwarvenCannoneer": rightWeapon = "PortableHandCannon"; break;
            case "ElvenRanger": leftWeapon = "LightBow"; break;
            case "Archangel": rightWeapon = "FlamingBattleaxe"; break;
            case "GrandPaladin": rightWeapon = "HolyGreatsword"; leftWeapon = "HeavyTowerShield"; break;
            case "PhoenixSummoner": rightWeapon = "MageStaff"; break;
            case "ArcaneSorcerer": rightWeapon = "MageStaff"; break;
            case "Lich": rightWeapon = "NecromancerStaff"; break;
            case "GoblinGrunt": rightWeapon = "DualShortDaggers"; leftWeapon = "RoundShield"; break;
            case "SkeletonWarrior": rightWeapon = "BasicBroadsword"; break;
            case "OrcMarauder": rightWeapon = "OrcAxe"; break;
            case "CultistInitiate": rightWeapon = "DualShortDaggers"; break;
            case "DarkKnight": rightWeapon = "CursedGreatsword"; break;
            case "SkeletonArcher": leftWeapon = "BoneBow"; break;
            case "Necromancer": rightWeapon = "NecromancerStaff"; break;
            case "OrcBerserker": rightWeapon = "OrcAxe"; leftWeapon = "OrcAxe"; break;
            case "ShadowAssassin": rightWeapon = "DualShortDaggers"; leftWeapon = "DualShortDaggers"; break;
            case "SpiderRider": rightWeapon = "IronSpear"; break;
            case "VampireLord": rightWeapon = "BasicBroadsword"; break;
            case "Succubus": rightWeapon = "BeastmasterWhip"; break;
            case "Wraith": rightWeapon = "SpectralScythe"; break;
            case "TrollBrute": rightWeapon = "TreeTrunkClub"; break;
            case "DarkElfSniper": rightWeapon = "HeavyMechanicalCrossbow"; break;
            case "DemonKing": rightWeapon = "FlamingBattleaxe"; break;
            case "DeathKnightCommander": rightWeapon = "KnightsLance"; break;
            case "BloodMage": rightWeapon = "NecromancerStaff"; break;
            // Hero specific weapons
            case "ArthurPendragon": rightWeapon = "BasicBroadsword"; leftWeapon = "RoundShield"; break;
            case "IronPaladin": rightWeapon = "OneHandedHammer"; leftWeapon = "HeavyTowerShield"; break;
            case "CelestialArchon": leftWeapon = "LightBow"; break;
            case "HolyScout": rightWeapon = "BasicBroadsword"; leftWeapon = "BasicBroadsword"; break;
            case "ShieldBearer": rightWeapon = "BasicBroadsword"; leftWeapon = "HeavyTowerShield"; break;
            case "SunKnight": rightWeapon = "HolyGreatsword"; break;
            case "CrimsonCount": rightWeapon = "BasicBroadsword"; break;
            case "SoulReaper": rightWeapon = "SpectralScythe"; break;
            case "AbyssalLord": rightWeapon = "SilverBattleaxe"; break;
            case "PlagueHerald": rightWeapon = "MageStaff"; break;
            case "BoneCommander": rightWeapon = "BasicBroadsword"; leftWeapon = "RoundShield"; break;
            case "NightStalker": rightWeapon = "DualShortDaggers"; leftWeapon = "DualShortDaggers"; break;
        }
    }
    
    private static Transform GetTransformRecursive(Transform parent, string nameToFind)
    {
        if (parent.name.Contains(nameToFind)) return parent;
        foreach (Transform child in parent)
        {
            Transform result = GetTransformRecursive(child, nameToFind);
            if (result != null) return result;
        }
        return null;
    }
    
    private static void AttachWeaponToBone(GameObject modelInstance, string unitName, string boneName, string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName)) return;

        Transform bone = GetTransformRecursive(modelInstance.transform, boneName);
        if (bone == null) 
        {
            if (boneName == "LeftHand") bone = GetTransformRecursive(modelInstance.transform, "LeftArm"); // Fallback
            if (bone == null) bone = modelInstance.transform; // Ultimate fallback
        }

        string[] weaponGuids = AssetDatabase.FindAssets($"{weaponName} t:GameObject", new[] { "Assets/Models/Weapons" });
        if (weaponGuids.Length > 0)
        {
            string weaponPath = AssetDatabase.GUIDToAssetPath(weaponGuids[0]);
            GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);
            if (weaponPrefab != null)
            {
                GameObject weaponInstance = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
                weaponInstance.transform.SetParent(bone);
                
                // Default position/rotation
                Vector3 localPos = Vector3.zero;
                Vector3 localRot = Vector3.zero;

                string n = unitName.Replace(" ", "").Replace("_", "");
                
                // Specific offsets requested by user
                if (n == "Archangel" && boneName == "RightHand") localPos = new Vector3(0, 0.1f, 0);
                else if (n == "DarkKnight" && boneName == "RightHand") localPos = new Vector3(0, 0.1f, -0.8f);
                else if (n == "CavalryRider" && boneName == "RightHand") localPos = new Vector3(0, 0.2f, 0);
                else if (n == "DeathKnightCommander" && boneName == "RightHand") localPos = new Vector3(0, 0.1f, 0);
                else if (n == "DwarvenCannoneer" && boneName == "RightHand") localPos = new Vector3(0, 0.9f, 0);
                else if (n == "ElvenRanger" && boneName == "LeftHand") { localPos = new Vector3(0, 0.1f, 0); localRot = new Vector3(180, 0, 0); }
                else if (n == "GoblinGrunt" && boneName == "LeftHand") { localPos = new Vector3(0, 0.3f, 0); localRot = new Vector3(180, 0, 0); }
                else if (n == "GoblinGrunt" && boneName == "RightHand") { localPos = new Vector3(-0.133f, 0.457f, 0f); localRot = new Vector3(180, 0, -30f); }
                else if (n == "LightSwordsman" && boneName == "RightHand") { localPos = new Vector3(0, 0.25f, 0); }
                else if (n == "MilitiaDefender" && boneName == "RightHand") { localPos = new Vector3(0.07f, 0.6f, 0.2f); }
                else if (n == "Crossbowman" && boneName == "RightHand") { localPos = new Vector3(-0.055f, 0.166f, 0.003f); localRot = new Vector3(-1.316f, 6.562f, -68.717f); }
                else if (n == "CultistInitiate" && boneName == "RightHand") { localPos = new Vector3(0.11f, 0.41f, 0f); localRot = new Vector3(180f, 0f, 0f); }
                else if (n == "DarkElfSniper" && boneName == "RightHand") { localPos = new Vector3(-0.021f, 0.15f, 0f); localRot = new Vector3(0f, 0f, -74.64f); }
                else if (n == "GrandPaladin" && boneName == "RightHand") { localRot = new Vector3(-21.668f, -87.826f, 31.072f); }
                else if (n == "GriffinTamer" && boneName == "RightHand") { localRot = new Vector3(-70f, -50f, 0f); }
                else if (n == "HolyKnight" && boneName == "RightHand") { localRot = new Vector3(0f, -100f, 0f); }
                else if (n == "Necromancer" && boneName == "RightHand") { localPos = new Vector3(0.002f, -0.258f, 0.598f); localRot = new Vector3(-57.415f, -0.426f, 0.505f); }
                else if (n == "Lich" && boneName == "RightHand") { localPos = new Vector3(-0.156f, 0.226f, 0.46f); localRot = new Vector3(0f, 40f, -90f); }
                else if (n == "BloodMage" && boneName == "RightHand") { localPos = new Vector3(0.459f, 0.093f, 0.323f); localRot = new Vector3(-85.171f, 0f, 55.586f); }
                else if (n == "ClericoftheDawn" && boneName == "RightHand") { localPos = new Vector3(0.314f, -0.017f, 0.167f); localRot = new Vector3(-72.599f, 0f, 62.466f); }
                else if (n == "NoviceArcher" && boneName == "LeftHand") { localRot = new Vector3(180f, 0f, 0f); }
                else if (n == "PegasusKnight" && boneName == "RightHand") { localRot = new Vector3(-82.8f, 0f, 0f); }
                else if (n == "Scout" && boneName == "RightHand") { localPos = new Vector3(0.087f, 0.457f, 0.018f); localRot = new Vector3(177.492f, 0.711f, -11.66f); }
                else if (n == "Scout" && boneName == "LeftHand") { localPos = new Vector3(-0.134f, 0.37f, 0.01f); localRot = new Vector3(181.759f, 0.27f, -34.403f); }
                else if (n == "ShadowAssassin" && boneName == "RightHand") { localPos = new Vector3(-0.088f, 0.389f, 0f); localRot = new Vector3(0f, 0f, 180f); }
                else if (n == "ShadowAssassin" && boneName == "LeftHand") { localPos = new Vector3(0.118f, 0.411f, 0f); localRot = new Vector3(0f, 0f, 142.1f); }
                else if (n == "Shieldmaiden" && boneName == "LeftHand") { localPos = new Vector3(0f, 0.199f, 0f); localRot = new Vector3(0f, 0f, 180f); }
                else if (n == "Shieldmaiden" && boneName == "RightHand") { localPos = new Vector3(0.006f, 0.13f, 0.031f); localRot = new Vector3(-136.749f, 0.002f, 1.206f); }
                else if (n == "SkeletonArcher" && boneName == "LeftHand") { localPos = new Vector3(-0.0015f, -0.0173f, -0.008f); localRot = new Vector3(66.442f, 87.481f, 55.308f); }
                else if (n == "SkeletonWarrior" && boneName == "RightHand") { localPos = new Vector3(0.006f, 0.231f, -0.003f); }
                else if (n == "Spearman" && boneName == "RightHand") { localPos = new Vector3(0f, 0.118f, 0f); localRot = new Vector3(-90f, 270f, 1f); }
                else if (n == "SpiderRider" && boneName == "RightHand") { localPos = new Vector3(0f, 0.275f, 0f); localRot = new Vector3(-90f, 0f, 1f); }
                else if (n == "Succubus" && boneName == "RightHand") { localPos = new Vector3(-0.007f, 0.157f, 0.002f); localRot = new Vector3(0f, 180f, 0f); }
                else if (n == "TrollBrute" && boneName == "RightHand") { localPos = new Vector3(0.001f, 0.155f, -0.421f); }
                else if (n == "Wraith" && boneName == "RightHand") { localPos = new Vector3(0.115f, 0.326f, 0.083f); localRot = new Vector3(0f, -66.934f, 0f); }
                // Hero specific offsets
                else if (n == "AbyssalLord" && boneName == "RightHand" && weaponName == "SilverBattleaxe") { localPos = new Vector3(0.019f, 0.289f, 0.064f); localRot = new Vector3(-53.404f, -4.937f, 52.444f); }
                else if (n == "CrimsonCount" && boneName == "RightHand" && weaponName == "BasicBroadsword") { localPos = new Vector3(0f, 0.3f, 0f); }
                else if (n == "HolyScout" && boneName == "RightHand" && weaponName == "BasicBroadsword") { localPos = new Vector3(0f, 0.3f, 0f); }
                else if (n == "HolyScout" && boneName == "LeftHand" && weaponName == "BasicBroadsword") { localPos = new Vector3(0f, 0.3f, 0f); }
                else if (n == "ArthurPendragon" && boneName == "LeftHand" && weaponName == "RoundShield") { localPos = new Vector3(0.00536f, 0.1236697f, -0.0156303f); localRot = new Vector3(-14.719f, 0f, 173.366f); }
                else if (n == "IronPaladin" && boneName == "LeftHand" && weaponName == "HeavyTowerShield") { localPos = new Vector3(-0.045f, -0.006f, 0.021f); localRot = new Vector3(0f, -70.423f, -14.894f); }
                else if (n == "IronPaladin" && boneName == "RightHand" && weaponName == "OneHandedHammer") { localPos = new Vector3(0.069f, 0.572f, 0.2f); }
                else if (n == "BoneCommander" && boneName == "LeftHand" && weaponName == "RoundShield") { localPos = new Vector3(0f, 0.198f, 0f); localRot = new Vector3(0f, 0f, 180f); }
                else if (n == "CelestialArchon" && boneName == "LeftHand" && weaponName == "LightBow") { localPos = new Vector3(0.01344674f, 0.1166808f, 0.00536013f); localRot = new Vector3(1.531f, 40.368f, 175.371f); }
                else if (n == "ShieldBearer" && boneName == "LeftHand" && weaponName == "HeavyTowerShield") { localPos = new Vector3(-0.076f, 0.145f, -0.096f); localRot = new Vector3(-39.585f, -9.913f, -74.341f); }
                else if (n == "ShieldBearer" && boneName == "RightHand" && weaponName == "BasicBroadsword") { localPos = new Vector3(0f, 0.3f, 0f); }
                else if (n == "SoulReaper" && boneName == "RightHand" && weaponName == "SpectralScythe") { localPos = new Vector3(-0.07f, 0.442f, 0.311f); localRot = new Vector3(-4.663f, -117.555f, -8.856f); }
                else if (n == "SunKnight" && boneName == "RightHand" && weaponName == "HolyGreatsword") { localPos = new Vector3(-0.03000016f, 0.08799966f, 0.01899962f); localRot = new Vector3(0f, -118.736f, 108.258f); }
                else if (n == "NightStalker" && boneName == "LeftHand" && weaponName == "DualShortDaggers") { localPos = new Vector3(0.024f, 0.424f, -0.089f); localRot = new Vector3(-18.845f, 7.012f, 159.153f); }
                else if (n == "NightStalker" && boneName == "RightHand" && weaponName == "DualShortDaggers") { localPos = new Vector3(-0.071f, 0.436f, 0f); localRot = new Vector3(0f, 0f, 180f); }
                else if (n == "ArthurPendragon" && boneName == "RightHand" && weaponName == "HolyGreatsword") { localRot = new Vector3(-21.668f, -87.826f, 31.072f); }

                weaponInstance.transform.localPosition = localPos;
                weaponInstance.transform.localRotation = Quaternion.Euler(localRot);
                weaponInstance.transform.localScale = Vector3.one; // Ensure scale is exactly (1,1,1)

                // If it's a ranged weapon, ensure fire point exists and is set up correctly
                if (IsRangedWeapon(weaponName) || (!string.IsNullOrEmpty(unitName) && unitName.Contains("Cleric")))
                {
                    // Use the EditorPrefabBuilder's method to create fire point if needed
                    EditorPrefabBuilder.EnsureFirePointOnWeapon(weaponInstance.transform, unitName);
                }
            }
        }
    }
    
    private static bool IsRangedWeapon(string weaponName)
    {
        switch (weaponName)
        {
            case "LightBow":
            case "BoneBow":
            case "MageStaff":
            case "NecromancerStaff":
            case "HeavyMechanicalCrossbow":
            case "PortableHandCannon":
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Solar Prism (Işık) ve Void Obelisk (Karanlık) kulelerini Aura Tower olarak otomatik yapılandırır.
    /// Bu kulelerin hasarı sıfır, rolleri tamamen destek ve buff verme üzerinedir.
    /// </summary>
    private static void ConfigureAuraTowers(string towerPath)
    {
        // Aura kulesi tanımlamaları: (dosya_adı, auraRadius, damagePct, fireRatePct)
        var auraTowers = new (string file, float radius, float dmgBonus, float frBonus)[]
        {
            // --- IŞIK AURA KULELERİ ---
            ("Solar_Prism",        8f, 0.25f, 0.15f),
            ("Luminous_Beamer",    9f, 0.35f, 0.20f), // Lvl-2 Solar Prism branşı
            ("Prismatic_Ray",      9f, 0.30f, 0.25f), // Lvl-2 Solar Prism branşı

            // --- KARANLIK AURA KULELERİ ---
            ("Void_Obelisk",       7f, 0.30f, 0.10f),
            ("Singularity_Pillar", 8f, 0.40f, 0.15f), // Lvl-2 Void Obelisk branşı
            ("Entropy_Core",       8f, 0.35f, 0.20f), // Lvl-2 Void Obelisk branşı
        };

        int count = 0;
        foreach (var (file, radius, dmgBonus, frBonus) in auraTowers)
        {
            string path = $"{towerPath}/{file}.asset";
            TowerData td = AssetDatabase.LoadAssetAtPath<TowerData>(path);
            if (td == null)
            {
                Debug.LogWarning($"⚠ [AuraTower] Bulunamadı: {path}");
                continue;
            }

            td.isAuraTower       = true;
            td.auraRadius        = radius;
            td.auraDamageBonus   = dmgBonus;
            td.auraFireRateBonus = frBonus;
            td.damage            = 0f;
            td.fireRate          = 0f;
            EditorUtility.SetDirty(td);
            count++;
            Debug.Log($"✔ [AuraTower] {file} → Radius:{radius} | Dmg+{dmgBonus*100:0}% | FR+{frBonus*100:0}%");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✔ [AuraTower] Toplam {count} aura kulesi yapılandırıldı.");
    }

    private static void LinkSpellData(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            System.IO.Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        // Meteor
        CreateSpellAsset(path, "Spell_Meteor", "Meteor Strike", "Deals 100 AoE damage and stuns enemies.", 50, 15f, SpellType.Meteor, 100f, 4f);
        // Freeze
        CreateSpellAsset(path, "Spell_Freeze", "Freeze Blast", "Freezes all enemies in area for 3 seconds.", 40, 20f, SpellType.Freeze, 3f, 5f);
        // Reinforcements
        CreateSpellAsset(path, "Spell_Reinforce", "Reinforcements", "Summons 2 temporary soldiers on the path.", 30, 25f, SpellType.Reinforcement, 0f, 0f);

        AssetDatabase.SaveAssets();
    }

    private static void CreateSpellAsset(string path, string id, string name, string desc, int cost, float cd, SpellType type, float power, float radius)
    {
        string fullPath = $"{path}/{id}.asset";
        SpellData spell = AssetDatabase.LoadAssetAtPath<SpellData>(fullPath);
        if (spell == null)
        {
            spell = ScriptableObject.CreateInstance<SpellData>();
            AssetDatabase.CreateAsset(spell, fullPath);
        }

        spell.spellID = id;
        spell.spellName = name;
        spell.description = desc;
        spell.manaCost = cost;
        spell.cooldown = cd;
        spell.spellType = type;
        spell.power = power;
        spell.radius = radius;

        EditorUtility.SetDirty(spell);
    }

    private static void LinkMetaSkills(string path)
    {
        EnsureDirectory(path, true); // TAM TEMİZLİK
        string spellPath = "Assets/Data/Spells";

        // Eco Branch
        SkillNodeData ecoRoot = CreateSkill(path, "Skill_Eco_Root", "Bountiful Start", "Start every level with +50 Gold", 200, UpgradeType.CurrencyStartBonus, 1.1f, Side.Neutral, null, null, SkillCategory.General_Economy, 0);

        // Light & Dark Root
        SkillNodeData lightRoot = CreateSkill(path, "Skill_Light_Root", "Light Initiation", "Light Tower damage +5%", 150, UpgradeType.TowerDamageBonus, 1.05f, Side.Light, null, null, SkillCategory.Light_Towers, 0);

        SkillNodeData darkRoot = CreateSkill(path, "Skill_Dark_Root", "Dark Initiation", "Dark Unit speed +5%", 150, UpgradeType.UnitSpeedBonus, 1.05f, Side.Dark, null, null, SkillCategory.Dark_Units, 0);

        // --- ECO UPGRADES ---
        SkillNodeData eco1 = CreateSkill(path, "Skill_Eco_1", "Wealthy Kingdom I", "Starting Gold +100", 400, UpgradeType.CurrencyStartBonus, 1.2f, Side.Neutral, ecoRoot, null, SkillCategory.General_Economy, 1);
        SkillNodeData eco2 = CreateSkill(path, "Skill_Eco_2", "Wealthy Kingdom II", "Starting Gold +250", 800, UpgradeType.CurrencyStartBonus, 1.5f, Side.Neutral, eco1, null, SkillCategory.General_Economy, 2);
        
        var goldSpell = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Neutral_Gold.asset");
        if (goldSpell != null)
            CreateSkill(path, "Skill_Unlock_GoldRush", "Gold Rush Spell", "Unlock the Gold Rush active spell", 1200, UpgradeType.UnlockSpell, 1, Side.Neutral, eco2, goldSpell, SkillCategory.General_Economy, 3);
            
        var eqSpell = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Neutral_Earthquake.asset");
        if (eqSpell != null)
            CreateSkill(path, "Skill_Unlock_Earthquake", "Earthquake Spell", "Unlock the Earthquake active spell", 1500, UpgradeType.UnlockSpell, 1, Side.Neutral, eco2, eqSpell, SkillCategory.General_Economy, 4);

        // --- LIGHT UPGRADES ---
        SkillNodeData archer1 = CreateSkill(path, "Skill_Archer_1", "Archer Potency I", "Light Archer damage +10%", 200, UpgradeType.TowerDamageBonus, 1.1f, Side.Light, lightRoot, null, SkillCategory.Light_Towers, 1);
        SkillNodeData archer2 = CreateSkill(path, "Skill_Archer_2", "Archer Potency II", "Light Archer damage +20%", 450, UpgradeType.TowerDamageBonus, 1.2f, Side.Light, archer1, null, SkillCategory.Light_Towers, 2);
        SkillNodeData archer3 = CreateSkill(path, "Skill_Archer_3", "Master Fletching", "Light Archer range +15%", 800, UpgradeType.RangeBonus, 1.15f, Side.Light, archer2, null, SkillCategory.Light_Towers, 4);

        // Hero Vitality
        SkillNodeData hero1 = CreateSkill(path, "Skill_Hero_1", "Heroic Vitality I", "Hero Health +15%", 300, UpgradeType.HealthBonus, 1.15f, Side.Light, lightRoot, null, SkillCategory.Light_Units, 1);
        SkillNodeData hero2 = CreateSkill(path, "Skill_Hero_2", "Heroic Vitality II", "Hero Health +30%", 600, UpgradeType.HealthBonus, 1.3f, Side.Light, hero1, null, SkillCategory.Light_Units, 2);
        
        var shieldSpell = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Shield.asset");
        if (shieldSpell != null)
            CreateSkill(path, "Skill_Unlock_Shield", "Divine Shield", "Unlock Divine Shield spell", 900, UpgradeType.UnlockSpell, 1, Side.Light, hero2, shieldSpell, SkillCategory.Light_Spells, 3);

        var blessSpell = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Blessing.asset");
        if (blessSpell != null)
            CreateSkill(path, "Skill_Unlock_Blessing", "Holy Blessing", "Unlock Holy Blessing spell", 1100, UpgradeType.UnlockSpell, 1, Side.Light, hero2, blessSpell, SkillCategory.Light_Spells, 4);

        // Meteor Path
        var met1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Meteor_1.asset");
        if (met1 != null)
        {
            SkillNodeData metNode1 = CreateSkill(path, "Skill_Met_1", "Meteor Strike", met1.description, 250, UpgradeType.UnlockSpell, 1, Side.Light, lightRoot, met1, SkillCategory.Light_Spells, 1);
            var met2 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Meteor_2.asset");
            if (met2 != null)
            {
                CreateSkill(path, "Skill_Met_2", "Elite Meteor", met2.description, 700, UpgradeType.UnlockSpell, 1, Side.Light, metNode1, met2, SkillCategory.Light_Spells, 2);
            }
        }

        // --- DARK UPGRADES ---
        SkillNodeData darkSpd1 = CreateSkill(path, "Skill_Dark_Spd_1", "Dark Haste I", "Dark unit speed +10%", 200, UpgradeType.UnitSpeedBonus, 1.1f, Side.Dark, darkRoot, null, SkillCategory.Dark_Units, 1);
        SkillNodeData darkSpd2 = CreateSkill(path, "Skill_Dark_Spd_2", "Dark Haste II", "Dark unit speed +20%", 450, UpgradeType.UnitSpeedBonus, 1.2f, Side.Dark, darkSpd1, null, SkillCategory.Dark_Units, 2);
        
        var bloodlust = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_Bloodlust.asset");
        if (bloodlust != null)
            CreateSkill(path, "Skill_Unlock_Bloodlust", "Bloodlust", "Unlock Bloodlust spell", 850, UpgradeType.UnlockSpell, 1, Side.Dark, darkSpd2, bloodlust, SkillCategory.Dark_Spells, 4);
            
        var plagueRain = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_PlagueRain.asset");
        if (plagueRain != null)
            CreateSkill(path, "Skill_Unlock_PlagueRain", "Plague Rain", "Unlock Plague Rain spell", 1100, UpgradeType.UnlockSpell, 1, Side.Dark, darkSpd2, plagueRain, SkillCategory.Dark_Spells, 5);

        // Magic/Lich Path
        SkillNodeData darkDmg1 = CreateSkill(path, "Skill_Dark_Dmg_1", "Void Essence I", "Dark Unit damage +10%", 300, UpgradeType.DamageBonus, 1.1f, Side.Dark, darkRoot, null, SkillCategory.Dark_Towers, 1);
        SkillNodeData darkDmg2 = CreateSkill(path, "Skill_Dark_Dmg_2", "Void Essence II", "Dark Unit damage +25%", 650, UpgradeType.DamageBonus, 1.25f, Side.Dark, darkDmg1, null, SkillCategory.Dark_Towers, 2);

        var riftSpell = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_Rift.asset");
        if (riftSpell != null)
            CreateSkill(path, "Skill_Unlock_Rift", "Abyssal Rift", "Unlock Abyssal Rift spell", 1000, UpgradeType.UnlockSpell, 1, Side.Dark, darkDmg2, riftSpell, SkillCategory.Dark_Spells, 3);

        // Freeze Path
        var freeze = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_Freeze.asset");
        if (freeze != null)
            CreateSkill(path, "Skill_Unlock_Freeze", "Shadow Freeze", "Unlock Shadow Freeze spell", 500, UpgradeType.UnlockSpell, 1, Side.Dark, darkRoot, freeze, SkillCategory.Dark_Spells, 1);

        // Extra Crystals / Modifiers
        SkillNodeData manaWell = CreateSkill(path, "Skill_Light_ManaWell", "Mana Well", "Starting Gold +30%", 350, UpgradeType.CurrencyStartBonus, 1.3f, Side.Light, hero1, null, SkillCategory.Light_Units, 3);
        SkillNodeData rapidFire = CreateSkill(path, "Skill_Light_RapidFire", "Rapid Fire", "Light Tower attack rate +15%", 400, UpgradeType.SpeedBonus, 1.15f, Side.Light, archer2, null, SkillCategory.Light_Towers, 3);

        CreateSkill(path, "Skill_Light_HolyBlessing", "Holy Blessing", "All Light units gain +15% to Health and Damage", 0, UpgradeType.HealthBonus, 1.15f, Side.Light, hero2, null, SkillCategory.Light_Units, 4, 50);
        CreateSkill(path, "Skill_Light_DivineProtection", "Divine Protection", "Light Tower Health +20%", 0, UpgradeType.HealthBonus, 1.2f, Side.Light, archer3, null, SkillCategory.Light_Towers, 5, 40);

        // Dark Modifiers
        SkillNodeData shadowArmor = CreateSkill(path, "Skill_Dark_ShadowArmor", "Shadow Armor", "Dark Unit Health +15%", 350, UpgradeType.HealthBonus, 1.15f, Side.Dark, darkSpd1, null, SkillCategory.Dark_Units, 3);
        SkillNodeData plague = CreateSkill(path, "Skill_Dark_Plague", "Plague", "Dark Tower damage +15%", 400, UpgradeType.TowerDamageBonus, 1.15f, Side.Dark, darkDmg1, null, SkillCategory.Dark_Towers, 3);

        CreateSkill(path, "Skill_Dark_DarkPact", "Dark Pact", "All Dark unit damage +20%", 0, UpgradeType.DamageBonus, 1.2f, Side.Dark, darkDmg2, null, SkillCategory.Dark_Towers, 4, 50);
        CreateSkill(path, "Skill_Dark_SoulHarvest", "Soul Harvest", "Dark Unit speed +25% and damage +10%", 0, UpgradeType.UnitSpeedBonus, 1.25f, Side.Dark, darkSpd2, null, SkillCategory.Dark_Units, 4, 40);

        // Extra Neutral
        SkillNodeData tradeRoutes = CreateSkill(path, "Skill_Neutral_TradeRoutes", "Trade Routes", "Starting Gold +200", 500, UpgradeType.CurrencyStartBonus, 1.7f, Side.Neutral, eco1, null, SkillCategory.General_Base, 2);
        
        CreateSkill(path, "Skill_Neutral_CrystalVault", "Crystal Vault", "Start every level with +15 Crystals", 0, UpgradeType.CurrencyStartBonus, 1f, Side.Neutral, eco2, null, SkillCategory.General_Base, 3, 30);
        CreateSkill(path, "Skill_Neutral_AncientWisdom", "Ancient Wisdom", "All units gain +10% to all stats", 0, UpgradeType.DamageBonus, 1.1f, Side.Neutral, ecoRoot, null, SkillCategory.General_Base, 1, 60);

        AssetDatabase.SaveAssets();
        PopulateManagerSkills(path);
    }

    private static void PopulateManagerSkills(string skillPath)
    {
        // Manager'ı bul (Genelde _Engine prefabı içindedir veya sahnede)
        MetaProgressionManager manager = UnityEngine.Object.FindAnyObjectByType<MetaProgressionManager>();
        if (manager == null)
        {
            // Eğer sahnede yoksa prefablardan ara
            string[] managerGuids = AssetDatabase.FindAssets("t:Prefab MetaProgressionManager");
            if (managerGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(managerGuids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                manager = prefab.GetComponent<MetaProgressionManager>();
            }
        }

        if (manager != null)
        {
            string[] skillGuids = AssetDatabase.FindAssets("t:SkillNodeData", new[] { skillPath });
            List<SkillNodeData> allSkills = new List<SkillNodeData>();
            foreach (string guid in skillGuids)
            {
                allSkills.Add(AssetDatabase.LoadAssetAtPath<SkillNodeData>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            var so = new SerializedObject(manager);
            var prop = so.FindProperty("allAvailableSkills");
            if (prop != null)
            {
                prop.ClearArray();
                for (int i = 0; i < allSkills.Count; i++)
                {
                    prop.InsertArrayElementAtIndex(i);
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = allSkills[i];
                }
                so.ApplyModifiedProperties();
                Debug.Log($"✔ Linked {allSkills.Count} skills to MetaProgressionManager.");
            }
        }
    }

    private static SkillNodeData CreateSkill(string path, string id, string name, string desc, int karmaCost, UpgradeType type, float mult, Side side, SkillNodeData req = null, SpellData grant = null, SkillCategory category = SkillCategory.General_Economy, int tier = 0, int crystalCost = 0)
    {
        string fullPath = $"{path}/{id}.asset";
        SkillNodeData skill = AssetDatabase.LoadAssetAtPath<SkillNodeData>(fullPath);
        if (skill == null)
        {
            skill = ScriptableObject.CreateInstance<SkillNodeData>();
            AssetDatabase.CreateAsset(skill, fullPath);
        }

        skill.skillID = id;
        skill.skillName = name;
        skill.description = desc;
        skill.karmaCost = karmaCost;
        skill.crystalCost = crystalCost;
        skill.upgradeType = type;
        skill.multiplier = mult;
        skill.side = side;
        skill.grantedSpell = grant;
        skill.category = category;
        skill.tier = tier;
        skill.requiredSkills = new System.Collections.Generic.List<SkillNodeData>();
        if (req != null) skill.requiredSkills.Add(req);
        
        // Skill ID -> İkon dosya adı eşleştirmesi (Assets/Data/Icons/Skills/ altındaki dosya isimleri)
        // skill_prompts.md belgesine göre aynı konsepti paylaşan yetenekler aynı ikonu kullanır.
        var iconMap = new System.Collections.Generic.Dictionary<string, string>
        {
            // === GENEL / NEUTRAL ===
            { "Skill_Eco_Root", "BountifulStart" },
            { "Skill_Eco_1", "BountifulStart" },          // Wealthy Kingdom I - aynı altın konsepti
            { "Skill_Eco_2", "BountifulStart" },          // Wealthy Kingdom II - aynı altın konsepti
            { "Skill_Unlock_GoldRush", "BountifulStart" },// Gold Rush - altın konsepti
            { "Skill_Unlock_Earthquake", "Earthquake" },
            { "Skill_Neutral_TradeRoutes", "TradeRoutes" },
            { "Skill_Neutral_CrystalVault", "CrystalVault" },
            { "Skill_Neutral_AncientWisdom", "AncientWisdom" },

            // === LIGHT FACTION ===
            { "Skill_Light_Root", "LightInitiation" },
            { "Skill_Archer_1", "ArcherPotency" },        // Archer Potency I
            { "Skill_Archer_2", "ArcherPotency" },        // Archer Potency II - aynı ikon
            { "Skill_Archer_3", "MasterFletching" },
            { "Skill_Light_RapidFire", "RapidFire" },
            { "Skill_Light_DivineProtection", "DivineProtection" },
            { "Skill_Hero_1", "HeroicVitality" },         // Heroic Vitality I
            { "Skill_Hero_2", "HeroicVitality" },         // Heroic Vitality II - aynı ikon
            { "Skill_Light_ManaWell", "ManaWell" },
            { "Skill_Light_HolyBlessing", "LightInitiation" }, // Holy Blessing passive - aynı konsept
            { "Skill_Unlock_Shield", "DivineShield" },
            { "Skill_Unlock_Blessing", "LightInitiation" }, // Holy Blessing spell
            { "Skill_Met_1", "MeteorStrike" },
            { "Skill_Met_2", "MeteorStrike" },             // Elite Meteor - aynı ikon

            // === DARK FACTION ===
            { "Skill_Dark_Root", "DarkInitiation" },
            { "Skill_Dark_Spd_1", "DarkHaste" },          // Dark Haste I
            { "Skill_Dark_Spd_2", "DarkHaste" },          // Dark Haste II - aynı ikon
            { "Skill_Dark_SoulHarvest", "SoulHarvest" },
            { "Skill_Dark_Dmg_1", "DarkInitiation" },     // Void Essence I - karanlık konsepti
            { "Skill_Dark_Dmg_2", "DarkInitiation" },     // Void Essence II - aynı ikon
            { "Skill_Dark_DarkPact", "DarkInitiation" },   // Dark Pact - karanlık konsepti
            { "Skill_Dark_Plague", "Plague" },
            { "Skill_Dark_ShadowArmor", "ShadowArmor" },
            { "Skill_Unlock_Bloodlust", "Bloodlust" },
            { "Skill_Unlock_PlagueRain", "PlagueRain" },
            { "Skill_Unlock_Rift", "AbyssalRift" },
            { "Skill_Unlock_Freeze", "ShadowFreeze" },
        };

        // Önce haritadan bul
        if (iconMap.TryGetValue(id, out string iconFileName))
        {
            skill.icon = FindIcon(iconFileName);
            if (skill.icon == null) skill.icon = FindIcon($"{iconFileName}_Icon");
        }

        // Haritada yoksa veya bulunamadıysa eski fallback yöntemini dene
        if (skill.icon == null)
        {
            string cleanName = name.Replace(" ", "").Replace("I", "").Replace("II", "").TrimEnd();
            skill.icon = FindIcon(cleanName);
        }
        if (skill.icon == null)
        {
            string cleanName = name.Replace(" ", "");
            skill.icon = FindIcon(cleanName);
        }

        EditorUtility.SetDirty(skill);
        AssetDatabase.SaveAssetIfDirty(skill);
        return skill;
    }

    private static void LinkEliteTowerSpecializations(string towerPath)
    {
        // --- LIGHT SIDE ---
        
        // 1. Archer Tower -> Sniper / Ranger
        TowerData archer = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Archer_Tower.asset");
        if (archer != null)
        {
            archer.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Sniper Tower", side: Side.Light, cost: 300, damage: 150, fireRate: 0.4f, range: 20f, explosion: 0f),
                CreateTower(towerPath, "Ranger Outpost", side: Side.Light, cost: 200, damage: 15, fireRate: 5f, range: 11f, explosion: 0f)
            };
            EditorUtility.SetDirty(archer);
        }

        // 2. Cannon Tower -> Siege / Volley
        TowerData cannon = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Cannon_Tower.asset");
        if (cannon != null)
        {
            cannon.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Siege Cannon", side: Side.Light, cost: 400, damage: 120, fireRate: 0.4f, range: 14f, explosion: 5f),
                CreateTower(towerPath, "Volley Mortar", side: Side.Light, cost: 350, damage: 50, fireRate: 1.5f, range: 18f, explosion: 3f)
            };
            EditorUtility.SetDirty(cannon);
        }

        // 3. Mage Tower -> Archmage / Elementalist
        TowerData mage = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Mage_Tower.asset");
        if (mage != null)
        {
            mage.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Arcane Archmage", side: Side.Light, cost: 450, damage: 100, fireRate: 1.0f, range: 15f, explosion: 0f, effect: StatusEffectType.Slow, duration: 3f, power: 0.7f),
                CreateTower(towerPath, "Elemental Summoner", side: Side.Light, cost: 500, damage: 80, fireRate: 1.2f, range: 13f, explosion: 2f)
            };
            EditorUtility.SetDirty(mage);
        }

        // 4. Ballista Tower -> Bolt / Great
        TowerData ballista = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Ballista_Tower.asset");
        if (ballista != null)
        {
            ballista.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Bolt Thrower", side: Side.Light, cost: 400, damage: 200, fireRate: 0.5f, range: 22f, explosion: 0f),
                CreateTower(towerPath, "Great Ballista", side: Side.Light, cost: 550, damage: 350, fireRate: 0.2f, range: 25f, explosion: 0f)
            };
            EditorUtility.SetDirty(ballista);
        }

        // 5. Solar Prism -> Beamer / Prismatic
        TowerData prism = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Solar_Prism.asset");
        if (prism != null)
        {
            prism.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Luminous Beamer", side: Side.Light, cost: 350, damage: 10, fireRate: 10.0f, range: 14f, explosion: 0f),
                CreateTower(towerPath, "Prismatic Ray", side: Side.Light, cost: 400, damage: 50, fireRate: 2.0f, range: 16f, explosion: 1f)
            };
            EditorUtility.SetDirty(prism);
        }

        // --- DARK SIDE ---

        // 6. Dark Sentry -> Void Guard / Soul Emitter
        TowerData sentry = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Dark_Sentry.asset");
        if (sentry != null)
        {
            sentry.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Void Guard", side: Side.Dark, cost: 350, damage: 80, fireRate: 0.8f, range: 14f, explosion: 0f),
                CreateTower(towerPath, "Soul Emitter", side: Side.Dark, cost: 250, damage: 20, fireRate: 4f, range: 10f, explosion: 2f)
            };
            EditorUtility.SetDirty(sentry);
        }

        // 7. Bone Catapult -> Fossil / Cursed
        TowerData catapult = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Bone_Catapult.asset");
        if (catapult != null)
        {
            catapult.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Fossil Hurler", side: Side.Dark, cost: 380, damage: 100, fireRate: 0.4f, range: 20f, explosion: 5f),
                CreateTower(towerPath, "Cursed Lobber", side: Side.Dark, cost: 420, damage: 70, fireRate: 0.6f, range: 18f, explosion: 4f, effect: StatusEffectType.Poison, duration: 4f, power: 1f)
            };
            EditorUtility.SetDirty(catapult);
        }

        // 8. Poison Spitter -> Venom / Acid
        TowerData spitter = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Poison_Spitter.asset");
        if (spitter != null)
        {
            spitter.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Venomous Cloud", side: Side.Dark, cost: 300, damage: 10, fireRate: 2.0f, range: 12f, explosion: 6f, effect: StatusEffectType.Poison, duration: 6f, power: 3f),
                CreateTower(towerPath, "Acid Sprayer", side: Side.Dark, cost: 320, damage: 40, fireRate: 1.5f, range: 10f, explosion: 0f, effect: StatusEffectType.Slow, duration: 2f, power: 0.5f)
            };
            EditorUtility.SetDirty(spitter);
        }

        // 9. Void Obelisk -> Entropy / Singularity
        TowerData obelisk = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Void_Obelisk.asset");
        if (obelisk != null)
        {
            obelisk.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Entropy Core", side: Side.Dark, cost: 500, damage: 250, fireRate: 0.2f, range: 18f, explosion: 0f),
                CreateTower(towerPath, "Singularity Pillar", side: Side.Dark, cost: 450, damage: 150, fireRate: 0.5f, range: 16f, explosion: 8f)
            };
            EditorUtility.SetDirty(obelisk);
        }

        // 10. Soul Harvester -> Life Drinker / Spectral Reaper
        TowerData harvester = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Soul_Harvester.asset");
        if (harvester != null)
        {
            harvester.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Life Drinker", side: Side.Dark, cost: 300, damage: 30, fireRate: 2.5f, range: 12f, explosion: 0f),
                CreateTower(towerPath, "Spectral Reaper", side: Side.Dark, cost: 400, damage: 100, fireRate: 0.8f, range: 14f, explosion: 3f)
            };
            EditorUtility.SetDirty(harvester);
        }

        // --- BARRACKS & GRAVEYARD SPECIALIZATIONS ---

        // 11. Barracks -> Paladin Barracks / Knight's Guild
        TowerData barracks = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Barracks.asset");
        if (barracks != null)
        {
            barracks.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Paladin Barracks", side: Side.Light, cost: 500, damage: 0, fireRate: 0f, range: 60f, explosion: 0f),
                CreateTower(towerPath, "Knights Guild", side: Side.Light, cost: 550, damage: 0, fireRate: 0f, range: 50f, explosion: 0f)
            };
            EditorUtility.SetDirty(barracks);
        }

        // 12. Graveyard -> Lich Crypt / Zombie Nest
        TowerData graveyard = AssetDatabase.LoadAssetAtPath<TowerData>($"{towerPath}/Graveyard.asset");
        if (graveyard != null)
        {
            graveyard.specializations = new System.Collections.Generic.List<TowerData>
            {
                CreateTower(towerPath, "Lich Crypt", side: Side.Dark, cost: 500, damage: 0, fireRate: 0f, range: 55f, explosion: 0f),
                CreateTower(towerPath, "Zombie Nest", side: Side.Dark, cost: 480, damage: 0, fireRate: 0f, range: 45f, explosion: 0f)
            };
            EditorUtility.SetDirty(graveyard);
        }

        AssetDatabase.SaveAssets();
    }

    public static void GenerateEliteTowerPrefabs(string towerPath)
    {
        string prefabPath = "Assets/Prefabs/Gameplay/Towers";
        string[] towerGuids = AssetDatabase.FindAssets("t:TowerData", new[] { towerPath });
        
        int count = 0;
        foreach (string guid in towerGuids)
        {
            TowerData eliteData = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
            if (eliteData == null || eliteData.isBaseTower) continue;

            // Eğer zaten prefabı varsa geç
            if (eliteData.prefab != null) continue;

            // Bu elit kulenin ana (base) kulesini bul
            TowerData baseData = FindBaseTowerOfSpecialization(eliteData);
            if (baseData == null || baseData.prefab == null) continue;

            // Prefab oluştur (Base prefab'ı kopyala)
            string newPfbPath = $"{prefabPath}/{eliteData.towerName.Replace(" ", "_")}.prefab";
            if (!System.IO.File.Exists(newPfbPath))
            {
                if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(baseData.prefab), newPfbPath))
                {
                    GameObject root = PrefabUtility.LoadPrefabContents(newPfbPath);
                    
                    // Görsel Fark: Renderer'lara renk ver
                    Color tint = (eliteData.side == Side.Light) ? new Color(1f, 0.9f, 0.5f) : new Color(0.6f, 0.4f, 1f);
                    foreach (var rend in root.GetComponentsInChildren<Renderer>())
                    {
                        foreach (var mat in rend.materials)
                        {
                            if (mat.HasProperty("_Color")) mat.color *= tint;
                            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * tint);
                        }
                    }

                    // Tower bileşenini güncelle
                    Tower tower = root.GetComponent<Tower>();
                    if (tower != null)
                    {
                        var so = new SerializedObject(tower);
                        so.FindProperty("towerData").objectReferenceValue = eliteData;
                        so.ApplyModifiedProperties();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, newPfbPath);
                    PrefabUtility.UnloadPrefabContents(root);
                    
                    eliteData.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPfbPath);
                    EditorUtility.SetDirty(eliteData);
                    count++;
                }
            }
        }

        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✔ Generated {count} Elite Tower Prefabs automatically!");
        }
    }

    private static void LinkUnitProjectiles()
    {
        string projPath = "Assets/Prefabs/Gameplay/Projectiles";
        string[] unitDataGuids = AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/Data/Units" });

        foreach (var guid in unitDataGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            
            if (data != null && data.attackRange > 2f)
            {
                string n = data.unitName.ToLower();
                string projName = "Arrow";

                if (n.Contains("mage") || n.Contains("necromancer")) projName = "Fireball";
                else if (n.Contains("lich") || n.Contains("bone") || n.Contains("skeleton")) projName = "Frostbolt";
                else if (n.Contains("priestess") || n.Contains("cleric") || n.Contains("arcane")) projName = "LightOrb";
                else if (n.Contains("dark orb") || n.Contains("blood")) projName = "DarkOrb";
                else if (n.Contains("cannon") || n.Contains("catapult")) projName = "Cannonball";
                else if (n.Contains("crossbow") || n.Contains("sniper")) projName = "CrossbowBolt";
                else if (n.Contains("skeleton archer")) projName = "Arrow"; // Or Frostbolt, but Arrow is fine for skeletons

                GameObject foundProj = AssetDatabase.LoadAssetAtPath<GameObject>($"{projPath}/{projName}.prefab");
                if (foundProj != null)
                {
                    data.projectilePrefab = foundProj;
                    EditorUtility.SetDirty(data);
                }
            }
        }
        AssetDatabase.SaveAssets();
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path);
        }
    }

    private static TowerData CreateTower(string path, string name, Side side, int cost, float damage, float fireRate, float range, float explosion, bool isBase = false, StatusEffectType effect = StatusEffectType.None, float duration = 0, float power = 0)
    {
        string safeName = name.Replace(" ", "_");
        string assetPath = $"{path}/{safeName}.asset";
        
        TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TowerData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.towerName = name;
        data.side = side;
        data.isBaseTower = isBase;
        data.cost = cost;
        data.upgradeCost = cost;
        data.damage = damage;
        data.fireRate = fireRate;
        data.range = range;
        data.explosionRadius = explosion;
        data.effectType = effect;
        data.effectDuration = duration;
        data.effectPower = power;
        
        // Varsayılan Can (Maliyetle ölçeklenen dayanıklılık)
        if (data.health <= 0) data.health = (cost * 2f) + 200f;
        data.targetLayer = (side == Side.Light) ? (1 << 7) : (1 << 6);

        // Otomatik Varlık Bulma
        data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Towers/{safeName}.prefab");
        data.icon = FindIcon($"{safeName}_Icon");

        // TERS BAĞLAMA: Prefab -> Data
        if (data.prefab != null)
        {
            Tower towerComp = data.prefab.GetComponent<Tower>();
            if (towerComp != null)
            {
                SerializedObject so = new SerializedObject(towerComp);
                so.FindProperty("towerData").objectReferenceValue = data;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(data.prefab);
            }
        }

        EditorUtility.SetDirty(data);
        return data;
    }

    private static UnitData CreateUnit(string path, string name, Side side, int cost, int reward, float health, float speed, float damage, float range, float rate)
    {
        string safeName = name.Replace(" ", "_");
        string assetPath = $"{path}/{safeName}.asset";

        UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<UnitData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.unitName = name;
        data.side = side;
        data.spawnCost = cost;
        data.killReward = reward;
        data.maxHealth = health;
        data.moveSpeed = speed;
        data.attackDamage = damage;
        data.attackRange = range;
        data.attackRate = rate;

        // Otomatik Varlık Bulma
        data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Units/{safeName}.prefab");
        data.icon = FindIcon($"{safeName}_Icon");

        // TERS BAĞLAMA: Prefab -> Data
        if (data.prefab != null)
        {
            Unit unitComp = data.prefab.GetComponent<Unit>();
            if (unitComp != null)
            {
                SerializedObject so = new SerializedObject(unitComp);
                so.FindProperty("unitData").objectReferenceValue = data;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(data.prefab);
            }
        }
        
        EditorUtility.SetDirty(data);
        return data;
    }

    public static void ConfigureTowerPrefabs(string towerPath = "Assets/Data/Towers")
    {
        string[] towerGuids = AssetDatabase.FindAssets("t:TowerData", new[] { towerPath });
        foreach (string guid in towerGuids)
        {
            TowerData td = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
            if (td != null && td.prefab != null)
            {
                ConfigureSingleTowerPrefab(td.prefab, td);
            }
        }
        AssetDatabase.Refresh();
    }

    private static void ConfigureSingleTowerPrefab(GameObject prefab, TowerData data)
    {
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(path)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);

        // 1. Tower Script (Barracks check - Uzmanlaşmış kışlaları da kapsar)
        bool isBarracks = data.towerName.Contains("Barracks") || data.towerName.Contains("Graveyard") || 
                         data.towerName.Contains("Crypt") || data.towerName.Contains("Nest") || 
                         data.towerName.Contains("Guild") || data.towerName.Contains("Paladin");
        
        Tower tower = root.GetComponent<Tower>();
        if (isBarracks)
        {
            if (tower != null && !(tower is BarracksTower)) { DestroyImmediate(tower, true); tower = null; }
            if (tower == null) tower = root.AddComponent<BarracksTower>();
            
            // Asker Verisi Ataması
            BarracksTower bt = (BarracksTower)tower;
            string soldierName = (data.side == Side.Light) ? "Light_Swordsman" : "Skeleton_Warrior";
            bt.soldierData = AssetDatabase.LoadAssetAtPath<UnitData>($"Assets/Data/Units/{soldierName}.asset");
            bt.soldierCount = 3;
            bt.respawnDelay = 12f;

            // Rally Indicator Ataması (Yeni oluşturduğun prefab)
            bt.rallyIndicatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/RallyIndicator.prefab");
        }
        else
        {
            if (tower == null) tower = root.AddComponent<Tower>();
        }
        
        var so = new SerializedObject(tower);
        so.FindProperty("towerData").objectReferenceValue = data;
        so.ApplyModifiedProperties();

        // Kışla kuleleri mermi atmaz: Weapon görselini kaldır
        if (isBarracks)
        {
            Transform visuals = root.transform.Find("Visuals");
            if (visuals != null)
            {
                Transform weapon = visuals.Find("Weapon");
                if (weapon != null)
                {
                    Object.DestroyImmediate(weapon.gameObject);
                    Debug.Log($"✔ Removed Weapon visual from barracks tower: {prefab.name}");
                }

                // Icon ataması: Visuals altındaki ilk nesnenin adıyla ara
                // (Kullanıcı icon dosyasını bu isimle oluşturdu)
                if (data.icon == null && visuals.childCount > 0)
                {
                    string visualChildName = visuals.GetChild(0).name;
                    
                    // Önce "{visualChildName}_Icon" dene
                    Sprite foundIcon = FindIcon($"{visualChildName}_Icon");
                    // Sonra "{visualChildName}" dene
                    if (foundIcon == null)
                        foundIcon = FindIcon(visualChildName);
                    // Son çare: "{towerName}_Icon" (standart format)
                    if (foundIcon == null)
                    {
                        string safeName = data.towerName.Replace(" ", "_");
                        foundIcon = FindIcon($"{safeName}_Icon");
                    }

                    if (foundIcon != null)
                    {
                        data.icon = foundIcon;
                        EditorUtility.SetDirty(data);
                        Debug.Log($"✔ Icon linked for barracks tower '{prefab.name}': {foundIcon.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Barracks] Icon not found for '{prefab.name}'. Looked for '{visualChildName}_Icon.png' in Assets/Data/Icons/");
                    }
                }
            }
            // Kışla kuleleri mermi atmaz ve menzili 0'dır
            data.projectilePrefab = null;
            data.range = 0;
            EditorUtility.SetDirty(data);
        }

        // 2. Collider & Layer Setup
        int towerLayer = (data.side == Side.Light) ? 6 : 7;
        SetLayerRecursive(root, towerLayer);

        if (root.GetComponent<Collider>() == null)
        {
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.5f, 0);
            col.size = new Vector3(2, 3, 2);
        }

        // 3. Embedded UI
        Transform uiTransform = root.transform.Find("TowerUpgradeUI");
        if (uiTransform != null)
        {
            uiTransform.localPosition = new Vector3(0, 6.5f, 0); // Kule tepesi (+2.5f)
        }
        else
        {
            GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/TowerUpgradeUI.prefab");
            if (uiPrefab != null)
            {
                GameObject uiInstance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab, root.transform);
                uiInstance.name = "TowerUpgradeUI";
                uiInstance.transform.localPosition = new Vector3(0, 6.5f, 0); // Kule tepesi (+2.5f)
            }
        }

        // 5. Health Bar UI (Düşmanlardaki sistemin aynısı)
        Transform hbTransform = root.transform.Find("HealthBarCanvas");
        Sprite emptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prefabs/UI/Empty.png");

        if (hbTransform != null)
        {
            hbTransform.localPosition = new Vector3(0, 6.0f, 0); // Can barı (+2.5f)
            
            // Update existing colors/sprites
            Transform bg = hbTransform.Find("Background");
            if (bg != null)
            {
                Image bgImg = bg.GetComponent<Image>();
                bgImg.sprite = emptySprite;
                bgImg.color = Color.red;
                
                Transform fill = bg.Find("Fill");
                if (fill != null)
                {
                    Image fillImg = fill.GetComponent<Image>();
                    fillImg.sprite = emptySprite;
                    fillImg.color = Color.green;
                }
            }
        }
        else
        {
            GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(TowerDefence.UI.HealthBarUI));
            canvasGo.transform.SetParent(root.transform);
            canvasGo.transform.localPosition = new Vector3(0, 6.0f, 0); 
            canvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.2f);
            
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Background & Fill setup (Minimalist)
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            bg.transform.SetParent(canvasGo.transform);
            bg.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            bg.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.2f);
            Image bgImg = bg.GetComponent<UnityEngine.UI.Image>();
            bgImg.sprite = emptySprite;
            bgImg.color = Color.red;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            fill.transform.SetParent(bg.transform);
            fill.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            fill.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.2f);
            UnityEngine.UI.Image fillImg = fill.GetComponent<UnityEngine.UI.Image>();
            fillImg.sprite = emptySprite;
            fillImg.color = Color.green;
            fillImg.type = UnityEngine.UI.Image.Type.Filled;
            fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

            // Script referanslarını bağla
            var hbScript = canvasGo.GetComponent<TowerDefence.UI.HealthBarUI>();
            var soHb = new SerializedObject(hbScript);
            soHb.FindProperty("fillImage").objectReferenceValue = fillImg;
            soHb.FindProperty("container").objectReferenceValue = canvasGo;
            soHb.ApplyModifiedProperties();
        }

        // 4. Aura Tower Bileşeni
        if (data.isAuraTower)
        {
            if (root.GetComponent<TowerDefence.Combat.AuraTower>() == null)
                root.AddComponent<TowerDefence.Combat.AuraTower>();
        }
        else
        {
            var existingAura = root.GetComponent<TowerDefence.Combat.AuraTower>();
            if (existingAura != null) Object.DestroyImmediate(existingAura);
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"✔ Configured tower prefab: {prefab.name}");
    }

    private static void LinkTowerCounterparts(string towerPath)
    {
        // Temel Kule Eşleştirmeleri
        LinkPair(towerPath, "Archer_Tower", "Dark_Sentry");
        LinkPair(towerPath, "Cannon_Tower", "Bone_Catapult");
        LinkPair(towerPath, "Mage_Tower", "Poison_Spitter");
        LinkPair(towerPath, "Ballista_Tower", "Void_Obelisk");
        LinkPair(towerPath, "Solar_Prism", "Soul_Harvester");
        LinkPair(towerPath, "Barracks", "Graveyard"); // Kışla <-> Mezarlık

        // Uzmanlaşmış Kule Eşleştirmeleri (Elite) - İsimler Senkronize Edildi
        LinkPair(towerPath, "Sniper_Tower", "Void_Guard");
        LinkPair(towerPath, "Ranger_Outpost", "Soul_Emitter");

        AssetDatabase.SaveAssets();
        Debug.Log("✔ Tower Enemy Counterparts linked successfully.");
    }

    private static void LinkPair(string path, string lightName, string darkName)
    {
        TowerData light = AssetDatabase.LoadAssetAtPath<TowerData>($"{path}/{lightName}.asset");
        TowerData dark = AssetDatabase.LoadAssetAtPath<TowerData>($"{path}/{darkName}.asset");

        if (light != null && dark != null)
        {
            light.enemyCounterpart = dark;
            dark.enemyCounterpart = light;
            EditorUtility.SetDirty(light);
            EditorUtility.SetDirty(dark);
        }
    }

    public static void FixMissingTowerReferences()
    {
        string towerPath = "Assets/Data/Towers";
        string[] guids = AssetDatabase.FindAssets("t:TowerData", new[] { towerPath });
        
        int fixes = 0;
        foreach (var guid in guids)
        {
            TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            bool changed = false;
            string safeName = data.towerName.Replace(" ", "_");

            // 1. Prefab Ataması
            if (data.prefab == null)
            {
                // Tam ismiyle ara
                GameObject found = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Towers/{safeName}.prefab");
                if (found != null) { data.prefab = found; changed = true; }
                else 
                {
                    // Eğer bir specialization ise, ana kuleyi bul ve onun prefabını kullan (Fallback)
                    TowerData baseTower = FindBaseTowerOfSpecialization(data);
                    if (baseTower != null && baseTower.prefab != null)
                    {
                        data.prefab = baseTower.prefab;
                        changed = true;
                        Debug.Log($"[REPAIR] {data.towerName} used base prefab from {baseTower.towerName}");
                    }
                }
            }

            // 2. Icon Ataması
            if (data.icon == null)
            {
                Sprite found = FindIcon($"{safeName}_Icon");
                if (found != null) { data.icon = found; changed = true; }
                else
                {
                    TowerData baseTower = FindBaseTowerOfSpecialization(data);
                    if (baseTower != null && baseTower.icon != null)
                    {
                        data.icon = baseTower.icon;
                        changed = true;
                        Debug.Log($"[REPAIR] {data.towerName} used base icon from {baseTower.towerName}");
                    }
                }
            }

            // 3. Projectile & Range Ataması (Kışla kontrolü eklendi)
            string n = data.towerName.ToLower();
            bool isSpawner = n.Contains("barracks") || n.Contains("graveyard") || n.Contains("crypt") || 
                            n.Contains("nest") || n.Contains("guild") || n.Contains("paladin");

            if (isSpawner)
            {
                data.projectilePrefab = null;
                // data.range = 0; // BU SATIRI KALDIRDIK - Artık menzil veriden geliyor
                changed = true;
            }
            else if (data.projectilePrefab == null)
            {
                string projName = "Arrow";
                if (n.Contains("mage") || n.Contains("prism")) projName = "Fireball";
                else if (n.Contains("soul") || n.Contains("void")) projName = "DarkOrb";
                else if (n.Contains("bone")) projName = "Frostbolt";
                else if (n.Contains("cannon") || n.Contains("catapult")) projName = "Cannonball";
                
                GameObject foundProj = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Projectiles/{projName}.prefab");
                if (foundProj == null)
                {
                    TowerData baseTower = FindBaseTowerOfSpecialization(data);
                    if (baseTower != null && baseTower.projectilePrefab != null)
                    {
                        foundProj = baseTower.projectilePrefab;
                    }
                }

                if (foundProj == null) foundProj = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Projectiles/Arrow.prefab");
                if (foundProj != null) { data.projectilePrefab = foundProj; changed = true; }
            }


            if (changed)
            {
                EditorUtility.SetDirty(data);
                fixes++;
            }
        }
        
        // Tüm işlemler bitince kuleleri birbirine bağla (Mirroring)
        LinkTowerCounterparts("Assets/Data/Towers");

        if (fixes > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✔ {fixes} TowerData asset repaired successfully!");
        }
        else
        {
            Debug.Log("✔ No missing references found in Towers.");
        }
    }

    private static TowerData FindBaseTowerOfSpecialization(TowerData spec)
    {
        string[] guids = AssetDatabase.FindAssets("t:TowerData", new[] { "Assets/Data/Towers" });
        foreach (var guid in guids)
        {
            TowerData baseT = AssetDatabase.LoadAssetAtPath<TowerData>(AssetDatabase.GUIDToAssetPath(guid));
            if (baseT != null && baseT.specializations != null && baseT.specializations.Contains(spec))
            {
                return baseT;
            }
        }
        return null;
    }

    public static void FixMissingUnitReferences()
    {
        string unitPath = "Assets/Data/Units";
        string[] guids = AssetDatabase.FindAssets("t:UnitData", new[] { unitPath });

        int fixes = 0;
        foreach (var guid in guids)
        {
            UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            bool changed = false;
            string safeName = data.unitName.Replace(" ", "_");

            // 1. Data -> Prefab Ataması
            if (data.prefab == null)
            {
                GameObject found = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Units/{safeName}.prefab");
                if (found != null) { data.prefab = found; changed = true; }
            }

            // 2. Data -> Icon Ataması
            if (data.icon == null)
            {
                Sprite found = FindIcon($"{safeName}_Icon");
                if (found != null) { data.icon = found; changed = true; }
            }

            // 3. TERS BAĞLAMA: Prefab -> Data (Kritik Kullanıcı Talebi)
            if (data.prefab != null)
            {
                // Prefab'ı yükle, bileşeni bul ve veriyi set et
                string pPath = AssetDatabase.GetAssetPath(data.prefab);
                GameObject root = PrefabUtility.LoadPrefabContents(pPath);
                Unit unitComp = root.GetComponent<Unit>();
                
                if (unitComp != null)
                {
                    var so = new SerializedObject(unitComp);
                    var prop = so.FindProperty("unitData");
                    if (prop != null && prop.objectReferenceValue != data)
                    {
                        prop.objectReferenceValue = data;
                        so.ApplyModifiedProperties();
                        PrefabUtility.SaveAsPrefabAsset(root, pPath);
                        Debug.Log($"✔ Linked UnitData to Prefab: {data.unitName}");
                        fixes++;
                    }
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
                fixes++;
            }
        }

        if (fixes > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✔ {fixes} Unit references repaired successfully!");
        }
        else
        {
            Debug.Log("✔ No missing references found in Units.");
        }
    }

    public static void FixHeroReferences()
    {
        string heroDataPath = "Assets/Data/Heroes";
        string[] guids = AssetDatabase.FindAssets("t:UnitData", new[] { heroDataPath });

        int fixes = 0;
        foreach (var guid in guids)
        {
            UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;

            bool changed = false;
            string safeName = data.unitName.Replace(" ", "_");

            // 1. Heroes don't have enemy counterparts - ensure it's null
            if (data.enemyCounterpart != null)
            {
                data.enemyCounterpart = null;
                changed = true;
            }

            // 2. Data -> Icon Ataması (refresh icon)
            Sprite foundIcon = FindIcon($"{safeName}_Icon");
            if (foundIcon != null && data.icon != foundIcon)
            {
                data.icon = foundIcon;
                changed = true;
            }
            else if (data.icon == null)
            {
                // Fallback to source icons if hero-specific ones don't exist
                string fallbackIconName = (data.side == Side.Light) ? "Light_Swordsman_Icon" : "Shadow_Stalker_Icon";
                Sprite fallbackIcon = FindIcon(fallbackIconName);
                if (fallbackIcon != null)
                {
                    data.icon = fallbackIcon;
                    changed = true;
                }
            }

            // 3. Data -> Prefab Ataması
            GameObject correctPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Heroes/{safeName}.prefab");
            if (correctPrefab != null && data.prefab != correctPrefab)
            {
                data.prefab = correctPrefab;
                changed = true;
            }

            // 4. TERS BAĞLAMA: Prefab -> Data
            if (data.prefab != null)
            {
                string pPath = AssetDatabase.GetAssetPath(data.prefab);
                GameObject root = PrefabUtility.LoadPrefabContents(pPath);
                HeroUnit heroComp = root.GetComponent<HeroUnit>();

                if (heroComp != null)
                {
                    var so = new SerializedObject(heroComp);
                    var prop = so.FindProperty("unitData");
                    if (prop != null && prop.objectReferenceValue != data)
                    {
                        prop.objectReferenceValue = data;
                        so.ApplyModifiedProperties();
                        PrefabUtility.SaveAsPrefabAsset(root, pPath);
                        Debug.Log($"✔ Linked Hero UnitData to Prefab: {data.unitName}");
                        fixes++;
                    }
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
                fixes++;
            }
        }

        // Also fix HeroData icons
        string[] heroDataGuids = AssetDatabase.FindAssets("t:HeroData", new[] { heroDataPath });
        foreach (var guid in heroDataGuids)
        {
            HeroData heroData = AssetDatabase.LoadAssetAtPath<HeroData>(AssetDatabase.GUIDToAssetPath(guid));
            if (heroData == null) continue;

            // Update HeroData icon to match UnitData icon
            if (heroData.unitData != null && heroData.icon != heroData.unitData.icon)
            {
                heroData.icon = heroData.unitData.icon;
                EditorUtility.SetDirty(heroData);
                fixes++;
            }
        }

        if (fixes > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✔ {fixes} Hero references repaired successfully!");
        }
        else
        {
            Debug.Log("✔ No missing references found in Heroes.");
        }
    }

    public static void ConfigureUnitPrefabs(string unitPath = "Assets/Data/Units")
    {
        ConfigureUnitPrefabsInFolder(unitPath);
        ConfigureUnitPrefabsInFolder("Assets/Data/Heroes");
        AssetDatabase.Refresh();
    }

    private static void ConfigureUnitPrefabsInFolder(string unitPath)
    {
        string[] unitGuids = AssetDatabase.FindAssets("t:UnitData", new[] { unitPath });
        foreach (string guid in unitGuids)
        {
            UnitData ud = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (ud != null && ud.prefab != null)
            {
                ConfigureSingleUnitPrefab(ud.prefab, ud);
            }
        }
    }

    private static void ConfigureSingleUnitPrefab(GameObject prefab, UnitData data)
    {
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(path)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        
        // 0. Layer Setup (Kritik: Kuleler bu katmanlara (6-7) göre hedefler)
        int targetLayer = (data.side == Side.Light) ? 6 : 7;
        SetLayerRecursive(root, targetLayer);

        // 1. Unit Script (Soldier check)
        bool isSoldier = data.unitName.Contains("Swordsman") || data.unitName.Contains("Skeleton") || data.unitName.Contains("Knight");
        Unit unit = root.GetComponent<Unit>();
        
        if (isSoldier)
        {
            if (unit != null && !(unit is Soldier)) { DestroyImmediate(unit, true); unit = null; }
            if (unit == null) unit = root.AddComponent<Soldier>();
        }
        else
        {
            if (unit == null) unit = root.AddComponent<Unit>();
        }

        // 2. Health Bar UI Setup
        Transform hbTransform = root.transform.Find("HealthBarCanvas");
        HealthBarUI hbScript = null;
        float targetY = 5f; // HealthBarCanvas Y position sabit 5
        Sprite emptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prefabs/UI/Empty.png");

        if (hbTransform == null)
        {
            GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(HealthBarUI));
            canvasGo.transform.SetParent(root.transform);
            canvasGo.transform.localPosition = new Vector3(0, targetY, 0); 
            canvasGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.2f);
            
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            hbScript = canvasGo.GetComponent<HealthBarUI>();

            // Background
            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform);
            bgGo.transform.localPosition = Vector3.zero;
            bgGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.2f);
            Image bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = emptySprite;
            bgImg.color = Color.red;

            // Fill
            GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(bgGo.transform);
            fillGo.transform.localPosition = Vector3.zero;
            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(1.5f, 0.2f);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = emptySprite;
            fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;

            // HealthBarUI Link
            var hbSo = new SerializedObject(hbScript);
            hbSo.FindProperty("fillImage").objectReferenceValue = fillImg;
            hbSo.FindProperty("container").objectReferenceValue = canvasGo;
            hbSo.ApplyModifiedProperties();
        }
        else
        {
            hbTransform.localPosition = new Vector3(0, targetY, 0);
            hbScript = hbTransform.GetComponent<HealthBarUI>();
            
            // Update existing colors/sprites
            Transform bg = hbTransform.Find("Background");
            if (bg != null)
            {
                Image bgImg = bg.GetComponent<Image>();
                bgImg.sprite = emptySprite;
                bgImg.color = Color.red;
                
                Transform fill = bg.Find("Fill");
                if (fill != null)
                {
                    Image fillImg = fill.GetComponent<Image>();
                    fillImg.sprite = emptySprite;
                    fillImg.color = Color.green;
                }
            }
        }

        // 3. Unit -> HealthBar Link
        var unitSo = new SerializedObject(unit);
        unitSo.FindProperty("healthBar").objectReferenceValue = hbScript;

        if (data.projectilePrefab != null)
        {
            EditorPrefabBuilder.EnsureRangedUnitFirePoints(root, prefab.name);
            Transform fp = EditorPrefabBuilder.GetFirstFirePointTransform(root);
            if (fp != null)
                unitSo.FindProperty("firePoint").objectReferenceValue = fp;
        }

        // Cleric_of_the_Dawn özel durum: fire point yoksa oluştur
        if (prefab.name == "Cleric_of_the_Dawn")
        {
            Transform fp = EditorPrefabBuilder.GetFirstFirePointTransform(root);
            if (fp == null)
            {
                Transform visuals = root.transform.Find("Visuals");
                Transform parent = visuals != null ? visuals : root.transform;
                GameObject firePointGO = new GameObject("FirePoint");
                firePointGO.transform.SetParent(parent, false);
                firePointGO.transform.localPosition = new Vector3(0, 0.5f, 0.3f);
                unitSo.FindProperty("firePoint").objectReferenceValue = firePointGO.transform;
            }
        }

        // Manuel fire point pozisyon düzeltmeleri
        ApplyFirePointOverride(root, prefab.name);

        ApplyBossScale(root, prefab.name);

        unitSo.ApplyModifiedProperties();

        FixVisualModelGroundOffset(root);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"✔ Configured unit prefab with HealthBar & Layer: {prefab.name}");
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
    }
    // --- MANUAL FIRE POINT OVERRIDES ---
    private static readonly System.Collections.Generic.Dictionary<string, Vector3> firePointOverrides = new()
    {
        { "Crossbowman",          new Vector3(-0.563f, 0.022f, 0.037f) },
        { "Dark_Elf_Sniper",      new Vector3(-0.527f, 0.034f, 0f) },
        { "Dwarven_Cannoneer",    new Vector3(0f, 0.441f, 0.136f) },
        { "Elven_Ranger",         new Vector3(0.016f, -0.11f, -0.008f) },
        { "Novice_Archer",        new Vector3(0.112f, -0.198f, -0.025f) },
        { "Phoenix_Summoner",     new Vector3(0.007f, 0.666f, -0.031f) },
        { "Skeleton_Archer",      new Vector3(-0.176f, -0.064f, -0.383f) },
        { "Sun_Priestess",        new Vector3(-0.039f, 0.684f, -0.023f) },
    };

    private static void ApplyFirePointOverride(GameObject root, string unitName)
    {
        if (!firePointOverrides.ContainsKey(unitName)) return;

        Transform fp = EditorPrefabBuilder.GetFirstFirePointTransform(root);
        if (fp != null)
        {
            fp.localPosition = firePointOverrides[unitName];
        }
    }

    private static readonly System.Collections.Generic.HashSet<string> bossUnits = new()
    {
        "Demon_King", "Bone_Dragon", "Abyssal_Behemoth", "Death_Knight_Commander", "Blood_Mage", "Shadow_Leviathan",
        "Archangel", "Holy_Colossus", "Grand_Paladin", "Phoenix_Summoner", "Avatar_of_Light", "Dragon_of_the_Sun"
    };

    private static void ApplyBossScale(GameObject root, string unitName)
    {
        if (bossUnits.Contains(unitName))
        {
            Transform visuals = root.transform.Find("Visuals");
            if (visuals != null)
            {
                visuals.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
        }
    }

    // --- LEVEL & WAVE GENERATION [NEW] ---

    public static void GenerateLevels()
    {
        string levelPath = "Assets/Levels";
        EnsureDirectory(levelPath);

        for (int lvlIdx = 1; lvlIdx <= 50; lvlIdx++)
        {
            string lvlDir = $"{levelPath}/Level{lvlIdx}";
            EnsureDirectory(lvlDir);

            // Generate waves dynamically based on design structure (layout complexity)
            List<WaveData> waves = new List<WaveData>();
            int waveCount = GetWaveCountForLevel(lvlIdx);
            int spawnerCount = GetSpawnerCountForLevel(lvlIdx);

            for (int w = 1; w <= waveCount; w++)
            {
                var composition = GetWaveComposition(lvlIdx, w);
                waves.Add(CreateWave(lvlDir, $"Wave{w}", composition, spawnerCount));
            }

            // Generate paths, bases and custom slots
            List<LevelPath> paths = GetPathsForLevel(lvlIdx);
            List<Vector3> bases = GetBasesForLevel(lvlIdx);
            List<Vector3> customSlots = GetCustomSlotsForLevel(lvlIdx);
            LevelTheme theme = GetThemeForLevel(lvlIdx);
            string levelName = $"Level {lvlIdx}: " + GetLevelNameSuffix(lvlIdx);

            int startCurrencyLight = 200 + (lvlIdx * 10);
            int startCurrencyDark = 300 + (lvlIdx * 10);

            CreateLevel(lvlDir, $"Level{lvlIdx}", levelName, startCurrencyLight, startCurrencyDark, theme, waves, paths, bases, customSlots);
        }
        Debug.Log("✔ 50 LEVELS AND WAVES GENERATED SUCCESSFULLY!");
    }

    private static LevelTheme GetThemeForLevel(int lvlIdx)
    {
        if (lvlIdx <= 12) return LevelTheme.Forest;
        if (lvlIdx <= 25) return LevelTheme.Desert;
        if (lvlIdx <= 38) return LevelTheme.Snow;
        return LevelTheme.Underworld;
    }

    private static int GetWaveCountForLevel(int lvlIdx)
    {
        if (lvlIdx == 50) return 10; // Final Boss
        
        int baseWaves = 4;
        if (lvlIdx > 10) baseWaves = 5;
        if (lvlIdx > 25) baseWaves = 6;
        if (lvlIdx > 40) baseWaves = 8;

        int layout = lvlIdx % 10;
        if (layout == 0 || layout == 7 || layout == 8) baseWaves += 1;
        
        return baseWaves;
    }

    private static string GetLevelNameSuffix(int lvlIdx)
    {
        string[] forestNames = { "Green Glade", "Whispering Woods", "Mossy Path", "Ancient Grove", "Wildwood", "Deep Forest", "Shaded Valley", "Timberland", "Shadowy Glen", "Sunken Swamp", "Overgrown Ruins", "Forest Core" };
        string[] desertNames = { "Dusty Pass", "Dune Crossing", "Sandstorm Ridge", "Scorched Oasis", "Canyon Gorge", "Arid Sands", "Mirage Valley", "Redstone Mesa", "Blighted Badlands", "Sinking Dunes", "Dust Devil Ravine", "Desert Heart", "Solar Furnace" };
        string[] snowNames = { "Frosty Foothills", "Frozen Lake", "Snowy Labyrinth", "Glacier Gates", "Winter Peak", "Avalanche Pass", "Ice Crystal Cave", "Tundra Waste", "Blizzard Plateau", "Chillwind Valley", "Frostbite Gorge", "Everfrost Sanctuary", "Glistening Summit" };
        string[] underworldNames = { "Lava Fissure", "Sulfur Pits", "Basalt Plains", "Obsidian Keep", "Nether Gate", "Shadow Abyss", "Infernal Core", "Doom Crater", "Plague Swamplands", "Crypt of Shadows", "Ashen Wastes", "Master's Domain" };

        if (lvlIdx <= 12) return forestNames[(lvlIdx - 1) % forestNames.Length];
        if (lvlIdx <= 25) return desertNames[(lvlIdx - 13) % desertNames.Length];
        if (lvlIdx <= 38) return snowNames[(lvlIdx - 26) % snowNames.Length];
        return underworldNames[(lvlIdx - 39) % underworldNames.Length];
    }

    private static List<LevelPath> GetPathsForLevel(int lvlIdx)
    {
        List<LevelPath> paths = new List<LevelPath>();

        // --- Level 11-19: Özel Harita Tasarımları ---
        switch (lvlIdx)
        {
            case 11: // Z-Shape
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(20, 0, 30), new Vector3(20, 0, 10),
                    new Vector3(-15, 0, 10), new Vector3(-15, 0, -10), new Vector3(20, 0, -10),
                    new Vector3(20, 0, -30), new Vector3(30, 0, -30) } });
                return paths;
            case 12: // S-Curve (Dual Path)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(10, 0, 30), new Vector3(10, 0, 15), new Vector3(-20, 0, 5),
                    new Vector3(-20, 0, -10), new Vector3(0, 0, -25), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(10, 0, 30), new Vector3(10, 0, 15), new Vector3(25, 0, 5),
                    new Vector3(25, 0, -10), new Vector3(0, 0, -25), new Vector3(0, 0, -30) } });
                return paths;
            case 13: // Y-Merge
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-20, 0, 30), new Vector3(-12, 0, 15), new Vector3(0, 0, 0),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(20, 0, 30), new Vector3(12, 0, 15), new Vector3(0, 0, 0),
                    new Vector3(0, 0, -30) } });
                return paths;
            case 14: // Loop/Spiral
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, -10), new Vector3(-30, 0, 30), new Vector3(20, 0, 30),
                    new Vector3(20, 0, 10), new Vector3(-10, 0, 10), new Vector3(-10, 0, -10),
                    new Vector3(20, 0, -10), new Vector3(20, 0, -30), new Vector3(30, 0, -30) } });
                return paths;
            case 15: // Y with extra branches (2 spawners)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-15, 0, 30), new Vector3(-10, 0, 15), new Vector3(0, 0, 0),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(15, 0, 30), new Vector3(10, 0, 15), new Vector3(0, 0, 0),
                    new Vector3(0, 0, -30) } });
                // Sağa sapan dal
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-15, 0, 30), new Vector3(-10, 0, 15), new Vector3(0, 0, 0),
                    new Vector3(15, 0, -10), new Vector3(15, 0, -25), new Vector3(0, 0, -30) } });
                return paths;
            case 16: // 3-Way Merge (3 spawners zigzag)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-25, 0, 30), new Vector3(-20, 0, 10), new Vector3(-10, 0, -5),
                    new Vector3(0, 0, -20), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 10), new Vector3(0, 0, -5),
                    new Vector3(0, 0, -20), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> {
                    new Vector3(25, 0, 30), new Vector3(20, 0, 10), new Vector3(10, 0, -5),
                    new Vector3(0, 0, -20), new Vector3(0, 0, -30) } });
                return paths;
            case 17: // O-Shape (Split & Merge)
                // Sol kol
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(-20, 0, 15),
                    new Vector3(-20, 0, -15), new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                // Sağ kol
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(20, 0, 15),
                    new Vector3(20, 0, -15), new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                return paths;
            case 18: // Inverted Y (Base üstte, spawnerlar altta)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-25, 0, -30), new Vector3(-12, 0, -10), new Vector3(0, 0, 0),
                    new Vector3(0, 0, 30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(25, 0, -30), new Vector3(12, 0, -10), new Vector3(0, 0, 0),
                    new Vector3(0, 0, 30) } });
                return paths;
            case 19: // Diamond/X (3 spawner + karmaşık çapraz)
                // Üstten düz aşağı
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(-15, 0, 0),
                    new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(15, 0, 0),
                    new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                // Soldan
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(-30, 0, 0), new Vector3(-15, 0, 0), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30) } });
                // Sağdan
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> {
                    new Vector3(30, 0, 0), new Vector3(15, 0, 0), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30) } });
                return paths;
            
            // --- Level 20-27: Özel Harita Tasarımları ---
            case 20: // Corner Arrows
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-10, 0, 30), new Vector3(-30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(10, 0, 30), new Vector3(30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-30, 0, -30), new Vector3(-10, 0, -30), new Vector3(-30, 0, -10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 3, points = new List<Vector3> { new Vector3(30, 0, -30), new Vector3(10, 0, -30), new Vector3(30, 0, -10), new Vector3(0, 0, 0) } });
                return paths;
            case 21: // Right-Angle Snake
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(20, 0, 30), new Vector3(20, 0, -10), new Vector3(-10, 0, -10), new Vector3(-10, 0, -30) } });
                return paths;
            case 22: // S-Curve
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(-20, 0, 5), new Vector3(-20, 0, -5), new Vector3(20, 0, -15), new Vector3(20, 0, -25), new Vector3(0, 0, -30) } });
                return paths;
            case 23: // Y-Merge
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-15, 0, 0), new Vector3(0, 0, -10), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(15, 0, 0), new Vector3(0, 0, -10), new Vector3(0, 0, -30) } });
                return paths;
            case 24: // L-Shape
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, -20), new Vector3(25, 0, -20), new Vector3(25, 0, 25) } });
                return paths;
            case 25: // O-Shape Split
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(-25, 0, 5), new Vector3(-25, 0, -5), new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(25, 0, 5), new Vector3(25, 0, -5), new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                return paths;
            case 26: // Side Loop
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-20, 0, 10), new Vector3(10, 0, -10), new Vector3(25, 0, 0), new Vector3(25, 0, 20), new Vector3(0, 0, 20), new Vector3(-10, 0, 0), new Vector3(0, 0, -30) } });
                return paths;
            case 27: // Cross +
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, -30) } });
                return paths;
            
            // --- Level 28-35: Özel Harita Tasarımları ---
            case 28: // Trident
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-20, 0, 0), new Vector3(-10, 0, -10), new Vector3(0, 0, -20), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(15, 0, 10), new Vector3(20, 0, 0), new Vector3(10, 0, -10), new Vector3(0, 0, -20), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(20, 0, 15), new Vector3(20, 0, 0), new Vector3(10, 0, -10), new Vector3(0, 0, -20), new Vector3(0, 0, -30) } });
                return paths;
            case 29: // Diamond Mesh
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, -10), new Vector3(-15, 0, 5), new Vector3(0, 0, 20), new Vector3(15, 0, 5), new Vector3(30, 0, -10), new Vector3(30, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(-15, 0, 15), new Vector3(0, 0, 0), new Vector3(15, 0, -15), new Vector3(30, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(30, 0, -10), new Vector3(15, 0, -15), new Vector3(30, 0, -30) } });
                return paths;
            case 30: // Corner Arrows (Same as 20)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-10, 0, 30), new Vector3(-30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(10, 0, 30), new Vector3(30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-30, 0, -30), new Vector3(-10, 0, -30), new Vector3(-30, 0, -10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 3, points = new List<Vector3> { new Vector3(30, 0, -30), new Vector3(10, 0, -30), new Vector3(30, 0, -10), new Vector3(0, 0, 0) } });
                return paths;
            case 31: // Double Z-Snake
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(20, 0, 30), new Vector3(20, 0, 0), new Vector3(-20, 0, 0), new Vector3(-20, 0, -30), new Vector3(30, 0, -30) } });
                return paths;
            case 32: // S-Curve
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, 20), new Vector3(-20, 0, 10), new Vector3(-20, 0, -10), new Vector3(20, 0, -10), new Vector3(20, 0, -20), new Vector3(0, 0, -30) } });
                return paths;
            case 33: // Y-Merge (Same as 23)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-15, 0, 0), new Vector3(0, 0, -10), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(15, 0, 0), new Vector3(0, 0, -10), new Vector3(0, 0, -30) } });
                return paths;
            case 34: // Straight Line
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 0), new Vector3(30, 0, 0) } });
                return paths;
            case 35: // Figure-8 Knot
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(0, 0, 10), new Vector3(15, 0, 15), new Vector3(20, 0, 0), new Vector3(15, 0, -15), new Vector3(0, 0, -10), new Vector3(-15, 0, -15), new Vector3(-20, 0, 0), new Vector3(-15, 0, 15), new Vector3(0, 0, -10), new Vector3(30, 0, -30) } });
                return paths;
            
            // --- Level 36-44: Özel Harita Tasarımları ---
            case 36: // U-Shape / Reverse C
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(25, 0, 30), new Vector3(25, 0, -20),
                    new Vector3(-20, 0, -20), new Vector3(-20, 0, -30) } });
                return paths;
            case 37: // Eye / Lens Shape (split & merge)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(-20, 0, 10), new Vector3(-20, 0, -10),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(20, 0, 10), new Vector3(20, 0, -10),
                    new Vector3(0, 0, -30) } });
                return paths;
            case 38: // 3-Way Merge with vertical trunk
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-25, 0, 30), new Vector3(-15, 0, 10), new Vector3(0, 0, -5),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 10), new Vector3(0, 0, -5),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> {
                    new Vector3(25, 0, 30), new Vector3(15, 0, 10), new Vector3(0, 0, -5),
                    new Vector3(0, 0, -30) } });
                return paths;
            case 39: // X-Cross (Base üstte, spawnerlar altta)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-25, 0, -30), new Vector3(-10, 0, -10), new Vector3(10, 0, 10),
                    new Vector3(0, 0, 30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(25, 0, -30), new Vector3(10, 0, -10), new Vector3(-10, 0, 10),
                    new Vector3(0, 0, 30) } });
                return paths;
            case 40: // Corner Arrows (4 köşeden merkeze)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-10, 0, 30), new Vector3(-30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(10, 0, 30), new Vector3(30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-30, 0, -30), new Vector3(-10, 0, -30), new Vector3(-30, 0, -10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 3, points = new List<Vector3> { new Vector3(30, 0, -30), new Vector3(10, 0, -30), new Vector3(30, 0, -10), new Vector3(0, 0, 0) } });
                return paths;
            case 41: // Snake with right angles
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(20, 0, 30), new Vector3(20, 0, 10),
                    new Vector3(-15, 0, 10), new Vector3(-15, 0, -10), new Vector3(20, 0, -10),
                    new Vector3(20, 0, -30) } });
                return paths;
            case 42: // Multi S-Curve
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(10, 0, 30), new Vector3(10, 0, 15), new Vector3(-15, 0, 10),
                    new Vector3(-15, 0, -5), new Vector3(15, 0, -10), new Vector3(15, 0, -20),
                    new Vector3(0, 0, -30) } });
                return paths;
            case 43: // Tight S-Zigzag
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(-15, 0, 20), new Vector3(15, 0, 10),
                    new Vector3(-15, 0, 0), new Vector3(15, 0, -10), new Vector3(-15, 0, -20),
                    new Vector3(0, 0, -30) } });
                return paths;
            case 44: // Diamond Star (3 spawner)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 0), new Vector3(-15, 0, 0), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> {
                    new Vector3(30, 0, 0), new Vector3(15, 0, 0), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30) } });
                return paths;
            
            // --- Level 45-50: Özel Harita Tasarımları ---
            case 45: // Large Ring
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(-15, 0, 25), new Vector3(-25, 0, 0),
                    new Vector3(-15, 0, -25), new Vector3(0, 0, -25), new Vector3(15, 0, -25),
                    new Vector3(30, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(-15, 0, 25), new Vector3(0, 0, 25),
                    new Vector3(25, 0, 15), new Vector3(15, 0, -10), new Vector3(15, 0, -25),
                    new Vector3(30, 0, -30) } });
                return paths;
            case 46: // D-Shape Dual Merge
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(-10, 0, 30), new Vector3(10, 0, 20),
                    new Vector3(20, 0, 0), new Vector3(30, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(-30, 0, -30), new Vector3(-10, 0, -30), new Vector3(10, 0, -20),
                    new Vector3(20, 0, 0), new Vector3(30, 0, 0) } });
                return paths;
            case 47: // Looping Y-Merge
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(-20, 0, 15), new Vector3(-10, 0, 30),
                    new Vector3(-30, 0, 20), new Vector3(-10, 0, 0), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30), new Vector3(30, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(30, 0, 30), new Vector3(20, 0, 15), new Vector3(10, 0, 30),
                    new Vector3(30, 0, 20), new Vector3(10, 0, 0), new Vector3(0, 0, -15),
                    new Vector3(0, 0, -30), new Vector3(30, 0, -30) } });
                return paths;
            case 48: // Winding Snake
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(0, 0, 30), new Vector3(0, 0, 10),
                    new Vector3(-20, 0, 10), new Vector3(-20, 0, -10), new Vector3(20, 0, -10),
                    new Vector3(20, 0, -30), new Vector3(30, 0, -30) } });
                return paths;
            case 49: // Intricate Mesh / Star Knot
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> {
                    new Vector3(-30, 0, 30), new Vector3(-15, 0, 0), new Vector3(-20, 0, -20),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> {
                    new Vector3(30, 0, 30), new Vector3(-20, 0, 10), new Vector3(20, 0, -10),
                    new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> {
                    new Vector3(30, 0, 0), new Vector3(0, 0, -10), new Vector3(20, 0, -20),
                    new Vector3(0, 0, -30) } });
                return paths;
            case 50: // Ultimate Corner Arrows
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-10, 0, 30), new Vector3(-30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(10, 0, 30), new Vector3(30, 0, 10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-30, 0, -30), new Vector3(-10, 0, -30), new Vector3(-30, 0, -10), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 3, points = new List<Vector3> { new Vector3(30, 0, -30), new Vector3(10, 0, -30), new Vector3(30, 0, -10), new Vector3(0, 0, 0) } });
                return paths;
        }

        // --- Level 1-10 ve 20+ için mevcut layout sistemi ---
        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        switch (layout)
        {
            case 1:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(30, 0, -30) } });
                break;
            case 2:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-30, 0, -20), new Vector3(30, 0, -20) } });
                break;
            case 3:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(15, 0, 30), new Vector3(15, 0, 0), new Vector3(-15, 0, 0), new Vector3(-15, 0, -30), new Vector3(30, 0, -30) } });
                break;
            case 4:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-15, 0, -10), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(15, 0, -10), new Vector3(0, 0, -30) } });
                break;
            case 5:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-15, 0, 15), new Vector3(-15, 0, -15), new Vector3(15, 0, -15), new Vector3(15, 0, 15), new Vector3(0, 0, 15), new Vector3(0, 0, 0), new Vector3(30, 0, 0), new Vector3(30, 0, -30) } });
                break;
            case 6:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 20), new Vector3(0, 0, 20), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 20), new Vector3(0, 0, 20), new Vector3(0, 0, -30) } });
                break;
            case 7:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, 15), new Vector3(20, 0, 15), new Vector3(20, 0, 0), new Vector3(-20, 0, 0), new Vector3(-20, 0, -15), new Vector3(0, 0, -15), new Vector3(0, 0, -30) } });
                break;
            case 8:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 0), new Vector3(-10, 0, 0), new Vector3(15, 0, -15), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(30, 0, 0), new Vector3(10, 0, 0), new Vector3(-15, 0, -15), new Vector3(0, 0, -30) } });
                break;
            case 9:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-15, 0, 0), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(0, 0, 30), new Vector3(0, 0, -30) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(30, 0, 30), new Vector3(15, 0, 0), new Vector3(0, 0, -30) } });
                break;
            case 0:
            default:
                if (lvlIdx == 50)
                {
                    paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(-20, 0, 20), new Vector3(0, 0, 0) } });
                    paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(40, 0, 40), new Vector3(20, 0, 20), new Vector3(0, 0, 0) } });
                    paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-40, 0, -40), new Vector3(-20, 0, -20), new Vector3(0, 0, 0) } });
                    paths.Add(new LevelPath { spawnerIndex = 3, points = new List<Vector3> { new Vector3(40, 0, -40), new Vector3(20, 0, -20), new Vector3(0, 0, 0) } });
                }
                else
                {
                    paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-10, 0, 10), new Vector3(-10, 0, -20), new Vector3(20, 0, -20), new Vector3(30, 0, -30) } });
                    paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 30), new Vector3(-10, 0, 10), new Vector3(20, 0, 10), new Vector3(20, 0, -20), new Vector3(30, 0, -30) } });
                }
                break;
        }
        return paths;
    }

    private static List<Vector3> GetBasesForLevel(int lvlIdx)
    {
        List<Vector3> bases = new List<Vector3>();

        // --- Level 11-19 özel base noktaları ---
        switch (lvlIdx)
        {
            case 11: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 12: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 13: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 14: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 15: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 16: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 17: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 18: bases.Add(new Vector3(0, 0, 30)); return bases;  // Base ÜSTTE!
            case 19: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 20: bases.Add(new Vector3(0, 0, 0)); return bases;
            case 21: bases.Add(new Vector3(-10, 0, -30)); return bases;
            case 22: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 23: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 24: bases.Add(new Vector3(25, 0, 25)); return bases;
            case 25: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 26: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 27: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 28: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 29: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 30: bases.Add(new Vector3(0, 0, 0)); return bases;
            case 31: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 32: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 33: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 34: bases.Add(new Vector3(30, 0, 0)); return bases;
            case 35: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 36: bases.Add(new Vector3(-20, 0, -30)); return bases;
            case 37: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 38: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 39: bases.Add(new Vector3(0, 0, 30)); return bases;  // Base ÜSTTE!
            case 40: bases.Add(new Vector3(0, 0, 0)); return bases;
            case 41: bases.Add(new Vector3(20, 0, -30)); return bases;
            case 42: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 43: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 44: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 45: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 46: bases.Add(new Vector3(30, 0, 0)); return bases;
            case 47: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 48: bases.Add(new Vector3(30, 0, -30)); return bases;
            case 49: bases.Add(new Vector3(0, 0, -30)); return bases;
            case 50: bases.Add(new Vector3(0, 0, 0)); return bases;
        }

        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        switch (layout)
        {
            case 1: bases.Add(new Vector3(30, 0, -30)); break;
            case 2: bases.Add(new Vector3(30, 0, -20)); break;
            case 3: bases.Add(new Vector3(30, 0, -30)); break;
            case 4: bases.Add(new Vector3(0, 0, -30)); break;
            case 5: bases.Add(new Vector3(30, 0, -30)); break;
            case 6: bases.Add(new Vector3(0, 0, -30)); break;
            case 7: bases.Add(new Vector3(0, 0, -30)); break;
            case 8: bases.Add(new Vector3(0, 0, -30)); break;
            case 9: bases.Add(new Vector3(0, 0, -30)); break;
            case 0:
            default:
                if (lvlIdx == 50) bases.Add(new Vector3(0, 0, 0));
                else bases.Add(new Vector3(30, 0, -30));
                break;
        }
        return bases;
    }

    private static List<Vector3> GetCustomSlotsForLevel(int lvlIdx)
    {
        List<Vector3> slots = new List<Vector3>();

        // --- Level 11-19 özel tower slot konumları ---
        switch (lvlIdx)
        {
            case 11: // Z-Shape: yol boyunca dağılmış 7 slot
                slots.Add(new Vector3(-5, 0, 20));   // Z'nin üst kenarı
                slots.Add(new Vector3(10, 0, 20));   // Z'nin üst kenarı
                slots.Add(new Vector3(30, 0, 15));   // Sağ üst köşe
                slots.Add(new Vector3(5, 0, 5));     // Orta çapraz
                slots.Add(new Vector3(-25, 0, 0));   // Sol orta
                slots.Add(new Vector3(-5, 0, -5));   // Orta
                slots.Add(new Vector3(10, 0, -20));  // Alt sağ
                slots.Add(new Vector3(-25, 0, -20)); // Alt sol
                slots.Add(new Vector3(5, 0, -25));   // Sağ alt köşe
                return slots;
            case 12: // S-Curve: sol ve sağ tarafa 7 slot
                slots.Add(new Vector3(-15, 0, 20));  // Sol üst
                slots.Add(new Vector3(-10, 0, 10));  // Sol orta üst
                slots.Add(new Vector3(-25, 0, -5));  // Sol kıvrım
                slots.Add(new Vector3(-10, 0, -15)); // Sol alt
                slots.Add(new Vector3(20, 0, 15));   // Sağ üst
                slots.Add(new Vector3(15, 0, -5));   // Sağ orta
                slots.Add(new Vector3(10, 0, -20));  // Sağ alt
                return slots;
            case 13: // Y-Merge: 9 diamond slot
                slots.Add(new Vector3(-20, 0, 15));  // Sol kol üst
                slots.Add(new Vector3(-10, 0, 5));   // Sol kol alt
                slots.Add(new Vector3(20, 0, 15));   // Sağ kol üst
                slots.Add(new Vector3(10, 0, 5));    // Sağ kol alt
                slots.Add(new Vector3(-8, 0, -8));   // Birleşim sol
                slots.Add(new Vector3(8, 0, -8));    // Birleşim sağ
                slots.Add(new Vector3(-5, 0, -18));  // Gövde sol
                slots.Add(new Vector3(5, 0, -18));   // Gövde sağ
                slots.Add(new Vector3(0, 0, -25));   // Alt orta
                return slots;
            case 14: // Loop: 9 slot
                slots.Add(new Vector3(-20, 0, 20));  // Üst sol iç
                slots.Add(new Vector3(5, 0, 20));    // Üst orta
                slots.Add(new Vector3(10, 0, 20));   // Üst sağ
                slots.Add(new Vector3(-20, 0, 5));   // Sol orta
                slots.Add(new Vector3(5, 0, 0));     // Merkez
                slots.Add(new Vector3(-20, 0, -20)); // Sol alt
                slots.Add(new Vector3(5, 0, -20));   // Alt orta
                slots.Add(new Vector3(10, 0, -20));  // Alt sağ
                slots.Add(new Vector3(30, 0, 0));    // Sağ kenar
                return slots;
            case 15: // Y branches: 8 diamond slot
                slots.Add(new Vector3(-15, 0, 15));  // Sol üst
                slots.Add(new Vector3(-8, 0, 5));    // Sol orta
                slots.Add(new Vector3(15, 0, 15));   // Sağ üst
                slots.Add(new Vector3(8, 0, 5));     // Sağ orta
                slots.Add(new Vector3(20, 0, -5));   // Sağ dal
                slots.Add(new Vector3(20, 0, -20));  // Sağ dal alt
                slots.Add(new Vector3(-8, 0, -15));  // Orta sol
                slots.Add(new Vector3(8, 0, -15));   // Orta sağ
                return slots;
            case 16: // 3-Way Merge: 6 slot (diamond + kare)
                slots.Add(new Vector3(-20, 0, 20));  // Sol üst
                slots.Add(new Vector3(-12, 0, 0));   // Sol orta
                slots.Add(new Vector3(12, 0, 0));    // Sağ orta
                slots.Add(new Vector3(20, 0, 20));   // Sağ üst
                slots.Add(new Vector3(-8, 0, -15));  // Alt sol
                slots.Add(new Vector3(8, 0, -15));   // Alt sağ
                return slots;
            case 17: // O-Shape: 8 kare slot dikdörtgenin etrafında
                slots.Add(new Vector3(-10, 0, 25));  // Üst sol
                slots.Add(new Vector3(10, 0, 25));   // Üst sağ
                slots.Add(new Vector3(-30, 0, 5));   // Sol üst
                slots.Add(new Vector3(-30, 0, -5));  // Sol alt
                slots.Add(new Vector3(30, 0, 5));    // Sağ üst
                slots.Add(new Vector3(30, 0, -5));   // Sağ alt
                slots.Add(new Vector3(-10, 0, -25)); // Alt sol
                slots.Add(new Vector3(10, 0, -25));  // Alt sağ
                return slots;
            case 18: // Inverted Y: 6 kare slot
                slots.Add(new Vector3(-8, 0, 15));   // Üst sol
                slots.Add(new Vector3(8, 0, 15));    // Üst sağ
                slots.Add(new Vector3(-8, 0, 5));    // Orta sol
                slots.Add(new Vector3(8, 0, 5));     // Orta sağ
                slots.Add(new Vector3(-15, 0, -15)); // Alt sol
                slots.Add(new Vector3(15, 0, -15));  // Alt sağ
                return slots;
            case 19: // Diamond/X: 8 diamond slot
                slots.Add(new Vector3(0, 0, 10));    // Üst
                slots.Add(new Vector3(-10, 0, 10));  // Üst sol
                slots.Add(new Vector3(10, 0, 10));   // Üst sağ
                slots.Add(new Vector3(-10, 0, -10)); // Alt sol
                slots.Add(new Vector3(10, 0, -10));  // Alt sağ
                slots.Add(new Vector3(-20, 0, -10)); // Sol kenar
                slots.Add(new Vector3(20, 0, -10));  // Sağ kenar
                slots.Add(new Vector3(0, 0, -20));   // Alt
                return slots;
            
            // --- Level 20-27 özel tower slot konumları ---
            case 20:
                slots.Add(new Vector3(-8, 0, 8)); slots.Add(new Vector3(8, 0, 8));
                slots.Add(new Vector3(-8, 0, -8)); slots.Add(new Vector3(8, 0, -8));
                slots.Add(new Vector3(-15, 0, 0)); slots.Add(new Vector3(15, 0, 0));
                slots.Add(new Vector3(0, 0, 15)); slots.Add(new Vector3(0, 0, -15));
                return slots;
            case 21:
                slots.Add(new Vector3(-10, 0, 20)); slots.Add(new Vector3(10, 0, 20));
                slots.Add(new Vector3(30, 0, 10)); slots.Add(new Vector3(10, 0, -20));
                slots.Add(new Vector3(10, 0, 0)); slots.Add(new Vector3(-20, 0, -20));
                return slots;
            case 22:
                slots.Add(new Vector3(-15, 0, 25)); slots.Add(new Vector3(15, 0, 5));
                slots.Add(new Vector3(-25, 0, -15)); slots.Add(new Vector3(25, 0, -5));
                slots.Add(new Vector3(5, 0, -15)); slots.Add(new Vector3(-5, 0, 15));
                return slots;
            case 23:
                slots.Add(new Vector3(-15, 0, 15)); slots.Add(new Vector3(15, 0, 15));
                slots.Add(new Vector3(-25, 0, 5)); slots.Add(new Vector3(25, 0, 5));
                slots.Add(new Vector3(-10, 0, -15)); slots.Add(new Vector3(10, 0, -15));
                slots.Add(new Vector3(-10, 0, -25)); slots.Add(new Vector3(10, 0, -25));
                return slots;
            case 24:
                slots.Add(new Vector3(-10, 0, -10)); slots.Add(new Vector3(10, 0, -10));
                slots.Add(new Vector3(15, 0, 5)); slots.Add(new Vector3(15, 0, -5));
                return slots;
            case 25:
                slots.Add(new Vector3(-35, 0, 0)); slots.Add(new Vector3(35, 0, 0));
                slots.Add(new Vector3(-10, 0, 5)); slots.Add(new Vector3(10, 0, 5));
                slots.Add(new Vector3(-10, 0, -5)); slots.Add(new Vector3(10, 0, -5));
                return slots;
            case 26:
                slots.Add(new Vector3(-20, 0, -5)); slots.Add(new Vector3(-5, 0, -15));
                slots.Add(new Vector3(15, 0, -20)); slots.Add(new Vector3(35, 0, -5));
                slots.Add(new Vector3(35, 0, 15)); slots.Add(new Vector3(15, 0, 25));
                slots.Add(new Vector3(-5, 0, 15));
                return slots;
            case 27:
                slots.Add(new Vector3(-10, 0, 10)); slots.Add(new Vector3(10, 0, 10));
                slots.Add(new Vector3(-10, 0, -10)); slots.Add(new Vector3(10, 0, -10));
                return slots;
            
            // --- Level 28-35 özel tower slot konumları ---
            case 28:
                slots.Add(new Vector3(-20, 0, 10)); slots.Add(new Vector3(-5, 0, 0));
                slots.Add(new Vector3(10, 0, 15)); slots.Add(new Vector3(25, 0, 5));
                slots.Add(new Vector3(5, 0, 0)); slots.Add(new Vector3(15, 0, -5));
                return slots;
            case 29:
                slots.Add(new Vector3(-15, 0, 15)); slots.Add(new Vector3(15, 0, 15));
                slots.Add(new Vector3(-15, 0, -15)); slots.Add(new Vector3(15, 0, -25));
                slots.Add(new Vector3(0, 0, -10)); slots.Add(new Vector3(0, 0, 10));
                return slots;
            case 30:
                slots.Add(new Vector3(-8, 0, 8)); slots.Add(new Vector3(8, 0, 8));
                slots.Add(new Vector3(-8, 0, -8)); slots.Add(new Vector3(8, 0, -8));
                slots.Add(new Vector3(-15, 0, 0)); slots.Add(new Vector3(15, 0, 0));
                slots.Add(new Vector3(0, 0, 15)); slots.Add(new Vector3(0, 0, -15));
                return slots;
            case 31:
                slots.Add(new Vector3(15, 0, 20)); slots.Add(new Vector3(-10, 0, 10));
                slots.Add(new Vector3(10, 0, -10)); slots.Add(new Vector3(-15, 0, -20));
                return slots;
            case 32:
                slots.Add(new Vector3(-15, 0, 0)); slots.Add(new Vector3(-5, 0, -20));
                slots.Add(new Vector3(15, 0, 0)); slots.Add(new Vector3(5, 0, 15));
                slots.Add(new Vector3(5, 0, -5)); slots.Add(new Vector3(-5, 0, -5));
                return slots;
            case 33:
                slots.Add(new Vector3(-15, 0, 15)); slots.Add(new Vector3(15, 0, 15));
                slots.Add(new Vector3(-25, 0, 5)); slots.Add(new Vector3(25, 0, 5));
                slots.Add(new Vector3(-10, 0, -15)); slots.Add(new Vector3(10, 0, -15));
                slots.Add(new Vector3(-10, 0, -25)); slots.Add(new Vector3(10, 0, -25));
                return slots;
            case 34:
                slots.Add(new Vector3(-10, 0, 10)); slots.Add(new Vector3(0, 0, 10));
                slots.Add(new Vector3(10, 0, 10));
                return slots;
            case 35:
                slots.Add(new Vector3(-15, 0, 20)); slots.Add(new Vector3(15, 0, 20));
                slots.Add(new Vector3(-25, 0, 0)); slots.Add(new Vector3(25, 0, 0));
                slots.Add(new Vector3(-15, 0, -20)); slots.Add(new Vector3(15, 0, -20));
                slots.Add(new Vector3(0, 0, 0));
                return slots;
            
            // --- Level 36-44 özel tower slot konumları ---
            case 36: // U-Shape
                slots.Add(new Vector3(-10, 0, 20)); slots.Add(new Vector3(10, 0, 20));
                slots.Add(new Vector3(15, 0, 10)); slots.Add(new Vector3(15, 0, -5));
                slots.Add(new Vector3(10, 0, -10)); slots.Add(new Vector3(-5, 0, -10));
                slots.Add(new Vector3(-10, 0, 0)); slots.Add(new Vector3(-10, 0, -25));
                return slots;
            case 37: // Eye
                slots.Add(new Vector3(-10, 0, 5)); slots.Add(new Vector3(10, 0, 5));
                slots.Add(new Vector3(-10, 0, -5)); slots.Add(new Vector3(10, 0, -5));
                slots.Add(new Vector3(0, 0, 0)); slots.Add(new Vector3(25, 0, 0));
                return slots;
            case 38: // 3-Way Merge
                slots.Add(new Vector3(-20, 0, 15)); slots.Add(new Vector3(20, 0, 15));
                slots.Add(new Vector3(-10, 0, 5)); slots.Add(new Vector3(10, 0, 5));
                slots.Add(new Vector3(-5, 0, -15)); slots.Add(new Vector3(5, 0, -15));
                return slots;
            case 39: // X-Cross
                slots.Add(new Vector3(0, 0, 0)); slots.Add(new Vector3(-15, 0, 5));
                slots.Add(new Vector3(15, 0, 5)); slots.Add(new Vector3(-15, 0, -15));
                slots.Add(new Vector3(15, 0, -15)); slots.Add(new Vector3(0, 0, -25));
                return slots;
            case 40: // Corner Arrows
                slots.Add(new Vector3(-8, 0, 8)); slots.Add(new Vector3(8, 0, 8));
                slots.Add(new Vector3(-8, 0, -8)); slots.Add(new Vector3(8, 0, -8));
                slots.Add(new Vector3(-15, 0, 0)); slots.Add(new Vector3(15, 0, 0));
                slots.Add(new Vector3(0, 0, 15)); slots.Add(new Vector3(0, 0, -15));
                return slots;
            case 41: // Snake
                slots.Add(new Vector3(-5, 0, 20)); slots.Add(new Vector3(10, 0, 20));
                slots.Add(new Vector3(30, 0, 20)); slots.Add(new Vector3(-25, 0, 0));
                slots.Add(new Vector3(5, 0, 0)); slots.Add(new Vector3(-25, 0, -20));
                slots.Add(new Vector3(-5, 0, -20)); slots.Add(new Vector3(10, 0, -20));
                return slots;
            case 42: // Multi S-Curve
                slots.Add(new Vector3(-5, 0, 15)); slots.Add(new Vector3(-20, 0, 0));
                slots.Add(new Vector3(5, 0, -5)); slots.Add(new Vector3(20, 0, -15));
                slots.Add(new Vector3(-10, 0, -20)); slots.Add(new Vector3(25, 0, 5));
                return slots;
            case 43: // Tight S-Zigzag
                slots.Add(new Vector3(-20, 0, 15)); slots.Add(new Vector3(20, 0, 15));
                slots.Add(new Vector3(-20, 0, -5)); slots.Add(new Vector3(20, 0, -5));
                slots.Add(new Vector3(-20, 0, -25)); slots.Add(new Vector3(20, 0, -25));
                slots.Add(new Vector3(0, 0, 5)); slots.Add(new Vector3(0, 0, -15));
                return slots;
            case 44: // Diamond Star
                slots.Add(new Vector3(-15, 0, 15)); slots.Add(new Vector3(15, 0, 15));
                slots.Add(new Vector3(-15, 0, -15)); slots.Add(new Vector3(15, 0, -15));
                slots.Add(new Vector3(0, 0, -25));
                return slots;
            
            // --- Level 45-50 özel tower slot konumları ---
            case 45: // Large Ring
                slots.Add(new Vector3(-5, 0, 10)); slots.Add(new Vector3(5, 0, -5));
                slots.Add(new Vector3(-5, 0, -15)); slots.Add(new Vector3(-20, 0, 0));
                slots.Add(new Vector3(-20, 0, -20)); slots.Add(new Vector3(20, 0, 5));
                slots.Add(new Vector3(20, 0, -15)); slots.Add(new Vector3(0, 0, 20));
                return slots;
            case 46: // D-Shape Dual Merge
                slots.Add(new Vector3(-5, 0, 15)); slots.Add(new Vector3(5, 0, 15));
                slots.Add(new Vector3(-5, 0, 5)); slots.Add(new Vector3(5, 0, 5));
                slots.Add(new Vector3(-5, 0, -5)); slots.Add(new Vector3(5, 0, -5));
                slots.Add(new Vector3(-5, 0, -15)); slots.Add(new Vector3(5, 0, -15));
                return slots;
            case 47: // Looping Y-Merge
                slots.Add(new Vector3(-20, 0, 20)); slots.Add(new Vector3(20, 0, 20));
                slots.Add(new Vector3(-10, 0, 10)); slots.Add(new Vector3(10, 0, 10));
                slots.Add(new Vector3(-20, 0, 0)); slots.Add(new Vector3(20, 0, 0));
                slots.Add(new Vector3(-10, 0, -10)); slots.Add(new Vector3(10, 0, -10));
                return slots;
            case 48: // Winding Snake
                slots.Add(new Vector3(-10, 0, 20)); slots.Add(new Vector3(0, 0, 20));
                slots.Add(new Vector3(10, 0, 20)); slots.Add(new Vector3(20, 0, 10));
                slots.Add(new Vector3(20, 0, 0)); slots.Add(new Vector3(10, 0, 0));
                slots.Add(new Vector3(20, 0, -10)); slots.Add(new Vector3(20, 0, -20));
                return slots;
            case 49: // Intricate Mesh / Star Knot
                slots.Add(new Vector3(-10, 0, 0)); slots.Add(new Vector3(10, 0, 0));
                slots.Add(new Vector3(0, 0, -15)); slots.Add(new Vector3(-20, 0, 15));
                slots.Add(new Vector3(20, 0, 15)); slots.Add(new Vector3(15, 0, -20));
                return slots;
            case 50: // Ultimate Corner Arrows
                slots.Add(new Vector3(-8, 0, 8)); slots.Add(new Vector3(8, 0, 8));
                slots.Add(new Vector3(-8, 0, -8)); slots.Add(new Vector3(8, 0, -8));
                slots.Add(new Vector3(-15, 0, 0)); slots.Add(new Vector3(15, 0, 0));
                slots.Add(new Vector3(0, 0, 15)); slots.Add(new Vector3(0, 0, -15));
                return slots;
        }

        // --- Level 1-10 ve 20+ için mevcut layout sistemi ---
        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        switch (layout)
        {
            case 1: // Level 1
                slots.Add(new Vector3(-5, 0, 25)); // Top right
                slots.Add(new Vector3(15, 0, 5));  // Top right
                slots.Add(new Vector3(-15, 0, -5)); // Bottom left
                slots.Add(new Vector3(5, 0, -25));  // Bottom left
                break;
            case 2: // Level 2
                slots.Add(new Vector3(-45, 0, 0));   // Far left
                slots.Add(new Vector3(-15, 0, 10));  // Right of vertical
                slots.Add(new Vector3(-15, 0, -10)); // Right of vertical corner
                slots.Add(new Vector3(0, 0, -10));   // Above horizontal
                slots.Add(new Vector3(0, 0, -35));   // Below horizontal
                slots.Add(new Vector3(15, 0, -35));  // Below horizontal
                break;
            case 3: // Level 3
                slots.Add(new Vector3(-10, 0, 15));  // Under top horiz
                slots.Add(new Vector3(30, 0, 15));   // Right of top-mid
                slots.Add(new Vector3(-30, 0, -15)); // Left of mid-bottom
                slots.Add(new Vector3(10, 0, -15));  // Above bottom horiz
                break;
            case 4: // Level 4
                slots.Add(new Vector3(-35, 0, 0));
                slots.Add(new Vector3(-15, 0, -25));
                slots.Add(new Vector3(-15, 0, 15));
                slots.Add(new Vector3(35, 0, 0));
                slots.Add(new Vector3(15, 0, -25));
                slots.Add(new Vector3(15, 0, 15));
                break;
            case 5: // Level 5
                slots.Add(new Vector3(-35, 0, 10));
                slots.Add(new Vector3(-35, 0, -10));
                slots.Add(new Vector3(-15, 0, 30));
                slots.Add(new Vector3(-5, 0, -5));
                slots.Add(new Vector3(5, 0, -25));
                slots.Add(new Vector3(25, 0, 0));
                break;
            case 6: // Level 6
                slots.Add(new Vector3(-15, 0, 5));
                slots.Add(new Vector3(-15, 0, -15));
                slots.Add(new Vector3(15, 0, 5));
                slots.Add(new Vector3(15, 0, -15));
                break;
            case 7: // Level 7
                slots.Add(new Vector3(15, 0, 25));
                slots.Add(new Vector3(-20, 0, 10));
                slots.Add(new Vector3(15, 0, -15));
                slots.Add(new Vector3(-20, 0, -25));
                slots.Add(new Vector3(15, 0, -25));
                slots.Add(new Vector3(-20, 0, 25)); // Extrapolated from image
                break;
            case 8: // Level 8
                slots.Add(new Vector3(-15, 0, 15));
                slots.Add(new Vector3(15, 0, 15));
                slots.Add(new Vector3(-15, 0, -35));
                slots.Add(new Vector3(15, 0, -35));
                break;
            case 9: // Level 9
                slots.Add(new Vector3(-30, 0, 5));
                slots.Add(new Vector3(-20, 0, -15));
                slots.Add(new Vector3(-10, 0, 10));
                slots.Add(new Vector3(0, 0, -5));
                slots.Add(new Vector3(10, 0, 10));
                slots.Add(new Vector3(20, 0, -15));
                slots.Add(new Vector3(30, 0, 5));
                break;
            case 0:
            default:
                if (lvlIdx == 50)
                {
                    slots.Add(new Vector3(-25, 0, 25));
                    slots.Add(new Vector3(15, 0, 25));
                    slots.Add(new Vector3(-30, 0, 0));
                    slots.Add(new Vector3(0, 0, -10));
                    slots.Add(new Vector3(-10, 0, -35));
                    slots.Add(new Vector3(25, 0, -25));
                }
                else
                {
                    slots.Add(new Vector3(-15, 0, 0));
                    slots.Add(new Vector3(10, 0, 20));
                    slots.Add(new Vector3(-25, 0, -15));
                    slots.Add(new Vector3(30, 0, -5));
                    slots.Add(new Vector3(5, 0, -10));
                    slots.Add(new Vector3(10, 0, -30));
                }
                break;
        }
        return slots;
    }

    private static int GetSpawnerCountForLevel(int lvlIdx)
    {
        // --- Level 11-19 özel spawner sayıları ---
        switch (lvlIdx)
        {
            case 11: return 1;  // Z-Shape: 1 spawner
            case 12: return 1;  // S-Curve: 1 spawner (çift yol ama aynı noktadan)
            case 13: return 2;  // Y-Merge: 2 spawner
            case 14: return 1;  // Loop: 1 spawner
            case 15: return 2;  // Y branches: 2 spawner
            case 16: return 3;  // 3-Way: 3 spawner
            case 17: return 1;  // O-Shape: 1 spawner (üstten bölünme)
            case 18: return 2;  // Inverted Y: 2 spawner (alttan)
            case 19: return 3;  // Diamond: 3 spawner (üst + sol + sağ)
            case 20: return 4;
            case 21: return 1;
            case 22: return 1;
            case 23: return 2;
            case 24: return 1;
            case 25: return 1;
            case 26: return 1;
            case 27: return 3;
            case 28: return 3;
            case 29: return 3;
            case 30: return 4;
            case 31: return 1;
            case 32: return 1;
            case 33: return 2;
            case 34: return 1;
            case 35: return 1;
            case 36: return 1;
            case 37: return 1;
            case 38: return 3;
            case 39: return 2;
            case 40: return 4;
            case 41: return 1;
            case 42: return 1;
            case 43: return 1;
            case 44: return 3;
            case 45: return 1;
            case 46: return 2;
            case 47: return 2;
            case 48: return 1;
            case 49: return 3;
            case 50: return 4;
        }

        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        if (layout == 0) return 4;
        if (layout == 7 || layout == 8) return 3;
        if (layout == 3 || layout == 5 || layout == 6) return 2;
        return 1;
    }

    private static (string unit, int count)[] GetWaveComposition(int lvlIdx, int wave)
    {
        int maxWaves = GetWaveCountForLevel(lvlIdx);
        bool isBossWave = (wave == maxWaves); // Her levelin son dalgası boss dalgası

        if (isBossWave)
        {
            // Her levelin son dalgasında sadece 1 boss gelir, yanında başka asker yok
            string[] forestBosses = { "Demon_King", "Bone_Dragon", "Abyssal_Behemoth", "Death_Knight_Commander", "Blood_Mage", "Shadow_Leviathan" };
            string[] desertBosses = { "Bone_Dragon", "Death_Knight_Commander", "Shadow_Leviathan", "Blood_Mage", "Abyssal_Behemoth", "Demon_King" };
            string[] snowBosses = { "Shadow_Leviathan", "Blood_Mage", "Demon_King", "Bone_Dragon", "Death_Knight_Commander", "Abyssal_Behemoth" };
            string[] underworldBosses = { "Demon_King", "Abyssal_Behemoth", "Shadow_Leviathan", "Bone_Dragon", "Blood_Mage", "Death_Knight_Commander" };

            string boss;
            if (lvlIdx <= 12) boss = forestBosses[(lvlIdx - 1) % forestBosses.Length];
            else if (lvlIdx <= 25) boss = desertBosses[(lvlIdx - 13) % desertBosses.Length];
            else if (lvlIdx <= 38) boss = snowBosses[(lvlIdx - 26) % snowBosses.Length];
            else boss = underworldBosses[(lvlIdx - 39) % underworldBosses.Length];

            return new (string, int)[] { (boss, 1) };
        }

        int multiplier = 1 + (lvlIdx / 15);
        int basicCount = 5 + (wave * 3 * multiplier);
        int eliteCount = Mathf.Max(0, (wave + (lvlIdx / 5)) - 4);

        // Level 1-12: Forest (Starter)
        if (lvlIdx <= 12)
        {
            if (wave <= 2) return new (string, int)[] { ("Skeleton_Warrior", basicCount), ("Zombie_Shambler", basicCount / 2) };
            if (wave <= 4) return new (string, int)[] { ("Skeleton_Warrior", basicCount), ("Goblin_Grunt", basicCount / 2), ("Skeleton_Archer", basicCount / 3) };
            return new (string, int)[] { ("Skeleton_Warrior", basicCount), ("Orc_Marauder", eliteCount + 1), ("Skeleton_Archer", basicCount / 2) };
        }
        // Level 13-25: Desert (Advanced)
        else if (lvlIdx <= 25)
        {
            if (wave <= 2) return new (string, int)[] { ("Orc_Marauder", basicCount / 2), ("Spider_Rider", basicCount / 3) };
            if (wave <= 4) return new (string, int)[] { ("Orc_Berserker", basicCount / 2), ("Gargoyle", eliteCount + 2), ("Dark_Knight", eliteCount) };
            return new (string, int)[] { ("Troll_Brute", eliteCount + 1), ("Orc_Berserker", basicCount / 2), ("Shadow_Assassin", eliteCount + 2) };
        }
        // Level 26-38: Snow (Elite)
        else if (lvlIdx <= 38)
        {
            if (wave <= 2) return new (string, int)[] { ("Wraith", basicCount / 3), ("Dark_Knight", basicCount / 4), ("Lich", eliteCount + 1) };
            if (wave <= 4) return new (string, int)[] { ("Bone_Golem", eliteCount + 1), ("Wraith", basicCount / 2), ("Necromancer", eliteCount + 1) };
            return new (string, int)[] { ("Vampire_Lord", eliteCount), ("Bone_Golem", eliteCount + 2), ("Wraith", basicCount) };
        }
        // Level 39-50: Underworld (Master)
        else
        {
            if (lvlIdx == 50 && wave < maxWaves) 
            {
                // 50. seviyede son dalga dışındaki dalgalar da zor olmalı, ama boss içermemeli
                return new (string, int)[] { 
                    ("Vampire_Lord", eliteCount + 2), 
                    ("Lich", eliteCount + 1),
                    ("Bone_Golem", 2)
                };
            }

            if (wave <= 3) return new (string, int)[] { ("Vampire_Lord", eliteCount + 2), ("Shadow_Assassin", basicCount / 2) };
            return new (string, int)[] { 
                ("Bone_Golem", eliteCount + 3), 
                ("Lich", eliteCount + 1), 
                ("Wraith", basicCount) 
            };
        }
    }

    private static WaveData CreateWave(string path, string name, (string unit, int count)[] composition, int pathCount = 1)
    {
        string assetPath = $"{path}/{name}.asset";
        WaveData wave = AssetDatabase.LoadAssetAtPath<WaveData>(assetPath);
        if (wave == null)
        {
            wave = ScriptableObject.CreateInstance<WaveData>();
            AssetDatabase.CreateAsset(wave, assetPath);
        }

        wave.unitGroups = new List<WaveUnitGroup>();
        
        // Üniteleri parçalara böl ve yollara dağıt (Daha dinamik dalgalar için)
        int batchSize = 5; // Her grupta max 5 ünite

        // Her ünite tipi için
        for (int i = 0; i < composition.Length; i++)
        {
            var item = composition[i];
            UnitData ud = AssetDatabase.LoadAssetAtPath<UnitData>($"Assets/Data/Units/{item.unit}.asset");
            if (ud == null) continue;

            int remaining = item.count;
            int batchCount = 0;

            while (remaining > 0)
            {
                int currentBatch = Mathf.Min(remaining, batchSize);
                int spawnerIdx = (i + batchCount) % pathCount; // Yollar arasında döndür

                wave.unitGroups.Add(new WaveUnitGroup {
                    unitData = ud,
                    count = currentBatch,
                    spawnInterval = 1.5f,
                    groupDelay = batchCount == 0 ? i * 2f : 3f, // Gruplar arası kısa bekleme
                    spawnerIndex = spawnerIdx
                });

                remaining -= currentBatch;
                batchCount++;
            }
        }
        
        wave.timeBeforeNextWave = 15f;

        EditorUtility.SetDirty(wave);
        return wave;
    }

    private static void CreateLevel(string path, string id, string name, int startLight, int startDark, LevelTheme theme, List<WaveData> waves, List<LevelPath> paths, List<Vector3> bases, List<Vector3> customSlots)
    {
        string assetPath = $"{path}/{id}.asset";
        LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
        if (level == null)
        {
            level = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(level, assetPath);
        }

        level.levelID = id;
        level.levelName = name;
        level.startingCurrencyLight = startLight;
        level.startingCurrencyDark = startDark;
        level.theme = theme;
        
        // Seviye adına göre dinamik zorluk belirle (Örn: Level 1-10 -> 1 yıldız, 11-20 -> 2 yıldız, 21+ -> 3 yıldız)
        int diff = 1;
        var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int levelNum))
        {
            if (levelNum > 20) diff = 3;
            else if (levelNum > 10) diff = 2;
        }
        level.difficulty = diff;

        if (level.waves == null) level.waves = new List<WaveData>();
        level.waves.Clear();
        level.waves.AddRange(waves);

        if (level.paths == null) level.paths = new List<LevelPath>();
        level.paths.Clear();
        level.paths.AddRange(paths);

        if (level.basePoints == null) level.basePoints = new List<Vector3>();
        level.basePoints.Clear();
        level.basePoints.AddRange(bases);

        if (level.customSlotPositions == null) level.customSlotPositions = new List<Vector3>();
        level.customSlotPositions.Clear();
        level.customSlotPositions.AddRange(customSlots);

        level.towerSlotCount = 15 + (waves.Count * 2); // Dalga sayısına göre slot artırımı
        level.sceneIndex = 2; // Default Gameplay Scene

        // Otomatik Icon (Level Preview) ataması
        string safeNameForIcon = level.levelName.Replace(" ", "_").Replace("'", "").Replace(":", "");
        string numStrForIcon = level.levelID.Replace("Level", "");
        string iconPath = $"Assets/Data/Icons/Levels/Level_{numStrForIcon}__{safeNameForIcon}_Map_Icon.png";
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        if (iconSprite != null)
        {
            level.levelPreview = iconSprite;
        }
        else
        {
            Debug.LogWarning($"Level Preview Icon not found at path: {iconPath}");
        }

        EditorUtility.SetDirty(level);
        
        // HARİTA PREFAB'INI DİNAMİK OLARAK OLUŞTUR
        // Level 1-50 zaten kullanıcı tarafından düzenlendi, yeniden oluşturma!
        int lvlNum = 0;
        var lvlMatch = System.Text.RegularExpressions.Regex.Match(id, @"\d+");
        if (lvlMatch.Success) int.TryParse(lvlMatch.Value, out lvlNum);
        
        if (lvlNum > 50)
        {
            GenerateMapPrefab(level);
        }
    }

    private static void GenerateMapPrefab(LevelData level)
    {
        string mapFolder = "Assets/Maps";
        EnsureDirectory(mapFolder);

        string safeName = level.levelName.Replace(" ", "_").Replace("'", "").Replace(":", "");
        string numStr = level.levelID.Replace("Level", "");
        string mapPath = $"{mapFolder}/Level_{numStr}__{safeName}_Map.prefab";

        GameObject root = new GameObject($"{level.levelName}_Map");

        // 1. Zemin (Ground)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(root.transform);
        ground.transform.localScale = new Vector3(100f, 0.5f, 100f);
        ground.transform.localPosition = new Vector3(0, -0.25f, 0);
        ground.layer = LayerMask.NameToLayer("Default");
        
        // Zemine temaya göre renk ver (Mevcut çalışan bir materyali klonla)
        Color groundColor = new Color(0.2f, 0.3f, 0.2f); // Default Forest
        if (level.theme == LevelTheme.Desert) groundColor = new Color(0.4f, 0.35f, 0.2f);
        else if (level.theme == LevelTheme.Snow) groundColor = new Color(0.8f, 0.85f, 0.9f);
        else if (level.theme == LevelTheme.Underworld) groundColor = new Color(0.25f, 0.15f, 0.15f);
        
        Material refMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/Environment/Desert/colormap.mat");
        Material groundMat = refMat != null ? new Material(refMat) : new Material(Shader.Find("Standard"));
        
        if (groundMat.HasProperty("_BaseColor")) groundMat.SetColor("_BaseColor", groundColor);
        else groundMat.color = groundColor;
        
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;

        // 2. Yollar (Paths)
        for (int i = 0; i < level.paths.Count; i++)
        {
            var pathData = level.paths[i];
            GameObject pathGo = new GameObject($"Path_{i}");
            pathGo.transform.SetParent(root.transform);
            
            PathWaypoints pw = pathGo.AddComponent<PathWaypoints>();
            
            // Visualizer & Waypoints
            for (int pIdx = 0; pIdx < pathData.points.Count; pIdx++)
            {
                Vector3 pt = pathData.points[pIdx];
                GameObject wp = new GameObject($"WP_{pIdx}");
                wp.transform.SetParent(pathGo.transform);
                wp.transform.position = pt;

                if (pIdx > 0)
                {
                    Vector3 prevPt = pathData.points[pIdx - 1];
                    Vector3 prevDir = (pt - prevPt).normalized;
                    if (pIdx < pathData.points.Count - 1)
                    {
                        Vector3 nextDir = (pathData.points[pIdx + 1] - pt).normalized;
                        if (Vector3.Dot(prevDir, nextDir) < 0.95f) // Köşe var!
                        {
                            GameObject cornerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Models/Environment/{level.theme}/PathCurved.fbx");
                            if (cornerPrefab != null)
                            {
                                GameObject corner = (GameObject)PrefabUtility.InstantiatePrefab(cornerPrefab, pathGo.transform);
                                corner.name = $"Corner_{pIdx}";
                                corner.transform.position = pt;
                                
                                // Dönüş yönünü ayarla: Gelen yön ile Giden yönün açı ortayını kullanıyoruz
                                // Cross product ile dönüşün sağa mı sola mı olduğunu bul
                                float sign = Mathf.Sign(Vector3.Cross(prevDir, nextDir).y);
                                // Yöne göre uygun rotasyonu ver. Standard tile'lar için genelde incoming veya outgoing direction'a göre 90 derece dönüş olur
                                corner.transform.rotation = Quaternion.LookRotation(prevDir) * Quaternion.Euler(0, sign * 90f > 0 ? 0 : -90f, 0);
                            }
                        }
                    }
                }

                if (pIdx < pathData.points.Count - 1)
                {
                    Vector3 nextPt = pathData.points[pIdx + 1];
                    Vector3 dir = nextPt - pt;
                    float dist = dir.magnitude;
                    Vector3 dirNorm = dir.normalized;
                    
                    GameObject pathSegmentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Models/Environment/{level.theme}/PathStraight.fbx");
                    
                    float tileSize = 2f; // Varsayılan boyut
                    if (pathSegmentPrefab != null)
                    {
                        Renderer r = pathSegmentPrefab.GetComponentInChildren<Renderer>();
                        if (r != null) tileSize = r.bounds.size.z;
                        if (tileSize <= 0.1f) tileSize = 2f;
                    }

                    int tileCount = Mathf.Max(1, Mathf.RoundToInt(dist / tileSize));
                    float actualTileDist = dist / tileCount;

                    for (int t = 0; t < tileCount; t++)
                    {
                        GameObject segment;
                        if (pathSegmentPrefab != null)
                        {
                            segment = (GameObject)PrefabUtility.InstantiatePrefab(pathSegmentPrefab, pathGo.transform);
                            Collider col = segment.GetComponent<Collider>();
                            if (col == null) col = segment.AddComponent<BoxCollider>();
                            col.isTrigger = true;
                            // Tile scale Z tam olarak mesafeyi kapatacak şekilde ayarlanır
                            segment.transform.localScale = new Vector3(1f, 1f, actualTileDist / tileSize);
                        }
                        else
                        {
                            segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            segment.GetComponent<Collider>().isTrigger = true;
                            Material pathMat = refMat != null ? new Material(refMat) : new Material(Shader.Find("Standard"));
                            if (pathMat.HasProperty("_BaseColor")) pathMat.SetColor("_BaseColor", new Color(0.4f, 0.4f, 0.45f));
                            else pathMat.color = new Color(0.4f, 0.4f, 0.45f);
                            segment.GetComponent<Renderer>().sharedMaterial = pathMat;
                            segment.transform.localScale = new Vector3(3f, 0.1f, actualTileDist);
                        }

                        segment.name = $"Segment_{pIdx}_{t}";
                        segment.layer = LayerMask.NameToLayer("Path");
                        segment.transform.position = pt + dirNorm * (t * actualTileDist + actualTileDist / 2f);
                        segment.transform.forward = dirNorm;
                    }
                }
            }
            
            // Serileştirme (Waypoints)
            var wpSo = new SerializedObject(pw);
            var wpProp = wpSo.FindProperty("waypoints");
            wpProp.ClearArray();
            for (int p = 0; p < pathGo.transform.childCount; p++)
            {
                Transform child = pathGo.transform.GetChild(p);
                if (child.name.StartsWith("WP_"))
                {
                    wpProp.InsertArrayElementAtIndex(wpProp.arraySize);
                    wpProp.GetArrayElementAtIndex(wpProp.arraySize - 1).objectReferenceValue = child;
                }
            }
            wpSo.ApplyModifiedProperties();

            // Spawner
            GameObject spawnerGo = new GameObject($"Spawner_{i}");
            spawnerGo.transform.SetParent(root.transform);
            if (pathData.points.Count > 0) spawnerGo.transform.position = pathData.points[0];
            
            Spawner sp = spawnerGo.AddComponent<Spawner>();
            sp.spawnerIndex = i;
            var spSo = new SerializedObject(sp);
            var assignedProp = spSo.FindProperty("assignedPaths");
            assignedProp.ClearArray();
            assignedProp.InsertArrayElementAtIndex(0);
            assignedProp.GetArrayElementAtIndex(0).objectReferenceValue = pw;
            spSo.FindProperty("spawnPoint").objectReferenceValue = spawnerGo.transform;
            spSo.ApplyModifiedProperties();
        }

        // 3. Base(s)
        for (int i = 0; i < level.basePoints.Count; i++)
        {
            GameObject bse = new GameObject($"Base_{i}");
            bse.transform.SetParent(root.transform);
            bse.transform.position = level.basePoints[i];
            bse.AddComponent<Base>();
            // Base artık tamamen boş bir obje (görseli yok)
        }

        // 4. Custom Tower Slots
        if (level.customSlotPositions != null)
        {
            GameObject slotsRoot = new GameObject("TowerSlots");
            slotsRoot.transform.SetParent(root.transform);
            
            // Zaten çalışan prefab'ı kullan (TowerSlot, Collider, Visuals, BaseTowerSelectionPrefab hepsi içinde)
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/BaseTowerSlotPrefab.prefab");
            GameObject selectModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Environment/Select.fbx");
            
            for (int i = 0; i < level.customSlotPositions.Count; i++)
            {
                GameObject slot;
                if (slotPrefab != null)
                {
                    slot = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, slotsRoot.transform);
                }
                else
                {
                    slot = new GameObject($"Slot_{i}");
                    slot.transform.SetParent(slotsRoot.transform);
                }
                
                slot.name = $"Slot_{i}";
                slot.transform.position = level.customSlotPositions[i];

                // Select.fbx görselini ekle
                if (selectModel != null)
                {
                    Transform visuals = slot.transform.Find("Visuals");
                    Transform parent = visuals != null ? visuals : slot.transform;
                    GameObject vis = (GameObject)PrefabUtility.InstantiatePrefab(selectModel, parent);
                    vis.name = "SelectVisual";
                    vis.transform.localPosition = Vector3.zero;

                    // Yalnızca SelectVisual MeshRenderer'ını kapat
                    Renderer r = vis.GetComponent<Renderer>();
                    if (r == null) r = vis.GetComponentInChildren<Renderer>();
                    if (r != null) r.enabled = false;
                }
            }
        }

        // 5. Dekoratif Çevre Objeleri (Scatter Environment Props)
        List<string> propNames = new List<string> { "Rock.fbx" };
        if (level.theme == LevelTheme.Forest) propNames.AddRange(new[] { "Tree.fbx", "Bush.fbx" });
        else if (level.theme == LevelTheme.Desert) propNames.Add("Cactus.fbx");
        else if (level.theme == LevelTheme.Snow) propNames.AddRange(new[] { "PineTree.fbx", "IceCrystal.fbx" });
        else if (level.theme == LevelTheme.Underworld) propNames.AddRange(new[] { "Crystal.fbx", "LavaPool.fbx" });

        List<GameObject> loadedProps = new List<GameObject>();
        foreach (string pName in propNames)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Models/Environment/{level.theme}/{pName}");
            if (prefab != null) loadedProps.Add(prefab);
        }

        if (loadedProps.Count > 0)
        {
            GameObject propsRoot = new GameObject("EnvironmentProps");
            propsRoot.transform.SetParent(root.transform);
            
            // Sabit bir seed ile rastgele yerleştirme (her haritanın prop düzeni aynı kalsın diye)
            Random.InitState(level.levelName.GetHashCode());

            for (int i = 0; i < 40; i++) // 40 adet dekoratif obje serpiştir
            {
                // Zemin -40 ile +40 arası rastgele koordinatlar
                float rx = Random.Range(-40f, 40f);
                float rz = Random.Range(-40f, 40f);
                Vector3 pos = new Vector3(rx, 0, rz);

                // Yollara, base'lere veya slotlara çok yakın olmasın
                bool tooClose = false;
                foreach (var p in level.paths) {
                    foreach (var wp in p.points) {
                        if (Vector3.Distance(pos, wp) < 6f) tooClose = true;
                    }
                }
                foreach (var bp in level.basePoints) {
                    if (Vector3.Distance(pos, bp) < 8f) tooClose = true;
                }
                if (level.customSlotPositions != null) {
                    foreach (var sp in level.customSlotPositions) {
                        if (Vector3.Distance(pos, sp) < 4f) tooClose = true;
                    }
                }

                if (!tooClose)
                {
                    GameObject pPrefab = loadedProps[Random.Range(0, loadedProps.Count)];
                    GameObject prop = (GameObject)PrefabUtility.InstantiatePrefab(pPrefab, propsRoot.transform);
                    prop.transform.position = pos;
                    prop.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    float scale = Random.Range(0.8f, 1.5f);
                    prop.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, mapPath);
        GameObject.DestroyImmediate(root);

        // Atama
        level.mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPath);
        EditorUtility.SetDirty(level);
    }

    [MenuItem("Tools/TD Setup/Generate Spells")]
    public static void GenerateDefaultSpells()
    {
        string path = "Assets/Data/Spells";
        EnsureDirectory(path, true); // TAM TEMİZLİK

        // LIGHT SPELLS (3)
        CreateSpell(path, "Spell_Light_Meteor_1", "Meteor Strike", Side.Light, 50, SpellType.Meteor, 100f, 4f, 15f);
        CreateSpell(path, "Spell_Light_Meteor_2", "Elite Meteor", Side.Light, 150, SpellType.Meteor, 200f, 6f, 15f);
        CreateSpell(path, "Spell_Light_Shield", "Divine Shield", Side.Light, 100, SpellType.Shield, 8f, 5f, 20f);
        
        // REINFORCEMENTS (2)
        CreateSpell(path, "Spell_Light_Reinforce_1", "Reinforcements", Side.Light, 30, SpellType.Reinforcement, 2f, 0f, 25f);
        CreateSpell(path, "Spell_Light_Reinforce_2", "Royal Guards", Side.Light, 120, SpellType.Reinforcement, 3f, 0f, 25f);

        // DARK SPELLS (3)
        CreateSpell(path, "Spell_Dark_Rift", "Abyssal Rift", Side.Dark, 120, SpellType.Meteor, 100f, 8f, 12f);
        CreateSpell(path, "Spell_Dark_Bloodlust", "Bloodlust", Side.Dark, 80, SpellType.Buff, 1.5f, 4f, 10f);
        CreateSpell(path, "Spell_Dark_Freeze", "Shadow Freeze", Side.Dark, 110, SpellType.Freeze, 4f, 6f, 18f);

        // NEUTRAL SPELLS (2)
        CreateSpell(path, "Spell_Neutral_Gold", "Gold Rush", Side.Neutral, 0, SpellType.GoldBoost, 100f, 0f, 60f);
        CreateSpell(path, "Spell_Neutral_Earthquake", "Earthquake", Side.Neutral, 150, SpellType.Meteor, 50f, 12f, 45f);

        // EXTRA LIGHT SPELL (1)
        CreateSpell(path, "Spell_Light_Blessing", "Holy Blessing", Side.Light, 60, SpellType.Buff, 2.0f, 5f, 25f);

        // EXTRA DARK SPELL (1)
        CreateSpell(path, "Spell_Dark_PlagueRain", "Plague Rain", Side.Dark, 140, SpellType.Meteor, 80f, 6f, 15f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✔ 12 Unique Spells Generated Successfully!");
    }

    private static void CreateSpell(string path, string id, string name, Side side, int cost, SpellType type, float power, float radius, float cooldown)
    {
        string assetPath = $"{path}/{id}.asset";
        SpellData data = AssetDatabase.LoadAssetAtPath<SpellData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<SpellData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.spellID = id;
        data.spellName = name;
        data.side = side;
        data.manaCost = cost;
        data.spellType = type;
        data.power = power;
        data.radius = radius;
        data.cooldown = cooldown;
        
        // Spell ID -> İkon dosya adı eşleştirmesi
        var spellIconMap = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Spell_Light_Meteor_1", "MeteorStrike" },
            { "Spell_Light_Meteor_2", "MeteorStrike" },
            { "Spell_Light_Shield", "DivineShield" },
            { "Spell_Light_Reinforce_1", "LightInitiation" },
            { "Spell_Light_Reinforce_2", "LightInitiation" },
            { "Spell_Light_Blessing", "LightInitiation" },
            { "Spell_Dark_Rift", "AbyssalRift" },
            { "Spell_Dark_Bloodlust", "Bloodlust" },
            { "Spell_Dark_Freeze", "ShadowFreeze" },
            { "Spell_Dark_PlagueRain", "PlagueRain" },
            { "Spell_Neutral_Gold", "BountifulStart" },
            { "Spell_Neutral_Earthquake", "Earthquake" },
        };

        // Önce haritadan bul
        if (spellIconMap.TryGetValue(id, out string spellIconName))
        {
            data.icon = FindIcon(spellIconName);
        }
        
        // Haritada yoksa veya bulunamadıysa isim bazlı dene
        if (data.icon == null)
        {
            string cleanName = name.Replace(" ", "");
            data.icon = FindIcon(cleanName);
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssetIfDirty(data);
    }

    private static void EnsureDirectory(string path, bool clear = false)
    {
        if (System.IO.Directory.Exists(path))
        {
            if (clear)
            {
                string[] files = System.IO.Directory.GetFiles(path);
                foreach (string f in files) System.IO.File.Delete(f);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            System.IO.Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
