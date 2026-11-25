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
        [SerializeField] protected TowerData towerData;

        public TowerData GetTowerData() => towerData;
        public int CurrentLevel => currentLevel;
        public void SetLevel(int level) => currentLevel = Mathf.Max(1, level);
        public List<TowerData> GetSpecializations() => towerData != null ? towerData.specializations : null;

        public float GetCurrentDamage() => damage;
        public float GetCurrentRange() => range;
        public float GetCurrentFireRate() => fireRate;
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;

        private float range;
        private float fireRate;
        private GameObject projectilePrefab;
        protected Transform firePoint;
        private Side towerSide;
        private LayerMask targetLayer;

        [Header("Targeting")]
        [SerializeField] private TargetingPriority currentPriority = TargetingPriority.First;
        public TargetingPriority GetTargetingPriority() => currentPriority;

        public void CycleTargetingPriority()
        {
            currentPriority = (TargetingPriority)(((int)currentPriority + 1) % 4);
        }

        [Header("Target Switch")]
        [SerializeField] private float targetSwitchCooldown = 0.5f; // Cooldown duration (seconds)
        private float targetSwitchTimer = 0f;
        private Transform previousTarget = null;
        private float previousTargetTimer = 0f;

        [Header("Rotation")]
        [SerializeField] private Transform partToRotate;
        [SerializeField] private float rotationSpeed = 10f;
        [Tooltip("Euler angle offset applied when rotating towards target. Default: X=-90, Y=180, Z=90")]
        [SerializeField] private Vector3 rotationOffset = new Vector3(-90f, 180f, 90f);
        [Tooltip("Additional Euler angle offset applied when rotating towards nearest path on spawn")]
        [SerializeField] private Vector3 spawnRotationOffset = Vector3.zero;

        private float fireCountdown = 0f;
        private Transform target;
        private bool isDisabled;
        private float disableTimer;

        // Physics Throttling
        private float targetScanTimer;
        private const float TARGET_SCAN_INTERVAL = 0.15f;

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

        private bool isInvulnerable = false;
        private float invulnerableTimer = 0f;
        private float tempFireRateMultiplier = 1f;
        private float tempBuffTimer = 0f;

        public bool IsDead => isDead;

        public void TakeDamage(float amount)
        {
            if (isDead || isInvulnerable) return;

            currentHealth -= amount;
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;


            // VFX
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.SpellEarthquake, transform.position, Quaternion.identity);

            if (AudioManager.Instance != null && AudioManager.Instance.TowerDestroyedSFX != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.TowerDestroyedSFX);
            TowerDefence.UI.WarningBannerUI.Show("TOWER DESTROYED!", playSound: false);

            // Slotu serbest birak
            if (mySlot != null) mySlot.ClearSlot();

            Destroy(gameObject);
        }

        private void Awake()
        {
            if (towerData != null) Initialize(towerData);
        }

        private float damage;
        private bool isBeamTower;

        // --- AURA BUFF SYSTEM ---
        private float auraDamageMultiplier = 1f;
        private float auraFireRateMultiplier = 1f;
        private bool isAuraBuffed = false;
        private LineRenderer buffRing;

        public bool IsAuraBuffed => isAuraBuffed;

        /// <summary>
        /// Statlar taban veriden yeniden yazilmadan once aura izini siler.
        /// carpan bolunmez; bir sonraki aura taramasi taze degerlere yeniden uygulanir.
        /// </summary>
        private void ForgetAuraBuff()
        {
            if (!isAuraBuffed && auraDamageMultiplier == 1f && auraFireRateMultiplier == 1f) return;
            isAuraBuffed = false;
            auraOwnerId = 0;
            auraDamageMultiplier = 1f;
            auraFireRateMultiplier = 1f;
            SetBuffRing(false);
        }

        private int auraOwnerId;

        /// <summary>Aura Tower tarafindan cagrilir. Daha guclu aura zayif olanin yerine gecer.</summary>
        public void ApplyAuraBuff(float damageBonus, float fireRateBonus, int ownerId)
        {
            float newDamage = 1f + Mathf.Max(0f, damageBonus);
            float newRate = 1f + Mathf.Max(0f, fireRateBonus);

            if (isAuraBuffed && auraOwnerId == ownerId)
            {
                damage /= auraDamageMultiplier;
                fireRate /= auraFireRateMultiplier;
                auraDamageMultiplier = newDamage;
                auraFireRateMultiplier = newRate;
                damage *= auraDamageMultiplier;
                fireRate *= auraFireRateMultiplier;
                return;
            }

            bool stronger = !isAuraBuffed
                || newDamage > auraDamageMultiplier + 0.0001f
                || (Mathf.Approximately(newDamage, auraDamageMultiplier) && newRate > auraFireRateMultiplier + 0.0001f);
            if (!stronger) return;

            if (isAuraBuffed)
            {
                damage /= auraDamageMultiplier;
                fireRate /= auraFireRateMultiplier;
            }

            isAuraBuffed = true;
            auraOwnerId = ownerId;
            auraDamageMultiplier = newDamage;
            auraFireRateMultiplier = newRate;
            damage *= auraDamageMultiplier;
            fireRate *= auraFireRateMultiplier;
            SetBuffRing(true);
        }

        /// <summary>Buff'i yalniz onu veren aura kaldirabilir.</summary>
        public void RemoveAuraBuff(int ownerId)
        {
            if (!isAuraBuffed || auraOwnerId != ownerId) return;
            isAuraBuffed = false;
            auraOwnerId = 0;
            damage /= auraDamageMultiplier;
            fireRate /= auraFireRateMultiplier;
            auraDamageMultiplier = 1f;
            auraFireRateMultiplier = 1f;
            SetBuffRing(false);
        }

        private void SetBuffRing(bool enabled)
        {
            if (!enabled)
            {
                if (buffRing != null) buffRing.gameObject.SetActive(false);
                return;
            }

            if (buffRing == null)
            {
                GameObject go = new GameObject("BuffRing");
                go.transform.SetParent(transform, false);
                buffRing = go.AddComponent<LineRenderer>();
                buffRing.positionCount = 40;
                buffRing.loop = true;
                buffRing.useWorldSpace = true;
                buffRing.startWidth = 0.16f;
                buffRing.endWidth = 0.16f;
                buffRing.alignment = LineAlignment.TransformZ;
                buffRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                buffRing.receiveShadows = false;
                buffRing.material = new Material(Shader.Find("Sprites/Default"));
                buffRing.startColor = Color.yellow;
                buffRing.endColor = Color.yellow;
            }

            float radius = Mathf.Max(1.1f, MeasureVisualRadius() * 0.72f);
            float angle = 0f;
            float step = 360f / buffRing.positionCount;
            Vector3 center = transform.position;
            for (int i = 0; i < buffRing.positionCount; i++)
            {
                float x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float z = center.z + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                buffRing.SetPosition(i, new Vector3(x, GroundHeight(x, z), z));
                angle += step;
            }
            buffRing.gameObject.SetActive(true);
        }

        private TowerObstacle towerObstacle;

        public float ObstacleRadius => towerObstacle != null ? towerObstacle.Radius : 1.5f;

        protected void EnsureTowerObstacle()
        {
            if (towerObstacle != null) return;

            float radius = MeasureVisualRadius();

            int layer = LayerMask.NameToLayer("TowerObstacle");
            if (layer < 0) layer = 10;

            GameObject obstacleObject = new GameObject("TowerObstacle");
            obstacleObject.layer = layer;
            obstacleObject.transform.SetParent(transform, false);
            obstacleObject.transform.localPosition = Vector3.zero;
            SphereCollider sphere = obstacleObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;
            sphere.center = new Vector3(0f, radius * 0.5f, 0f);
            towerObstacle = obstacleObject.AddComponent<TowerObstacle>();
            towerObstacle.Radius = radius;
        }

        private float MeasureVisualRadius()
        {
            Transform visuals = transform.Find("Visuals");
            Renderer[] renderers = visuals != null
                ? visuals.GetComponentsInChildren<Renderer>(true)
                : GetComponentsInChildren<Renderer>(true);

            Vector3 origin = transform.position;
            float reach = 0f;
            bool found = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer rend = renderers[i];
                if (rend == null) continue;

                Mesh mesh = null;
                if (rend is MeshRenderer)
                {
                    MeshFilter filter = rend.GetComponent<MeshFilter>();
                    if (filter != null) mesh = filter.sharedMesh;
                }
                else if (rend is SkinnedMeshRenderer skinned)
                {
                    mesh = skinned.sharedMesh;
                }

                if (mesh == null) continue;

                Bounds local = mesh.bounds;
                Vector3 ext = local.extents;
                Vector3 center = local.center;
                for (int xi = -1; xi <= 1; xi += 2)
                {
                    for (int zi = -1; zi <= 1; zi += 2)
                    {
                        Vector3 world = rend.transform.TransformPoint(center + new Vector3(ext.x * xi, 0f, ext.z * zi));
                        float dx = world.x - origin.x;
                        float dz = world.z - origin.z;
                        reach = Mathf.Max(reach, Mathf.Sqrt(dx * dx + dz * dz));
                    }
                }
                found = true;
            }

            if (!found)
            {
                Collider[] cols = GetComponents<Collider>();
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i] == null || !cols[i].enabled) continue;
                    Vector3 ext = cols[i].bounds.extents;
                    reach = Mathf.Max(reach, ext.x, ext.z);
                    found = true;
                }
            }

            if (!found) return 1.5f;
            return Mathf.Max(0.8f, reach + 0.35f);
        }

        private void OnDestroy()
        {
            if (buffRing != null && buffRing.material != null)
                Destroy(buffRing.material);
        }


        public void Initialize(TowerData data)
        {
            ForgetAuraBuff();
            towerData = data;
            towerSide = data.side;

            // Meta-Gelisim carpanlarini Uygula
            // DamageBonus birim hasaridir. Kule hasari yalnizca TowerDamageBonus ile gelir.
            float rangeMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.RangeBonus, towerSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.TowerDamageBonus, towerSide);
            if (IsLightArcher(data))
            {
                damageMult *= MetaProgressionManager.Instance.GetSkillGroupMultiplier("Skill_Archer_", UpgradeType.TowerDamageBonus, towerSide);
                rangeMult *= MetaProgressionManager.Instance.GetSkillGroupMultiplier("Skill_Archer_", UpgradeType.RangeBonus, towerSide);
            }
            float speedMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.SpeedBonus, towerSide);
            float healthMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.TowerHealthBonus, towerSide);

            range = data.range * rangeMult;
            fireRate = data.fireRate * speedMult;
            damage = data.damage * damageMult;
            targetLayer = data.targetLayer;
            projectilePrefab = data.projectilePrefab;
            isBeamTower = data.isBeamTower;

            // Health Initialization
            maxHealth = (data.health > 0 ? data.health : 100f) * healthMult;
            currentHealth = maxHealth;
            healthBar = GetComponentInChildren<TowerDefence.UI.HealthBarUI>();
            if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);
            
            if (firePoint == null) firePoint = transform.Find("FirePoint");
            EnsureTowerObstacle();
            if (this is BarracksTower && TutorialDirector.Instance != null)
                TutorialDirector.Instance.Notify("barracks");
            
            // 3D Kuleler icin Weapon nesnesini kaldirdik, kule modelinin kendisi donecek
            if (partToRotate == null)
            {
                Transform visuals = transform.Find("Visuals");
                if (visuals != null && visuals.childCount > 0)
                {
                    // Visuals altindaki ilk obje kule modelidir
                    partToRotate = visuals.GetChild(0);
                    if (partToRotate != null)
                    {
                        firePoint = partToRotate.Find("FirePoint");
                    }
                }
            }

            if (firePoint == null) firePoint = transform;

            // Attack Range Indicator Initialization
            if (attackRangeLine == null)
            {
                GameObject go = new GameObject("AttackRangeIndicator");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                attackRangeLine = go.AddComponent<LineRenderer>();
                attackRangeLine.startWidth = 0.4f;
                attackRangeLine.endWidth = 0.4f;
                attackRangeLine.positionCount = 51;
                attackRangeLine.useWorldSpace = true;
                attackRangeLine.loop = true;
                attackRangeLine.alignment = LineAlignment.TransformZ;
                attackRangeLine.material = new Material(Shader.Find("Sprites/Default"));
                attackRangeLine.startColor = Color.white;
                attackRangeLine.endColor = Color.white;
                attackRangeLine.gameObject.SetActive(false);
            }

            // Dinamik Hedefleme (Light kuleler Dark layer'i (7), Dark kuleler Light layer'i (6) hedefler)
            targetLayer = (towerSide == Side.Light) ? (1 << 7) : (1 << 6);

            FaceNearestPathOnSpawn();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.HasMatchEnded()) return;

            if (isDisabled)
            {
                TickDisable();
                return;
            }

            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            // --- SPELL TIMERS ---
            if (invulnerableTimer > 0)
            {
                invulnerableTimer -= Time.deltaTime;
                if (invulnerableTimer <= 0) isInvulnerable = false;
            }

            if (tempBuffTimer > 0)
            {
                tempBuffTimer -= Time.deltaTime;
                if (tempBuffTimer <= 0) tempFireRateMultiplier = 1f;
            }

            // Aura towers do not shoot, they only provide buffs
            if (fireRate <= 0f) return;

            if (target != null)
            {
                Unit held = target.GetComponent<Unit>() ?? target.GetComponentInParent<Unit>();
                bool heldValid = held != null && !held.IsDead && Vector3.Distance(transform.position, held.transform.position) <= range;
                if (heldValid)
                    previousTargetTimer += Time.deltaTime;
                else
                    previousTargetTimer = targetSwitchCooldown;
            }

            targetScanTimer -= Time.deltaTime;
            if (targetScanTimer <= 0)
            {
                UpdateTarget();
                targetScanTimer = TARGET_SCAN_INTERVAL;
            }

            if (target != null && !IsLivingEnemy(target))
                target = null;

            if (target != null)
            {
                LockOnTarget();
                if (fireCountdown <= 0f)
                {
                    if (!isBeamTower)
                    {
                        Shoot();
                    }
                    fireCountdown = 1f / (fireRate * tempFireRateMultiplier);
                }
            }

            fireCountdown -= Time.deltaTime;
        }

        public void SetInvulnerable(float duration)
        {
            isInvulnerable = true;
            invulnerableTimer = duration;
            // Visual feedback: Kalkan efekti (varsa)
        }

        public void ApplyTempBuff(float speedMult, float duration)
        {
            tempFireRateMultiplier = speedMult;
            tempBuffTimer = duration;
            // Visual feedback: Haste effect
        }

        private void LockOnTarget()
        {
            if (partToRotate == null) return;

            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            if (dir == Vector3.zero) return;

            Quaternion lookRotation = Quaternion.LookRotation(dir);
            float targetY = lookRotation.eulerAngles.y + rotationOffset.y;

            Quaternion targetRotation = Quaternion.Euler(rotationOffset.x, targetY, rotationOffset.z);
            partToRotate.rotation = Quaternion.RotateTowards(partToRotate.rotation, targetRotation, rotationSpeed * 16f * Time.deltaTime);
        }

        private void FaceNearestPathOnSpawn()
        {
            if (partToRotate == null) return;

            PathWaypoints[] paths = FindObjectsByType<PathWaypoints>(FindObjectsSortMode.None);
            if (paths.Length == 0) return;

            Vector3 nearestPoint = Vector3.zero;
            float minDist = float.MaxValue;

            foreach (var path in paths)
            {
                var wps = path.GetPathPoints();
                if (wps.Count < 2) continue;

                for (int i = 0; i < wps.Count - 1; i++)
                {
                    Vector3 pA = wps[i]; pA.y = 0;
                    Vector3 pB = wps[i + 1]; pB.y = 0;
                    Vector3 towerPos = transform.position; towerPos.y = 0;

                    Vector3 closest = GetClosestPointOnSegment(towerPos, pA, pB);
                    float dist = Vector3.Distance(towerPos, closest);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestPoint = closest;
                    }
                }
            }

            if (nearestPoint == Vector3.zero) return;

            Vector3 dir = nearestPoint - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude <= 0.001f) return;

            Quaternion lookRotation = Quaternion.LookRotation(dir);
            float targetY = lookRotation.eulerAngles.y + spawnRotationOffset.y;
            Quaternion targetRotation = Quaternion.Euler(spawnRotationOffset.x, targetY, spawnRotationOffset.z);

            partToRotate.rotation = targetRotation;
        }

        private static Vector3 GetClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;
            float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab));
            return a + t * ab;
        }

        private void UpdateTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, targetLayer);
            if (colliders.Length == 0)
            {
                previousTargetTimer = 0f;
                previousTarget = null;
                target = null;
                return;
            }

            bool attackersInRange = false;
            foreach (Collider col in colliders)
            {
                if (TryGetLivingUnit(col, out Unit unit) && IsAttackingThisTower(unit))
                {
                    attackersInRange = true;
                    break;
                }
            }

            GameObject selectedEnemy = null;

            switch (currentPriority)
            {
                case TargetingPriority.First:
                    float maxProgress = -1f;
                    foreach (Collider col in colliders)
                    {
                        if (!TryGetLivingUnit(col, out Unit unit)) continue;
                        if (attackersInRange && !IsAttackingThisTower(unit)) continue;
                        
                        // First: Highest waypoint index + closest to next waypoint
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
                        if (!TryGetLivingUnit(col, out Unit unit)) continue;
                        if (attackersInRange && !IsAttackingThisTower(unit)) continue;

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
                        if (!TryGetLivingUnit(col, out Unit unit)) continue;
                        if (attackersInRange && !IsAttackingThisTower(unit)) continue;

                        float h = unit.GetHealth(); 
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
                        if (!TryGetLivingUnit(col, out Unit unit)) continue;
                        if (attackersInRange && !IsAttackingThisTower(unit)) continue;

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
                if (target != null && target != selectedEnemy.transform && previousTargetTimer < targetSwitchCooldown)
                    return;

                previousTarget = target;
                previousTargetTimer = 0f;
                target = selectedEnemy.transform;
            }
            else
            {
                target = null;
            }
        }

        private bool IsAttackingThisTower(Unit unit)
        {
            if (unit == null) return false;
            IDamageable aim = unit.GetTarget();
            if (aim == this) return true;
            Component component = aim as Component;
            return component != null && component.GetComponentInParent<Tower>() == this;
        }

        private static bool TryGetLivingUnit(Collider col, out Unit unit)
        {
            unit = col.GetComponent<Unit>() ?? col.GetComponentInParent<Unit>();
            return unit != null && !unit.IsDead;
        }

        private static bool IsLivingEnemy(Transform aim)
        {
            if (aim == null) return false;
            Unit unit = aim.GetComponent<Unit>() ?? aim.GetComponentInParent<Unit>();
            return unit != null && !unit.IsDead;
        }

        private float CalculateUnitProgress(Unit unit)
        {
            return unit != null ? unit.GetPathProgress01() : 0f;
        }

        private void Shoot()
        {
            GameObject projGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projectile = projGO.GetComponent<Projectile>();

            if (projectile != null)
            {
                VFXType impactType = towerSide == Side.Light ? VFXType.LightImpact : VFXType.DarkImpact;
                projectile.Initialize(damage, towerData.explosionRadius, impactType, towerData.effectType, towerData.effectDuration, towerData.effectPower, towerData.isArtillery);
                
                projectile.Seek(target);

                if (AudioManager.Instance != null && towerData.shootSFX != null)
                {
                    AudioManager.Instance.PlaySFX(towerData.shootSFX);
                }
            }
        }

        protected int currentLevel = 1;
        private int investedGold;

        public int GetInvestedGold() => investedGold;
        public void SetInvestedGold(int amount) => investedGold = Mathf.Max(0, amount);
        public void AddInvestedGold(int amount)
        {
            if (amount > 0) investedGold += amount;
        }
        public int GetSellRefund() => (investedGold + 1) / 2;

        public bool IsDisabled => isDisabled;

        protected void TickDisable()
        {
            if (!isDisabled) return;
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f) isDisabled = false;
        }

        public void Disable(float duration)
        {
            isDisabled = true;
            disableTimer = duration;
            target = null;
            BeamTower beam = GetComponent<BeamTower>();
            if (beam != null) beam.StopBeam();
        }

        private static bool IsLightArcher(TowerData data)
        {
            if (data == null || data.side != Side.Light) return false;
            return data.towerName == "Archer Tower"
                || data.towerName == "Sniper Tower"
                || data.towerName == "Ranger Outpost";
        }

        public virtual bool Upgrade()
        {
            if (currentLevel >= 3 && GetSpecializations() != null && GetSpecializations().Count > 0)
            {
                return false;
            }

            currentLevel++;
            ForgetAuraBuff();

            // Level 2 is x1.4 and level 3 is x1.8, so a second tower stays worth building.
            // Range increases by 20% per level.
            float damageLevel = DamageLevelScale(currentLevel);
            float rangeLevel = 1f + (currentLevel - 1) * 0.2f;

            float rangeMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.RangeBonus, towerSide);
            float damageMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.TowerDamageBonus, towerSide);
            if (IsLightArcher(towerData))
            {
                damageMult *= MetaProgressionManager.Instance.GetSkillGroupMultiplier("Skill_Archer_", UpgradeType.TowerDamageBonus, towerSide);
                rangeMult *= MetaProgressionManager.Instance.GetSkillGroupMultiplier("Skill_Archer_", UpgradeType.RangeBonus, towerSide);
            }
            float speedMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.SpeedBonus, towerSide);

            range = towerData.range * rangeMult * rangeLevel;
            damage = towerData.damage * damageMult * damageLevel;
            fireRate = towerData.fireRate * speedMult;
            ApplyLevelHealth();

            AuraTower aura = GetComponent<AuraTower>();
            if (aura != null) aura.OnTowerUpgraded(currentLevel);

            if (attackRangeLine != null && attackRangeLine.gameObject.activeSelf)
                DrawRangeCircle();

            return true;
        }

        public void Specialize(TowerData newData)
        {
            if (newData == null) return;
            
            // Change data and restart
            towerData = newData;
            currentLevel++; // Advance to next level (1->2, 2->3 etc.)
            
            Initialize(newData);
            
        }

        public static float DamageLevelScale(int level)
        {
            return 1f + Mathf.Max(0, level - 1) * 0.4f;
        }

        public float HealthForLevel(int level)
        {
            float healthMult = 1f;
            if (MetaProgressionManager.Instance != null)
                healthMult = MetaProgressionManager.Instance.GetMultiplierForType(UpgradeType.TowerHealthBonus, towerSide);
            float baseHealth = (towerData != null && towerData.health > 0f ? towerData.health : 100f) * healthMult;
            return baseHealth * DamageLevelScale(level);
        }

        protected void ApplyLevelHealth()
        {
            float newMax = HealthForLevel(currentLevel);
            float gained = newMax - maxHealth;
            maxHealth = newMax;
            if (gained > 0f)
                currentHealth = Mathf.Min(maxHealth, currentHealth + gained);
            else
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            if (healthBar != null)
                healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        public int GetLevel() => currentLevel;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        protected LineRenderer attackRangeLine;

        protected static float GroundHeight(float x, float z)
        {
            int mask = LayerMask.GetMask("Default", "Path");
            Vector3 origin = new Vector3(x, 80f, z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 160f, mask, QueryTriggerInteraction.Ignore))
                return hit.point.y + 0.3f;
            return 0.2f;
        }

        protected virtual void DrawRangeCircle()
        {
            if (attackRangeLine == null) return;

            float angle = 0f;
            Vector3 center = transform.position;

            for (int i = 0; i < 51; i++)
            {
                float x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * range;
                float z = center.z + Mathf.Cos(Mathf.Deg2Rad * angle) * range;
                attackRangeLine.SetPosition(i, new Vector3(x, GroundHeight(x, z), z));
                angle += (360f / 50);
            }
        }

        public virtual void SetRangeVisible(bool visible)
        {
            if (attackRangeLine != null && !towerData.isAuraTower)
            {
                if (visible) DrawRangeCircle();
                attackRangeLine.gameObject.SetActive(visible);
            }
        }
    }
}
