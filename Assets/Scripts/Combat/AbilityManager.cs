using UnityEngine;
using System;
using TowerDefence.Core;

namespace TowerDefence.Combat
{
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager Instance { get; private set; }

        public event Action<float, float> OnAbilityCooldownChanged;

        [Header("Settings")]
        [SerializeField] private float abilityCooldown = 60f;
        
        private float currentCooldown;
        private bool isReady = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!isReady)
            {
                currentCooldown -= Time.deltaTime;
                OnAbilityCooldownChanged?.Invoke(currentCooldown, abilityCooldown);

                if (currentCooldown <= 0)
                {
                    currentCooldown = 0;
                    isReady = true;
                }
            }
        }

        public void UseAbility()
        {
            if (!isReady) return;

            Side currentSide = SideController.Instance.GetPlayerSide();

            if (currentSide == Side.Light)
            {
                ExecuteLightAbility();
            }
            else
            {
                ExecuteDarkAbility();
            }

            isReady = false;
            currentCooldown = abilityCooldown;
        }

        private void ExecuteLightAbility()
        {
            if (SideController.Instance == null) return;

            Side playerSide = SideController.Instance.GetPlayerSide();
            Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (Unit u in units)
            {
                if (u == null || u.IsDead || u.GetSide() != playerSide) continue;
                u.Heal(50f);
            }
        }

        private void ExecuteDarkAbility()
        {
            if (SideController.Instance == null) return;

            Side opponentSide = SideController.Instance.GetOpponentSide();
            Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in towers)
            {
                if (t == null || t.IsDead || t.GetSide() != opponentSide) continue;
                t.Disable(5f);
            }
        }

        public bool IsReady() => isReady;
    }
}
