using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Core;
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
        private readonly System.Collections.Generic.List<float> respawnReadyAt = new System.Collections.Generic.List<float>();
        
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

            for (int i = 0; i < 51; i++)
            {
                float x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * rallyRadius;
                float z = center.z + Mathf.Cos(Mathf.Deg2Rad * angle) * rallyRadius;
                rangeLine.SetPosition(i, new Vector3(x, GroundHeight(x, z), z));
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
                if (visible) PlaceRallyIndicator();
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
            PlaceRallyIndicator();
            if (currentIndicator != null)
                currentIndicator.SetActive(false);

            SpawnInitialSoldiers();
        }

        public override bool Upgrade()
        {
            if (currentLevel >= 3) return false;

            float previousPower = soldierStatMultiplier;
            currentLevel++;

            soldierCount++;
            respawnDelay = baseRespawnDelay * (1f - (currentLevel - 1) * 0.15f);
            soldierStatMultiplier = 1f + (currentLevel - 1) * 0.5f;

            for (int i = activeSoldiers.Count - 1; i >= 0; i--)
            {
                Soldier soldier = activeSoldiers[i];
                if (soldier != null && !soldier.IsDead)
                    soldier.MatchBarracksPower(soldierStatMultiplier);
            }

            SpawnSoldier();

            Debug.Log($"[Barracks] Upgraded to Level {currentLevel}! Soldiers: {soldierCount}, Respawn: {respawnDelay:F1}s, Power: x{soldierStatMultiplier} (was x{previousPower})");
            return true;
        }

        private void Update()
        {
            // Tower base'in Update'ini (ateş etme) engellemek için base.Update() ÇAĞIRMIYORUZ
            // Kışla kulesi ateş etmez, asker yönetir.
            if (GameManager.Instance != null && GameManager.Instance.HasMatchEnded()) return;
            if (IsDisabled)
            {
                TickDisable();
                return;
            }

            // Ölen askerleri kontrol et ve respawn başlat
            for (int i = activeSoldiers.Count - 1; i >= 0; i--)
            {
                if (activeSoldiers[i] == null || activeSoldiers[i].IsDead)
                {
                    activeSoldiers.RemoveAt(i);
                    respawnReadyAt.Add(Time.time + respawnDelay);
                }
            }

            if (PhaseManager.Instance != null && PhaseManager.Instance.GetCurrentPhase() == GamePhase.Combat)
            {
                for (int i = respawnReadyAt.Count - 1; i >= 0; i--)
                {
                    if (activeSoldiers.Count >= soldierCount)
                    {
                        respawnReadyAt.RemoveAt(i);
                        continue;
                    }
                    if (Time.time < respawnReadyAt[i]) continue;
                    if (!SpawnSoldier()) continue;
                    respawnReadyAt.RemoveAt(i);
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

        private bool SpawnSoldier()
        {
            if (soldierData == null || soldierData.prefab == null)
            {
                Debug.LogWarning($"[Barracks] SoldierData or Prefab missing on {gameObject.name}");
                return false;
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
            soldier.SetAppliedStatMultiplier(soldierStatMultiplier);
            soldier.SetBarracks(this);
            
            // Rally point etrafında küçük bir ofset ver (üst üste binmesinler)
            Vector3 offset = Random.insideUnitSphere * 0.8f;
            offset.y = 0;
            soldier.SetRallyPoint(currentRallyPoint + offset);

            activeSoldiers.Add(soldier);
            return true;
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
                    var wps = path.GetPathPoints();
                    for (int i = 0; i < wps.Count; i++)
                    {
                        float d = Vector3.Distance(transform.position, wps[i]);
                        if (d < minDistWp && d <= searchRange)
                        {
                            minDistWp = d;
                            nearestWp = wps[i];
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

        private void PlaceRallyIndicator()
        {
            if (currentIndicator == null) return;
            float y = GroundHeight(currentRallyPoint.x, currentRallyPoint.z);
            currentIndicator.transform.position = new Vector3(currentRallyPoint.x, y, currentRallyPoint.z);
            currentIndicator.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        public void SetRallyPoint(Vector3 newPoint)
        {
            currentRallyPoint = newPoint;

            // Görsel İşaretçiyi Güncelle
            if (currentIndicator == null && rallyIndicatorPrefab != null)
            {
                currentIndicator = Instantiate(rallyIndicatorPrefab, currentRallyPoint, Quaternion.Euler(90, 0, 0), transform);
            }

            PlaceRallyIndicator();

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
