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

        // --- TOWERS (5 Light, 5 Dark) ---
        // Light Towers
        CreateTower(towerPath, "Archer Tower", Side.Light, 100, 10, 1.5f, 12, 0, true);
        CreateTower(towerPath, "Cannon Tower", Side.Light, 250, 40, 0.5f, 12, 3, true);
        CreateTower(towerPath, "Mage Tower", Side.Light, 200, 20, 0.8f, 11, 0, true, StatusEffectType.Slow, 2f, 0.5f);
        CreateTower(towerPath, "Ballista Tower", Side.Light, 350, 150, 0.3f, 17, 0, true); 
        CreateTower(towerPath, "Solar Prism", Side.Light, 280, 0, 0f, 13, 0, true); 

        // Dark Towers
        CreateTower(towerPath, "Dark Sentry", Side.Dark, 80, 8, 2.0f, 11, 0, true);
        CreateTower(towerPath, "Void Obelisk", Side.Dark, 380, 0, 0f, 15, 0, true); 
        CreateTower(towerPath, "Poison Spitter", Side.Dark, 220, 15, 1.0f, 11, 0, true, StatusEffectType.Poison, 5f, 2f);
        CreateTower(towerPath, "Bone Catapult", Side.Dark, 320, 60, 0.5f, 18, 4, true); 
        CreateTower(towerPath, "Soul Harvester", Side.Dark, 150, 12, 3.0f, 11, 0, true);

        // --- UNITS (5 Light, 5 Dark) ---
        // --- UNITS (5 Light, 5 Dark) ---
        // Yavaş: 0.5 - 0.7 | Orta: 0.9 - 1.1 | Hızlı: 1.3 - 1.5
        // Unit Creation (cost, reward, hp, speed, dmg, range, rate)
        UnitData lightSwordsman = CreateUnit(unitPath, "Light Swordsman", Side.Light, 50, 20, 100, 0.9f, 10, 1.5f, 1f);
        UnitData ironKnight = CreateUnit(unitPath, "Iron Knight", Side.Light, 150, 60, 400, 0.6f, 25, 1.5f, 0.5f);
        UnitData holyScout = CreateUnit(unitPath, "Holy Scout", Side.Light, 40, 25, 60, 1.4f, 5, 1.5f, 1.5f);
        UnitData shieldBearer = CreateUnit(unitPath, "Shield Bearer", Side.Light, 100, 45, 600, 0.5f, 15, 1.2f, 0.6f);
        UnitData celestialArcher = CreateUnit(unitPath, "Celestial Archer", Side.Light, 80, 35, 90, 1.1f, 20, 5.0f, 1.2f);
        
        UnitData shadowStalker = CreateUnit(unitPath, "Shadow Stalker", Side.Dark, 70, 30, 80, 1.3f, 35, 1.2f, 1.2f);
        UnitData abyssalBehemoth = CreateUnit(unitPath, "Abyssal Behemoth", Side.Dark, 300, 120, 1200, 0.5f, 60, 2f, 0.4f);
        UnitData plagueRunner = CreateUnit(unitPath, "Plague Runner", Side.Dark, 35, 20, 50, 1.5f, 8, 1.0f, 1.8f);
        UnitData skeletonWarrior = CreateUnit(unitPath, "Skeleton Warrior", Side.Dark, 60, 25, 150, 0.8f, 18, 1.5f, 0.9f);
        UnitData wraith = CreateUnit(unitPath, "Wraith", Side.Dark, 90, 40, 70, 1.2f, 25, 6.0f, 1.0f);

        // --- Dinamik Eşleşmeleri Kur (Side-Swapping için) ---
        lightSwordsman.enemyCounterpart = shadowStalker;
        shadowStalker.enemyCounterpart = lightSwordsman;

        ironKnight.enemyCounterpart = abyssalBehemoth;
        abyssalBehemoth.enemyCounterpart = ironKnight;

        holyScout.enemyCounterpart = plagueRunner;
        plagueRunner.enemyCounterpart = holyScout;

        shieldBearer.enemyCounterpart = skeletonWarrior;
        skeletonWarrior.enemyCounterpart = shieldBearer;

        celestialArcher.enemyCounterpart = wraith;
        wraith.enemyCounterpart = celestialArcher;

        // Değişiklikleri kaydetmek için nesneleri "kirli" (dirty) olarak işaretle
        EditorUtility.SetDirty(lightSwordsman);
        EditorUtility.SetDirty(shadowStalker);
        EditorUtility.SetDirty(ironKnight);
        EditorUtility.SetDirty(abyssalBehemoth);
        EditorUtility.SetDirty(holyScout);
        EditorUtility.SetDirty(plagueRunner);
        EditorUtility.SetDirty(shieldBearer);
        EditorUtility.SetDirty(skeletonWarrior);
        EditorUtility.SetDirty(celestialArcher);
        EditorUtility.SetDirty(wraith);

        // --- TAMİR VE YAPILANDIRMA ZİNCİRİ (Chain Repair) ---
        // Bu bölüm, oluşturulan tüm verilerin birbirine ve prefablara kusursuz bağlanmasını sağlar
        LinkEliteTowerSpecializations(towerPath);
        LinkTowerCounterparts(towerPath);
        FixMissingTowerReferences();
        FixMissingUnitReferences();
        ConfigureAuraTowers(towerPath);
        ConfigureTowerPrefabs(towerPath);
        ConfigureUnitPrefabs(unitPath);
        PopulateManagerSkills("Assets/Data/Skills");
        LinkUnitProjectiles();
        GenerateLevels(); // [NEW] Generate Level and Wave data

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✔ ALL DATA AND PREFABS GENERATED, LINKED, AND REPAIRED SUCCESSFULLY!");
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
        if (!AssetDatabase.IsValidFolder(path))
        {
            System.IO.Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        // Archer Damage I -> II
        SkillNodeData archerDmg1 = CreateSkill(path, "Archer_Potency_1", "Archer Potency I", "Increases Light Archer damage by 10%", 100, UpgradeType.DamageBonus, 1.1f, Side.Light);
        SkillNodeData archerDmg2 = CreateSkill(path, "Archer_Potency_2", "Archer Potency II", "Further increases Light Archer damage by 15%", 250, UpgradeType.DamageBonus, 1.15f, Side.Light, archerDmg1);

        // Dark Speed I -> II
        SkillNodeData darkSpd1 = CreateSkill(path, "Dark_Haste_1", "Dark Haste I", "Increases Dark unit move speed by 10%", 150, UpgradeType.SpeedBonus, 1.1f, Side.Dark);
        SkillNodeData darkSpd2 = CreateSkill(path, "Dark_Haste_2", "Dark Haste II", "Further increases Dark unit move speed by 15%", 300, UpgradeType.SpeedBonus, 1.15f, Side.Dark, darkSpd1);

        // General Economy
        CreateSkill(path, "Bountiful_Start", "Bountiful Start", "Start every level with +50 Gold", 200, UpgradeType.CurrencyStartBonus, 1.5f, Side.Neutral);

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

    private static SkillNodeData CreateSkill(string path, string id, string name, string desc, int cost, UpgradeType type, float mult, Side side, SkillNodeData req = null)
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
        skill.requiredSkills = new System.Collections.Generic.List<SkillNodeData>();
        if (req != null) skill.requiredSkills.Add(req);

        EditorUtility.SetDirty(skill);
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

        AssetDatabase.SaveAssets();
    }

    private static void LinkUnitProjectiles()
    {
        string unitPath = "Assets/Data/Units";
        string projPath = "Assets/Prefabs/Gameplay/Projectiles";

        // Celestial Archer -> Archer_Arrow
        UnitData archer = AssetDatabase.LoadAssetAtPath<UnitData>($"{unitPath}/Celestial_Archer.asset");
        if (archer != null)
        {
            archer.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{projPath}/Archer_Arrow.prefab");
            EditorUtility.SetDirty(archer);
        }

        // Wraith -> Mage_Bolt (Veya Dark_Pulse/Void_Missile)
        UnitData wraith = AssetDatabase.LoadAssetAtPath<UnitData>($"{unitPath}/Wraith.asset");
        if (wraith != null)
        {
            wraith.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{projPath}/Mage_Bolt.prefab");
            EditorUtility.SetDirty(wraith);
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
        data.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Data/Icons/{safeName}_Icon.png");

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
        data.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Data/Icons/{safeName}_Icon.png");

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

        // 1. Tower Script
        Tower tower = root.GetComponent<Tower>();
        if (tower == null) tower = root.AddComponent<Tower>();
        
        var so = new SerializedObject(tower);
        so.FindProperty("towerData").objectReferenceValue = data;
        so.ApplyModifiedProperties();

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
        if (uiTransform == null)
        {
            GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/TowerUpgradeUI.prefab");
            if (uiPrefab != null)
            {
                GameObject uiInstance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab, root.transform);
                uiInstance.name = "TowerUpgradeUI";
                uiInstance.transform.localPosition = new Vector3(0, 4f, 0); // Kule tepesi
            }
        }

        // 5. Health Bar UI (Düşmanlardaki sistemin aynısı)
        Transform hbTransform = root.transform.Find("HealthBarCanvas");
        if (hbTransform == null)
        {
            GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(TowerDefence.UI.HealthBarUI));
            canvasGo.transform.SetParent(root.transform);
            canvasGo.transform.localPosition = new Vector3(0, 3.5f, 0); 
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
                Sprite found = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Data/Icons/{safeName}_Icon.png");
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

            // 3. Projectile Ataması (Güçlendirildi: Miras desteği)
            if (data.projectilePrefab == null)
            {
                string projName = "Archer_Arrow";
                string n = data.towerName.ToLower();
                if (n.Contains("mage") || n.Contains("soul") || n.Contains("void") || n.Contains("prism")) projName = "Mage_Bolt";
                else if (n.Contains("cannon") || n.Contains("catapult") || n.Contains("bone")) projName = "Cannon_Ball";
                
                GameObject foundProj = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Projectiles/{projName}.prefab");
                if (foundProj == null)
                {
                    TowerData baseTower = FindBaseTowerOfSpecialization(data);
                    if (baseTower != null && baseTower.projectilePrefab != null)
                    {
                        foundProj = baseTower.projectilePrefab;
                        Debug.Log($"[REPAIR] {data.towerName} used base projectile from {baseTower.towerName}");
                    }
                }

                if (foundProj == null) foundProj = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Gameplay/Projectiles/Archer_Arrow.prefab");
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
                Sprite found = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Data/Icons/{safeName}_Icon.png");
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
        string[] unitGuids = AssetDatabase.FindAssets("t:UnitData", new[] { unitPath });
        foreach (string guid in unitGuids)
        {
            UnitData ud = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid));
            if (ud != null && ud.prefab != null)
            {
                ConfigureSingleUnitPrefab(ud.prefab, ud);
            }
        }
        AssetDatabase.Refresh();
    }

    private static void ConfigureSingleUnitPrefab(GameObject prefab, UnitData data)
    {
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(path)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        
        // 0. Layer Setup (Kritik: Kuleler bu katmanlara (6-7) göre hedefler)
        int targetLayer = (data.side == Side.Light) ? 6 : 7;
        SetLayerRecursive(root, targetLayer);

        // 1. Unit Script (Zaten vardır ama garantiye alalım)
        Unit unit = root.GetComponent<Unit>();
        if (unit == null) unit = root.AddComponent<Unit>();

        // 2. Health Bar UI Setup
        Transform hbTransform = root.transform.Find("HealthBarCanvas");
        HealthBarUI hbScript = null;

        if (hbTransform == null)
        {
            GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(HealthBarUI));
            canvasGo.transform.SetParent(root.transform);
            float targetY = (root.name.Contains("Behemoth")) ? 1.8f : 1.2f; // Karakterin kafasının üzerine çıkarıldı
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
            float targetY = (root.name.Contains("Behemoth")) ? 1.8f : 1.2f;
            hbTransform.localPosition = new Vector3(0, targetY, 0);
            hbScript = hbTransform.GetComponent<HealthBarUI>();
        }

        // 3. Unit -> HealthBar Link
        var unitSo = new SerializedObject(unit);
        unitSo.FindProperty("healthBar").objectReferenceValue = hbScript;
        unitSo.ApplyModifiedProperties();

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

        // --- LEVEL 1: Orman Yolu (3 Merging Paths Style) ---
        string lvl1Dir = $"{levelPath}/Level1";
        EnsureDirectory(lvl1Dir);
        
        List<WaveData> lvl1Waves = new List<WaveData>();
        // 3 giriş için 3 ayrı grup (Dengeli Dağılım)
        lvl1Waves.Add(CreateWave(lvl1Dir, "Wave1", new (string, int)[] { ("Skeleton_Warrior", 3), ("Light_Swordsman", 2), ("Plague_Runner", 2) }, 3));
        lvl1Waves.Add(CreateWave(lvl1Dir, "Wave2", new (string, int)[] { ("Skeleton_Warrior", 8), ("Light_Swordsman", 5), ("Iron_Knight", 2) }, 3));
        lvl1Waves.Add(CreateWave(lvl1Dir, "Wave3", new (string, int)[] { ("Abyssal_Behemoth", 1), ("Shadow_Stalker", 5), ("Wraith", 3) }, 3));

        // Çizimdeki (2. resim) 3 girişli yapı - Daha temiz birleşme için koordinat revizyonu
        List<LevelPath> lvl1Paths = new List<LevelPath> {
            new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-25, 0, 20), new Vector3(-5, 0, 20), new Vector3(5, 0, 0), new Vector3(25, 0, 0) }},
            new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(-25, 0, 0), new Vector3(25, 0, 0) }},
            new LevelPath { spawnerIndex = 2, points = new List<Vector3> { new Vector3(-25, 0, -20), new Vector3(-5, 0, -20), new Vector3(5, 0, 0), new Vector3(25, 0, 0) }}
        };
        List<Vector3> lvl1Bases = new List<Vector3> { new Vector3(25, 0, 0) };

        CreateLevel(lvl1Dir, "Level1", "Level 1: Triple Path", 250, 350, LevelTheme.Forest, lvl1Waves, lvl1Paths, lvl1Bases);

        // --- LEVEL 2: Çöl Karşılaşması (Split Paths & 2 Bases) ---
        string lvl2Dir = $"{levelPath}/Level2";
        EnsureDirectory(lvl2Dir);

        List<WaveData> lvl2Waves = new List<WaveData>();
        lvl2Waves.Add(CreateWave(lvl2Dir, "Wave1", new (string, int)[] { ("Iron_Knight", 5), ("Skeleton_Warrior", 10) }, 2));
        lvl2Waves.Add(CreateWave(lvl2Dir, "Wave2", new (string, int)[] { ("Shadow_Stalker", 8), ("Celestial_Archer", 5) }, 2));
        lvl2Waves.Add(CreateWave(lvl2Dir, "Wave3", new (string, int)[] { ("Plague_Runner", 12), ("Wraith", 4) }, 2));
        lvl2Waves.Add(CreateWave(lvl2Dir, "Wave4", new (string, int)[] { ("Abyssal_Behemoth", 2), ("Iron_Knight", 8) }, 2));

        List<LevelPath> lvl2Paths = new List<LevelPath> {
            new LevelPath { spawnerIndex = 0, points = new List<Vector3> { new Vector3(-20, 0, 20), new Vector3(5, 0, 25), new Vector3(20, 0, 25) }},
            new LevelPath { spawnerIndex = 1, points = new List<Vector3> { new Vector3(-20, 0, -20), new Vector3(5, 0, -25), new Vector3(20, 0, -25) }}
        };
        List<Vector3> lvl2Bases = new List<Vector3> { new Vector3(20, 0, 25), new Vector3(20, 0, -25) };

        CreateLevel(lvl2Dir, "Level2", "Level 2: Dusty Pass", 400, 500, LevelTheme.Desert, lvl2Waves, lvl2Paths, lvl2Bases);

        // --- LEVEL 3: Buzlu Labirent (Complex Winding) ---
        string lvl3Dir = $"{levelPath}/Level3";
        EnsureDirectory(lvl3Dir);

        List<WaveData> lvl3Waves = new List<WaveData>();
        lvl3Waves.Add(CreateWave(lvl3Dir, "Wave1", new (string, int)[] { ("Abyssal_Behemoth", 2), ("Wraith", 5), ("Shadow_Stalker", 5) }, 1));
        lvl3Waves.Add(CreateWave(lvl3Dir, "Wave2", new (string, int)[] { ("Shadow_Stalker", 10), ("Iron_Knight", 5), ("Wraith", 2) }, 1));
        lvl3Waves.Add(CreateWave(lvl3Dir, "Wave3", new (string, int)[] { ("Abyssal_Behemoth", 5), ("Skeleton_Warrior", 20), ("Wraith", 10) }, 1));
        lvl3Waves.Add(CreateWave(lvl3Dir, "Wave4", new (string, int)[] { ("Celestial_Archer", 15), ("Holy_Scout", 10) }, 1));
        lvl3Waves.Add(CreateWave(lvl3Dir, "Wave5", new (string, int)[] { ("Abyssal_Behemoth", 8), ("Iron_Knight", 15), ("Wraith", 15) }, 1));
        
        List<LevelPath> lvl3Paths = new List<LevelPath> {
            new LevelPath { spawnerIndex = 0, points = new List<Vector3> { 
                new Vector3(-40, 0, 30), new Vector3(-20, 0, 30), new Vector3(-20, 0, 10), 
                new Vector3(20, 0, 10), new Vector3(20, 0, -10), new Vector3(-20, 0, -10),
                new Vector3(-20, 0, -30), new Vector3(40, 0, -30) 
            }}
        };
        List<Vector3> lvl3Bases = new List<Vector3> { new Vector3(40, 0, -30) };

        CreateLevel(lvl3Dir, "Level3", "Level 3: Snow Labyrinth", 600, 700, LevelTheme.Snow, lvl3Waves, lvl3Paths, lvl3Bases);

        Debug.Log("✔ ALL LEVELS AND WAVES GENERATED SUCCESSFULLY!");
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

    private static void CreateLevel(string path, string id, string name, int startLight, int startDark, LevelTheme theme, List<WaveData> waves, List<LevelPath> paths, List<Vector3> bases)
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
        level.waves = waves;
        level.paths = paths;
        level.basePoints = bases;
        level.towerSlotCount = 15 + (waves.Count * 2); // Dalga sayısına göre slot artırımı
        level.sceneIndex = 2; // Default Gameplay Scene

        // Otomatik Harita Ataması (Senin dosya isimlendirme formatına uyarlandı)
        GameObject mapPfb = null;
        
        // 1. Level numarasını ayıkla (Level1 -> 1)
        string numStr = id.Replace("Level", "");
        string[] mapGuids = new string[0];

        if (int.TryParse(numStr, out int num))
        {
            // Senin formatın: "Level_1__" şeklinde başlıyor
            mapGuids = AssetDatabase.FindAssets($"Level_{num}__ t:GameObject");
        }

        // 2. Eğer hala bulunamadıysa ID ile genel arama yap
        if (mapGuids.Length == 0)
        {
            mapGuids = AssetDatabase.FindAssets($"{id} t:GameObject", new[] { "Assets/Maps" });
        }

        if (mapGuids.Length > 0)
        {
            string mapPath = AssetDatabase.GUIDToAssetPath(mapGuids[0]);
            mapPfb = AssetDatabase.LoadAssetAtPath<GameObject>(mapPath);
        }

        level.mapPrefab = mapPfb;
        if (mapPfb != null) Debug.Log($"✔ [LevelGen] Harita atandı: {id} -> {mapPfb.name}");
        else Debug.LogWarning($"⚠ [LevelGen] DİKKAT: {id} ({name}) için harita bulunamadı! Lütfen Assets/Maps/ altını kontrol edin.");

        EditorUtility.SetDirty(level);
    }
}
