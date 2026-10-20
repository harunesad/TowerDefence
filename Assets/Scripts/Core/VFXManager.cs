using UnityEngine;
using System.Collections.Generic;
using TowerDefence.VFX;

namespace TowerDefence.Core
{
    public enum VFXType
    {
        LightImpact,
        DarkImpact,
        HealingAura,
        CorruptionPulse,
        UnitSpawn,
        UnitDeath,
        SlowEffect,
        SpellThunderstrike,
        SpellRift,
        SpellEarthquake,
        SpellPlagueRain,
        MuzzleFlashLight,
        MuzzleFlashDark,
        EconomyGold,
        EconomySoul,
        UpgradeSparkle,
        None
    }

    [System.Serializable]
    public class VFXPoolItem
    {
        public VFXType type;
        public GameObject prefab;
        public int initialAmount = 5;
    }

    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [SerializeField] private List<VFXPoolItem> poolConfig;
        private Dictionary<VFXType, Queue<GameObject>> pools = new Dictionary<VFXType, Queue<GameObject>>();

        private void Awake()
        {
            enabled = true;
            if (Instance == null)
            {
                Instance = this;
                InitializePools();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            if (poolConfig == null) poolConfig = new List<VFXPoolItem>();

            foreach (var item in poolConfig)
            {
                if (item == null || item.prefab == null) continue;

                if (!pools.ContainsKey(item.type))
                {
                    pools.Add(item.type, new Queue<GameObject>());
                }

                for (int i = 0; i < item.initialAmount; i++)
                {
                    GameObject obj = Instantiate(item.prefab, transform);
                    obj.SetActive(false);
                    pools[item.type].Enqueue(obj);
                }
            }
        }

        public GameObject SpawnVFX(VFXType type, Vector3 position, Quaternion rotation)
        {
            if (type == VFXType.None) return null;

            if (!pools.ContainsKey(type))
            {
                pools[type] = new Queue<GameObject>();
            }

            GameObject obj = null;
            while (pools[type].Count > 0)
            {
                obj = pools[type].Dequeue();
                if (obj != null) break;
            }

            if (obj == null)
            {
                GameObject prefab = GetVFXPrefab(type);
                if (prefab != null)
                {
                    obj = Instantiate(prefab, transform);
                }
            }

            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);

                // Eski parçacık kalıntılarını temizle ve baştan başlat (Çift patlama/hayalet efekt sorununu engeller)
                var pSystems = obj.GetComponentsInChildren<ParticleSystem>(true);
                for (int i = 0; i < pSystems.Length; i++)
                {
                    pSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    pSystems[i].Play(true);
                }

                return obj;
            }

            Debug.LogWarning($"[VFXManager] VFX Type {type} prefab not found!");
            return null;
        }

        public GameObject SpawnVFX(VFXType type, Transform parent, Vector3 localPosition, Quaternion localRotation, float destroyDelay)
        {
            var prefab = GetVFXPrefab(type);
            if (prefab == null) return null;

            GameObject obj = Instantiate(prefab, parent);
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = localRotation;
            obj.SetActive(true);
            Destroy(obj, destroyDelay);
            return obj;
        }

        public GameObject GetVFXPrefab(VFXType type)
        {
            if (type == VFXType.None) return null;

            if (poolConfig != null)
            {
                var config = poolConfig.Find(p => p.type == type);
                if (config != null && config.prefab != null)
                {
                    return config.prefab;
                }
            }

#if UNITY_EDITOR
            string path = $"Assets/Prefabs/VFX/VFX_{type}.prefab";
            GameObject loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (loaded != null)
            {
                if (poolConfig == null) poolConfig = new List<VFXPoolItem>();
                var existing = poolConfig.Find(p => p.type == type);
                if (existing == null)
                {
                    poolConfig.Add(new VFXPoolItem { type = type, prefab = loaded, initialAmount = 5 });
                }
                else
                {
                    existing.prefab = loaded;
                }
                return loaded;
            }
#endif
            return null;
        }

        public void ReturnVFX(VFXType type, GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            if (!pools.ContainsKey(type))
            {
                pools[type] = new Queue<GameObject>();
            }
            pools[type].Enqueue(obj);
        }
    }
}
