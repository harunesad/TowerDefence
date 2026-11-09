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
        private readonly Dictionary<Unit, float> nextSlowVfx = new Dictionary<Unit, float>();
        private const float VfxInterval = 1.2f;

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
            if (tower != null && tower.IsDisabled) return;
            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            ApplySlowEffect();
        }

        private void ApplySlowEffect()
        {
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

                if (VFXManager.Instance == null) continue;
                if (nextSlowVfx.TryGetValue(unit, out float nextAt) && Time.time < nextAt) continue;
                nextSlowVfx[unit] = Time.time + VfxInterval;
                VFXManager.Instance.SpawnVFX(VFXType.SlowEffect, unit.transform.position + Vector3.up, Quaternion.identity);
            }

            if (nextSlowVfx.Count == 0) return;
            List<Unit> stale = null;
            foreach (KeyValuePair<Unit, float> pair in nextSlowVfx)
            {
                if (pair.Key == null || pair.Key.IsDead || !slowed.Contains(pair.Key))
                {
                    stale ??= new List<Unit>();
                    stale.Add(pair.Key);
                }
            }
            if (stale == null) return;
            for (int i = 0; i < stale.Count; i++)
                nextSlowVfx.Remove(stale[i]);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, slowRadius);
        }
    }
}
