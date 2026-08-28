using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TowerDefence.Combat;

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
        
        [Header("Audio")]
        [SerializeField] private AudioClip battleBGM;
        [SerializeField] private AudioClip victorySFX;
        [SerializeField] private AudioClip defeatSFX;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                
                // Otomatik AdManager ekle
                if (GetComponent<AdManager>() == null)
                    gameObject.AddComponent<AdManager>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Eğer oyun Bootstrap sahnesinden başlıyorsa otomatik Menüye git
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                ChangeState(GameState.MainMenu);
                SceneManager.LoadScene(1); // 1 genelde MainMenu sahnesidir
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
                    if (AudioManager.Instance != null && battleBGM != null)
                        AudioManager.Instance.PlayBGM(battleBGM);
                    break;
                case GameState.Victory:
                    Debug.Log("Victory!");
                    if (AudioManager.Instance != null && victorySFX != null)
                        AudioManager.Instance.PlaySFX(victorySFX);
                    CampaignManager.Instance.CompleteCurrentLevel();
                    TriggerResultUI(true);
                    break;
                case GameState.Defeat:
                    Debug.Log("Defeat.");
                    if (AudioManager.Instance != null && defeatSFX != null)
                        AudioManager.Instance.PlaySFX(defeatSFX);
                    MetaProgressionManager.Instance.AddKarma(20); // Teselli ödülü
                    TriggerResultUI(false);
                    break;
            }
        }

        private void TriggerResultUI(bool isVictory)
        {
            var resultUI = FindFirstObjectByType<TowerDefence.UI.LevelResultUI>(FindObjectsInactive.Include);
            if (resultUI != null)
            {
                int remainingLives = LivesManager.Instance.GetCurrentLives();
                int totalLives = LivesManager.Instance.GetMaxLives();
                resultUI.Show(isVictory, remainingLives, totalLives);
            }
        }

        public GameState GetCurrentState() => currentState;
    }
}
