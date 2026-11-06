using UnityEngine;
using TowerDefence.Core;
using System.Collections.Generic;

namespace TowerDefence.Combat
{
    public class SlowTower : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float slowRadius = 5f;
        [SerializeField] private float slowPercentage = 0.5f; // %50 yavaşlatma
        [SerializeField] private LayerMask targetLayer;

        private Tower tower;
        private Side towerSide;
        private float vfxCooldown;
        private const float VfxInterval = 0.6f;

        private void Start()
        {
            tower = GetComponent<Tower>();
            if (tower == null) return;

            towerSide = tower.GetSide();
            TowerDefence.Data.TowerData data = tower.GetTowerData();
            if (data != null && data.effectPower > 0f)
                slowPercentage = Mathf.Clamp01(data.effectPower);

            int enemyLayer = towerSide == Side.Light ? 7 : 6;
            targetLayer = 1 << enemyLayer;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.HasMatchEnded()) return;
            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            ApplySlowEffect();
        }

        private void ApplySlowEffect()
        {
            if (vfxCooldown > 0f) vfxCooldown -= Time.deltaTime;
            bool spawnVfx = vfxCooldown <= 0f;

            float radius = tower != null ? tower.GetCurrentRange() : slowRadius;
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, targetLayer);
            HashSet<Unit> slowed = new HashSet<Unit>();
            foreach (Collider col in colliders)
            {
                Unit unit = col.GetComponent<Unit>();
                if (unit == null) unit = col.GetComponentInParent<Unit>();
                if (unit == null || unit.IsDead || unit.GetSide() == towerSide) continue;
                if (!slowed.Add(unit)) continue;

                unit.ApplySlow(slowPercentage, 0.35f);

                if (spawnVfx && VFXManager.Instance != null)
                    VFXManager.Instance.SpawnVFX(VFXType.SlowEffect, unit.transform.position, Quaternion.identity);
            }

            if (spawnVfx && slowed.Count > 0) vfxCooldown = VfxInterval;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, slowRadius);
        }
    }
}
