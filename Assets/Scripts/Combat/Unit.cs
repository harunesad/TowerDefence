using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using TowerDefence.Data;

namespace TowerDefence.Combat
{
    public class Unit : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private UnitData unitData;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 10f;

        private float maxHealth;
        private float attackDamage;
        private float attackRange;
        private float attackRate;
        private Side unitSide;
        
        private float currentHealth;
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
            animator = GetComponentInChildren<Animator>();
            if (unitData != null) Initialize(unitData);
        }

        public void Initialize(UnitData data)
        {
            this.unitData = data;
            unitSide = data.side;

            maxHealth = data.maxHealth;
            moveSpeed = data.moveSpeed;
            attackDamage = data.attackDamage;
            attackRange = data.attackRange;
            attackRate = data.attackRate;

            currentHealth = maxHealth;
            
            // Dinamik Katman ve Hedef Katmanı Atama (LightUnit: 6 (Hedef 7), DarkUnit: 7 (Hedef 6))
            gameObject.layer = (unitSide == Side.Light) ? 6 : 7;
            targetLayer = (unitSide == Side.Light) ? (1 << 7) : (1 << 6);

            SetVisuals();
        }

        private void SetVisuals()
        {
            Color teamColor = (unitSide == Side.Light) ? Color.cyan : Color.red;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            
            // MaterialPropertyBlock kullanarak daha performanslı ve shader uyumlu renklendirme
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            
            foreach (var r in renderers)
            {
                r.GetPropertyBlock(propBlock);
                // URP, Toon ve Standart shader özellikleri
                propBlock.SetColor("_Color", teamColor);
                propBlock.SetColor("_BaseColor", teamColor);
                propBlock.SetColor("_MainColor", teamColor);
                propBlock.SetColor("_TintColor", teamColor);
                r.SetPropertyBlock(propBlock);
            }
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

            // Savaş Durumu Check
            bool isFighting = false;
            if (targetCombatant != null && !targetCombatant.IsDead)
            {
                float distance = Vector3.Distance(transform.position, ((MonoBehaviour)targetCombatant).transform.position);
                if (distance <= attackRange)
                {
                    isFighting = true;
                    HandleRotation(((MonoBehaviour)targetCombatant).transform.position);
                    
                    if (Time.time >= nextAttackTime)
                    {
                        Attack();
                        nextAttackTime = Time.time + 1f / attackRate;
                    }
                }
            }

            // Hareket Mantığı
            bool isStunned = activeEffects.Exists(e => e.type == StatusEffectType.Stun);
            
            if (!isFighting && !isStunned)
            {
                Vector3 targetPos = Vector3.zero;

                // 1. Düşman Takibi (Eğer menzil dışındaysa ama hedefi varsa)
                if (targetCombatant != null && !targetCombatant.IsDead && !(targetCombatant is Base))
                {
                    targetPos = ((MonoBehaviour)targetCombatant).transform.position;
                }
                // 2. Waypoint Takibi
                else if (currentPath != null && currentWaypointIndex < currentPath.GetWaypoints().Count)
                {
                    targetPos = currentPath.GetWaypoints()[currentWaypointIndex].position;
                    // Y Eksenini yoksayarak mesafe ölçümü yap (Overshoot ve geri gitmeyi engeller)
                    Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 flatTarget = new Vector3(targetPos.x, 0, targetPos.z);
                    
                    if (Vector3.Distance(flatPos, flatTarget) < 0.1f)
                    {
                        currentWaypointIndex++;
                        if (currentWaypointIndex >= currentPath.GetWaypoints().Count)
                        {
                            OnReachPathEnd();
                            return;
                        }
                    }
                }
                // 3. Üs Hedefi
                else if (targetBase != null)
                {
                    targetPos = targetBase.transform.position;
                }

                if (targetPos != Vector3.zero)
                {
                    MoveTowardsTarget(targetPos);
                    HandleRotation(targetPos);
                    if (animator != null) animator.SetBool("IsMoving", true);
                }
                else
                {
                    if (animator != null) animator.SetBool("IsMoving", false);
                }
            }
            else
            {
                if (animator != null) animator.SetBool("IsMoving", false);
            }
        }

        private void MoveTowardsTarget(Vector3 targetPos)
        {
            Vector3 targetWithMyY = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            // MoveTowards kullanımı, hedefi geçip geri dönme (titreme) sorununu tamamen engeller
            transform.position = Vector3.MoveTowards(transform.position, targetWithMyY, moveSpeed * Time.deltaTime);
        }

        private void HandleRotation(Vector3 targetPos)
        {
            Vector3 direction = (targetPos - transform.position);
            direction.y = 0; // Yerden yükselmeyi veya aşağı bakmayı engelle
            
            // Eğer sıfıra çok yakınsa dönmeye çalışma (LookRotation hatasını engeller)
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void SnapRotationToTarget(Vector3 targetPos)
        {
            Vector3 direction = (targetPos - transform.position);
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized);
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
                // Sadece üniteleri hedef al ki Waypointler çalışabilsin
                targetCombatant = null;
            }
        }

        private PathWaypoints currentPath;
        private int currentWaypointIndex = 0;

        public void SetPath(PathWaypoints path)
        {
            currentPath = path;
            currentWaypointIndex = 0;
            
            if (currentPath != null && currentPath.GetWaypoints().Count > 0)
            {
                SnapRotationToTarget(currentPath.GetWaypoints()[0].position);
            }
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
            
            if (currentWaypointIndex < wps.Count)
            {
                SnapRotationToTarget(wps[currentWaypointIndex].position);
            }
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
            
            // Eğer yol atanmışsa, hedef olarak yolun son noktasını (Base kapısını) referans al
            Vector3 referencePos = transform.position;
            if (currentPath != null && currentPath.GetWaypoints().Count > 0)
            {
                var wps = currentPath.GetWaypoints();
                referencePos = wps[wps.Count - 1].position;
            }

            float minDistance = float.MaxValue;
            foreach (Base b in bases)
            {
                if (b.GetSide() != unitSide)
                {
                    // Artık doğduğu yere göre değil, gideceği yolun sonuna en yakın Base'i seçecek
                    float dist = Vector3.Distance(referencePos, b.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        targetBase = b;
                    }
                }
            }
        }

        private void OnReachPathEnd()
        {
            if (targetBase != null && !targetBase.IsDead)
            {
                // Üsse ulaştığında çarpma hasarı ver ve kendini yok et (Kamikaze)
                targetBase.TakeDamage(attackDamage * 5f);
                Debug.Log($"{gameObject.name} reached base and exploded!");
                
                // Kamikaze efekti
                if (VFXManager.Instance != null)
                {
                    // Özel ölüm veya standart ölüm efekti
                    VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);
                }
            }
            
            isDead = true;
            if (animator != null) animator.SetTrigger("Die");
            Destroy(gameObject, 0.5f);
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
