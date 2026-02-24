using UnityEngine;
using System;

namespace TowerDefence.Core
{
    public enum GameState
    {
        Bootstrap,
        MainMenu,
        LevelSelection,
        Playing,
        Paused,
        Victory,
        Defeat
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnGameStateChanged;

        [SerializeField] private GameState currentState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ChangeState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"Game State changed to: {newState}");
            OnGameStateChanged?.Invoke(newState);
            
            // State'e özgü eventler burada tetiklenebilir
            switch (newState)
            {
                case GameState.Playing:
                    // Oyun başladığında yapılacaklar
                    break;
                case GameState.Victory:
                    Debug.Log("Victory!");
                    CampaignManager.Instance.CompleteCurrentLevel();
                    break;
                case GameState.Defeat:
                    Debug.Log("Defeat.");
                    MetaProgressionManager.Instance.AddKarma(20); // Teselli ödülü
                    break;
            }
        }

        public GameState GetCurrentState() => currentState;
    }
}
