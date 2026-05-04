using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using TowerDefence.Data;
using TowerDefence.UI;

namespace TowerDefence.Combat
{
    public class Unit : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private UnitData unitData;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private HealthBarUI healthBar;

        private float maxHealth;
        private float attackDamage;
        private float attackRange;
        private float attackRate;
        private Side unitSide;
        private float pathOffset; // Yol üzerindeki yanal sapma (Sabitlendi)
        private Vector3 currentMoveTarget; // Bir sonraki waypoint hedefi (Ofset dahil)
        
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
        public float GetHealth() => currentHealth;
        public int GetCurrentWaypointIndex() => currentWaypointIndex;
        public Vector3 GetCurrentMoveTarget() => currentMoveTarget;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            if (unitData != null) Initialize(unitData);
        }

        public void Initialize(UnitData data)
        {
            this.unitData = data;
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
            
            // Rastgele yanal sapma ata
            pathOffset = Random.Range(-0.8f, 0.8f);

            // Animasyon hızını hareket hızıyla senkronize et
            // 1.0f = referans hız, animasyon bu hıza göre kalibre edilmiş sayılır
            SyncAnimatorSpeed(moveSpeed);

            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

            InitializeStatusEffects();
            SetLayerRecursive(gameObject, (unitSide == Side.Light) ? 6 : 7);
            targetLayer = (unitSide == Side.Light) ? (1 << 7) : (1 << 6);
        }

        /// <summary>Animator hızını verilen hareket hızına göre ayarlar.</summary>
        private void SyncAnimatorSpeed(float speed)
        {
            if (animator == null) return;
            // Referans hızı: 1.0f. Animasyonlar bu hıza göre çekilmiş sayılır.
            // Kısmi çarpım (0.85f) aşırı hızlı görünmeyi engeller.
            const float baseSpeed = 1.0f;
            animator.speed = Mathf.Clamp((speed / baseSpeed) * 0.85f, 0.3f, 2.5f);
        }

        private void InitializeStatusEffects()
        {
            activeEffects = new List<StatusEffect>();
        }

        private void SetLayerRecursive(GameObject obj, int newLayer)
        {
            if (null == obj) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (null == child) continue;
                SetLayerRecursive(child.gameObject, newLayer);
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
                    
                    if (Time.time >= nextAttackTime && !isAttacking)
                    {
                        StartCoroutine(PerformAttack((MonoBehaviour)targetCombatant));
                        nextAttackTime = Time.time + 1f / attackRate;
                    }
                }
            }

            // Hareket Mantığı
            bool isStunned = activeEffects.Exists(e => e.type == StatusEffectType.Stun);
            
            if (!isFighting && !isStunned && !isAttacking)
            {
                if (targetCombatant != null && !targetCombatant.IsDead)
                {
                    // 1. Hedefi Kovala (Chase Logic - aggro radius içindelerse)
                    Vector3 chaseTarget = ((MonoBehaviour)targetCombatant).transform.position;
                    MoveTowardsTarget(chaseTarget);
                    HandleRotation(chaseTarget);
                    if (animator != null) animator.SetBool("IsMoving", true);
                }
                else
                {
                    // 2. Düşman yoksa Waypoint Takibi yap
                    Vector3 targetPos = Vector3.zero;

                    // 1. Waypoint Takibi
                    if (currentPath != null && currentWaypointIndex < currentPath.GetWaypoints().Count)
                    {
                        targetPos = currentPath.GetWaypoints()[currentWaypointIndex].position;
                        // Y Eksenini yoksayarak mesafe ölçümü yap (Overshoot ve geri gitmeyi engeller)
                        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
                        Vector3 flatTarget = new Vector3(currentMoveTarget.x, 0, currentMoveTarget.z);
                        
                        if (Vector3.Distance(flatPos, flatTarget) < 0.2f)
                        {
                            currentWaypointIndex++;
                            if (currentWaypointIndex >= currentPath.GetWaypoints().Count)
                            {
                                OnReachPathEnd();
                                return;
                            }
                            UpdateMoveTarget(); // Yeni waypoint için hedefi güncelle
                        }
                    }
                    // 3. Üs Hedefi
                    else if (targetBase != null)
                    {
                        targetPos = targetBase.transform.position;
                    }

                    if (targetPos != Vector3.zero)
                    {
                        MoveTowardsTarget(currentMoveTarget);
                        HandleRotation(currentMoveTarget);
                        if (animator != null) animator.SetBool("IsMoving", true);
                    }
                    else
                    {
                        if (animator != null) animator.SetBool("IsMoving", false);
                    }
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

        private void UpdateMoveTarget()
        {
            if (currentPath == null || currentWaypointIndex >= currentPath.GetWaypoints().Count) return;

            Vector3 baseTarget = currentPath.GetWaypoints()[currentWaypointIndex].position;
            Vector3 targetWithMyY = new Vector3(baseTarget.x, transform.position.y, baseTarget.z);
            
            // Yanal sapmayı (lane içinde mikro-varyans) waypoint bazlı hesapla ve sabitle
            Vector3 direction = (targetWithMyY - transform.position).normalized;
            if (direction.sqrMagnitude > 0.01f)
            {
                Vector3 right = Vector3.Cross(Vector3.up, direction);
                currentMoveTarget = baseTarget + right * pathOffset;
            }
            else
            {
                currentMoveTarget = baseTarget;
            }
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
            // Aggro Range: Menzilli ise menzili kadar, yakın dövüş ise en az 8 birimlik geniş aggro yelpazesi
            float aggroRange = Mathf.Max(attackRange, 8f);
            
            // Önce yakındaki birimleri tara
            Collider[] colliders = Physics.OverlapSphere(transform.position, aggroRange, targetLayer);
            float shortestDist = aggroRange;
            IDamageable nearestUnit = null;

            foreach (var col in colliders)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead && damageable.GetSide() != unitSide)
                {
                    // FİLTRE: Yakın dövüş birimleri kuleleri hedef alamaz
                    if (unitData.projectilePrefab == null && damageable is Tower)
                        continue;

                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        nearestUnit = damageable;
                    }
                }
            }

            if (nearestUnit != null)
            {
                targetCombatant = nearestUnit;
            }
            else
            {
                targetCombatant = null;
            }
        }

        private PathWaypoints currentPath;
        private int currentWaypointIndex = 0;
        private bool isAttacking = false;

        public void SetPath(PathWaypoints path)
        {
            currentPath = path;
            currentWaypointIndex = 0;
            
            if (currentPath != null && currentPath.GetWaypoints().Count > 0)
            {
                UpdateMoveTarget();
                SnapRotationToTarget(currentMoveTarget);
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
                UpdateMoveTarget();
                SnapRotationToTarget(wps[currentWaypointIndex].position);
            }
        }

        public void SetPathWithExactTarget(PathWaypoints path, int targetIdx)
        {
            currentPath = path;
            if (path == null) return;

            var wps = path.GetWaypoints();
            if (wps.Count == 0) return;

            currentWaypointIndex = Mathf.Clamp(targetIdx, 0, wps.Count - 1);
            
            UpdateMoveTarget();
            SnapRotationToTarget(currentMoveTarget);
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
            
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

            Debug.Log($"{gameObject.name} healed by {amount}. Current Health: {currentHealth}");
        }

        public void ApplySlow(float multiplier, float duration)
        {
            AddStatusEffect(StatusEffectType.Slow, duration, multiplier);
            // Slow uygulandığında animasyonu da yavaşlat
            SyncAnimatorSpeed(moveSpeed * multiplier);
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

        private System.Collections.IEnumerator PerformAttack(MonoBehaviour targetMB)
        {
            isAttacking = true;
            IDamageable targetC = targetMB as IDamageable;

            if (targetC != null && !targetC.IsDead)
            {
                if (animator != null) 
                {
                    // Animator hızını attackRate ile orantılı artırarak yavaş/hızlı birimlerin animasyonlarını senkronize et
                    // (Orijinal animasyon çok yavaşsa kılıç inmeden süre bitebileceği için min. oran korunur)
                    animator.speed = Mathf.Max(1f, attackRate / 1.5f);
                    animator.SetTrigger("Attack");
                }

                // Animasyonun "vurma anı" için bekleme (Saldırı döngüsünün %50'si)
                float attackDuration = 1f / attackRate;
                float hitTime = attackDuration * 0.5f;

                yield return new WaitForSeconds(hitTime);

                // Phantom Hit Fix (Ölüye vurmayı engelle)
                if (targetMB != null && targetC != null && !targetC.IsDead)
                {
                    if (unitData.projectilePrefab != null)
                    {
                        // Menzilli saldırı: Mermi oluştur ve hedefe yönlendir
                        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
                        GameObject projGO = Instantiate(unitData.projectilePrefab, spawnPos, transform.rotation);
                        Projectile proj = projGO.GetComponent<Projectile>();
                        if (proj != null)
                        {
                            proj.Initialize(attackDamage, 0f, (unitSide == Side.Light) ? VFXType.LightImpact : VFXType.DarkImpact);
                            proj.Seek(targetMB.transform);
                        }
                    }
                    else
                    {
                        // Yakın dövüş: Sadece başka ÜNİTELERE hasar verebilir, KULELERE vuramaz
                        if (!(targetC is Tower))
                        {
                            targetC.TakeDamage(attackDamage);
                        }
                        else
                        {
                            Debug.Log($"[UNIT] {gameObject.name} is Melee and cannot reach tower {targetMB.name}");
                        }
                    }
                }

                // Geri kalan attack animasyon süresini (Recoil) bitirmesini bekle ki hemen kaymaya başlamasın
                yield return new WaitForSeconds(attackDuration - hitTime);
            }
            
            if (animator != null) animator.speed = 1f; // Animator hızını normale çek
            isAttacking = false;
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
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;
            if (animator != null) animator.SetTrigger("Die");
            if (healthBar != null) healthBar.SetVisible(false);

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
            int rewardAmount = unitData.killReward;
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
