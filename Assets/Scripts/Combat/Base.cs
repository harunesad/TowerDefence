using UnityEngine;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using System;

namespace TowerDefence.Combat
{
    public class Base : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private Side baseSide;
        [SerializeField] private AudioClip damageSFX;
        
        private float currentHealth;
        private bool isGameOver = false;

        public bool IsDead => isGameOver;
        public Side GetSide() => baseSide;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (isGameOver) return;

            currentHealth -= amount;
            Debug.Log($"{gameObject.name} (Base) took {amount} damage. Current health: {currentHealth}");

            // Görsel ve İşitsel Geri Bildirim
            if (ScreenShake.Instance != null)
            {
                ScreenShake.Instance.Shake(0.2f, 0.4f);
            }

            if (AudioManager.Instance != null && damageSFX != null)
            {
                AudioManager.Instance.PlaySFX(damageSFX);
            }

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            isGameOver = true;
            Debug.Log($"{baseSide} Base Destroyed!");
            
            if (baseSide == SideController.Instance.GetPlayerSide())
            {
                GameManager.Instance.ChangeState(GameState.Defeat);
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Victory);
            }
        }
    }
}
