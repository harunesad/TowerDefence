using UnityEngine;
using System.Collections.Generic;

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
        SlowEffect
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
                    // Havuz boşsa yeni oluştur (Dinamik büyüme)
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

        public void ReturnVFX(VFXType type, GameObject obj)
        {
            obj.SetActive(false);
            if (pools.ContainsKey(type))
            {
                pools[type].Enqueue(obj);
            }
            else
            {
                Destroy(obj); // Eğer havuz yoksa (beklenmedik durum)
            }
        }
    }
}
