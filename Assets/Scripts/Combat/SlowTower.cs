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

        private Side towerSide;

        private void Start()
        {
            Tower t = GetComponent<Tower>();
            if (t != null) towerSide = t.GetSide();
        }

        private void Update()
        {
            if (PhaseManager.Instance.GetCurrentPhase() != GamePhase.Combat) return;

            ApplySlowEffect();
        }

        private void ApplySlowEffect()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, slowRadius, targetLayer);
            foreach (Collider col in colliders)
            {
                Unit unit = col.GetComponent<Unit>();
                if (unit != null)
                {
                    // Sadece rakip birimlere etki etsin
                    if (unit.GetSide() != towerSide)
                    {
                        unit.ApplySlow(slowPercentage, 0.2f); // 0.2 saniye süren yavaşlatma (sürekli yenilenir)
                        
                        // Yavaşlatma efekti (VFX)
                        if (VFXManager.Instance != null)
                        {
                            VFXManager.Instance.SpawnVFX(VFXType.SlowEffect, unit.transform.position, Quaternion.identity);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, slowRadius);
        }
    }
}
