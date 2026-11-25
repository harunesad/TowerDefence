using UnityEngine;
using TowerDefence.Interfaces;
using TowerDefence.Core;

namespace TowerDefence.Combat
{
    public class Shield : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private float maxShieldAmount = 100f;
        [SerializeField] private float regenRate = 5f;
        [SerializeField] private float regenDelay = 3f;

        private float currentShieldAmount;
        private float lastDamageTime;
        private bool isBroken;

        public bool IsDead => false; // Shield does not die, only breaks
        public Side GetSide() => GetComponent<Unit>()?.GetSide() ?? GetComponent<Base>()?.GetSide() ?? Side.Neutral;

        private void Start()
        {
            currentShieldAmount = maxShieldAmount;
        }

        private void Update()
        {
            if (currentShieldAmount < maxShieldAmount && Time.time >= lastDamageTime + regenDelay)
            {
                RegenerateShield();
            }
        }

        public void TakeDamage(float amount)
        {
            lastDamageTime = Time.time;
            
            if (currentShieldAmount > 0)
            {
                if (currentShieldAmount >= amount)
                {
                    currentShieldAmount -= amount;
                }
                else
                {
                    float remainingDamage = amount - currentShieldAmount;
                    currentShieldAmount = 0;
                    isBroken = true;
                    
                    // Tasan hasari ana birime ilet
                    SendMessageUpwards("TakeHealthDamage", remainingDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                // Kalkan yoksa hasar ana birime gecer
                SendMessageUpwards("TakeHealthDamage", amount, SendMessageOptions.DontRequireReceiver);
            }
        }

        public float AbsorbDamage(float amount)
        {
            lastDamageTime = Time.time;
            
            if (currentShieldAmount >= amount)
            {
                currentShieldAmount -= amount;
                return 0f;
            }
            else
            {
                float remaining = amount - currentShieldAmount;
                currentShieldAmount = 0;
                isBroken = true;
                return remaining;
            }
        }

        private void RegenerateShield()
        {
            currentShieldAmount += regenRate * Time.deltaTime;
            if (currentShieldAmount > maxShieldAmount) currentShieldAmount = maxShieldAmount;
            
            if (isBroken && currentShieldAmount > 0)
            {
                isBroken = false;
            }
        }

        public float GetCurrentShield() => currentShieldAmount;
    }
}
