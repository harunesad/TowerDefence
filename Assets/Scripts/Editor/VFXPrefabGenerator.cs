using UnityEngine;
using UnityEditor;
using TowerDefence.Core;
using System.Collections.Generic;

namespace TowerDefence.Editor
{
    /// <summary>
    /// Bu script, Asset Store'dan indirilen AAA kaliteli (Hovl Studio ve JMO Assets)
    /// prefabları otomatik olarak bularak bizim sisteme (PooledVFX) entegre eder.
    /// </summary>
    public static class VFXPrefabGenerator
    {
        private const string VFX_PREFAB_PATH = "Assets/Prefabs/VFX";

        private class VFXTweak
        {
            public float scale = 1f;
            public float fillMult = 1f;
            public float shapeRadius = 0f;
        }

        [MenuItem("Tower Defence/Generate/VFX Prefabs (Asset Store Integration)")]
        public static void GenerateVFXPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(VFX_PREFAB_PATH))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "VFX");
            }

            // Eşleştirmeler: Bizim enum adımız -> İndirilen paketteki orijinal Prefab adı
            var map = new Dictionary<string, string>()
            {
                { "VFX_LightImpact", "Holy hit" }, // Hovl - Kutsal ışık patlaması
                { "VFX_DarkImpact", "Red energy explosion" }, // Hovl - Karanlık enerji patlaması
                
                { "VFX_HealingAura", "Healing circle" }, // Hovl - Dönen şifa rünleri
                { "VFX_CorruptionPulse", "Plexus AoE" }, // Hovl - Alan yozlaşma puls dalgası
                { "VFX_UnitSpawn", "Teleport" }, // Hovl - Büyülü teleport ışık sütunu
                
                { "VFX_UnitDeath", "CFXR2 Souls Escape" }, // JMO - Ruhların göğe yükselmesi
                { "VFX_SlowEffect", "Freeze circle" }, // Hovl - Buz rün çemberi
                
                { "VFX_SpellThunderstrike", "Electro hit" }, // Hovl - Elektrik alan patlaması
                { "VFX_SpellRift", "Red energy explosion" }, // Hovl - Kızıl boyutsal yarık
                { "VFX_SpellEarthquake", "Ground AOE explosion" }, // Hovl - Güçlü zemin sarsıntısı
                { "VFX_SpellPlagueRain", "CFXR2 Poison Cloud" }, // JMO - Zehir bulutu
                
                { "VFX_MuzzleFlashLight", "CFXR Flash" }, // JMO
                { "VFX_MuzzleFlashDark", "CFXR Flash" }, // JMO
                
                { "VFX_EconomyGold", "Sparks explode yellow" }, // Hovl - Altın saçılma kıvılcımları
                { "VFX_EconomySoul", "CFXR2 Souls Escape" }, // JMO - Ruh kaçışı
                { "VFX_UpgradeSparkle", "Sparks explode yellow" } // Hovl - Yıldız/altın yükselme patlaması
            };

            // VFX bazında ölçek / doluluk / disk yayılımı düzeltmeleri (CSR her çalıştığında yeniden uygulanır)
            var vfxTweaks = new Dictionary<string, VFXTweak>
            {
                { "VFX_SpellThunderstrike", new VFXTweak { scale = 2.5f, fillMult = 2f, shapeRadius = 1.2f } },
                { "VFX_SpellEarthquake", new VFXTweak { scale = 2.0f, fillMult = 1.5f, shapeRadius = 1.5f } },
                { "VFX_SpellRift", new VFXTweak { scale = 2.0f, fillMult = 2f, shapeRadius = 1.2f } },
                { "VFX_UnitSpawn", new VFXTweak { scale = 1.2f, fillMult = 1.2f, shapeRadius = 1.0f } },
                { "VFX_UpgradeSparkle", new VFXTweak { scale = 1.5f, fillMult = 1.5f, shapeRadius = 1.0f } }
            };

            int successCount = 0;

            foreach (var kvp in map)
            {
                string targetName = kvp.Key;
                string sourceName = kvp.Value;

                // Asset veritabanında prefabı bul (tam isim eşleşmesini önceliklendir)
                string[] guids = AssetDatabase.FindAssets(sourceName + " t:Prefab");
                if (guids.Length == 0)
                {
                    Debug.LogWarning($"[VFX] Bulunamadı: '{sourceName}' (Hedef: {targetName}). Lütfen paketlerin import edildiğinden emin ol.");
                    continue;
                }

                string sourcePath = null;
                for (int i = 0; i < guids.Length; i++)
                {
                    string candidate = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (candidate.EndsWith("/" + sourceName + ".prefab", System.StringComparison.OrdinalIgnoreCase))
                    {
                        sourcePath = candidate;
                        break;
                    }
                }
                if (sourcePath == null) sourcePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                
                if (sourcePrefab == null) continue;

                // Sahnede geçici olarak yarat
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);

                // VFX tweak'lerini uygula (ölçek + doluluk + disk yayılımı)
                if (vfxTweaks.TryGetValue(targetName, out VFXTweak tweak))
                {
                    if (tweak.scale > 1f)
                        instance.transform.localScale = instance.transform.localScale * tweak.scale;

                    if (tweak.fillMult > 1f || tweak.shapeRadius > 0f)
                    {
                        var tweakPss = instance.GetComponentsInChildren<ParticleSystem>(true);
                        foreach (var tweakPs in tweakPss)
                        {
                            if (tweak.fillMult > 1f)
                            {
                                var emission = tweakPs.emission;
                                emission.rateOverTimeMultiplier *= tweak.fillMult;
                                int burstCount = emission.burstCount;
                                for (int i = 0; i < burstCount; i++)
                                {
                                    var burst = emission.GetBurst(i);
                                    burst.minCount = (short)(burst.minCount * tweak.fillMult);
                                    burst.maxCount = (short)(burst.maxCount * tweak.fillMult);
                                    emission.SetBurst(i, burst);
                                }
                                var tweakMain = tweakPs.main;
                                tweakMain.maxParticles = Mathf.Max(100, (int)(tweakMain.maxParticles * tweak.fillMult));
                            }

                            if (tweak.shapeRadius > 0f)
                            {
                                var shape = tweakPs.shape;
                                shape.radius = tweak.shapeRadius;
                            }
                        }
                    }
                }
                
                // PooledVFX bileşenini ekle 
                var pooled = instance.GetComponent<TowerDefence.VFX.PooledVFX>();
                if (pooled == null)
                {
                    pooled = instance.AddComponent<TowerDefence.VFX.PooledVFX>();
                }

                // Türü belirle
                SerializedObject so = new SerializedObject(pooled);
                
                string typeStr = targetName.Replace("VFX_", "");
                if (System.Enum.TryParse(typeStr, out VFXType vfxType))
                {
                    so.FindProperty("type").intValue = (int)vfxType;
                }

                // Yaşam süresini (lifetime) hesapla
                float maxDuration = 3f; // Default
                var ps = instance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    maxDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                }
                else
                {
                    // Alt objelerdeki particle systemleri kontrol et
                    var pss = instance.GetComponentsInChildren<ParticleSystem>();
                    foreach(var p in pss)
                    {
                        float dur = p.main.duration + p.main.startLifetime.constantMax;
                        if (dur > maxDuration) maxDuration = dur;
                    }
                }

                so.FindProperty("lifeTime").floatValue = maxDuration + 0.5f;
                so.FindProperty("autoReturnByTime").boolValue = true;
                
                // Bütün efektler (Loop edenler dahil) belirli bir süre sonra havuza dönmeli
                // Çünkü oyun mimarimizde kalıcı aura takibi yapılmıyor (Büyüler ve Kahraman yetenekleri anlık).
                // so.FindProperty("autoReturnByTime").boolValue = false; // İPTAL EDİLDİ
                
                so.ApplyModifiedProperties();

                // Prefab olarak kaydet
                string savePath = $"{VFX_PREFAB_PATH}/{targetName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, savePath);
                GameObject.DestroyImmediate(instance);
                
                successCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[VFX] {successCount} / {map.Count} AAA kalite efekt başarıyla sisteme entegre edildi!");
        }
    }
}
