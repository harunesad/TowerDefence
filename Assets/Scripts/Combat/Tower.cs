using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using TowerDefence.Data;
using TowerDefence.Grid;

namespace TowerDefence.Combat
{
    public enum TargetingPriority
    {
        First,
        Last,
        Strongest,
        Weakest
    }

    public class Tower : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private TowerData towerData;

        public TowerData GetTowerData() => towerData;
        public int CurrentLevel => currentLevel;
        public List<TowerData> GetSpecializations() => towerData != null ? towerData.specializations : null;

        private float range;
        private float fireRate;
        private GameObject projectilePrefab;
        private Transform firePoint;
        private Side towerSide;
        private LayerMask targetLayer;

        [Header("Targeting")]
        [SerializeField] private TargetingPriority currentPriority = TargetingPriority.First;
        public TargetingPriority GetTargetingPriority() => currentPriority;

        public void CycleTargetingPriority()
        {
            currentPriority = (TargetingPriority)(((int)currentPriority + 1) % 4);
            Debug.Log($"Targeting Priority changed to: {currentPriority}");
        }

        [Header("Rotation")]
        [SerializeField] private Transform partToRotate;
        [SerializeField] private float rotationSpeed = 10f;

        private float fireCountdown = 0f;
        private Transform target;
        private bool isDisabled;
        private float disableTimer;

        private TowerSlot mySlot;
        public void SetSlot(TowerSlot slot) => mySlot = slot;
        public TowerSlot GetSlot() => mySlot;

        public Side GetSide() => towerSide;
        public Transform GetCurrentTarget() => target;

        // --- HEALTH SYSTEM ---
        private float currentHealth;
        private float maxHealth;
        private bool isDead;
        private TowerDefence.UI.HealthBarUI healthBar;

        public bool IsDead => isDead;

        public void TakeDamage(float amount)
        {
            if (isDead) return;

            currentHealth -= amount;
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"[TOWER] {towerData.towerName} destroyed!");

