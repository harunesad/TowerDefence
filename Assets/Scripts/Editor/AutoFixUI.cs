using UnityEditor;
using TowerDefence.Editor;

public class AutoFixUI
{
    [MenuItem("Tools/Fix Tower Click Blocking")]
    public static void Fix()
    {
        // 1. Tüm Veri Assetlarını (TowerData vb.) oluştur ve branşları (Specializations) bağla
        DataAssetGenerator.GenerateAllData();

        // 2. TowerUpgradeUI prefabını yeniden oluştur (Yeni Canvas, Ölçek ve Gizli panel ile)
        UIMasterPrefabCreator.CreateTowerUpgradeUIPrefab();
        
        // 3. Kule prefablarını bu yeni UI ile yapılandır
        DataAssetGenerator.ConfigureTowerPrefabs();
        
        // 4. Engine prefabını da güncelle (Manager değişiklikleri için)
        UIMasterPrefabCreator.CreateCoreEnginePrefab();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        UnityEngine.Debug.Log("✔ ALL SYSTEMS REPAIRED: Data branched, UI scaled, and Click Blocking fixed!");
    }
}
