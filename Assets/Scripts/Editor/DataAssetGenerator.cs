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
            ("Hero_Light_Paladin", "Iron Paladin",      Side.Light, "Iron_Knight",       650, 0.85f, 35, 1.6f, 0.9f, 600, false, HeroAbilityType.HolyShield,     "Holy Shield",     "Heals when health drops below 35%.", 16f, 1f, 0f),
            ("Hero_Light_Archon",  "Celestial Archon",  Side.Light, "Celestial_Archer",  420, 1.1f,  45, 2.2f, 1.1f, 750, false, HeroAbilityType.ArrowRain,      "Arrow Rain",      "Damages all enemies in an area.", 12f, 1f, 4f),
            ("Hero_Light_Scout",   "Holy Scout",        Side.Light, "Holy_Scout",        380, 1.25f, 32, 1.7f, 1.3f, 550, false, HeroAbilityType.SwiftStrike,    "Swift Strike",    "Deals a burst of bonus damage.", 10f, 1f, 0f),
            ("Hero_Light_Bulwark", "Shield Bearer",     Side.Light, "Shield_Bearer",     720, 0.8f,  30, 1.5f, 0.85f,650, false, HeroAbilityType.FortifyTaunt,   "Fortify",         "Taunts nearby enemies.", 18f, 1f, 5f),
            ("Hero_Light_Solar",   "Sun Knight",        Side.Light, "Iron_Knight",       480, 1.0f,  38, 1.9f, 1.0f, 800, false, HeroAbilityType.SolarSmite,     "Solar Smite",     "Holy explosion around the hero.", 13f, 1f, 3.5f),
            ("Hero_Dark_Vampire",  "Vampire Lord",      Side.Dark,  "Shadow_Stalker",    450, 1.2f,  35, 1.5f, 1.2f, 600, false, HeroAbilityType.LifeDrain,      "Life Drain",      "Steals health from the target.", 11f, 1f, 0f),
            ("Hero_Dark_Reaper",   "Soul Reaper",       Side.Dark,  "Wraith",            380, 1.3f,  42, 1.7f, 1.3f, 750, false, HeroAbilityType.SoulExecute,    "Soul Execute",    "Executes wounded enemies.", 15f, 1f, 0f),
            ("Hero_Dark_Behemoth", "Abyssal Lord",      Side.Dark,  "Abyssal_Behemoth",  700, 0.75f, 50, 1.4f, 0.8f, 900, false, HeroAbilityType.GroundSlam,     "Ground Slam",     "Slams the ground for heavy AoE damage.", 14f, 1f, 4f),
            ("Hero_Dark_Plague",   "Plague Herald",     Side.Dark,  "Plague_Runner",     400, 1.15f, 36, 1.6f, 1.15f,550, false, HeroAbilityType.PlagueCloud,    "Plague Cloud",    "Poisons enemies in an area.", 12f, 1f, 4f),
            ("Hero_Dark_Bone",     "Bone Commander",    Side.Dark,  "Skeleton_Warrior",  520, 0.95f, 34, 1.5f, 1.0f, 650, false, HeroAbilityType.BoneArmor,      "Bone Armor",      "Reinforces the hero with bone plating.", 16f, 1f, 0f),
            ("Hero_Dark_Stalker",  "Night Stalker",     Side.Dark,  "Shadow_Stalker",    430, 1.35f, 40, 1.6f, 1.25f,800, false, HeroAbilityType.ShadowStep,     "Shadow Step",     "Teleports behind the target.", 13f, 1f, 0f),
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

            ConfigureHeroPrefab(targetPfb, unitData);

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

    private static Sprite FindIcon(string iconName)
    {
        string[] guids = AssetDatabase.FindAssets(iconName + " t:Sprite", new[] { "Assets/Data/Icons" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path).Equals(iconName, System.StringComparison.OrdinalIgnoreCase))
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        return null;
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

        EditorUtility.SetDirty(data);
        return data;
    }

    private static void ConfigureHeroPrefab(string prefabPath, UnitData data)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null) return;

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
        so.ApplyModifiedProperties();

        // 2. Health Bar UI Setup (Same as normal unit setup but offset for hero visual distinction if desired)
        Transform hbTransform = root.transform.Find("HealthBarCanvas");
        HealthBarUI hbScript = null;
        float targetY = 6.0f; // Above unit head

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
            bgGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

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
        }

        // Link Hero to its HealthBar
        var heroSo = new SerializedObject(heroComp);
        heroSo.FindProperty("healthBar").objectReferenceValue = hbScript;
        heroSo.ApplyModifiedProperties();

        FixVisualModelGroundOffset(root);

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


        // --- PASSIVE SKILLS (5 Total) ---
        CreateSkill(path, "Skill_Passive_Neutral_Bountiful", "Bountiful Start", "Start every level with +50 Gold", 200, UpgradeType.CurrencyStartBonus, 1.5f, Side.Neutral);
        
        SkillNodeData archer1 = CreateSkill(path, "Skill_Passive_Light_Archer_1", "Archer Potency I", "Light Archer damage +10%", 150, UpgradeType.TowerDamageBonus, 1.1f, Side.Light);
        CreateSkill(path, "Skill_Passive_Light_Archer_2", "Archer Potency II", "Light Archer damage +20%", 300, UpgradeType.TowerDamageBonus, 1.2f, Side.Light, archer1);
        
        SkillNodeData haste1 = CreateSkill(path, "Skill_Passive_Dark_Haste_1", "Dark Haste I", "Dark unit speed +10%", 150, UpgradeType.UnitSpeedBonus, 1.1f, Side.Dark);
        CreateSkill(path, "Skill_Passive_Dark_Haste_2", "Dark Haste II", "Dark unit speed +20%", 300, UpgradeType.UnitSpeedBonus, 1.2f, Side.Dark, haste1);

        // --- ACTIVE SPELLS UNLOCKS (9 Total) ---
        string spellPath = "Assets/Data/Spells";
        
        // Light Spells (3)
        var m1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Meteor_1.asset");
        if (m1 != null) CreateSkill(path, "Skill_Unlock_Light_Meteor_1", "Meteor Strike", m1.description, 100, UpgradeType.UnlockSpell, 1, Side.Light, null, m1);

        var m2 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Meteor_2.asset");
        if (m2 != null) CreateSkill(path, "Skill_Unlock_Light_Meteor_2", "Elite Meteor", m2.description, 400, UpgradeType.UnlockSpell, 1, Side.Light, null, m2);

        var s1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Shield.asset");
        if (s1 != null) CreateSkill(path, "Skill_Unlock_Light_Shield", "Divine Shield", s1.description, 350, UpgradeType.UnlockSpell, 1, Side.Light, null, s1);

        // Dark Spells (3)
        var r1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_Rift.asset");
        if (r1 != null) CreateSkill(path, "Skill_Unlock_Dark_Rift", "Abyssal Rift", r1.description, 400, UpgradeType.UnlockSpell, 1, Side.Dark, null, r1);

        var b1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_Bloodlust.asset");
        if (b1 != null) CreateSkill(path, "Skill_Unlock_Dark_Bloodlust", "Bloodlust", b1.description, 350, UpgradeType.UnlockSpell, 1, Side.Dark, null, b1);

        var f1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Dark_Freeze.asset");
        if (f1 != null) CreateSkill(path, "Skill_Unlock_Dark_Freeze", "Shadow Freeze", f1.description, 450, UpgradeType.UnlockSpell, 1, Side.Dark, null, f1);

        // Reinforcements (2)
        var re1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Reinforce_1.asset");
        if (re1 != null) CreateSkill(path, "Skill_Unlock_Light_Reinforce_1", "Reinforcements", re1.description, 100, UpgradeType.UnlockSpell, 1, Side.Light, null, re1);

        var re2 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Light_Reinforce_2.asset");
        if (re2 != null) CreateSkill(path, "Skill_Unlock_Light_Reinforce_2", "Royal Guards", re2.description, 450, UpgradeType.UnlockSpell, 1, Side.Light, null, re2);

        // Neutral Spells (1)
        var g1 = AssetDatabase.LoadAssetAtPath<SpellData>(spellPath + "/Spell_Neutral_Gold.asset");
        if (g1 != null) CreateSkill(path, "Skill_Unlock_Neutral_Gold", "Gold Rush", g1.description, 500, UpgradeType.UnlockSpell, 1, Side.Neutral, null, g1);

        AssetDatabase.SaveAssets();

        // MetaProgressionManager'a tüm yetenekleri bağla
        PopulateManagerSkills(path);
    }

    private static void PopulateManagerSkills(string skillPath)
    {
        // Manager'ı bul (Genelde _Engine prefabı içindedir veya sahnede)
        MetaProgressionManager manager = GameObject.FindObjectOfType<MetaProgressionManager>();
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

    private static SkillNodeData CreateSkill(string path, string id, string name, string desc, int cost, UpgradeType type, float mult, Side side, SkillNodeData req = null, SpellData grant = null)
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
        skill.karmaCost = cost;
        skill.upgradeType = type;
        skill.multiplier = mult;
        skill.side = side;
        skill.grantedSpell = grant;
        skill.requiredSkills = new System.Collections.Generic.List<SkillNodeData>();
        if (req != null) skill.requiredSkills.Add(req);
        
        // Icon bulmaya çalış
        skill.icon = FindIcon($"{id}_Icon");

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
        if (hbTransform != null)
        {
            hbTransform.localPosition = new Vector3(0, 6.0f, 0); // Can barı (+2.5f)
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
            bg.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.5f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            fill.transform.SetParent(bg.transform);
            fill.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            fill.GetComponent<RectTransform>().sizeDelta = new Vector2(1.5f, 0.2f);
            UnityEngine.UI.Image fillImg = fill.GetComponent<UnityEngine.UI.Image>();
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
            bgGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

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
        }

        // 3. Unit -> HealthBar Link
        var unitSo = new SerializedObject(unit);
        unitSo.FindProperty("healthBar").objectReferenceValue = hbScript;

        if (data.projectilePrefab != null)
        {
            EditorPrefabBuilder.EnsureRangedUnitFirePoints(root);
            Transform fp = EditorPrefabBuilder.GetFirstFirePointTransform(root);
            if (fp != null)
                unitSo.FindProperty("firePoint").objectReferenceValue = fp;
        }

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
        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        switch (layout)
        {
            case 0: return 8; // 4 spawner boss maps
            case 7: 
            case 8: return 6; // 3 spawner maps
            case 3: 
            case 5: 
            case 6: return 5; // 2 spawner maps
            case 1: 
            case 2: 
            case 4: 
            case 9: 
            default: return 4; // 1 spawner maps
        }
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
        int layout = lvlIdx % 10;
        
        if (lvlIdx == 50) layout = 0; // Level 50 is the ultimate 4-corner layout

        switch (layout)
        {
            case 1: // Diagonal/Zigzag
                if (lvlIdx < 15)
                {
                    paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(40, 0, -40) } });
                }
                else if (lvlIdx < 30)
                {
                    paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(40, 0, 40), new Vector3(40, 0, -40) } });
                }
                else
                {
                    paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(-40, 0, 10), new Vector3(10, 0, 10), new Vector3(10, 0, -20), new Vector3(40, 0, -20), new Vector3(40, 0, -40) } });
                }
                break;

            case 2: // Level 2: L-shape
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-30, 0, 35), new Vector3(-30, 0, -30), new Vector3(30, 0, -30) } });
                break;

            case 3: // Level 3: Z-shape / "2"-shape
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { 
                    new Vector3(-30, 0, 35), new Vector3(10, 0, 35), new Vector3(10, 0, 0), 
                    new Vector3(-15, 0, 0), new Vector3(-15, 0, -35), new Vector3(30, 0, -35) 
                } });
                break;

            case 4: // Level 4: Y-shape
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-35, 0, 35), new Vector3(-20, 0, 5), new Vector3(0, 0, -15), new Vector3(0, 0, -35) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(35, 0, 35), new Vector3(20, 0, 5), new Vector3(0, 0, -15), new Vector3(0, 0, -35) } });
                break;

            case 5: // Level 5: Spiral G-loop
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { 
                    new Vector3(-35, 0, 35), new Vector3(-25, 0, 15), new Vector3(-25, 0, -25), 
                    new Vector3(15, 0, -25), new Vector3(15, 0, 15), new Vector3(-10, 0, 15), 
                    new Vector3(-10, 0, 0), new Vector3(10, 0, 0), new Vector3(25, 0, -10), new Vector3(30, 0, -35) 
                } });
                break;

            case 6: // Level 6: T-shape
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 25), new Vector3(0, 0, 25), new Vector3(0, 0, -35) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(40, 0, 25), new Vector3(0, 0, 25), new Vector3(0, 0, -35) } });
                break;

            case 7: // Level 7: S-curve S
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { 
                    new Vector3(0, 0, 35), new Vector3(0, 0, 20), new Vector3(30, 0, 15), 
                    new Vector3(-25, 0, 0), new Vector3(25, 0, -15), new Vector3(0, 0, -20), new Vector3(0, 0, -35) 
                } });
                break;

            case 8: // Trident Merge (3 spawners)
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(-20, 0, 10), new Vector3(0, 0, -10), new Vector3(0, 0, -40) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(0, 0, 40), new Vector3(0, 0, -40) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(40, 0, 40), new Vector3(20, 0, 10), new Vector3(0, 0, -10), new Vector3(0, 0, -40) } });
                break;

            case 9: // Helix or overlap
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(40, 0, 40), new Vector3(-40, 0, -40), new Vector3(40, 0, -40) } });
                break;

            case 0: // 4 Corners to Center
            default:
                paths.Add(new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-40, 0, 40), new Vector3(-20, 0, 20), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(40, 0, 40), new Vector3(20, 0, 20), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-40, 0, -40), new Vector3(-20, 0, -20), new Vector3(0, 0, 0) } });
                paths.Add(new LevelPath { spawnerIndex = 3, points = new List<Vector3> { new Vector3(40, 0, -40), new Vector3(20, 0, -20), new Vector3(0, 0, 0) } });
                break;
        }
        return paths;
    }

    private static List<Vector3> GetBasesForLevel(int lvlIdx)
    {
        List<Vector3> bases = new List<Vector3>();
        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        switch (layout)
        {
            case 1:
                if (lvlIdx < 41) bases.Add(new Vector3(40, 0, -40));
                else bases.Add(new Vector3(20, 0, 0));
                break;
            case 2:
                bases.Add(new Vector3(30, 0, -40));
                break;
            case 3:
                bases.Add(new Vector3(0, 0, -40));
                break;
            case 4:
                if (lvlIdx < 20) bases.Add(new Vector3(40, 0, 0));
                else bases.Add(new Vector3(40, 0, -40));
                break;
            case 5:
                bases.Add(new Vector3(0, 0, -40));
                break;
            case 6:
                bases.Add(new Vector3(0, 0, -40));
                break;
            case 7:
                bases.Add(new Vector3(0, 0, -40));
                break;
            case 8:
                bases.Add(new Vector3(0, 0, -40));
                break;
            case 9:
                bases.Add(new Vector3(40, 0, -40));
                break;
            case 0:
            default:
                bases.Add(new Vector3(0, 0, 0));
                break;
        }
        return bases;
    }

    private static List<Vector3> GetCustomSlotsForLevel(int lvlIdx)
    {
        List<Vector3> slots = new List<Vector3>();
        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        switch (layout)
        {
            case 1: // Level 1: Diagonal
                slots.Add(new Vector3(-15, 0, 25));
                slots.Add(new Vector3(-25, 0, 15));
                slots.Add(new Vector3(25, 0, -15));
                slots.Add(new Vector3(15, 0, -25));
                break;
            case 2: // Level 2: L-shape
                slots.Add(new Vector3(-45, 0, -5)); // Left of vertical
                slots.Add(new Vector3(-15, 0, 20)); // Right of vertical (upper)
                slots.Add(new Vector3(-15, 0, -5)); // Right of vertical (lower)
                slots.Add(new Vector3(-5, 0, -15)); // Above horizontal (left)
                slots.Add(new Vector3(15, 0, -15)); // Above horizontal (right)
                slots.Add(new Vector3(-5, 0, -45)); // Below horizontal (left)
                slots.Add(new Vector3(15, 0, -45)); // Below horizontal (right)
                break;
            case 3: // Level 3: Z-shape
                slots.Add(new Vector3(-30, 0, -15));
                slots.Add(new Vector3(-5, 0, 15));
                slots.Add(new Vector3(25, 0, 20));
                slots.Add(new Vector3(15, 0, -20));
                break;
            case 4: // Level 4: Y-shape
                slots.Add(new Vector3(-35, 0, 5));
                slots.Add(new Vector3(-15, 0, 20));
                slots.Add(new Vector3(-15, 0, -15));
                slots.Add(new Vector3(35, 0, 5));
                slots.Add(new Vector3(15, 0, 20));
                slots.Add(new Vector3(15, 0, -15));
                break;
            case 5: // Level 5: Loop / spiral
                slots.Add(new Vector3(-40, 0, 10));
                slots.Add(new Vector3(-40, 0, -15));
                slots.Add(new Vector3(-10, 0, 30));
                slots.Add(new Vector3(-15, 0, -15));
                slots.Add(new Vector3(0, 0, -35));
                slots.Add(new Vector3(35, 0, -5));
                break;
            case 6: // Level 6: T-shape
                slots.Add(new Vector3(-15, 0, 10));
                slots.Add(new Vector3(-15, 0, -15));
                slots.Add(new Vector3(15, 0, 10));
                slots.Add(new Vector3(15, 0, -15));
                break;
            case 7: // Level 7: S-curve S
                slots.Add(new Vector3(15, 0, 25));
                slots.Add(new Vector3(-20, 0, 10));
                slots.Add(new Vector3(15, 0, -15));
                slots.Add(new Vector3(-20, 0, -25));
                slots.Add(new Vector3(15, 0, -25));
                break;
            case 8: // Level 8: Crossover / DNA
                slots.Add(new Vector3(-15, 0, 25));
                slots.Add(new Vector3(15, 0, 25));
                slots.Add(new Vector3(-25, 0, -5));
                slots.Add(new Vector3(25, 0, -5));
                slots.Add(new Vector3(-15, 0, -35));
                slots.Add(new Vector3(15, 0, -35));
                break;
            case 9: // Level 9: Trident
                slots.Add(new Vector3(-20, 0, 25));
                slots.Add(new Vector3(-35, 0, 10));
                slots.Add(new Vector3(-15, 0, -5));
                slots.Add(new Vector3(-10, 0, -25));
                slots.Add(new Vector3(20, 0, 25));
                slots.Add(new Vector3(35, 0, 10));
                slots.Add(new Vector3(15, 0, -5));
                slots.Add(new Vector3(10, 0, -25));
                break;
            case 0: // Level 10: Loop / arrow split-merge
            default:
                slots.Add(new Vector3(-25, 0, 25));
                slots.Add(new Vector3(15, 0, 25));
                slots.Add(new Vector3(-30, 0, 0));
                slots.Add(new Vector3(0, 0, -10));
                slots.Add(new Vector3(-10, 0, -35));
                slots.Add(new Vector3(25, 0, -25));
                break;
        }
        return slots;
    }

    private static int GetSpawnerCountForLevel(int lvlIdx)
    {
        int layout = lvlIdx % 10;
        if (lvlIdx == 50) layout = 0;

        if (layout == 0) return 4;
        if (layout == 7 || layout == 8) return 3;
        if (layout == 3 || layout == 5 || layout == 6) return 2;
        return 1;
    }

    private static (string unit, int count)[] GetWaveComposition(int lvlIdx, int wave)
    {
        int multiplier = 1 + (lvlIdx / 10);
        int basicCount = 3 + wave * 2 * multiplier;
        int eliteCount = Mathf.Max(0, -2 + wave * multiplier);

        if (lvlIdx <= 12)
        {
            return new (string, int)[] {
                ("Skeleton_Warrior", basicCount),
                ("Light_Swordsman", basicCount / 2),
                ("Plague_Runner", basicCount / 3)
            };
        }
        else if (lvlIdx <= 25)
        {
            return new (string, int)[] {
                ("Skeleton_Warrior", basicCount),
                ("Shadow_Stalker", basicCount / 2),
                ("Iron_Knight", eliteCount)
            };
        }
        else if (lvlIdx <= 38)
        {
            return new (string, int)[] {
                ("Shield_Bearer", basicCount),
                ("Celestial_Archer", basicCount / 2),
                ("Wraith", eliteCount)
            };
        }
        else
        {
            return new (string, int)[] {
                ("Abyssal_Behemoth", eliteCount + 1),
                ("Wraith", basicCount / 2),
                ("Shadow_Stalker", basicCount)
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
        for (int i = 0; i < composition.Length; i++)
        {
            var item = composition[i];
            UnitData ud = AssetDatabase.LoadAssetAtPath<UnitData>($"Assets/Data/Units/{item.unit}.asset");
            if (ud != null)
            {
                wave.unitGroups.Add(new WaveUnitGroup {
                    unitData = ud,
                    count = item.count,
                    spawnInterval = 2f,
                    spawnerIndex = i % pathCount // Düşman gruplarını tüm yollara (spawner'lara) dengeli dağıt
                });
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
        level.difficulty = 1;
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

        // NOT: mapPrefab ataması elle yapılacak, otomatik arama kaldırıldı.

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

        // NEUTRAL SPELLS (1)
        CreateSpell(path, "Spell_Neutral_Gold", "Gold Rush", Side.Neutral, 0, SpellType.GoldBoost, 100f, 0f, 60f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✔ 9 Unique Spells Generated Successfully with New Naming!");
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
        
        // Icon bulmaya çalış
        data.icon = FindIcon($"{id}_Icon");

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
