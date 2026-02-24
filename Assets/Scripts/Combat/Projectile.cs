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

        public void Initialize(float _damage, float _explosionRadius, VFXType _vfxType, StatusEffectType _effectType = StatusEffectType.None, float _effectDuration = 0, float _effectPower = 0)
        {
            damage = _damage;
            explosionRadius = _explosionRadius;
            impactVFX = _vfxType;
            effectType = _effectType;
            effectDuration = _effectDuration;
            effectPower = _effectPower;
        }

        [SerializeField] private float explosionRadius = 0f;
        [SerializeField] private VFXType impactVFX = VFXType.LightImpact;

        private Transform target;

        public void Seek(Transform _target)
        {
            target = _target;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            if (dir.magnitude <= distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
            transform.LookAt(target);
        }

        private void HitTarget()
        {
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnVFX(impactVFX, transform.position, transform.rotation);
            }

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
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
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
