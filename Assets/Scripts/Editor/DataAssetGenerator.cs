using UnityEngine;
using UnityEditor;
using TowerDefence.Data;
using TowerDefence.Core;
using TowerDefence.Combat;
using System.IO;

public class DataAssetGenerator : Editor
{
    [MenuItem("Tower Defence/Tools/Generate 10 Towers and 10 Units")]
    public static void GenerateDataAssets()
    {
        string towerPath = "Assets/Resources/Data/Towers";
        string unitPath = "Assets/Resources/Data/Units";

        EnsureDirectory(towerPath);
        EnsureDirectory(unitPath);

        GameObject baseTower = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/BaseTowerPrefab.prefab");
        GameObject baseUnit = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/BaseUnitPrefab.prefab");

        // --- TOWERS (5 Light, 5 Dark) ---
        // Light Towers
        CreateTower(towerPath, "Archer Tower", Side.Light, 100, 10, 1.5f, 5, 0, baseTower);
        CreateTower(towerPath, "Cannon Tower", Side.Light, 250, 40, 0.5f, 7, 3, baseTower);
        CreateTower(towerPath, "Mage Tower", Side.Light, 200, 20, 0.8f, 6, 0, baseTower, StatusEffectType.Slow, 2f, 0.5f);
        CreateTower(towerPath, "Ballista Tower", Side.Light, 450, 150, 0.3f, 12, 0, baseTower);
        CreateTower(towerPath, "Solar Prism", Side.Light, 300, 5, 5.0f, 8, 0, baseTower);

        // Dark Towers
        CreateTower(towerPath, "Dark Sentry", Side.Dark, 80, 8, 2.0f, 4, 0, baseTower);
        CreateTower(towerPath, "Void Obelisk", Side.Dark, 400, 120, 0.2f, 10, 5, baseTower);
        CreateTower(towerPath, "Poison Spitter", Side.Dark, 220, 15, 1.0f, 6, 0, baseTower, StatusEffectType.Poison, 5f, 2f);
        CreateTower(towerPath, "Bone Catapult", Side.Dark, 350, 60, 0.4f, 15, 4, baseTower);
        CreateTower(towerPath, "Soul Harvester", Side.Dark, 150, 12, 3.0f, 5, 0, baseTower);

        // --- UNITS (5 Light, 5 Dark) ---
        UnitData lightSwordsman = CreateUnit(unitPath, "Light Swordsman", Side.Light, 50, 100, 3f, 10, 1.5f, 1f, baseUnit);
        UnitData ironKnight = CreateUnit(unitPath, "Iron Knight", Side.Light, 150, 400, 1.5f, 25, 1.5f, 0.5f, baseUnit);
        UnitData holyScout = CreateUnit(unitPath, "Holy Scout", Side.Light, 40, 60, 6f, 5, 1.5f, 1.5f, baseUnit);
        UnitData shieldBearer = CreateUnit(unitPath, "Shield Bearer", Side.Light, 100, 600, 1.2f, 15, 1.2f, 0.6f, baseUnit);
        UnitData celestialArcher = CreateUnit(unitPath, "Celestial Archer", Side.Light, 80, 90, 3.5f, 20, 5.0f, 1.2f, baseUnit);
        
        UnitData shadowStalker = CreateUnit(unitPath, "Shadow Stalker", Side.Dark, 70, 80, 5f, 35, 1.2f, 1.2f, baseUnit);
        UnitData abyssalBehemoth = CreateUnit(unitPath, "Abyssal Behemoth", Side.Dark, 300, 1200, 1f, 60, 2f, 0.4f, baseUnit);
        UnitData plagueRunner = CreateUnit(unitPath, "Plague Runner", Side.Dark, 35, 50, 7f, 8, 1.0f, 1.8f, baseUnit);
        UnitData skeletonWarrior = CreateUnit(unitPath, "Skeleton Warrior", Side.Dark, 60, 150, 2.5f, 18, 1.5f, 0.9f, baseUnit);
        UnitData wraith = CreateUnit(unitPath, "Wraith", Side.Dark, 90, 70, 4f, 25, 6.0f, 1.0f, baseUnit);

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
        Debug.Log("5 Towers and 5 Units generated successfully!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path);
        }
    }

    private static TowerData CreateTower(string path, string name, Side side, int cost, float damage, float fireRate, float range, float explosion, GameObject prefab, StatusEffectType effect = StatusEffectType.None, float duration = 0, float power = 0)
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
        data.prefab = prefab;
        data.effectType = effect;
        data.effectDuration = duration;
        data.effectPower = power;
        // Dinamik Hedefleme Ataması (Light -> Dark (7), Dark -> Light (6))
        data.targetLayer = (side == Side.Light) ? (1 << 7) : (1 << 6);

        string safeName = name.Replace(" ", "_");
        AssetDatabase.CreateAsset(data, $"{path}/{safeName}.asset");
        return data;
    }

    private static UnitData CreateUnit(string path, string name, Side side, int cost, float health, float speed, float damage, float range, float rate, GameObject prefab)
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
        data.prefab = prefab;

        string safeName = name.Replace(" ", "_");
        AssetDatabase.CreateAsset(data, $"{path}/{safeName}.asset");
        return data;
    }
}
