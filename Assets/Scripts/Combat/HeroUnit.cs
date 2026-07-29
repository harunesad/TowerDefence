using UnityEngine;
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

            // Tıklanabilmesi için Collider eksikse dinamik olarak ekle (Root objesinde olmasını garanti ediyoruz)
            if (GetComponent<Collider>() == null)
            {
                CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
                col.height = 2f;
                col.radius = 0.5f;
                col.center = new Vector3(0, 1f, 0);
                col.isTrigger = false;
            }

            // Ölçeği ayarla (Gereksinim: Herolar scale (1.5, 1.5, 1.5))
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
            // Dalga bittiğinde (Hazırlık fazına geçildiğinde) animasyonu sıfırla
            if (phase == GamePhase.Preparation)
            {
                ResetHeroAnimationState();
            }
        }

        private void ResetHeroAnimationState()
        {
            if (IsDead) return;

            // 1. Animasyonları Idle'a çek
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.speed = 1f;
                // Triggers temizliği
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Ability");
                // Sert sıfırlama: Idle animasyonuna anında geç
                animator.Play("Idle", 0, 0f);
            }

            // 2. Saldırı ve Hareket durumlarını temizle
            isAttacking = false;
            isMovingToManualTarget = false;
            hasMoveDestination = false;
            StopAllCoroutines(); // PerformAttack korutinini durdurur
            ClearTarget();       // Hedefi ve engellemeyi temizler

            // 3. Ölçeği tekrar garanti et
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);

            Debug.Log($"[HeroReset] {gameObject.name} FORCED to Idle and state cleared.");
        }

        public void SetMoveDestination(Vector3 point)
        {
            moveDestination = point;
            hasMoveDestination = true;
            isMovingToManualTarget = true;
            
            // Mevcut saldırı ve hedef eylemlerini anında iptal et
            StopAllCoroutines();
            isAttacking = false;
            
            if (animator != null)
            {
                animator.speed = 1f;
                animator.ResetTrigger("Attack");
                animator.SetBool("IsMoving", true);
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
                if (animator != null) animator.SetBool("IsMoving", false);
                Debug.Log($"[STATE] MOVE→IDLE (destination reached). hasMoveDest={hasMoveDestination}");
            }
        }

        protected override void Update()
        {
            if (IsDead) return;

            GamePhase currentPhase = PhaseManager.Instance != null ? PhaseManager.Instance.GetCurrentPhase() : GamePhase.Preparation;

            if (currentPhase == GamePhase.Combat)
            {
                base.Update();
                TickAbility();

                IDamageable currentTarget = GetTarget();
                if (currentTarget == null || currentTarget.IsDead)
                {
                    if ((isAttacking || (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))) && !hasMoveDestination)
                    {
                        Debug.Log($"[STATE] target lost → RESET");
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
                // Combat fazı dışındayken saldırı veya hedef varsa temizle
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
                return;
            }

            if (TryUseAbility())
            {
                abilityCooldownTimer = heroConfig.abilityCooldown;
                CheckAndPlayAbilityAnimation();
            }
        }

        private void CheckAndPlayAbilityAnimation()
        {
            if (animator == null || heroConfig == null) return;

            // Sadece vuruş/hasar odaklı yeteneklerde "Ability" animasyonunu tetikle
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
                    break;
                
                // Pasif veya iyileştirme gibi destek yetenekleri animasyon oynatmaz
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
                    Heal(GetMaxHealth() * 0.15f * heroConfig.abilityPower);
                    if (VFXManager.Instance != null) VFXManager.Instance.SpawnVFX(VFXType.HealingAura, transform.position, Quaternion.identity);
                    return true;

                case HeroAbilityType.HolyShield:
                    if (GetHealth() / GetMaxHealth() <= 0.35f)
                    {
                        Heal(GetMaxHealth() * 0.2f * heroConfig.abilityPower);
                        if (VFXManager.Instance != null) VFXManager.Instance.SpawnVFX(VFXType.HealingAura, transform.position, Quaternion.identity);
                        return true;
                    }
                    return false;

                case HeroAbilityType.ArrowRain:
                    return DealAbilityAreaDamage(heroConfig.abilityRadius, heroConfig.abilityPower * 25f);

                case HeroAbilityType.SwiftStrike:
                    if (GetTarget() is Unit swiftTarget && !swiftTarget.IsDead)
                    {
                        swiftTarget.TakeDamage(heroConfig.abilityPower * 18f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.FortifyTaunt:
                    return PullNearbyEnemies(heroConfig.abilityRadius);

                case HeroAbilityType.SolarSmite:
                    return DealAbilityAreaDamage(heroConfig.abilityRadius, heroConfig.abilityPower * 35f);

                case HeroAbilityType.LifeDrain:
                    if (GetTarget() is Unit target && !target.IsDead)
                    {
                        float dmg = heroConfig.abilityPower * 20f;
                        target.TakeDamage(dmg);
                        Heal(dmg * 0.5f);
                        if (VFXManager.Instance != null) VFXManager.Instance.SpawnVFX(VFXType.HealingAura, transform.position, Quaternion.identity);
                        return true;
                    }
                    return false;

                case HeroAbilityType.SoulExecute:
                    if (GetTarget() is Unit execTarget && !execTarget.IsDead &&
                        execTarget.GetHealth() / execTarget.GetMaxHealth() <= 0.25f)
                    {
                        execTarget.TakeDamage(heroConfig.abilityPower * 60f);
                        return true;
                    }
                    return false;

                case HeroAbilityType.GroundSlam:
                    return DealAbilityAreaDamage(heroConfig.abilityRadius, heroConfig.abilityPower * 40f);

                case HeroAbilityType.PlagueCloud:
                    return ApplyAbilityPoison(heroConfig.abilityRadius, heroConfig.abilityPower * 8f, 4f);

                case HeroAbilityType.BoneArmor:
                    if (GetHealth() >= GetMaxHealth()) return false;
                    Heal(GetMaxHealth() * 0.12f * heroConfig.abilityPower);
                    if (VFXManager.Instance != null) VFXManager.Instance.SpawnVFX(VFXType.HealingAura, transform.position, Quaternion.identity);
                    return true;

                case HeroAbilityType.ShadowStep:
                    if (GetTarget() is Unit stepTarget && !stepTarget.IsDead)
                    {
                        Vector3 behind = stepTarget.transform.position - stepTarget.transform.forward * 1.2f;
                        transform.position = new Vector3(behind.x, transform.position.y, behind.z);
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

        private bool PullNearbyEnemies(float radius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, GetEnemyLayerMask());
            bool pulled = false;
            foreach (var hit in hits)
            {
                Unit enemy = hit.GetComponentInParent<Unit>();
                if (enemy != null && !enemy.IsDead && enemy.GetSide() != GetSide() && CanBlockMore())
                {
                    StartBlocking(enemy);
                    pulled = true;
                }
            }
            return pulled;
        }

        private LayerMask GetEnemyLayerMask()
        {
            return (GetSide() == Side.Light) ? (1 << 7) : (1 << 6);
        }

        protected override void Die()
        {
            SetSelectionState(HeroSelectionState.Hidden);
            onDeathCallback?.Invoke(this);
            
            // Bizi engelleyen varsa onu boşa çıkar
            if (currentBlocker != null)
            {
                currentBlocker.OnBlockedEnemyDied(this);
                currentBlocker = null;
            }
            isBlocked = false;

            // Bizim engellediğimiz düşmanlar varsa onları serbest bırak
            foreach (var enemy in blockedEnemies)
            {
                if (enemy != null) enemy.Unblock();
            }
            blockedEnemies.Clear();

            // Unit.Die()'daki Destroy'u engellemek için base.Die() çağırmıyoruz
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

            Debug.Log($"[HeroUnit] {gameObject.name} is down. Released units and cleared move state.");

            // Ölüm efekti
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnVFX(VFXType.UnitDeath, transform.position, Quaternion.identity);

            if (AudioManager.Instance != null && unitData.deathSFX != null)
                AudioManager.Instance.PlaySFX(unitData.deathSFX);

            // Ekonomi ödülü
            Side opponentSide = GetSide() == Side.Light ? Side.Dark : Side.Light;
            CurrencyManager.Instance.AddCurrency(opponentSide, unitData.killReward);
        }

        public void Respawn(Vector3 position)
        {
            isDead = false;
            isBlocked = false;
            currentBlocker = null;
            blockedEnemies.Clear();
            isMovingToManualTarget = false;
            hasMoveDestination = false;

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

            Debug.Log($"[HeroUnit] {gameObject.name} respawned and state fully reset.");
        }
    }
}
