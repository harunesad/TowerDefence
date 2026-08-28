using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    public class BarracksTower : Tower
    {
        [Header("Barracks Settings")]
        public UnitData soldierData;
        public int soldierCount = 3;
        public float respawnDelay = 10f;
        public float rallyRadius = 15f;
        public GameObject rallyIndicatorPrefab;

        private List<Soldier> activeSoldiers = new List<Soldier>();
        private Vector3 currentRallyPoint;
        private GameObject currentIndicator;
        private float baseRespawnDelay;
        private float soldierStatMultiplier = 1f;
        private float respawnTimer;
        
        [Header("Range Visual")]
        private LineRenderer rangeLine;
        private Color rangeColor = new Color(0.2f, 0.8f, 1f, 0.6f); // Turkuaz/Mavi parlak çizgi

        private void Awake()
        {
            baseRespawnDelay = respawnDelay;
            
            // Eğer inspector/kod tarafında değer girilmişse onu kullan
            if (rallyRadius > 0.1f)
            {
                // Mevcut rallyRadius değerini koru
            }
            else if (towerData != null && towerData.range > 0.1f)
            {
                rallyRadius = towerData.range;
            }
            else
            {
                rallyRadius = 30f; // Güvenli fallback
            }

            SetupRangeIndicator();
        }
    
        // Menzili elle de artırabilmen için (Hızlı müdahale)
        public void IncreaseRallyRadius(float amount)
        {
            rallyRadius += amount;
            SetRangeVisible(true); // Ölçeği tazelemesi için
        }

        private void SetupRangeIndicator()
        {
            GameObject go = new GameObject("RallyRangeLine");
            go.transform.SetParent(transform);
            
            rangeLine = go.AddComponent<LineRenderer>();
            rangeLine.startWidth = 0.4f; 
            rangeLine.endWidth = 0.4f;
            rangeLine.positionCount = 51;
            rangeLine.useWorldSpace = true; // Kule ölçeğinden bağımsız dünyada çiz
            rangeLine.loop = true;
            rangeLine.alignment = LineAlignment.TransformZ;
            
            rangeLine.material = new Material(Shader.Find("Sprites/Default"));
            rangeLine.startColor = rangeColor;
            rangeLine.endColor = rangeColor;

            rangeLine.gameObject.SetActive(false); 
        }

        private void DrawCircle()
        {
            if (rangeLine == null) return;

            float angle = 0f;
            Vector3 center = transform.position;
            center.y = 0.2f; // Zeminden hafif yukarıda

            for (int i = 0; i < 51; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * rallyRadius;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * rallyRadius;
                rangeLine.SetPosition(i, center + new Vector3(x, 0, z));
                angle += (360f / 50);
            }
        }

        public override void SetRangeVisible(bool visible)
        {
            if (rangeLine != null)
            {
                if (visible) DrawCircle();
                rangeLine.gameObject.SetActive(visible);
            }
            if (currentIndicator != null)
            {
                currentIndicator.SetActive(visible);
            }
        }

        private void Start()
        {
            currentRallyPoint = FindNearestPathInRange();

            // İşaretçiyi oluştur ama gizli başlat (SetRangeVisible ile menü açılınca görünecek)
            if (currentIndicator == null && rallyIndicatorPrefab != null)
            {
                currentIndicator = Instantiate(rallyIndicatorPrefab, currentRallyPoint, Quaternion.Euler(90, 0, 0), transform);
            }
            if (currentIndicator != null)
            {
                currentIndicator.transform.position = currentRallyPoint;
                currentIndicator.SetActive(false);

                // SpriteRenderer rengini yeşil yap (materyali değiştirmeden)
                var sr = currentIndicator.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.green;
                }
            }

            SpawnInitialSoldiers();
        }

        public override void Upgrade()
        {
            if (currentLevel >= 3) return;

            currentLevel++;

            // Kışla tarzı geliştirmeler:
            // 1. Asker Sayısı Artışı
            soldierCount++; 

            // 2. Canlanma Süresi Azalışı (Her seviyede %15 daha hızlı)
            respawnDelay = baseRespawnDelay * (1f - (currentLevel - 1) * 0.15f);

            // 3. Asker Gücü Artışı (Seviye başına %20 daha fazla can/hasar)
            soldierStatMultiplier = 1f + (currentLevel - 1) * 0.2f;

            Debug.Log($"[Barracks] Upgraded to Level {currentLevel}! Soldiers: {soldierCount}, Respawn: {respawnDelay:F1}s, Power: x{soldierStatMultiplier}");
        }

        private void Update()
        {
            // Tower base'in Update'ini (ateş etme) engellemek için base.Update() ÇAĞIRMIYORUZ
            // Kışla kulesi ateş etmez, asker yönetir.

            // Ölen askerleri kontrol et ve respawn başlat
            for (int i = activeSoldiers.Count - 1; i >= 0; i--)
            {
                if (activeSoldiers[i] == null || activeSoldiers[i].IsDead)
                {
                    activeSoldiers.RemoveAt(i);
                }
            }

            if (activeSoldiers.Count < soldierCount)
            {
                respawnTimer += Time.deltaTime;
                if (respawnTimer >= respawnDelay)
                {
                    SpawnSoldier();
                    respawnTimer = 0;
                }
            }
        }

        private void SpawnInitialSoldiers()
        {
            for (int i = 0; i < soldierCount; i++)
            {
                SpawnSoldier();
            }
        }

        private void SpawnSoldier()
        {
            if (soldierData == null || soldierData.prefab == null)
            {
                Debug.LogWarning($"[Barracks] SoldierData or Prefab missing on {gameObject.name}");
                return;
            }

            // Kulenin etrafında rastgele bir noktada doğur
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 1.5f;
            spawnPos.y = transform.position.y;

            GameObject soldierGO = Instantiate(soldierData.prefab, spawnPos, Quaternion.identity);
            Soldier soldier = soldierGO.GetComponent<Soldier>();
            
            if (soldier == null)
            {
                // Eğer prefab'da Soldier scripti yoksa ekle (Veya Unit'i Soldier'a çevir)
                // Genelde prefab'da olması tercih edilir.
                soldier = soldierGO.AddComponent<Soldier>();
            }

            soldier.Initialize(soldierData, soldierStatMultiplier);
            soldier.SetBarracks(this);
            
            // Rally point etrafında küçük bir ofset ver (üst üste binmesinler)
            Vector3 offset = Random.insideUnitSphere * 0.8f;
            offset.y = 0;
            soldier.SetRallyPoint(currentRallyPoint + offset);

            activeSoldiers.Add(soldier);
        }

        private Vector3 FindNearestPathInRange()
        {
            float searchRange = rallyRadius > 0.1f ? rallyRadius : 30f;
            int pathLayerId = LayerMask.NameToLayer("Path");

            PathWaypoints[] paths = FindObjectsByType<PathWaypoints>(FindObjectsSortMode.None);
            if (paths.Length > 0)
            {
                Vector3 nearestWp = transform.position;
                float minDistWp = float.MaxValue;
                foreach (var path in paths)
                {
                    var wps = path.GetWaypoints();
                    for (int i = 0; i < wps.Count; i++)
                    {
                        float d = Vector3.Distance(transform.position, wps[i].position);
                        if (d < minDistWp && d <= searchRange)
                        {
                            minDistWp = d;
                            nearestWp = wps[i].position;
                        }
                    }
                }
                if (minDistWp < float.MaxValue)
                    return nearestWp;
            }

            int pathLayerMask = pathLayerId >= 0 ? 1 << pathLayerId : 0;
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRange, pathLayerMask, QueryTriggerInteraction.Collide);
            if (hits.Length == 0)
                return firePoint != null ? firePoint.position : transform.position;

            Vector3 nearest = hits[0].ClosestPoint(transform.position);
            float minDist = Vector3.Distance(transform.position, nearest);
            for (int i = 1; i < hits.Length; i++)
            {
                Vector3 cp = hits[i].ClosestPoint(transform.position);
                float d = Vector3.Distance(transform.position, cp);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = cp;
                }
            }
            return nearest;
        }

        public void SetRallyPoint(Vector3 newPoint)
        {
            currentRallyPoint = newPoint;

            // Görsel İşaretçiyi Güncelle
            if (currentIndicator == null && rallyIndicatorPrefab != null)
            {
                currentIndicator = Instantiate(rallyIndicatorPrefab, currentRallyPoint, Quaternion.Euler(90, 0, 0), transform);
            }
            
            if (currentIndicator != null)
            {
                currentIndicator.transform.position = currentRallyPoint;
                currentIndicator.transform.rotation = Quaternion.Euler(90, 0, 0);

                // SpriteRenderer rengini yeşil yap (materyali değiştirmeden)
                var sr = currentIndicator.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.green;
                }
            }

            foreach (var soldier in activeSoldiers)
            {
                if (soldier != null)
                {
                    Vector3 offset = Random.insideUnitSphere * 0.8f;
                    offset.y = 0;
                    soldier.SetRallyPoint(currentRallyPoint + offset);
                }
            }
        }

        private void OnDestroy()
        {
            // Askerleri temizle
            foreach (var soldier in activeSoldiers)
            {
                if (soldier != null)
                {
                    Destroy(soldier.gameObject);
                }
            }
            activeSoldiers.Clear();

            // İşaretçiyi temizle
            if (currentIndicator != null)
            {
                Destroy(currentIndicator);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentRallyPoint, 0.5f);
            Gizmos.DrawLine(transform.position, currentRallyPoint);
        }
    }
}
