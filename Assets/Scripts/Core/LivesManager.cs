using UnityEngine;
using System;
using TowerDefence.Core;

namespace TowerDefence.Combat
{
    public class LivesManager : MonoBehaviour
    {
        public static LivesManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxLives = 20;

        public static event Action<int, int> OnLivesChanged; // current, max

        private int currentLives;
        private bool isGameOver = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            currentLives = maxLives;
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(currentLives, maxLives);
        }

        public void ReduceLives(Side side)
        {
            if (isGameOver) return;

            // Sadece oyuncunun tarafı için can eksiltir
            if (side == SideController.Instance.GetPlayerSide())
            {
                currentLives--;
                Debug.Log($"LivesManager: Unit leaked! Current lives: {currentLives}");

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

        public int GetCurrentLives() => currentLives;
        public int GetMaxLives() => maxLives;
    }
}
