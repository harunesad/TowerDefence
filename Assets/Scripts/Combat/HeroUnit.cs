using UnityEngine;
using System.Collections.Generic;
using CartoonFX;
using TowerDefence.Core;
using TowerDefence.Data;
using TowerDefence.Interfaces;

namespace TowerDefence.Combat
{
    public class HeroUnit : Unit
    {
        [Header("Hero Settings")]
        [SerializeField] private string heroID;
        [SerializeField] private float respawnTime = 15f;

        [Header("Ability VFX")]
        [SerializeField] private Transform abilityVFXContainer;

        public string HeroID => heroID;
        public float RespawnTime => respawnTime;

        private HeroData heroConfig;
        private System.Action<HeroUnit> onDeathCallback;
        private Vector3 moveDestination;
        private bool hasMoveDestination;
        private HeroSelectionIndicator selectionIndicator;
        private float abilityCooldownTimer;
        private bool isMovingToManualTarget;
        public bool IsMovingToManualTarget => isMovingToManualTarget;

        public HeroData HeroConfig => heroConfig;
        public float AbilityCooldownRemaining => abilityCooldownTimer;
        public float AbilityCooldownMax => ScaledAbilityCooldown;
        public bool IsAbilityReady => abilityCooldownTimer <= 0f && heroConfig != null;

        private float ScaledAbilityCooldown
        {
            get
            {
                if (heroConfig == null) return 0f;
                if (MetaProgressionManager.Instance == null) return heroConfig.abilityCooldown;
                return MetaProgressionManager.Instance.GetHeroAbilityCooldown(heroConfig);
            }
        }

        private float ScaledAbilityPower
        {
            get
            {
                if (heroConfig == null) return 1f;
                if (MetaProgressionManager.Instance == null) return heroConfig.abilityPower;
                return MetaProgressionManager.Instance.GetHeroAbilityPower(heroConfig);
            }
        }

        public bool IsSelected => selectionIndicator != null &&
            selectionIndicator.CurrentState == HeroSelectionState.Active;

        public void SetupHero(HeroData config, float respawnDuration, System.Action<HeroUnit> onDeath)
        {
            heroConfig = config;
            heroID = config != null ? config.heroID : heroID;
            respawnTime = respawnDuration;
            onDeathCallback = onDeath;

            selectionIndicator = GetComponent<HeroSelectionIndicator>();
            if (selectionIndicator == null)
                selectionIndicator = gameObject.AddComponent<HeroSelectionIndicator>();

            // Dynamically add Collider if missing so it can be clicked (guaranteed on root object)
            if (GetComponent<Collider>() == null)
            {
                CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
                col.height = 2f;
                col.radius = 0.5f;
                col.center = new Vector3(0, 1f, 0);
                col.isTrigger = false;
            }

            // Adjust scale (Requirement: Heroes scale (1.5, 1.5, 1.5))
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);

            SetSelectionState(HeroSelectionState.Passive);
            abilityCooldownTimer = 0f;
        }

        private void OnEnable()
        {
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            // Reset animation when wave ends (entering Preparation phase)
            if (phase == GamePhase.Preparation)
            {
                ResetHeroAnimationState();
            }
        }

        private void ResetHeroAnimationState()
        {
            if (IsDead) return;

            // VFX coroutine'i kesilmeden once VFX'i deaktive et
            if (activeVFXInstance != null)
                activeVFXInstance.SetActive(false);

            // 1. Set animations to Idle
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.speed = 1f;
                // Clear triggers
                if (animator.parameters != null)
                {
                    foreach (var param in animator.parameters)
                    {
                        if (param.type == AnimatorControllerParameterType.Trigger &&
                            (param.name == "Attack" || param.name == "Ability"))
                        {
                            animator.ResetTrigger(param.name);
                        }
                    }
                }
                // Hard reset: immediately transition to Idle animation
                animator.Play("Idle", 0, 0f);
            }

            // 2. Clear attack and movement states
            isAttacking = false;
            isMovingToManualTarget = false;
            hasMoveDestination = false;
            StopAllCoroutines(); // PerformAttack korutinini durdurur
            ClearTarget();       // Hedefi ve engellemeyi temizler

