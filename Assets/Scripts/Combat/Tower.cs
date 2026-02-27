using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    public class Tower : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private TowerData towerData;

        private float range;
        private float fireRate;
        private GameObject projectilePrefab;
        private Transform firePoint;
        private Side towerSide;
        private LayerMask targetLayer;

        private float fireCountdown = 0f;
        private Transform target;
        private bool isDisabled;
        private float disableTimer;

        public Side GetSide() => towerSide;
        public Transform GetCurrentTarget() => target;

        private void Awake()
        {
            if (towerData != null) Initialize(towerData);
        }

        private float damage;

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
            projectilePrefab = data.prefab;
            
            if (firePoint == null) firePoint = transform.Find("FirePoint");
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

            UpdateTarget();

            if (target == null) return;

            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }

            fireCountdown -= Time.deltaTime;
        }

        private void UpdateTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, targetLayer);
            float shortestDistance = Mathf.Infinity;
            GameObject nearestEnemy = null;

            foreach (Collider collider in colliders)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, collider.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = collider.gameObject;
                }
            }

            if (nearestEnemy != null && shortestDistance <= range)
            {
                target = nearestEnemy.transform;
            }
            else
            {
                target = null;
            }
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
            currentLevel++;
            
            // Seviye başına %20 artış (Basit yazılım mantığı)
            float levelBonus = 1f + (currentLevel - 1) * 0.2f;
            
            float rangeMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.RangeBonus, towerSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.DamageBonus, towerSide);

            range = towerData.range * rangeMult * levelBonus;
            damage = towerData.damage * damageMult * levelBonus;

            Debug.Log($"{towerData.towerName} upgraded to Level {currentLevel}! New Damage: {damage}");
        }

        public int GetLevel() => currentLevel;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
