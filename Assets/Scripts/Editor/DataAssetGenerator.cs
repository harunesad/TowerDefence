using UnityEngine;
using UnityEditor;
using TowerDefence.Data;
using TowerDefence.Core;
using TowerDefence.Combat;
using System.IO;

public class DataAssetGenerator : Editor
{
    [MenuItem("Tower Defence/🚀 Setup/📦 Generate All Data Assets")]
    public static void GenerateAllDataAssets()
    {
        string towerPath = "Assets/Data/Towers";
        string unitPath = "Assets/Data/Units";

        EnsureDirectory(towerPath);
        EnsureDirectory(unitPath);

        EnsureDirectory(towerPath);
        EnsureDirectory(unitPath);

        // --- TOWERS (5 Light, 5 Dark) ---
        // Light Towers
        CreateTower(towerPath, "Archer Tower", Side.Light, 100, 10, 1.5f, 5, 0);
        CreateTower(towerPath, "Cannon Tower", Side.Light, 250, 40, 0.5f, 7, 3);
        CreateTower(towerPath, "Mage Tower", Side.Light, 200, 20, 0.8f, 6, 0, StatusEffectType.Slow, 2f, 0.5f);
        CreateTower(towerPath, "Ballista Tower", Side.Light, 450, 150, 0.3f, 12, 0);
        CreateTower(towerPath, "Solar Prism", Side.Light, 300, 5, 5.0f, 8, 0);

        // Dark Towers
        CreateTower(towerPath, "Dark Sentry", Side.Dark, 80, 8, 2.0f, 4, 0);
        CreateTower(towerPath, "Void Obelisk", Side.Dark, 400, 120, 0.2f, 10, 5);
        CreateTower(towerPath, "Poison Spitter", Side.Dark, 220, 15, 1.0f, 6, 0, StatusEffectType.Poison, 5f, 2f);
        CreateTower(towerPath, "Bone Catapult", Side.Dark, 350, 60, 0.4f, 15, 4);
        CreateTower(towerPath, "Soul Harvester", Side.Dark, 150, 12, 3.0f, 5, 0);

        // --- UNITS (5 Light, 5 Dark) ---
        // --- UNITS (5 Light, 5 Dark) ---
        // Yavaş: 0.5 - 0.7 | Orta: 0.9 - 1.1 | Hızlı: 1.3 - 1.5
        UnitData lightSwordsman = CreateUnit(unitPath, "Light Swordsman", Side.Light, 50, 100, 0.9f, 10, 1.5f, 1f);
        UnitData ironKnight = CreateUnit(unitPath, "Iron Knight", Side.Light, 150, 400, 0.6f, 25, 1.5f, 0.5f);
        UnitData holyScout = CreateUnit(unitPath, "Holy Scout", Side.Light, 40, 60, 1.4f, 5, 1.5f, 1.5f);
        UnitData shieldBearer = CreateUnit(unitPath, "Shield Bearer", Side.Light, 100, 600, 0.5f, 15, 1.2f, 0.6f);
        UnitData celestialArcher = CreateUnit(unitPath, "Celestial Archer", Side.Light, 80, 90, 1.1f, 20, 5.0f, 1.2f);
        
        UnitData shadowStalker = CreateUnit(unitPath, "Shadow Stalker", Side.Dark, 70, 80, 1.3f, 35, 1.2f, 1.2f);
        UnitData abyssalBehemoth = CreateUnit(unitPath, "Abyssal Behemoth", Side.Dark, 300, 1200, 0.5f, 60, 2f, 0.4f);
        UnitData plagueRunner = CreateUnit(unitPath, "Plague Runner", Side.Dark, 35, 50, 1.5f, 8, 1.0f, 1.8f);
        UnitData skeletonWarrior = CreateUnit(unitPath, "Skeleton Warrior", Side.Dark, 60, 150, 0.8f, 18, 1.5f, 0.9f);
        UnitData wraith = CreateUnit(unitPath, "Wraith", Side.Dark, 90, 70, 1.2f, 25, 6.0f, 1.0f);

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

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✔ All Tower and Unit Data Assets generated successfully!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path);
        }
    }

    private static TowerData CreateTower(string path, string name, Side side, int cost, float damage, float fireRate, float range, float explosion, StatusEffectType effect = StatusEffectType.None, float duration = 0, float power = 0)
    {
        TowerData data = ScriptableObject.CreateInstance<TowerData>();
        data.towerName = name;
        data.side = side;
        data.cost = cost;
        data.upgradeCost = cost;
        data.damage = damage;
        data.fireRate = fireRate;
        data.range = range;
        data.explosionRadius = explosion;
        data.effectType = effect;
        data.effectDuration = duration;
        data.effectPower = power;
        data.targetLayer = (side == Side.Light) ? (1 << 7) : (1 << 6);

        // Otomatik Varlık Bulma
        string safeName = name.Replace(" ", "_");
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

        AssetDatabase.CreateAsset(data, $"{path}/{safeName}.asset");
        return data;
    }

    private static UnitData CreateUnit(string path, string name, Side side, int cost, float health, float speed, float damage, float range, float rate)
    {
        UnitData data = ScriptableObject.CreateInstance<UnitData>();
        data.unitName = name;
        data.side = side;
        data.spawnCost = cost;
        data.maxHealth = health;
        data.moveSpeed = speed;
        data.attackDamage = damage;
        data.attackRange = range;
        data.attackRate = rate;

        // Otomatik Varlık Bulma
        string safeName = name.Replace(" ", "_");
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

        AssetDatabase.CreateAsset(data, $"{path}/{safeName}.asset");
        return data;
    }
}
