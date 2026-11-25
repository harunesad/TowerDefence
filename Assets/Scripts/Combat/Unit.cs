using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CartoonFX;
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
        private float slowMultiplier = 1f;
        private float corruptionMultiplier = 1f;
        private float corruptionTimer;
        [SerializeField] protected HealthBarUI healthBar;
        [SerializeField] private Transform firePoint;

        private float maxHealth;
        private float attackDamage;
        public float GetDamage() => attackDamage * tempDamageMultiplier;

        public static float ResolveAttackDamage(UnitData data, float damageScale)
        {
            if (data == null) return 0f;

            float damageMult = 1f;
            float allStatMult = 1f;
            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta != null)
            {
                damageMult = meta.GetMultiplierForType(UpgradeType.DamageBonus, data.side);
                if (data.side == Side.Light && meta.IsSkillUnlocked("Skill_Light_HolyBlessing"))
                    damageMult *= 1.15f;
                if (data.side == Side.Dark && meta.IsSkillUnlocked("Skill_Dark_SoulHarvest"))
                    damageMult *= 1.1f;
                if (meta.IsSkillUnlocked("Skill_Neutral_AncientWisdom"))
                    allStatMult = 1.1f;
            }

            return data.attackDamage * damageMult * allStatMult * damageScale;
        }

        private float absorbShield;

        public void AddAbsorbShield(float amount)
        {
            if (amount <= 0f) return;
            absorbShield = Mathf.Max(absorbShield, amount);
        }

        public float GetPathProgress01()
        {
            if (currentPath == null) return 0f;
            var points = currentPath.GetPathPoints();
            if (points == null || points.Count < 2) return 0f;

            float total = 0f;
            for (int i = 1; i < points.Count; i++)
                total += Vector3.Distance(points[i - 1], points[i]);
            if (total < 0.01f) return 0f;

            int idx = Mathf.Clamp(currentWaypointIndex, 0, points.Count - 1);
            float traveled = 0f;
            for (int i = 1; i <= idx; i++)
                traveled += Vector3.Distance(points[i - 1], points[i]);

            float segment = Vector3.Distance(points[Mathf.Max(0, idx - 1)], points[idx]);
            float remain = Vector3.Distance(transform.position, points[idx]);
            traveled -= Mathf.Clamp(remain, 0f, segment);
            float progress = Mathf.Clamp01(traveled / total);
            return walkPathBackward ? 1f - progress : progress;
        }
        private float attackRange;
        private float attackRate;
        private Side unitSide;
        private float pathOffset; // Lateral deviation on path (Fixed)
        private Vector3 currentMoveTarget; // Bir sonraki waypoint hedefi (Ofset dahil)
        
        protected float currentHealth;
        protected Animator animator;
        private float nextAttackTime;
        protected bool isDead;
        private LayerMask targetLayer;
        private IDamageable targetCombatant;
        private bool walkPathBackward;

        [Header("Indicator")]
        [SerializeField] private float indicatorRadius = 0.6f;
        [SerializeField] private Vector3 indicatorPosition = new Vector3(0f, 3f, 0f);
        private GameObject unitIndicator;

        // Status effects
        private List<StatusEffect> activeEffects = new List<StatusEffect>();

        // Physics Throttling
        private float targetSearchTimer;
        private const float TARGET_SEARCH_INTERVAL = 0.15f;

        public bool IsDead => isDead;
        public bool IsBoss => unitData != null && SideSelectionUI.BossNames.Contains(unitData.unitName);
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
        protected List<Unit> blockedEnemies = new List<Unit>(); // Enemies blocked by us
        public bool IsBlocked => isBlocked;
        public void Block(Unit blocker)
        {
            if (currentBlocker != null && currentBlocker != blocker)
                currentBlocker.OnBlockedEnemyDied(this);
            isBlocked = true;
            currentBlocker = blocker;
        }
        public void Unblock() { isBlocked = false; currentBlocker = null; }

        public static List<Unit> AllUnits = new List<Unit>();

        // Hit Flash (URP uyumlu — _BaseColor/_Color ikisini de destekler)
        private Renderer[] flashRenderers;
        private MaterialPropertyBlock flashBlock;
        private float hitFlashUntil;
        private bool hitFlashClearNow;
        private static readonly int ColorPropId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            AllUnits.Add(this);
            animator = GetComponentInChildren<Animator>();
            
            // Runtime Bulletproof Fix: Eger AnimationEventHandler yoksa otomatik ekle
            if (animator != null && animator.GetComponent<AnimationEventHandler>() == null)
            {
                animator.gameObject.AddComponent<AnimationEventHandler>();
            }
            
            flashRenderers = GetComponentsInChildren<Renderer>();
            flashBlock = new MaterialPropertyBlock();

            if (unitData != null) Initialize(unitData);
        }

        protected virtual void OnDestroy()
        {
            AllUnits.Remove(this);
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private float killRewardMultiplier = 1f;
        protected bool grantKillReward = true;

        public void SuppressKillReward() => grantKillReward = false;

        public void SetKillRewardMultiplier(float multiplier)
        {
            killRewardMultiplier = Mathf.Max(1f, multiplier);
        }

        protected int GetScaledKillReward()
        {
            if (unitData == null) return 0;
            return Mathf.Max(0, Mathf.RoundToInt(unitData.killReward * killRewardMultiplier));
        }

        public void Initialize(UnitData data, float statMultiplier = 1f, float damageStatMultiplier = -1f)
        {
            this.unitData = data;
            unitSide = data.side;

            float healthScale = statMultiplier;
            float damageScale = damageStatMultiplier < 0f ? statMultiplier : damageStatMultiplier;
            if (!(this is HeroUnit)
                && SideController.Instance != null
                && data.side == SideController.Instance.GetPlayerSide())
                damageScale *= 1f / 3f;

            // Meta-Gelisim carpanlarini Uygula
            // SpeedBonus kule ates hizidir (Rapid Fire). Birim hizi yalnizca UnitSpeedBonus.
            float healthMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.HealthBonus, unitSide);
            if (this is HeroUnit)
                healthMult *= MetaProgressionManager.Instance.GetSkillGroupMultiplier("Skill_Hero_", UpgradeType.HealthBonus, unitSide);
            float speedMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.UnitSpeedBonus, unitSide);
            float allStatMult = MetaProgressionManager.Instance.IsSkillUnlocked("Skill_Neutral_AncientWisdom") ? 1.1f : 1f;

            maxHealth = data.maxHealth * healthMult * allStatMult * healthScale;
            moveSpeed = data.moveSpeed * speedMult * allStatMult;
            attackDamage = ResolveAttackDamage(data, damageScale);
            attackRange = data.attackRange * allStatMult;
            attackRate = data.attackRate * allStatMult;

            currentHealth = maxHealth;
            
            // pathOffset will be set when the path is assigned
            pathOffset = 0f;

            // Animasyon hizini hareket hiziyla senkronize et
            // 1.0f = referans hiz, animasyon bu hiza gore kalibre edilmis sayilir
            SyncAnimatorSpeed(moveSpeed);

            if (healthBar != null) 
            {
                // Dost birimler mavi, dusman birimler yesil/kirmizi (prefabin orijinal rengi veya yesil)
                if (unitSide == SideController.Instance.GetPlayerSide())
                    healthBar.SetColor(new Color(0.2f, 0.6f, 1f)); // Mavi
                else
                    healthBar.SetColor(new Color(0.2f, 0.8f, 0.2f)); // Yesil
                    
                healthBar.UpdateHealth(currentHealth, maxHealth);
            }

            ResolveFirePoint();

            InitializeStatusEffects();
            SetLayerRecursive(gameObject, (unitSide == Side.Light) ? 6 : 7);
            targetLayer = (unitSide == Side.Light) ? (1 << 7) : (1 << 6);
            
            // Per user request: Indicator ring under friendly units completely removed (distinguished by health bar colors)
            // if (unitSide == SideController.Instance.GetPlayerSide())
            //     CreateUnitIndicator();

            if (AudioManager.Instance != null && unitData.spawnSFX != null)
                AudioManager.Instance.PlaySFX(unitData.spawnSFX);
        }

        /// <summary>Adjusts animator speed based on given movement speed.</summary>
        private void SyncAnimatorSpeed(float speed)
        {
            if (animator == null || isAttacking) return;
            // Reference speed: 1.0f. Animations considered calibrated to this.
            // Partial factor (0.85f) prevents appearing excessively fast.
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

        [Header("UI")]
        public Sprite unitIndicatorSprite;
        [Header("Status Effect Visuals")]
        public Sprite burnIcon;
        public Sprite poisonIcon;
        public Sprite slowIcon;
        public Sprite stunIcon;
        private Dictionary<StatusEffectType, GameObject> activeEffectVisuals = new Dictionary<StatusEffectType, GameObject>();

        private void CreateUnitIndicator()
        {
            if (unitIndicator != null) return;

            unitIndicator = new GameObject("UnitIndicator");
            unitIndicator.transform.SetParent(transform, false);
            unitIndicator.transform.localPosition = indicatorPosition; // Position configured from editor
            unitIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            unitIndicator.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var sr = unitIndicator.AddComponent<SpriteRenderer>();
            sr.sprite = unitIndicatorSprite != null ? unitIndicatorSprite : CreateFallbackSprite();
            sr.color = Color.white;
            sr.sortingOrder = 10;
        }

        private static Sprite CreateFallbackSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = dist <= radius ? 1f : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Base targetBase;

        private void Start()
        {
            if (ShouldSeekEnemyBase())
                FindTargetBase();

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.Preparation)
                ResetAttackState();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Victory || state == GameState.Defeat)
                ResetAttackState();
        }

        protected void ResetAttackState()
        {
            if (isDead) return;

            StopAllCoroutines();
            isAttacking = false;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.speed = 1f;
                animator.ResetTrigger("Attack");
                animator.Play("Idle", 0, 0f);
            }

            ClearTarget();
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

        protected bool IsLevelFinished()
        {
            return GameManager.Instance != null && GameManager.Instance.HasMatchEnded();
        }

        protected virtual void Update()
        {
            if (isDead || IsLevelFinished()) return;
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.HoldsUnitMarch()) return;

            UpdateBuffTimers(Time.deltaTime);

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

            // Combat State Check — operates independently of isBlocked
            bool isStunned = activeEffects.Exists(e => e.type == StatusEffectType.Stun);
            bool isFighting = false;
            if (!isStunned && targetCombatant != null && !targetCombatant.IsDead)
            {
                Vector3 fightPos = ((MonoBehaviour)targetCombatant).transform.position;
                float distance = GetCombatDistance(targetCombatant, fightPos);
                
                // Add a small tolerance to attack range (+0.5f)
                if (distance <= attackRange + 0.5f)
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

            // Movement Logic — isBlocked only prevents path/chase movement
            if (!isFighting && !isStunned && !isAttacking)
            {
                // Change: Even if isBlocked, walk towards duel partner if we have one
                if (targetCombatant != null && !targetCombatant.IsDead)
                {
                    // 1. Hedefi Kovala (Chase Logic)
                    wasChasing = true;
                    Vector3 chaseTarget = ((MonoBehaviour)targetCombatant).transform.position;
                    float distance = GetCombatDistance(targetCombatant, chaseTarget);
                    
                    // If melee (no projectile), close in closely (1.2f)
                    float stopDistance = (unitData.projectilePrefab == null) ? 1.2f : attackRange * 0.8f;
                    
                    if (distance > stopDistance)
                    {
                        MoveTowardsTarget(chaseTarget);
                        if (animator != null) animator.SetBool("IsMoving", true);
                    }
                    else
                    {
                        if (animator != null) animator.SetBool("IsMoving", false);
                    }
                    
                    HandleRotation(chaseTarget);
                }
                else if (!isBlocked || (this is HeroUnit && ((HeroUnit)this).IsMovingToManualTarget))
                {
                    if (wasChasing)
                    {
                        wasChasing = false;
                        RecalculateWaypointIndex();
                    }

                    if (!ShouldFollowPath())
                    {
                        TryMoveToManualDestination();
                    }
                    else
                    {
                    // 2. If no enemy and unblocked, follow waypoints
                    Vector3 targetPos = Vector3.zero;
                    bool hasTarget = false;

                    if (currentPath != null && 
                        (walkPathBackward ? currentWaypointIndex >= 0 : currentWaypointIndex < currentPath.GetPathPoints().Count))
                    {
                        targetPos = currentPath.GetPathPoints()[currentWaypointIndex];
                        hasTarget = true;
                        
                        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
                        Vector3 flatTarget = new Vector3(currentMoveTarget.x, 0, currentMoveTarget.z);
                        
                        if (Vector3.Distance(flatPos, flatTarget) < 0.2f)
                        {
                            if (walkPathBackward)
                                currentWaypointIndex--;
                            else
                                currentWaypointIndex++;

                            bool reachedEnd = walkPathBackward 
                                ? currentWaypointIndex < 0 
                                : currentWaypointIndex >= currentPath.GetPathPoints().Count;

                            if (reachedEnd)
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
                        hasTarget = true;
                    }

                    if (hasTarget)
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
                    // isBlocked: Stay in place, face target
                    if (currentBlocker != null)
                        HandleRotation(currentBlocker.transform.position);
                    if (animator != null) animator.SetBool("IsMoving", false);
                }
            }
            else
            {
                // Stop walk animation during combat/attack — but only for normal units.
                // Heroes manage their own animations inside HeroUnit.Update().
                if (!(this is HeroUnit) && animator != null) animator.SetBool("IsMoving", false);
            }
        }

        private float GetCorruptionMultiplier()
        {
            if (CorruptionManager.Instance == null) return 1f;
            corruptionTimer -= Time.deltaTime;
            if (corruptionTimer > 0f) return corruptionMultiplier;
            corruptionTimer = 0.25f;
            corruptionMultiplier = CorruptionManager.Instance.GetSpeedModifier(transform.position);
            return corruptionMultiplier;
        }

        protected void MoveTowardsTarget(Vector3 targetPos)
        {
            float step = moveSpeed * slowMultiplier * GetCorruptionMultiplier() * Time.deltaTime;
            Vector3 from = transform.position;
            Vector3 next = Vector3.MoveTowards(from, targetPos, step);
            transform.position = SlideAroundTowers(from, next);
        }

        private static Vector3 SlideAroundTowers(Vector3 from, Vector3 to)
        {
            const float skin = 0.4f;
            Vector3 pos = to;
            List<TowerObstacle> obstacles = TowerObstacle.All;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < obstacles.Count; i++)
                {
                    TowerObstacle obstacle = obstacles[i];
                    if (obstacle == null) continue;

                    Vector3 center = obstacle.Center;
                    float limit = obstacle.Radius + skin;
                    float dx = pos.x - center.x;
                    float dz = pos.z - center.z;
                    float distSq = dx * dx + dz * dz;
                    if (distSq >= limit * limit) continue;

                    float ox = from.x - center.x;
                    float oz = from.z - center.z;
                    float outside = Mathf.Sqrt(ox * ox + oz * oz);
                    Vector3 normal;
                    if (outside > 0.001f)
                        normal = new Vector3(ox / outside, 0f, oz / outside);
                    else if (distSq > 0.0001f)
                        normal = new Vector3(dx, 0f, dz).normalized;
                    else
                        normal = Vector3.right;

                    Vector3 move = new Vector3(to.x - from.x, 0f, to.z - from.z);
                    Vector3 tangent = new Vector3(-normal.z, 0f, normal.x);
                    if (Vector3.Dot(tangent, move) < 0f)
                        tangent = -tangent;

                    Vector3 slid = from + tangent * move.magnitude;
                    float sx = slid.x - center.x;
                    float sz = slid.z - center.z;
                    float slidDist = Mathf.Sqrt(sx * sx + sz * sz);
                    if (slidDist < limit)
                    {
                        if (slidDist < 0.001f)
                        {
                            slid.x = center.x + normal.x * limit;
                            slid.z = center.z + normal.z * limit;
                        }
                        else
                        {
                            float scale = limit / slidDist;
                            slid.x = center.x + sx * scale;
                            slid.z = center.z + sz * scale;
                        }
                    }

                    slid.y = to.y;
                    pos = slid;
                    from = pos;
                }
            }

            return pos;
        }

        private void UpdateMoveTarget()
        {
            if (currentPath == null || currentWaypointIndex < 0 || currentWaypointIndex >= currentPath.GetPathPoints().Count) return;

            var points = currentPath.GetPathPoints();
            Vector3 baseTarget = points[currentWaypointIndex];
            
            // Calculate lateral offset direction from path segment, not unit's current position.
            // This prevents jitter when the unit gets very close to target.
            Vector3 pathDirection = Vector3.forward;

            if (points.Count > 1)
            {
                int prevIndex = walkPathBackward ? currentWaypointIndex + 1 : currentWaypointIndex - 1;
                
                if (prevIndex >= 0 && prevIndex < points.Count)
                {
                    // Direction from previous waypoint towards current target
                    pathDirection = (baseTarget - points[prevIndex]).normalized;
                }
                else
                {
                    // For first waypoint, determine direction looking towards next point
                    int nextIndex = walkPathBackward ? currentWaypointIndex - 1 : currentWaypointIndex + 1;
                    if (nextIndex >= 0 && nextIndex < points.Count)
                    {
                        pathDirection = (points[nextIndex] - baseTarget).normalized;
                    }
                }
            }

            pathDirection.y = 0;
            pathDirection.Normalize();

            if (pathDirection.sqrMagnitude > 0.01f)
            {
                Vector3 right = Vector3.Cross(Vector3.up, pathDirection);
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
            direction.y = 0; // Prevent tilting up or down
            
            // Do not rotate if vector is near zero (prevents LookRotation error)
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
            // 1. PRIORITY: Duel Partner Check
            if (blockedEnemies.Count > 0 && blockedEnemies[0] != null && !blockedEnemies[0].IsDead)
            {
                targetCombatant = blockedEnemies[0];
                return;
            }
            if (isBlocked && currentBlocker != null && !currentBlocker.IsDead)
            {
                targetCombatant = currentBlocker;
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

                // 1 ON 1 RULE: Unit vs Unit, heroes exempt
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

            if (nearestTarget != null)
                targetCombatant = nearestTarget;
            else if (targetCombatant == null || targetCombatant.IsDead)
                targetCombatant = null;
        }

        private PathWaypoints currentPath;
        private int currentWaypointIndex = 0;
        protected bool isAttacking = false;
        protected bool hasDealtDamage = false;
        protected MonoBehaviour currentTargetMB;

        private void CalculateLaneOffset(PathWaypoints path)
        {
            if (path == null) return;
            float hw = path.PathWidth / 2f;
            // 4 lanes or continuous random spread. Calculated via PathWidth for natural look.
            // e.g. if width = 3, offset is between -1.5 and 1.5.
            float[] lanes = new float[] { -hw, -hw/3f, hw/3f, hw };
            pathOffset = lanes[Random.Range(0, lanes.Length)];
        }

        public void SetPath(PathWaypoints path)
        {
            currentPath = path;
            CalculateLaneOffset(path);
            
            if (currentPath != null && currentPath.GetPathPoints().Count > 0)
            {
                currentWaypointIndex = 0;
                UpdateMoveTarget();
                SnapRotationToTarget(currentMoveTarget);
            }
        }

        private bool wasChasing = false;

        private void RecalculateWaypointIndex()
        {
            if (currentPath == null) return;
            var wps = currentPath.GetPathPoints();
            if (wps.Count == 0) return;

            float minDistance = float.PositiveInfinity;
            int nearestIdx = currentWaypointIndex;

            for (int i = 0; i < wps.Count; i++)
            {
                float dist = Vector3.Distance(transform.position, wps[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestIdx = i;
                }
            }

            if (walkPathBackward)
                currentWaypointIndex = Mathf.Max(nearestIdx - 1, 0);
            else
                currentWaypointIndex = Mathf.Min(nearestIdx + 1, wps.Count - 1);
            
            UpdateMoveTarget();
        }

        public void SetPathAtNearestWaypoint(PathWaypoints path, Vector3 currentPos)
        {
            currentPath = path;
            CalculateLaneOffset(path);
            if (path == null) return;

            var wps = path.GetPathPoints();
            float minDistance = float.PositiveInfinity;
            int nearestIdx = 0;

            for (int i = 0; i < wps.Count; i++)
            {
                float dist = Vector3.Distance(currentPos, wps[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestIdx = i;
                }
            }

            // Consider reached to closest waypoint and steer to next
            currentWaypointIndex = Mathf.Min(nearestIdx + 1, wps.Count - 1);
            
            if (currentWaypointIndex >= 0 && currentWaypointIndex < wps.Count)
            {
                UpdateMoveTarget();
                SnapRotationToTarget(wps[currentWaypointIndex]);
            }
        }

        public void SetPathWithExactTarget(PathWaypoints path, int targetIdx, bool walkBackward = false)
        {
            currentPath = path;
            CalculateLaneOffset(path);
            walkPathBackward = walkBackward;
            if (path == null) return;

            var wps = path.GetPathPoints();
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
                    StatusEffectType typeToRemove = activeEffects[i].type;
                    activeEffects.RemoveAt(i);
                    RemoveStatusVisual(typeToRemove);
                }
            }

            RefreshSlowFactor();
            UpdateStatusVisualPositions();
        }

        private void RemoveStatusVisual(StatusEffectType type)
        {
            if (activeEffectVisuals.TryGetValue(type, out GameObject visualObj))
            {
                if (visualObj != null) Destroy(visualObj);
                activeEffectVisuals.Remove(type);
            }
        }

        private void UpdateStatusVisualPositions()
        {
            if (activeEffectVisuals.Count == 0) return;
            
            int index = 0;
            float spacing = 0.6f;
            float startX = -(activeEffectVisuals.Count - 1) * spacing / 2f;
            
            // Slightly below health bar Y position
            float yPos = (healthBar != null) ? healthBar.transform.localPosition.y - 0.75f : 2.5f;

            foreach (var kvp in activeEffectVisuals)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.transform.localPosition = new Vector3(startX + (index * spacing), yPos, 0f);
                    // Billboard towards camera; HealthBarUI parent already handles billboard if present.
                    // Should it lie flat like UnitIndicator or face the screen? 
                    // Facing screen with SpriteRenderer is better since they are icons.
                    if (Camera.main != null)
                    {
                        kvp.Value.transform.rotation = Camera.main.transform.rotation;
                    }
                }
                index++;
            }
        }

        protected void RefreshSlowFactor()
        {
            StatusEffect slow = activeEffects.Find(e => e.type == StatusEffectType.Slow);
            slowMultiplier = (slow != null) ? slow.power : 1f;
            SyncAnimatorSpeed(moveSpeed * slowMultiplier);
        }

        public void AddStatusEffect(StatusEffectType type, float duration, float power)
        {
            // Refresh duration or take stronger effect if same type exists
            StatusEffect existing = activeEffects.Find(e => e.type == type);
            if (existing != null)
            {
                bool stronger = type == StatusEffectType.Slow ? power < existing.power : power > existing.power;
                bool longer = existing.GetRemainingDuration() < duration;
                if (stronger || longer)
                {
                    float keptPower = stronger ? power : existing.power;
                    float keptDuration = Mathf.Max(duration, existing.GetRemainingDuration());
                    existing.Extend(keptDuration, keptPower);
                    if (type == StatusEffectType.Slow)
                        RefreshSlowFactor();
                }
            }
            else
            {
                activeEffects.Add(new StatusEffect(type, duration, power));
                AddStatusVisual(type);
            }
        }

        private void AddStatusVisual(StatusEffectType type)
        {
            Sprite icon = type switch
            {
                StatusEffectType.Burn => burnIcon,
                StatusEffectType.Poison => poisonIcon,
                StatusEffectType.Slow => slowIcon,
                StatusEffectType.Stun => stunIcon,
                _ => null
            };

            if (icon == null) return;

            GameObject visualObj = new GameObject($"StatusVisual_{type}");
            visualObj.transform.SetParent(transform, false);
            
            SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = icon;
            sr.sortingOrder = 15; // Clearly visible above/below healthbar
            visualObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Icon size
            
            activeEffectVisuals[type] = visualObj;
            UpdateStatusVisualPositions();
        }

        public void ScaleCombatStats(float ratio)
        {
            if (ratio <= 0f || Mathf.Approximately(ratio, 1f)) return;
            maxHealth *= ratio;
            currentHealth = Mathf.Clamp(currentHealth * ratio, 1f, maxHealth);
            attackDamage *= ratio;
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

        }

        public void ApplySlow(float multiplier, float duration)
        {
            AddStatusEffect(StatusEffectType.Slow, duration, multiplier);
            // Slow down animation and movement when Slow is applied
            RefreshSlowFactor();
        }

        private void FindTargetBase()
        {
            Base[] bases = FindObjectsByType<Base>(FindObjectsSortMode.None);
            
            // If path is assigned, target end of path (Base gate) as destination
            Vector3 referencePos = transform.position;
            if (currentPath != null && currentPath.GetPathPoints().Count > 0)
            {
                var wps = currentPath.GetPathPoints();
                referencePos = wps[wps.Count - 1];
            }

            float minDistance = float.MaxValue;
            foreach (Base b in bases)
            {
                if (b.GetSide() != unitSide)
                {
                    // Selects the closest Base to the end of its path rather than spawn point
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
            if (targetBase == null || targetBase.GetSide() == unitSide)
                FindTargetBase();

            if (targetBase != null)
            {
                targetBase.Leak(unitData != null ? unitData.LeakLives : 1);
                
                // Kamikaze efekti
                if (VFXManager.Instance != null)
                {
                    // Special death or standard death effect
                    VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);
                }
            }
            
            isDead = true;
            if (animator != null) animator.SetTrigger("Die");
            Destroy(gameObject, 0.5f);
        }

        protected System.Collections.IEnumerator PerformAttack(MonoBehaviour targetMB)
        {
            isAttacking = true;
            hasDealtDamage = false;
            currentTargetMB = targetMB;

            IDamageable targetC = targetMB as IDamageable;
            float attackDuration = 1f / Mathf.Max(0.01f, attackRate);

            if (targetC != null && !targetC.IsDead)
            {
                if (animator != null)
                {
                    float clipLength = GetAttackClipLength();
                    animator.speed = clipLength > 0.05f ? clipLength / attackDuration : 1f;
                    animator.ResetTrigger("Attack");
                    animator.SetBool("IsMoving", false);
                    animator.SetTrigger("Attack");
                }

                yield return new WaitForSeconds(attackDuration);

                if (this is HeroUnit && ((HeroUnit)this).IsMovingToManualTarget)
                {
                    EndAttackSwing();
                    yield break;
                }

                if (!hasDealtDamage)
                    ExecuteAttackHit();
            }

            EndAttackSwing();
        }

        private float GetAttackClipLength()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            float longest = 0f;
            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip clip = clips[i];
                if (clip == null) continue;
                AnimationEvent[] events = clip.events;
                for (int e = 0; e < events.Length; e++)
                {
                    if (events[e].functionName != "OnAttackHit") continue;
                    if (clip.length > longest) longest = clip.length;
                }
            }
            return longest;
        }

        private void EndAttackSwing()
        {
            if (animator != null)
            {
                animator.speed = 1f;
                animator.ResetTrigger("Attack");
                if (!isDead && IsInAttackState())
                    animator.Play("Idle", 0, 0f);
            }
            isAttacking = false;
        }

        private bool IsInAttackState()
        {
            if (animator == null) return false;
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) return true;
            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Attack");
        }

        // Method called from Animation Event
        public void OnAttackHit()
        {
            // Exit if damage was already dealt (fallback or multiple events)
            if (hasDealtDamage || isDead) return;
            
            // Do not hit if attack was cancelled or completed
            if (!isAttacking) return;
            if (SuppressAttackHit()) return;

            ExecuteAttackHit();
        }

        protected virtual bool SuppressAttackHit()
        {
            return false;
        }

        private void ExecuteAttackHit()
        {
            if (isDead) return;
            hasDealtDamage = true;

            if (currentTargetMB == null) return;
            IDamageable targetC = currentTargetMB as IDamageable;

            // Phantom Hit Fix (Prevent hitting dead targets)
            if (targetC != null && !targetC.IsDead)
            {
                if (AudioManager.Instance != null && unitData != null && unitData.attackSFX != null)
                    AudioManager.Instance.PlaySFX(unitData.attackSFX);

                if (unitData.breathVfxPrefab != null)
                {
                    PlayBreath(currentTargetMB.transform);
                    targetC.TakeDamage(attackDamage * tempDamageMultiplier);
                }
                else if (unitData.projectilePrefab != null)
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
                        VFXType projImpactVFX = unitSide == Side.Light ? VFXType.LightImpact : VFXType.DarkImpact;
                        proj.Initialize(attackDamage * tempDamageMultiplier, 0f, projImpactVFX);
                        proj.Seek(currentTargetMB.transform);
                    }
                }
                else
                {
                    targetC.TakeDamage(attackDamage * tempDamageMultiplier);
                }
            }
        }

        private void PlayBreath(Transform aim)
        {
            ResolveFirePoint();
            Vector3 origin = firePoint != null
                ? firePoint.position
                : transform.position + Vector3.up * 1.2f;
            Vector3 dir = aim != null ? (aim.position + Vector3.up) - origin : transform.forward;
            if (dir.sqrMagnitude < 0.0001f)
                dir = transform.forward;

            float scale = unitData.breathVfxScale > 0.01f ? unitData.breathVfxScale : 0.55f;
            GameObject vfx = Instantiate(unitData.breathVfxPrefab, origin, Quaternion.LookRotation(dir.normalized));
            vfx.transform.localScale = Vector3.one * scale;

            ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem.MainModule main = systems[i].main;
                main.playOnAwake = false;
                main.loop = false;
                systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            CFXR_Effect[] clearEffects = vfx.GetComponentsInChildren<CFXR_Effect>(true);
            for (int i = 0; i < clearEffects.Length; i++)
                clearEffects[i].clearBehavior = CFXR_Effect.ClearBehavior.None;

            for (int i = 0; i < systems.Length; i++)
                systems[i].Play(false);

            Destroy(vfx, 1.3f);
        }

        public void TakeDamage(float amount)
        {
            if (isDead) return;
            if (invulnerableTimer > 0f) return;

            if (absorbShield > 0f && amount > 0f)
            {
                float absorbed = Mathf.Min(absorbShield, amount);
                absorbShield -= absorbed;
                amount -= absorbed;
                if (amount <= 0f) return;
            }

            // If shield exists, let shield absorb damage
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
                BeginHitFlash();

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        void BeginHitFlash()
        {
            if (flashRenderers == null || flashBlock == null) return;
            Color flashColor = new Color(1f, 0.75f, 0.75f, 1f);

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer renderer = flashRenderers[i];
                if (renderer == null) continue;
                Material shared = renderer.sharedMaterial;
                if (shared == null) continue;
                renderer.GetPropertyBlock(flashBlock);
                if (shared.HasProperty(BaseColorPropId))
                    flashBlock.SetColor(BaseColorPropId, flashColor);
                if (shared.HasProperty(ColorPropId))
                    flashBlock.SetColor(ColorPropId, flashColor);
                renderer.SetPropertyBlock(flashBlock);
            }

            hitFlashUntil = Time.time + 0.1f;
        }

        void LateUpdate()
        {
            OnLateTick();
            if (hitFlashClearNow || (hitFlashUntil > 0f && Time.time >= hitFlashUntil))
                ClearHitFlash();
        }

        protected virtual void OnLateTick() { }

        protected void CancelHitFlash()
        {
            hitFlashUntil = 0f;
            hitFlashClearNow = true;
        }

        protected void ClearHitFlash()
        {
            hitFlashUntil = 0f;
            hitFlashClearNow = false;
            if (flashRenderers == null) return;
            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer renderer = flashRenderers[i];
                if (renderer == null) continue;
                renderer.SetPropertyBlock(null);
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;
            StopAllCoroutines();
            
            // Release anyone blocking us
            if (currentBlocker != null)
            {
                currentBlocker.OnBlockedEnemyDied(this);
                currentBlocker = null;
            }

            // Release enemies blocked by us if any
            foreach (var enemy in blockedEnemies)
            {
                if (enemy != null) enemy.Unblock();
            }
            blockedEnemies.Clear();

            isDead = true;
            if (healthBar != null) healthBar.SetVisible(false);
            
            // Efekt gorsellerini temizle
            ClearAllStatusEffects();

            if (animator != null) animator.SetTrigger("Die");

            // Death effect
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);
            }

            if (AudioManager.Instance != null && unitData.deathSFX != null)
            {
                AudioManager.Instance.PlaySFX(unitData.deathSFX);
            }

            int reward = grantKillReward ? GetScaledKillReward() : 0;
            if (reward > 0 && CurrencyManager.Instance != null)
            {
                Side opponentSide = GetSide() == Side.Light ? Side.Dark : Side.Light;
                CurrencyManager.Instance.AddCurrency(opponentSide, reward);
                if (SideController.Instance != null && opponentSide == SideController.Instance.GetPlayerSide())
                    CurrencyFloatUI.Show(transform.position, reward, opponentSide);
            }

            Destroy(gameObject, 2f);
        }

        protected void ClearAllStatusEffects()
        {
            activeEffects.Clear();
            foreach (var kvp in activeEffectVisuals)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            activeEffectVisuals.Clear();
            RefreshSlowFactor();
        }


        // --- SOLDIER SPECIFIC LOGIC ---
        public void OnBlockedEnemyDied(Unit enemy)
        {
            blockedEnemies.Remove(enemy);
            if (blockedEnemies.Count == 0)
            {
                // Artik birini engellemiyoruz, normal harekete donebiliriz (Veya rally point'e)
            }
        }

        protected void PruneBlockedEnemies()
        {
            for (int i = blockedEnemies.Count - 1; i >= 0; i--)
            {
                Unit enemy = blockedEnemies[i];
                if (enemy == null || enemy.IsDead)
                    blockedEnemies.RemoveAt(i);
            }
        }

        public virtual bool CanBlockMore()
        {
            PruneBlockedEnemies();
            return blockedEnemies.Count < 1;
        }

        public bool StartBlocking(Unit enemy)
        {
            if (enemy == null || enemy.IsDead) return false;
            PruneBlockedEnemies();
            if (blockedEnemies.Contains(enemy)) return true;
            if (!CanBlockMore()) return false;

            blockedEnemies.Add(enemy);
            enemy.Block(this);
            targetCombatant = enemy;
            return true;
        }

        private float invulnerableTimer;
        private float tempDamageMultiplier = 1f;
        private float tempBuffTimer;

        public void SetInvulnerable(float duration)
        {
            invulnerableTimer = Mathf.Max(invulnerableTimer, duration);
        }

        public void ApplyTempBuff(float multiplier, float duration)
        {
            tempDamageMultiplier = multiplier;
            tempBuffTimer = Mathf.Max(tempBuffTimer, duration);
        }

        public void UpdateBuffTimers(float dt)
        {
            if (invulnerableTimer > 0f) invulnerableTimer -= dt;
            if (tempBuffTimer > 0f)
            {
                tempBuffTimer -= dt;
                if (tempBuffTimer <= 0f) tempDamageMultiplier = 1f;
            }
        }

        private float GetFlatDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
        }

        private float GetCombatDistance(IDamageable target, Vector3 targetPos)
        {
            float distance = GetFlatDistance(transform.position, targetPos);
            if (target is Tower tower)
                distance = Mathf.Max(0f, distance - tower.ObstacleRadius);
            return distance;
        }
    }
}
