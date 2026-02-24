using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using System;
using TowerDefence.Combat;

namespace TowerDefence.Combat
{
    public enum AuraType
    {
        Healing,
        DamageBoost,
        RangeBoost
    }

    public class Aura : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AuraType type;
        [SerializeField] private float radius = 5f;
        [SerializeField] private float effectValue = 10f;
        [SerializeField] private float tickRate = 1f;
        [SerializeField] private LayerMask targetLayer;

        private Side auraSide;
        private float nextTickTime;

        private void Start()
        {
            // Genellikle kulenin veya birimin tarafını alır
            Tower tower = GetComponent<Tower>();
            if (tower != null) 
            {
                // towerSide'a erişmek için Tower sınıfına bir getter eklenebilir
                // auraSide = tower.GetSide();
            }
        }

        private void Update()
        {
            if (Time.time >= nextTickTime)
            {
                ApplyAuraEffect();
                nextTickTime = Time.time + tickRate;
            }
        }

        private void ApplyAuraEffect()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, targetLayer);
            foreach (Collider col in colliders)
            {
                Unit unit = col.GetComponent<Unit>();
                if (unit != null)
                {
                    // Burada sadece dost birimlere etki etmesi için kontrol eklenmeli
                    ExecuteEffect(unit);
                }
            }
        }

        private void ExecuteEffect(Unit unit)
        {
            switch (type)
            {
                case AuraType.Healing:
                    unit.Heal(effectValue);
                    // İyileşme efekti
                    if (VFXManager.Instance != null)
                    {
                        VFXManager.Instance.SpawnVFX(VFXType.HealingAura, unit.transform.position, Quaternion.identity);
                    }
                    break;
                case AuraType.DamageBoost:
                    // Unit sınıfına geçici hasar artışı eklenebilir
                    break;
                case AuraType.RangeBoost:
                    // Menzil artış mantığı
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