            // VFX
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);

            // Slotu serbest bırak
            if (mySlot != null) mySlot.ClearSlot();

            Destroy(gameObject);
        }

        private void Awake()
        {
            if (towerData != null) Initialize(towerData);
        }

        private float damage;

        // --- AURA BUFF SYSTEM ---
        private float auraDamageMultiplier = 1f;
        private float auraFireRateMultiplier = 1f;
        private bool isAuraBuffed = false;
        private Color originalEmissionColor = Color.black;
        private static readonly Color auraGlowColor = new Color(0.3f, 0.8f, 1f); // Turkuaz parlaklık

        public bool IsAuraBuffed => isAuraBuffed;

        /// <summary>Aura Tower tarafından çağrılır. Buff çarpanlarını uygular.</summary>
        public void ApplyAuraBuff(float damageBonus, float fireRateBonus)
        {
            if (isAuraBuffed) return; // Zaten bufflu, tekrar uygulama
            isAuraBuffed = true;
            auraDamageMultiplier = 1f + damageBonus;
            auraFireRateMultiplier = 1f + fireRateBonus;
            damage *= auraDamageMultiplier;
            fireRate *= auraFireRateMultiplier;
            SetGlow(true);
        }

        /// <summary>Aura alanından çıkıldığında buff kaldırılır.</summary>
        public void RemoveAuraBuff()
        {
            if (!isAuraBuffed) return;
            isAuraBuffed = false;
            damage /= auraDamageMultiplier;
            fireRate /= auraFireRateMultiplier;
            auraDamageMultiplier = 1f;
            auraFireRateMultiplier = 1f;
            SetGlow(false);
        }

        private void SetGlow(bool enabled)
        {
            // Sahnedeki tüm Renderer'lara emission uygula
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in rend.materials)
                {
                    if (enabled)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", auraGlowColor * 0.6f);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.black);
                    }
                }
            }
        }


        public void Initialize(TowerData data)
        {
            towerData = data;
            towerSide = data.side;

            // Meta-Gelişim Çarpanlarını Uygula
            float rangeMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.RangeBonus, towerSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.DamageBonus, towerSide);
            float speedMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.SpeedBonus, towerSide); // Ateş hızı için kullanılabilir

            range = data.range * rangeMult;
            fireRate = data.fireRate * speedMult;
            damage = data.damage * damageMult;
            targetLayer = data.targetLayer;
            projectilePrefab = data.projectilePrefab;

            // Health Initialization
            maxHealth = data.health > 0 ? data.health : 100f; // Fallback
            currentHealth = maxHealth;
            healthBar = GetComponentInChildren<TowerDefence.UI.HealthBarUI>();
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);
            
            if (firePoint == null) firePoint = transform.Find("FirePoint");
            
            // 3D Kuleler için Weapon nesnesini bul ve ona kilitlen
            if (partToRotate == null)
            {
                Transform visuals = transform.Find("Visuals");
                if (visuals != null)
                {
                    partToRotate = visuals.Find("Weapon");
                    // Eğer dursa firePoint'i de bunun altında ara
                    if (partToRotate != null)
                    {
                        firePoint = partToRotate.Find("FirePoint");
                    }
                }
            }

            if (firePoint == null) firePoint = transform;

            // Dinamik Hedefleme (Light kuleler Dark layer'ı (7), Dark kuleler Light layer'ı (6) hedefler)
            targetLayer = (towerSide == Side.Light) ? (1 << 7) : (1 << 6);
        }

        private void Update()
        {
            if (isDisabled)
            {
                disableTimer -= Time.deltaTime;
                if (disableTimer <= 0) isDisabled = false;
                return;
            }

            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            // Aura kuleleri ateş etmez, sadece buff verir
            if (fireRate <= 0f) return;

            UpdateTarget();

            if (target != null)
            {
                LockOnTarget();
                if (fireCountdown <= 0f)
                {
                    Shoot();
                    fireCountdown = 1f / fireRate;
                }
            }

            fireCountdown -= Time.deltaTime;
        }

        private void LockOnTarget()
        {
            if (partToRotate == null) return;

            Vector3 dir = target.position - transform.position;
            if (dir == Vector3.zero) return;

            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f); // Sadece Y ekseninde döner
        }

        private void UpdateTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, targetLayer);
            if (colliders.Length == 0)
            {
                target = null;
                return;
            }

            GameObject selectedEnemy = null;

            switch (currentPriority)
            {
                case TargetingPriority.First:
                    float maxProgress = -1f;
                    foreach (Collider col in colliders)
                    {
                        Unit unit = col.GetComponent<Unit>();
                        if (unit == null) continue;
                        
                        // First: En yüksek waypoint index'i + waypoint'e en yakın olan
                        float progress = CalculateUnitProgress(unit);
                        if (progress > maxProgress)
                        {
                            maxProgress = progress;
                            selectedEnemy = col.gameObject;
                        }
                    }
                    break;

                case TargetingPriority.Last:
                    float minProgress = Mathf.Infinity;
                    foreach (Collider col in colliders)
                    {
                        Unit unit = col.GetComponent<Unit>();
                        if (unit == null) continue;

                        float progress = CalculateUnitProgress(unit);
                        if (progress < minProgress)
                        {
                            minProgress = progress;
                            selectedEnemy = col.gameObject;
                        }
                    }
                    break;

                case TargetingPriority.Strongest:
                    float maxHealth = -1f;
                    foreach (Collider col in colliders)
                    {
                        Unit unit = col.GetComponent<Unit>();
                        if (unit == null) continue;

                        // Unit sınıfında currentHealth private, ama UnitData'dan veya başka yoldan çekebiliriz
                        // Şimdilik Unit scriptine bir getter eklediğimizi varsayalım veya can barından alalım
                        // Ama UnitData'dan çekmek yerine Unit instance'ından çekmek daha mantıklı.
                        // Unit scriptine GetHealth() eklemem gerekecek.
                        float h = unit.GetComponent<Unit>().GetHealth(); 
                        if (h > maxHealth)
                        {
                            maxHealth = h;
                            selectedEnemy = col.gameObject;
                        }
                    }
                    break;

                case TargetingPriority.Weakest:
                    float minHealth = Mathf.Infinity;
                    foreach (Collider col in colliders)
                    {
                        Unit unit = col.GetComponent<Unit>();
                        if (unit == null) continue;

                        float h = unit.GetHealth();
                        if (h < minHealth)
                        {
                            minHealth = h;
                            selectedEnemy = col.gameObject;
                        }
                    }
                    break;
            }

            if (selectedEnemy != null)
            {
                target = selectedEnemy.transform;
            }
            else
            {
                target = null;
            }
        }

        private float CalculateUnitProgress(Unit unit)
        {
            // Basit bir ilerleme puanı: Waypoint Index * 1000 + (Waypoint'e olan mesafe tersi)
            // Bu sayede daha ileri waypoint'teki her zaman "daha önde" sayılır.
            int wpIndex = unit.GetCurrentWaypointIndex();
            float distToNext = Vector3.Distance(unit.transform.position, unit.GetCurrentMoveTarget());
            
            return (wpIndex * 1000f) - distToNext;
        }

        private void Shoot()
        {
            GameObject projGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projectile = projGO.GetComponent<Projectile>();

            if (projectile != null)
            {
                VFXType impactType = towerSide == Side.Light ? VFXType.LightImpact : VFXType.DarkImpact;
                projectile.Initialize(damage, towerData.explosionRadius, impactType, towerData.effectType, towerData.effectDuration, towerData.effectPower);
                
                projectile.Seek(target);

                if (AudioManager.Instance != null && towerData.shootSFX != null)
                {
                    AudioManager.Instance.PlaySFX(towerData.shootSFX);
                }
            }
        }

        private int currentLevel = 1;

        public void Disable(float duration)
        {
            isDisabled = true;
            disableTimer = duration;
            Debug.Log($"{gameObject.name} disabled for {duration} seconds.");
        }

        public void Upgrade()
        {
            if (currentLevel >= 3 && GetSpecializations() != null && GetSpecializations().Count > 0)
            {
                Debug.LogWarning("Tower: Level 3 reached. Use Specialize() instead of normal Upgrade.");
                return;
            }

            currentLevel++;
            
            // Seviye başına %20 artış (Basit yazılım mantığı)
            float levelBonus = 1f + (currentLevel - 1) * 0.2f;
            
            float rangeMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.RangeBonus, towerSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.DamageBonus, towerSide);

            range = towerData.range * rangeMult * levelBonus;
            damage = towerData.damage * damageMult * levelBonus;

            Debug.Log($"{towerData.towerName} upgraded to Level {currentLevel}! New Damage: {damage}");
        }

        public void Specialize(TowerData newData)
        {
            if (newData == null) return;
            
            // Veriyi değiştir ve yeniden başlat
            towerData = newData;
            currentLevel++; // Bir üst seviyeye geç (1->2, 2->3 vb.)
            
            Initialize(newData);
            
            Debug.Log($"{gameObject.name} specialized into {newData.towerName}!");
        }

        public int GetLevel() => currentLevel;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
