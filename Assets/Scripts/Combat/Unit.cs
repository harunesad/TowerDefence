using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Unit : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private UnitData unitData;

        private float maxHealth;
        private float moveSpeed;
        private float attackDamage;
        private float attackRange;
        private float attackRate;
        private Side unitSide;
        
        private float currentHealth;
        private NavMeshAgent agent;
        private float nextAttackTime;
        private bool isDead;

        // Status effects
        private List<StatusEffect> activeEffects = new List<StatusEffect>();

        public bool IsDead => isDead;
        public Side GetSide() => unitSide;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (unitData != null) Initialize(unitData);
        }

        public void Initialize(UnitData data)
        {
            unitData = data;
            unitSide = data.side;

            // Meta-Gelişim Çarpanlarını Uygula
            float healthMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.HealthBonus, unitSide);
            float speedMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.SpeedBonus, unitSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.DamageBonus, unitSide);

            maxHealth = data.maxHealth * healthMult;
            moveSpeed = data.moveSpeed * speedMult;
            attackDamage = data.attackDamage * damageMult;
            
            attackRange = data.attackRange;
            attackRate = data.attackRate;

            currentHealth = maxHealth;
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
        }

        private Base targetBase;

        private void Start()
        {
            FindTargetBase();
        }

        private void Update()
        {
            if (isDead) return;

            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            if (targetBase == null)
            {
                FindTargetBase();
                return;
            }

            // Status Effect Update
            HandleStatusEffects();

            float distanceToBase = Vector3.Distance(transform.position, targetBase.transform.position);

            if (distanceToBase <= attackRange)
            {
                agent.isStopped = true;
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
            else
            {
                // Eğer Stun etkisindeyse hareket etme
                bool isStunned = activeEffects.Exists(e => e.type == StatusEffectType.Stun);
                agent.isStopped = isStunned;

                if (!isStunned)
                {
                    // Hız Hesaplama
                    float currentSpeedModifier = 1f;
                    if (unitSide == Side.Dark && CorruptionManager.Instance != null)
                    {
                        currentSpeedModifier = CorruptionManager.Instance.GetSpeedModifier(transform.position);
                    }

                    // Yavaşlatma etkilerini topla
                    float slowFactor = 1f;
                    foreach (var effect in activeEffects)
                    {
                        if (effect.type == StatusEffectType.Slow) 
                            slowFactor = Mathf.Min(slowFactor, effect.power);
                    }

                    agent.speed = moveSpeed * currentSpeedModifier * slowFactor;

                    // Waypoint Takibi veya Direk Üsse İlerleme
                    if (currentPath != null && currentWaypointIndex < currentPath.GetWaypoints().Count)
                    {
                        Transform wp = currentPath.GetWaypoints()[currentWaypointIndex];
                        agent.SetDestination(wp.position);

                        if (Vector3.Distance(transform.position, wp.position) < 1.0f)
                        {
                            currentWaypointIndex++;
                        }
                    }
                    else
                    {
                        agent.SetDestination(targetBase.transform.position);
                    }
                }
            }
        }

        private PathWaypoints currentPath;
        private int currentWaypointIndex = 0;

        public void SetPath(PathWaypoints path)
        {
            currentPath = path;
            currentWaypointIndex = 0;
        }

        private void HandleStatusEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (!activeEffects[i].Update(this))
                {
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void AddStatusEffect(StatusEffectType type, float duration, float power)
        {
            // Aynı tipte bir efekt varsa süreyi yenile veya daha güçlüsünü al
            StatusEffect existing = activeEffects.Find(e => e.type == type);
            if (existing != null)
            {
                if (existing.GetRemainingDuration() < duration)
                {
                    activeEffects.Remove(existing);
                    activeEffects.Add(new StatusEffect(type, duration, power));
                }
            }
            else
            {
                activeEffects.Add(new StatusEffect(type, duration, power));
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            Debug.Log($"{gameObject.name} healed by {amount}. Current Health: {currentHealth}");
        }

        public void ApplySlow(float multiplier, float duration)
        {
            AddStatusEffect(StatusEffectType.Slow, duration, multiplier);
        }

        private void FindTargetBase()
        {
            Base[] bases = FindObjectsByType<Base>(FindObjectsSortMode.None);
            foreach (Base b in bases)
            {
                if (b.GetSide() != unitSide)
                {
                    targetBase = b;
                    break;
                }
            }
        }

        private void Attack()
        {
            if (targetBase != null)
            {
                targetBase.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} attacked the base!");
            }
        }

        public void TakeDamage(float amount)
        {
            if (isDead) return;

            // Eğer kalkan varsa hasarı kalkan emsin
            Shield shield = GetComponent<Shield>();
            if (shield != null && shield.GetCurrentShield() > 0)
            {
                shield.TakeDamage(amount);
            }
            else
            {
                TakeHealthDamage(amount);
            }
        }

        public void TakeHealthDamage(float amount)
        {
            if (isDead) return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            agent.enabled = false;
            Debug.Log($"{gameObject.name} died!");

            // Ölüm efekti
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);
            }

            if (AudioManager.Instance != null && unitData.deathSFX != null)
            {
                AudioManager.Instance.PlaySFX(unitData.deathSFX);
            }

            // Ekonomi ödülü: Ölen birimin karşı tarafına kaynak ver
            Side opponentSide = unitSide == Side.Light ? Side.Dark : Side.Light;
            int rewardAmount = unitData.spawnCost / 2; // Örn: Maliyetin yarısı geri döner
            CurrencyManager.Instance.AddCurrency(opponentSide, rewardAmount);

            // Rakip birim öldüğünde Karma ödülü ver (Örn: 1 Karma)
            if (unitSide != SideController.Instance.GetPlayerSide())
            {
                MetaProgressionManager.Instance.AddKarma(1);
            }

            Destroy(gameObject, 2f);
        }
    }
}
