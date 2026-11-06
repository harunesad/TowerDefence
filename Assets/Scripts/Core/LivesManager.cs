using UnityEngine;
using System;
using TowerDefence.Core;
using TowerDefence.Core.Security;

namespace TowerDefence.Combat
{
    public class LivesManager : MonoBehaviour
    {
        public static LivesManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxLives = 20;

        public static event Action<int, int> OnLivesChanged; // current, max

        private ObscuredInt currentLives;
        private bool isGameOver = false;

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

            currentLives = maxLives;
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public void ReduceLives(Side side)
        {
            ReduceLives(side, 1);
        }

        public void ReduceLives(Side side, int amount)
        {
            if (isGameOver) return;
            if (GameManager.Instance != null && GameManager.Instance.HasMatchEnded()) return;
            if (amount < 1) amount = 1;

            if (side == SideController.Instance.GetPlayerSide())
            {
                currentLives -= amount;
                Debug.Log($"LivesManager: Unit leaked! Lives lost: {amount}. Current lives: {currentLives}");

                OnLivesChanged?.Invoke(currentLives, maxLives);

                if (currentLives <= 0)
                {
                    currentLives = 0;
                    GameOver();
                }
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            Debug.Log("LivesManager: Game Over! All lives lost.");
            GameManager.Instance.ChangeState(GameState.Defeat);
        }

        public void ResetLives()
        {
            currentLives = maxLives;
            isGameOver = false;
            OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public void SetCurrentLivesForDebug(int lives)
        {
            currentLives = Mathf.Clamp(lives, 1, maxLives);
            OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public int GetCurrentLives() => currentLives;
        public int GetMaxLives() => maxLives;
    }
}
