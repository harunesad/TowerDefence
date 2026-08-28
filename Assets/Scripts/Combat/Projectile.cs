using UnityEngine;
using TowerDefence.Interfaces;
using TowerDefence.Core;

namespace TowerDefence.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float damage = 20f;
        private StatusEffectType effectType;
        private float effectDuration;
        private float effectPower;

        [SerializeField] private bool useArc = false;
        [SerializeField] private float arcGravity = 20f;

        private bool isBallistic;
        private Vector3 aimPoint;
        private Vector3 launchPos;
        private Vector3 velocity;
        private float flightTime;
        private float elapsed;
        private Side cachedTargetSide = Side.Dark;

        public void Initialize(float _damage, float _explosionRadius, VFXType _vfxType, StatusEffectType _effectType = StatusEffectType.None, float _effectDuration = 0, float _effectPower = 0, bool _useArc = false)
        {
            damage = _damage;
            explosionRadius = _explosionRadius;
            impactVFX = _vfxType;
            effectType = _effectType;
            effectDuration = _effectDuration;
            effectPower = _effectPower;
            useArc = _useArc;
        }

        [SerializeField] private float explosionRadius = 0f;
        [SerializeField] private VFXType impactVFX = VFXType.LightImpact;

        private Transform target;

        private Vector3 GetTargetCenter()
        {
            if (target == null) return transform.position;
            // Karakterlerin merkez noktası ayaklarında olduğu için mermiyi göğüs hizasına (1 birim yukarı) nişanla
            return target.position + Vector3.up * 1f;
        }

        public void Seek(Transform _target)
        {
            target = _target;

            // Balistik atış: atış anındaki hedef noktasını dondur (dumb lob — hedef yürürse ıskalanabilir)
            if (useArc && target != null)
            {
                isBallistic = true;
                launchPos = transform.position;
                aimPoint = GetTargetCenter();

                IDamageable cachedDmg = target.GetComponentInParent<IDamageable>();
                if (cachedDmg != null) cachedTargetSide = cachedDmg.GetSide();

                Vector3 flat = aimPoint - launchPos;
                flat.y = 0f;
                float horizontalDist = flat.magnitude;
                if (horizontalDist < 0.01f) horizontalDist = 0.01f;

                flightTime = Mathf.Max(0.1f, horizontalDist / speed);

                velocity = flat / flightTime;
                velocity.y = (aimPoint.y - launchPos.y + 0.5f * arcGravity * flightTime * flightTime) / flightTime;
                elapsed = 0f;

                transform.LookAt(aimPoint);
            }
        }

        private void Update()
        {
            if (target == null && !isBallistic)
            {
                Destroy(gameObject);
                return;
            }

            if (isBallistic)
            {
                UpdateBallistic();
                return;
            }

            Vector3 currentTargetPos = GetTargetCenter();
            Vector3 dir = currentTargetPos - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            if (dir.magnitude <= distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
            transform.LookAt(currentTargetPos);
        }

        private void UpdateBallistic()
        {
            elapsed += Time.deltaTime;

            if (elapsed >= flightTime)
            {
                transform.position = aimPoint;
                HitTarget();
                return;
            }

            velocity.y -= arcGravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;

            if (velocity.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }

        private void HitTarget()
        {
            if (explosionRadius > 0f)
            {
                Explode();
            }
            else
            {
                Damage(target);
            }

            Destroy(gameObject);
        }

        private void Explode()
        {
            if (target == null && !isBallistic) return;

            // Hedefin tarafını (Side) alalım ki sadece hedefle aynı taraftaki birimlere hasar verelim.
            IDamageable targetDamageable = target != null ? target.GetComponentInParent<IDamageable>() : null;
            Side targetSide = targetDamageable != null ? targetDamageable.GetSide() : cachedTargetSide;

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            System.Collections.Generic.HashSet<IDamageable> damagedEntities = new System.Collections.Generic.HashSet<IDamageable>();

            foreach (Collider collider in colliders)
            {
                IDamageable dmg = collider.GetComponentInParent<IDamageable>();
                if (dmg != null && dmg.GetSide() == targetSide && !damagedEntities.Contains(dmg))
                {
                    damagedEntities.Add(dmg);
                    Damage(collider.transform);
                }
            }
        }

        private void Damage(Transform enemy)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            Unit unit = enemy.GetComponent<Unit>();
            if (unit != null && effectType != StatusEffectType.None)
            {
                unit.AddStatusEffect(effectType, effectDuration, effectPower);
            }
        }
    }
}
