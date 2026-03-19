using UnityEngine;
using TowerDefence.Interfaces;
using TowerDefence.Core;
using System;

namespace TowerDefence.Combat
{
    public class Base : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private Side baseSide;
        [SerializeField] private AudioClip damageSFX;
        
        public bool IsDead => LivesManager.Instance != null && LivesManager.Instance.GetCurrentLives() <= 0;
        public Side GetSide() => baseSide;

        private void Start()
        {
            // Seviye başladığında sahnede bulunan üs oyuncunun tarafına geçer
            if (SideController.Instance != null)
            {
                baseSide = SideController.Instance.GetPlayerSide();
                Debug.Log($"Base initialized for side: {baseSide}");
            }
        }

        public void TakeDamage(float amount)
        {
            if (LivesManager.Instance == null) return;

            // Merkezi can sistemine bildir
            LivesManager.Instance.ReduceLives(baseSide);

            // Görsel ve İşitsel Geri Bildirim
            if (ScreenShake.Instance != null)
            {
                ScreenShake.Instance.Shake(0.3f, 0.5f);
            }

            if (AudioManager.Instance != null && damageSFX != null)
            {
                AudioManager.Instance.PlaySFX(damageSFX);
            }
        }
    }
}
