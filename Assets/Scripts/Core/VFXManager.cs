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
            if (Instance == null)
            {
                Instance = this;
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            foreach (var item in poolConfig)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                for (int i = 0; i < item.initialAmount; i++)
                {
                    GameObject obj = Instantiate(item.prefab, transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                pools.Add(item.type, objectPool);
            }
        }

        public GameObject SpawnVFX(VFXType type, Vector3 position, Quaternion rotation)
        {
            if (pools.ContainsKey(type))
            {
                GameObject obj = null;
                if (pools[type].Count > 0)
                {
                    obj = pools[type].Dequeue();
                }
                else
                {
                    var config = poolConfig.Find(p => p.type == type);
                    obj = Instantiate(config.prefab, transform);
                }

                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }

            Debug.LogWarning($"VFX Type {type} not found in pools!");
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
            var config = poolConfig.Find(p => p.type == type);
            if (config == null || config.prefab == null)
            {
                Debug.LogWarning($"VFX Type {type} not found in pools!");
                return null;
            }
            return config.prefab;
        }

        public void ReturnVFX(VFXType type, GameObject obj)
        {
            obj.SetActive(false);
            if (pools.ContainsKey(type))
            {
                pools[type].Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
