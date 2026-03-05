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
        private Animator animator;
        private float nextAttackTime;
        private bool isDead;
        private LayerMask targetLayer;
        private IDamageable targetCombatant;

        // Status effects
        private List<StatusEffect> activeEffects = new List<StatusEffect>();

        public bool IsDead => isDead;
        public Side GetSide() => unitSide;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            if (unitData != null) Initialize(unitData);
        }

        public void Initialize(UnitData data)
        {
            unitData = data;
            unitSide = data.side;

            maxHealth = data.maxHealth;
            moveSpeed = data.moveSpeed;
            attackDamage = data.attackDamage;
            attackRange = data.attackRange;
            attackRate = data.attackRate;

            currentHealth = maxHealth;
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;

            // Dinamik Katman ve Hedef Katmanı Atama (LightUnit: 6 (Hedef 7), DarkUnit: 7 (Hedef 6))
            gameObject.layer = (unitSide == Side.Light) ? 6 : 7;
            targetLayer = (unitSide == Side.Light) ? (1 << 7) : (1 << 6);
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

            HandleStatusEffects();
            UpdateTargetConflict();

            // Savaş Durumu
            if (targetCombatant != null && !targetCombatant.IsDead)
            {
                float distance = Vector3.Distance(transform.position, ((MonoBehaviour)targetCombatant).transform.position);
                
                if (distance <= attackRange)
                {
                    agent.isStopped = true;
                    if (Time.time >= nextAttackTime)
                    {
                        Attack();
                        nextAttackTime = Time.time + 1f / attackRate;
                    }
                    return; // Savaşırken hareket etme
                }
            }

            // Hareket Durumu
            bool isStunned = activeEffects.Exists(e => e.type == StatusEffectType.Stun);
            agent.isStopped = isStunned;

            if (!isStunned)
            {
                // Hız Ayarı
                agent.speed = moveSpeed;
                
                // Animasyon Ayarı
                if (animator != null)
                {
                    animator.SetBool("IsMoving", agent.velocity.magnitude > 0.1f);
                }

                // 1. Birim hedefine doğru git
                if (targetCombatant != null && !targetCombatant.IsDead)
                {
                    agent.SetDestination(((MonoBehaviour)targetCombatant).transform.position);
                }
                // 2. Üsse (Exit Point) doğru git
                else if (targetBase != null)
                {
                    agent.SetDestination(targetBase.transform.position);
                }
                // 3. Waypoint takibi yap
                else if (currentPath != null && currentWaypointIndex < currentPath.GetWaypoints().Count)
                {
                    Transform wp = currentPath.GetWaypoints()[currentWaypointIndex];
                    agent.SetDestination(wp.position);
                    if (Vector3.Distance(transform.position, wp.position) < 1.0f) currentWaypointIndex++;
                }
                else
                {
                    agent.isStopped = true;
                }
            }
        }

        private void UpdateTargetConflict()
        {
            // Önce yakındaki birimleri tara
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange * 2f, targetLayer);
            float shortestDist = Mathf.Infinity;
            IDamageable nearestUnit = null;

            foreach (var col in colliders)
            {
                Unit u = col.GetComponent<Unit>();
                if (u != null && !u.IsDead)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        nearestUnit = u;
                    }
                }
            }

            if (nearestUnit != null)
            {
                targetCombatant = nearestUnit;
            }
            else
            {
                // Yakında birim yoksa üssü (Exit Point) hedef al
                if (targetBase == null) FindTargetBase();
                targetCombatant = targetBase;
            }
        }

        private PathWaypoints currentPath;
        private int currentWaypointIndex = 0;

        public void SetPath(PathWaypoints path)
        {
            currentPath = path;
            currentWaypointIndex = 0;
        }

        public void SetPathAtNearestWaypoint(PathWaypoints path, Vector3 currentPos)
        {
            currentPath = path;
            if (path == null) return;

            var wps = path.GetWaypoints();
            float minDistance = float.PositiveInfinity;
            int nearestIdx = 0;

            for (int i = 0; i < wps.Count; i++)
            {
                float dist = Vector3.Distance(currentPos, wps[i].position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestIdx = i;
                }
            }

            // En yakın noktaya ulaştı sayıp bir sonrakine yönlendiriyoruz
            currentWaypointIndex = Mathf.Min(nearestIdx + 1, wps.Count - 1);
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
                    return;
                }
            }
        }

        private void Attack()
        {
            if (targetCombatant != null && !targetCombatant.IsDead)
            {
                if (animator != null) animator.SetTrigger("Attack");
                targetCombatant.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} attacked {((MonoBehaviour)targetCombatant).name}!");
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
            if (animator != null) animator.SetTrigger("Die");
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