            // 3. Re-ensure scale
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);

        }

        public void SetMoveDestination(Vector3 point)
        {
            moveDestination = point;
            hasMoveDestination = true;
            isMovingToManualTarget = true;
            
            // Immediately cancel current attack and target actions
            StopAllCoroutines();
            isAttacking = false;
            
            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetBool("IsMoving", true);
                animator.speed = IsFrozen() ? 0f : 1f;
            }
            
            ClearTarget();
        }

        public void SetSelectionState(HeroSelectionState state)
        {
            if (selectionIndicator != null)
                selectionIndicator.SetState(state);
        }

        protected override bool ShouldFollowPath() => false;
        protected override bool ShouldSeekEnemyBase() => false;
        protected override bool CanAcquireTarget() => !isMovingToManualTarget;

        private Coroutine activeVFXCoroutine;
        private GameObject activeVFXInstance;
        bool abilityPose;
        int abilityPoseHash;

        private void SpawnHeroVFX(HeroAbilityType abilityType, float delay)
        {
            Transform container = abilityVFXContainer;
            if (container == null)
                container = transform.Find("AbilityVFX");
            if (container == null)
            {
                return;
            }
            Transform vfxTransform = container.Find(abilityType.ToString());
            if (vfxTransform == null)
            {
                return;
            }

            if (activeVFXCoroutine != null)
                StopCoroutine(activeVFXCoroutine);

            bool oneShot = abilityType == HeroAbilityType.SwiftStrike
                || abilityType == HeroAbilityType.SolarSmite
                || abilityType == HeroAbilityType.SoulExecute;
            activeVFXInstance = vfxTransform.gameObject;
            activeVFXCoroutine = StartCoroutine(PlayAbilityVfx(vfxTransform.gameObject, delay, oneShot));
        }

        public static System.Collections.IEnumerator PlayAbilityVfx(GameObject vfx, float delay, bool oneShot)
        {
            ParticleSystem[] allPs = vfx.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < allPs.Length; i++)
            {
                ParticleSystem.MainModule main = allPs[i].main;
                main.playOnAwake = false;
                if (oneShot)
                    main.loop = false;
                allPs[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            vfx.SetActive(true);

            CFXR_Effect[] clearEffects = vfx.GetComponentsInChildren<CFXR_Effect>(true);
            for (int c = 0; c < clearEffects.Length; c++)
                clearEffects[c].clearBehavior = CFXR_Effect.ClearBehavior.None;

            for (int i = 0; i < allPs.Length; i++)
                allPs[i].Play(false);

            yield return new WaitForSeconds(delay);

            if (vfx != null)
                vfx.SetActive(false);
        }

        void StopAbilityVfx()
        {
            activeVFXCoroutine = null;
            abilityPose = false;
            abilityPoseHash = 0;
            if (activeVFXInstance == null) return;

            ParticleSystem[] systems = activeVFXInstance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] == null) continue;
                ParticleSystem.MainModule main = systems[i].main;
                main.playOnAwake = false;
                systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            activeVFXInstance.SetActive(false);
            activeVFXInstance = null;
        }

        protected override bool SuppressAttackHit()
        {
            return AbilityPoseActive();
        }

        protected override void OnLateTick()
        {
            if (!abilityPose || animator == null) return;

            bool transitioning = animator.IsInTransition(0);
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo next = transitioning ? animator.GetNextAnimatorStateInfo(0) : default;

            if (abilityPoseHash == 0)
            {
                if (transitioning && !next.IsName("Attack"))
                    abilityPoseHash = next.shortNameHash;
                else if (!transitioning && !current.IsName("Attack") && !current.IsName("Idle"))
                    abilityPoseHash = current.shortNameHash;
                return;
            }

            bool inPose = current.shortNameHash == abilityPoseHash
                || (transitioning && next.shortNameHash == abilityPoseHash);
            if (!inPose)
            {
                abilityPose = false;
                abilityPoseHash = 0;
            }
        }

        bool AbilityPoseActive()
        {
            if (!abilityPose || animator == null || abilityPoseHash == 0) return false;
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            if (current.shortNameHash == abilityPoseHash) return true;
            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == abilityPoseHash;
        }

        private void SpawnArrowRain(Vector3 center, float radius, int arrowCount)
        {
            GameObject arrowPrefab = unitData != null ? unitData.projectilePrefab : null;
            if (arrowPrefab == null) return;

            for (int i = 0; i < arrowCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * radius;
                Vector3 spawnPos = new Vector3(center.x + randomCircle.x, 12f, center.z + randomCircle.y);
                Quaternion spawnRot = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);

                GameObject arrow = Instantiate(arrowPrefab, spawnPos, spawnRot);
                Projectile proj = arrow.GetComponent<Projectile>();
                if (proj != null) Destroy(proj);
                arrow.AddComponent<FallingArrow>();
            }
        }

        protected override void TryMoveToManualDestination()
        {
            if (!hasMoveDestination) return;

            Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatDest = new Vector3(moveDestination.x, 0, moveDestination.z);

            if (Vector3.Distance(flatPos, flatDest) > 0.25f)
            {
                MoveTowardsTarget(moveDestination);
                HandleRotation(moveDestination);
                if (animator != null) animator.SetBool("IsMoving", true);
            }
            else 
            {
                isMovingToManualTarget = false;
                hasMoveDestination = false;
                if (animator != null) animator.SetBool("IsMoving", false);
            }
        }

        protected override void Update()
        {
            if (IsDead || IsLevelFinished()) return;
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.HoldsUnitMarch()) return;

            GamePhase currentPhase = PhaseManager.Instance != null ? PhaseManager.Instance.GetCurrentPhase() : GamePhase.Preparation;

            if (currentPhase == GamePhase.Combat)
            {
                base.Update();
                TickAbility();

                IDamageable currentTarget = GetTarget();
                if (!IsTargetAlive(currentTarget))
                {
                    if ((isAttacking || (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))) && !isMovingToManualTarget)
                    {
                        ResetHeroAnimationState();
                    }
                }

                if (!isAttacking && !isMovingToManualTarget && !IsBlocked && currentTarget == null && animator != null && animator.GetBool("IsMoving"))
                {
                    animator.SetBool("IsMoving", false);
                }
            }
            else 
            {
                // Clear attack or target when outside combat phase
                if (isAttacking || GetTarget() != null)
                {
                    ResetHeroAnimationState();
                }
                TryMoveToManualDestination();
            }
        }

        private void TickAbility()
        {
            if (heroConfig == null) return;

            if (abilityCooldownTimer > 0f)
            {
                abilityCooldownTimer -= Time.deltaTime;
            }
        }

        public bool CastAbility()
        {
            if (heroConfig == null || abilityCooldownTimer > 0f || IsDead) return false;

            if (TryUseAbility())
            {
                abilityCooldownTimer = ScaledAbilityCooldown;
                CheckAndPlayAbilityAnimation();

                AudioClip sfx = heroConfig.abilitySFX;
                if (sfx == null)
                {
#if UNITY_EDITOR
                    sfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/HeroAbility.wav");
#endif
                }
                if (AudioManager.Instance != null && sfx != null)
                {
                    AudioManager.Instance.PlaySFX(sfx);
                }

                if (TutorialDirector.Instance != null)
                    TutorialDirector.Instance.Notify("hero_skill");

                return true;
            }
            
            return false;
        }

        private void CheckAndPlayAbilityAnimation()
        {
            if (animator == null || heroConfig == null) return;

            // Only trigger "Ability" animation on strike/damage focused abilities
            switch (heroConfig.abilityType)
            {
                case HeroAbilityType.ArrowRain:
                case HeroAbilityType.SwiftStrike:
                case HeroAbilityType.SolarSmite:
                case HeroAbilityType.LifeDrain:
                case HeroAbilityType.SoulExecute:
                case HeroAbilityType.GroundSlam:
                case HeroAbilityType.PlagueCloud:
                    animator.SetTrigger("Ability");
                    abilityPose = true;
                    abilityPoseHash = 0;
                    break;
                
                // Passive or support abilities like healing do not play animation
                case HeroAbilityType.RallyHeal:
                case HeroAbilityType.HolyShield:
                case HeroAbilityType.FortifyTaunt:
                case HeroAbilityType.BoneArmor:
                case HeroAbilityType.ShadowStep:
                default:
                    break;
            }
        }

        private bool TryUseAbility()
        {
            if (heroConfig == null) return false;

            switch (heroConfig.abilityType)
            {
                case HeroAbilityType.RallyHeal:
                    if (GetHealth() >= GetMaxHealth()) return false;
                    Heal(GetMaxHealth() * 0.15f * ScaledAbilityPower);
                    SpawnHeroVFX(HeroAbilityType.RallyHeal, 2f);
                    return true;

                case HeroAbilityType.HolyShield:
                    if (GetHealth() / Mathf.Max(1f, GetMaxHealth()) <= 0.35f)
                        Heal(GetMaxHealth() * 0.2f * ScaledAbilityPower);
                    AddAbsorbShield(GetMaxHealth() * 0.35f * ScaledAbilityPower);
                    SpawnHeroVFX(HeroAbilityType.HolyShield, 2f);
                    return true;

                case HeroAbilityType.LifeDrain:
                    if (IsTargetAlive(GetTarget()) && GetTarget() is Unit target)
                    {
                        float dmg = GetDamage() * ScaledAbilityPower * 1.5f;
                        target.TakeDamage(dmg);
                        Heal(dmg * 0.5f);
                        SpawnHeroVFX(HeroAbilityType.LifeDrain, 2f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.BoneArmor:
                    AddAbsorbShield(GetMaxHealth() * 0.25f * ScaledAbilityPower);
                    SpawnHeroVFX(HeroAbilityType.BoneArmor, 2f);
                    return true;

                case HeroAbilityType.ArrowRain:
                    if (DealAbilityAreaDamage(heroConfig.abilityRadius, GetDamage() * ScaledAbilityPower * 2.0f))
                    {
                        SpawnHeroVFX(HeroAbilityType.ArrowRain, 2f);
                        SpawnArrowRain(transform.position, heroConfig.abilityRadius, 32);
                        return true;
                    }
                    return false;

                case HeroAbilityType.SwiftStrike:
                    if (IsTargetAlive(GetTarget()) && GetTarget() is Unit swiftTarget)
                    {
                        swiftTarget.TakeDamage(GetDamage() * ScaledAbilityPower * 2.5f);
                        SpawnHeroVFX(HeroAbilityType.SwiftStrike, 1f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.FortifyTaunt:
                    if (PullNearbyEnemies(heroConfig.abilityRadius))
                    {
                        SpawnHeroVFX(HeroAbilityType.FortifyTaunt, 2f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.SolarSmite:
                    if (DealAbilityAreaDamage(heroConfig.abilityRadius, GetDamage() * ScaledAbilityPower * 2.0f))
                    {
                        SpawnHeroVFX(HeroAbilityType.SolarSmite, 1f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.SoulExecute:
                    if (IsTargetAlive(GetTarget()) && GetTarget() is Unit execTarget &&
                        execTarget.GetHealth() / execTarget.GetMaxHealth() <= 0.40f)
                    {
                        execTarget.TakeDamage(GetDamage() * ScaledAbilityPower * 4.0f);
                        SpawnHeroVFX(HeroAbilityType.SoulExecute, 1.5f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.GroundSlam:
                    if (DealAbilityAreaDamage(heroConfig.abilityRadius, GetDamage() * ScaledAbilityPower * 2.2f))
                    {
                        SpawnHeroVFX(HeroAbilityType.GroundSlam, 2f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.PlagueCloud:
                    if (ApplyAbilityPoison(heroConfig.abilityRadius, GetDamage() * ScaledAbilityPower * 0.5f, 4f))
                    {
                        SpawnHeroVFX(HeroAbilityType.PlagueCloud, 2f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.ShadowStep:
                    if (IsTargetAlive(GetTarget()) && GetTarget() is Unit stepTarget)
                    {
                        Vector3 behind = stepTarget.transform.position - stepTarget.transform.forward * 1.2f;
                        transform.position = new Vector3(behind.x, transform.position.y, behind.z);
                        stepTarget.TakeDamage(GetDamage() * ScaledAbilityPower * 3.0f);
                        SpawnHeroVFX(HeroAbilityType.ShadowStep, 1f);
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        private bool DealAbilityAreaDamage(float radius, float damage)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, GetEnemyLayerMask());
            bool hitAnyone = false;
            foreach (var hit in hits)
            {
                IDamageable dmg = hit.GetComponentInParent<IDamageable>();
                if (dmg != null && !dmg.IsDead && dmg.GetSide() != GetSide())
                {
                    dmg.TakeDamage(damage);
                    hitAnyone = true;
                }
            }
            return hitAnyone;
        }

        private bool AnyEnemyInRange(float radius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, GetEnemyLayerMask());
            foreach (var hit in hits)
            {
                IDamageable dmg = hit.GetComponentInParent<IDamageable>();
                if (dmg != null && !dmg.IsDead && dmg.GetSide() != GetSide())
                    return true;
            }
            return false;
        }

        private bool ApplyAbilityPoison(float radius, float power, float duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, GetEnemyLayerMask());
            bool hitAnyone = false;
            foreach (var hit in hits)
            {
                Unit unit = hit.GetComponentInParent<Unit>();
                if (unit != null && !unit.IsDead && unit.GetSide() != GetSide())
                {
                    unit.AddStatusEffect(StatusEffectType.Poison, duration, power);
                    hitAnyone = true;
                }
            }
            return hitAnyone;
        }

        public override bool CanBlockMore()
        {
            PruneBlockedEnemies();
            return blockedEnemies.Count < 3;
        }

        private bool PullNearbyEnemies(float radius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, GetEnemyLayerMask());
            HashSet<Unit> seen = new HashSet<Unit>();
            bool pulled = false;
            foreach (var hit in hits)
            {
                if (!CanBlockMore()) break;
                Unit enemy = hit.GetComponentInParent<Unit>();
                if (enemy == null || enemy.IsDead || enemy.GetSide() == GetSide()) continue;
                if (!seen.Add(enemy)) continue;
                if (StartBlocking(enemy))
                    pulled = true;
            }
            return pulled;
        }

        private LayerMask GetEnemyLayerMask()
        {
            return (GetSide() == Side.Light) ? (1 << 7) : (1 << 6);
        }

        protected override void Die()
        {
            if (isDead) return;
            isDead = true;
            StopAllCoroutines();
            StopAbilityVfx();
            CancelHitFlash();

            SetSelectionState(HeroSelectionState.Hidden);
            onDeathCallback?.Invoke(this);
            
            // Release anyone blocking us
            if (currentBlocker != null)
            {
                currentBlocker.OnBlockedEnemyDied(this);
                currentBlocker = null;
            }
            isBlocked = false;

            // Release enemies blocked by us if any
            foreach (var enemy in blockedEnemies)
            {
                if (enemy != null) enemy.Unblock();
            }
            blockedEnemies.Clear();
            
            ClearAllStatusEffects();

            // Do not call base.Die() to prevent Destroy in Unit.Die()
            isDead = true;
            isMovingToManualTarget = false;
            hasMoveDestination = false;
            
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            if (animator != null) 
            {
                animator.speed = 1f;
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("Die");
            }
            if (healthBar != null) healthBar.SetVisible(false);


            // Death effect
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);

            if (AudioManager.Instance != null && unitData.deathSFX != null)
                AudioManager.Instance.PlaySFX(unitData.deathSFX);

            // Economy reward
            Side opponentSide = GetSide() == Side.Light ? Side.Dark : Side.Light;
            int reward = grantKillReward ? GetScaledKillReward() : 0;
            if (reward > 0 && CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCurrency(opponentSide, reward);
                if (SideController.Instance != null && opponentSide == SideController.Instance.GetPlayerSide())
                    TowerDefence.UI.CurrencyFloatUI.Show(transform.position, reward, opponentSide);
            }
        }

        public void Respawn(Vector3 position)
        {
            StopAbilityVfx();
            CancelHitFlash();
            isDead = false;
            isBlocked = false;
            currentBlocker = null;
            blockedEnemies.Clear();
            isMovingToManualTarget = false;
            hasMoveDestination = false;
            abilityCooldownTimer = heroConfig != null ? ScaledAbilityCooldown : abilityCooldownTimer;
            RefreshSlowFactor();

            currentHealth = GetMaxHealth();
            transform.position = new Vector3(position.x, 0.2f, position.z);
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
            
            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth, GetMaxHealth());
                healthBar.SetVisible(true);
            }

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
                animator.SetBool("IsMoving", false);
            }

            // Respawn efekti
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.UnitSpawn, transform.position, Quaternion.identity);

        }
    }
}
