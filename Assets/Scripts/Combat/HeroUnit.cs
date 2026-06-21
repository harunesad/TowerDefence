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

            SetSelectionState(HeroSelectionState.Passive);
            abilityCooldownTimer = 0f;
        }

        public void SetMoveDestination(Vector3 point)
        {
            moveDestination = point;
            hasMoveDestination = true;
            isMovingToManualTarget = true;
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
            }
        }

        protected override void Update()
        {
            if (IsDead) return;

            if (PhaseManager.Instance != null &&
                PhaseManager.Instance.GetCurrentPhase() == GamePhase.Combat)
            {
                base.Update();
                TickAbility();
            }
            else if (!IsAttacking() && GetTarget() == null)
            {
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
                abilityCooldownTimer = heroConfig.abilityCooldown;
        }

        private bool TryUseAbility()
        {
            if (heroConfig == null) return false;

            switch (heroConfig.abilityType)
            {
                case HeroAbilityType.RallyHeal:
                    Heal(GetMaxHealth() * 0.15f * heroConfig.abilityPower);
                    return true;

                case HeroAbilityType.HolyShield:
                    if (GetHealth() / GetMaxHealth() <= 0.35f)
                    {
                        Heal(GetMaxHealth() * 0.2f * heroConfig.abilityPower);
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
                    Heal(GetMaxHealth() * 0.12f * heroConfig.abilityPower);
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
            base.Die();
        }
    }
}
