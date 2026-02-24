using UnityEngine;
using TowerDefence.Interfaces;

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
                currentShieldAmount -= amount;
                Debug.Log($"Shield absorbed damage! Current Shield: {currentShieldAmount}");
                
                if (currentShieldAmount <= 0)
                {
                    currentShieldAmount = 0;
                    isBroken = true;
                    Debug.Log("Shield Broken!");
                }
            }
            else
            {
                // Kalkan yoksa hasar ana birime geçer
                // Bu bileşen Unit bileşeniyle aynı objede olmalı ve Unit.TakeDamage çağrılmalı
                SendMessageUpwards("TakeHealthDamage", amount, SendMessageOptions.DontRequireReceiver);
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
