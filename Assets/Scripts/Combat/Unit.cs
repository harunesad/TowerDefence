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
        [SerializeField] protected UnitData unitData;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] protected HealthBarUI healthBar;
        [SerializeField] private Transform firePoint;

        private float maxHealth;
        private float attackDamage;
        private float attackRange;
        private float attackRate;
        private Side unitSide;
        private float pathOffset; // Yol üzerindeki yanal sapma (Sabitlendi)
        private Vector3 currentMoveTarget; // Bir sonraki waypoint hedefi (Ofset dahil)
        
        protected float currentHealth;
        protected Animator animator;
        private float nextAttackTime;
        protected bool isDead;
        private LayerMask targetLayer;
        private IDamageable targetCombatant;

        // Status effects
        private List<StatusEffect> activeEffects = new List<StatusEffect>();

        // Physics Throttling
        private float targetSearchTimer;
        private const float TARGET_SEARCH_INTERVAL = 0.15f;

        public bool IsDead => isDead;
        public Side GetSide() => unitSide;
        public float GetHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public int GetCurrentWaypointIndex() => currentWaypointIndex;
        public Vector3 GetCurrentMoveTarget() => currentMoveTarget;
        public bool IsAttacking() => isAttacking;
        public IDamageable GetTarget() => targetCombatant;

        // --- BLOCKING SYSTEM ---
        protected bool isBlocked;
        protected Unit currentBlocker;
        protected List<Unit> blockedEnemies = new List<Unit>(); // Bizim engellediğimiz düşmanlar
        public bool IsBlocked => isBlocked;
        public void Block(Unit blocker) { isBlocked = true; currentBlocker = blocker; }
        public void Unblock() { isBlocked = false; currentBlocker = null; }

        public static List<Unit> AllUnits = new List<Unit>();

        // Hit Flash
        private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
        private Coroutine flashCoroutine;

        private void Awake()
        {
            AllUnits.Add(this);
            animator = GetComponentInChildren<Animator>();
            
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.material != null && r.material.HasProperty("_Color"))
                {
                    originalColors[r] = r.material.color;
                }
            }

            if (unitData != null) Initialize(unitData);
        }

        protected virtual void OnDestroy()
        {
            AllUnits.Remove(this);
        }

        public void Initialize(UnitData data, float statMultiplier = 1f)
        {
            this.unitData = data;
            unitSide = data.side;

            // Meta-Gelişim Çarpanlarını Uygula
            float healthMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.HealthBonus, unitSide);
            float speedMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.SpeedBonus, unitSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.DamageBonus, unitSide);

            maxHealth = data.maxHealth * healthMult * statMultiplier;
            moveSpeed = data.moveSpeed * speedMult;
            attackDamage = data.attackDamage * damageMult * statMultiplier;
            attackRange = data.attackRange;
            attackRate = data.attackRate;

            currentHealth = maxHealth;
            
            // 4 farklı random şerit (lane) ilerleyişi ata
            float[] lanes = new float[] { -1.5f, -0.5f, 0.5f, 1.5f };
            pathOffset = lanes[Random.Range(0, lanes.Length)];

            // Animasyon hızını hareket hızıyla senkronize et
            // 1.0f = referans hız, animasyon bu hıza göre kalibre edilmiş sayılır
            SyncAnimatorSpeed(moveSpeed);

            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

            ResolveFirePoint();

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

        private void ResolveFirePoint()
        {
            if (firePoint != null) return;

            Transform direct = transform.Find("FirePoint");
            if (direct != null)
            {
                firePoint = direct;
                return;
            }

            Transform visuals = transform.Find("Visuals");
            if (visuals == null) return;

            foreach (Transform child in visuals.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "FirePoint")
                {
                    firePoint = child;
                    return;
                }
            }
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
            if (ShouldSeekEnemyBase())
                FindTargetBase();
        }

        protected virtual bool ShouldFollowPath() => true;
        protected virtual bool ShouldSeekEnemyBase() => true;
        protected virtual void TryMoveToManualDestination() { }
        protected virtual bool CanAcquireTarget() => true;

        protected void ClearTarget()
        {
            targetCombatant = null;
            if (isBlocked && currentBlocker != null)
            {
                currentBlocker.OnBlockedEnemyDied(this);
                currentBlocker = null;
                isBlocked = false;
            }
            foreach (var enemy in blockedEnemies)
            {
                if (enemy != null) enemy.Unblock();
            }
            blockedEnemies.Clear();
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            HandleStatusEffects();
            
            targetSearchTimer -= Time.deltaTime;
            if (targetSearchTimer <= 0)
            {
                if (CanAcquireTarget())
                {
                    UpdateTargetConflict();
                }
                targetSearchTimer = TARGET_SEARCH_INTERVAL;
            }

            // Savaş Durumu Check — isBlocked'dan bağımsız çalışır
            bool isFighting = false;
            if (targetCombatant != null && !targetCombatant.IsDead)
            {
                float distance = GetFlatDistance(transform.position, ((MonoBehaviour)targetCombatant).transform.position);
                
                if (this is HeroUnit)
                {
                    Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} target: {((MonoBehaviour)targetCombatant).name}, distance: {distance}, attackRange: {attackRange}, nextAttackTime: {nextAttackTime}, Time.time: {Time.time}, isAttacking: {isAttacking}");
                }

                // Saldırı menziline küçük bir tolerans ekle (+0.5f)
                if (distance <= attackRange + 0.5f)
                {
                    isFighting = true;
                    HandleRotation(((MonoBehaviour)targetCombatant).transform.position);
                    
                    if (Time.time >= nextAttackTime && !isAttacking)
                    {
                        if (this is HeroUnit)
                        {
                            Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} starting PerformAttack coroutine.");
                        }
                        StartCoroutine(PerformAttack((MonoBehaviour)targetCombatant));
                        nextAttackTime = Time.time + 1f / attackRate;
                    }
                }
            }

            // Hareket Mantığı — isBlocked sadece yol/kovalama hareketini engeller
            bool isStunned = activeEffects.Exists(e => e.type == StatusEffectType.Stun);

            if (!isFighting && !isStunned && !isAttacking)
            {
                // Değişiklik: isBlocked olsa bile eğer bir hedefimiz (düello partnerimiz) varsa ona yürüyebilelim
                if (targetCombatant != null && !targetCombatant.IsDead)
                {
                    // 1. Hedefi Kovala (Chase Logic)
                    Vector3 chaseTarget = ((MonoBehaviour)targetCombatant).transform.position;
                    float distance = GetFlatDistance(transform.position, chaseTarget);
                    
                    // Yakın dövüşçü ise (Projectile yoksa) iyice dibine (1.2f) gir
                    float stopDistance = (unitData.projectilePrefab == null) ? 1.2f : attackRange * 0.8f;
                    
                    if (distance > stopDistance)
                    {
                        if (this is HeroUnit) Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} chasing: moving towards target. stopDistance: {stopDistance}");
                        MoveTowardsTarget(chaseTarget);
                        if (animator != null) animator.SetBool("IsMoving", true);
                    }
                    else
                    {
                        if (this is HeroUnit) Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} close enough to stop chasing. stopDistance: {stopDistance}");
                        if (animator != null) animator.SetBool("IsMoving", false);
                    }
                    
                    HandleRotation(chaseTarget);
                }
                else if (!isBlocked)
                {
                    if (!ShouldFollowPath())
                    {
                        TryMoveToManualDestination();
                    }
                    else
                    {
                    // 2. Düşman yoksa ve engellenmemişsek Waypoint Takibi yap
                    Vector3 targetPos = Vector3.zero;

                    if (currentPath != null && currentWaypointIndex < currentPath.GetWaypoints().Count)
                    {
                        targetPos = currentPath.GetWaypoints()[currentWaypointIndex].position;
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
                            UpdateMoveTarget();
                        }
                    }
                    else if (targetBase != null && ShouldSeekEnemyBase())
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
                    // isBlocked: Yerinde dur, hedefe dön
                    if (currentBlocker != null)
                        HandleRotation(currentBlocker.transform.position);
                    if (animator != null) animator.SetBool("IsMoving", false);
                }
            }
            else
            {
                if (animator != null) animator.SetBool("IsMoving", false);
            }
        }

        protected void MoveTowardsTarget(Vector3 targetPos)
        {
            Vector3 targetWithMyY = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            // MoveTowards kullanımı, hedefi geçip geri dönme (titreme) sorununu tamamen engeller
            transform.position = Vector3.MoveTowards(transform.position, targetWithMyY, moveSpeed * Time.deltaTime);
        }

        private void UpdateMoveTarget()
        {
            if (currentPath == null || currentWaypointIndex < 0 || currentWaypointIndex >= currentPath.GetWaypoints().Count) return;

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

        protected void HandleRotation(Vector3 targetPos)
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

        public bool IsBlockingSomeone() => blockedEnemies.Count > 0;

        private void UpdateTargetConflict()
        {
            // 1. ÖNCELİK: Düello Partneri Kontrolü
            if (blockedEnemies.Count > 0 && blockedEnemies[0] != null && !blockedEnemies[0].IsDead)
            {
                targetCombatant = blockedEnemies[0];
                if (this is HeroUnit)
                {
                    Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} targeting blockedEnemy: {((MonoBehaviour)targetCombatant).name}");
                }
                return;
            }
            if (isBlocked && currentBlocker != null && !currentBlocker.IsDead)
            {
                targetCombatant = currentBlocker;
                if (this is HeroUnit)
                {
                    Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} targeting currentBlocker: {currentBlocker.name}");
                }
                return;
            }

            // 2. Yeni Hedef Arama (Unit + Tower)
            float aggroRange = Mathf.Max(attackRange, 8f);
            Collider[] colliders = Physics.OverlapSphere(transform.position, aggroRange, targetLayer);
            float shortestDist = aggroRange;
            IDamageable nearestTarget = null;

            foreach (var col in colliders)
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead || damageable.GetSide() == unitSide)
                    continue;

                // Tower hedefleme kontrolü: Sadece ranged birimler kulelere saldırabilir
                if (damageable is Tower && unitData.projectilePrefab == null)
                    continue;

                // 1'E 1 KURALI: Unit vs Unit, kahramanlar muaftır
                Unit otherUnit = damageable as Unit;
                if (otherUnit != null && !(this is HeroUnit) && !(otherUnit is HeroUnit))
                {
                    if (otherUnit.IsBlockingSomeone() && otherUnit.blockedEnemies[0] != this)
                        continue;
                    if (otherUnit.IsBlocked && otherUnit.currentBlocker != this)
                        continue;
                }

                float dist = GetFlatDistance(transform.position, col.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    nearestTarget = damageable;
                }
            }

            targetCombatant = nearestTarget;
        }

        private PathWaypoints currentPath;
        private int currentWaypointIndex = 0;
        protected bool isAttacking = false;

        public void SetPath(PathWaypoints path)
        {
            currentPath = path;
            
            if (currentPath != null && currentPath.GetWaypoints().Count > 0)
            {
                currentWaypointIndex = 0;
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
            
            if (currentWaypointIndex >= 0 && currentWaypointIndex < wps.Count)
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

        protected System.Collections.IEnumerator PerformAttack(MonoBehaviour targetMB)
        {
            if (this is HeroUnit) Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} PerformAttack started.");
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

                // Kahraman bu bekleme süresinde hareket emri aldıysa vurmayı iptal et
                if (this is HeroUnit && ((HeroUnit)this).IsMovingToManualTarget)
                {
                    isAttacking = false;
                    if (animator != null) animator.speed = 1f;
                    yield break;
                }

                // Phantom Hit Fix (Ölüye vurmayı engelle)
                if (targetMB != null && targetC != null && !targetC.IsDead)
                {
                    if (unitData.projectilePrefab != null)
                    {
                        ResolveFirePoint();
                        Vector3 spawnPos = firePoint != null
                            ? firePoint.position
                            : transform.position + Vector3.up * 1.2f;
                        Quaternion spawnRot = firePoint != null
                            ? firePoint.rotation
                            : transform.rotation;

                        GameObject projGO = Instantiate(unitData.projectilePrefab, spawnPos, spawnRot);
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
            if (this is HeroUnit) Debug.Log($"[UNIT_COMBAT_DEBUG] Hero {gameObject.name} PerformAttack finished. isAttacking reset to false.");
        }

        public void TakeDamage(float amount)
        {
            if (isDead) return;

            // Eğer kalkan varsa hasarı kalkan emsin
            Shield shield = GetComponent<Shield>();
            if (shield != null && shield.GetCurrentShield() > 0)
            {
                float remainingDamage = shield.AbsorbDamage(amount);
                if (remainingDamage > 0)
                {
                    TakeHealthDamage(remainingDamage);
                }
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

            if (gameObject.activeInHierarchy)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashRoutine());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator FlashRoutine()
        {
            foreach (var kvp in originalColors)
            {
                if (kvp.Key != null && kvp.Key.material != null && kvp.Key.material.HasProperty("_Color"))
                    kvp.Key.material.color = Color.white; // Vurulunca beyaz veya kırmızı (Color.red de olabilir) parlama
            }
            yield return new WaitForSeconds(0.1f);
            foreach (var kvp in originalColors)
            {
                if (kvp.Key != null && kvp.Key.material != null && kvp.Key.material.HasProperty("_Color"))
                    kvp.Key.material.color = kvp.Value;
            }
        }

        protected virtual void Die()
        {
            isDead = true;
            
            // Bizi engelleyen varsa onu boşa çıkar
            if (currentBlocker != null)
            {
                currentBlocker.OnBlockedEnemyDied(this);
                currentBlocker = null;
            }

            // Bizim engellediğimiz düşmanlar varsa onları serbest bırak
            foreach (var enemy in blockedEnemies)
            {
                if (enemy != null) enemy.Unblock();
            }
            blockedEnemies.Clear();

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

        // --- SOLDIER SPECIFIC LOGIC ---
        public void OnBlockedEnemyDied(Unit enemy)
        {
            blockedEnemies.Remove(enemy);
            if (blockedEnemies.Count == 0)
            {
                // Artık birini engellemiyoruz, normal harekete dönebiliriz (Veya rally point'e)
            }
        }

        public bool CanBlockMore() => blockedEnemies.Count < 1; // Şimdilik 1 asker 1 düşman

        public void StartBlocking(Unit enemy)
        {
            if (!blockedEnemies.Contains(enemy))
            {
                blockedEnemies.Add(enemy);
                enemy.Block(this);
                // Düşmana kilitlen
                targetCombatant = enemy;
            }
        }

        private float GetFlatDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
        }
    }
}
