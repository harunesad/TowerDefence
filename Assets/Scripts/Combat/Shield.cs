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

        public bool IsDead => false; // Kalkan ölmez, sadece kırılır
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
                    Debug.Log($"Shield absorbed damage! Current Shield: {currentShieldAmount}");
                }
                else
                {
                    float remainingDamage = amount - currentShieldAmount;
                    currentShieldAmount = 0;
                    isBroken = true;
                    Debug.Log("Shield Broken!");
                    
                    // Taşan hasarı ana birime ilet
                    SendMessageUpwards("TakeHealthDamage", remainingDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                // Kalkan yoksa hasar ana birime geçer
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
                Debug.Log("Shield Regenerated!");
            }
        }

        public float GetCurrentShield() => currentShieldAmount;
    }
}
