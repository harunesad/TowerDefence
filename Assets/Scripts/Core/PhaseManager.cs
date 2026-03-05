using UnityEngine;
using System;
using TowerDefence.Combat;
using TowerDefence.Data;

namespace TowerDefence.Core
{
    public enum GamePhase
    {
        Preparation,
        Combat
    }

    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<float> OnTimerUpdated;

        [Header("Settings")]
        [SerializeField] private float prepPhaseDuration = 30f;
        
        private GamePhase currentPhase;
        private float timer;
        private bool isTimerActive;
        private int currentWaveIndex = 0;

        public int GetCurrentWaveIndex() => currentWaveIndex;

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

        private void Start()
        {
            // StartPreparationPhase(); // Artık otomatik başlamayacak
        }

        private void Update()
        {
            if (isTimerActive)
            {
                timer -= Time.deltaTime;
                OnTimerUpdated?.Invoke(timer);

                if (timer <= 0)
                {
                    timer = 0;
                    isTimerActive = false;
                    StartCombatPhase();
                }
            }
            else if (currentPhase == GamePhase.Combat)
            {
                CheckWaveCompletion();
            }
        }

        public void StartPreparationPhase()
        {
            currentPhase = GamePhase.Preparation;
            timer = prepPhaseDuration;
            isTimerActive = true;
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log("Preparation Phase Started!");
        }

        public void StartCombatPhase()
        {
            currentPhase = GamePhase.Combat;
            isTimerActive = false;
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log("Combat Phase Started!");
        }

        public void SkipPreparation()
        {
            if (currentPhase == GamePhase.Preparation)
            {
                StartCombatPhase();
            }
        }

        public GamePhase GetCurrentPhase() => currentPhase;
        public float GetRemainingTime() => timer;

        private void CheckWaveCompletion()
        {
            if (currentPhase != GamePhase.Combat) return;
            if (GameManager.Instance.GetCurrentState() != GameState.Playing) return;

            // Sahnedeki düşman birimlerini kontrol et 
            Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            
            // Eğer sahada birim kalmadıysa dalga bitmiştir
            // Not: Burada Spawner'ın o dalga için üretimini bitirdiğinden emin olunmalı.
            // Spawner.IsSpawning kontrolü eklenebilir. Şimdilik basitçe birim yoksa biter.
            if (units.Length == 0)
            {
                WaveCompleted();
            }
        }

        public void ResetWaveIndex()
        {
            currentWaveIndex = 0;
            Debug.Log("Core: Wave Index Reset.");
        }

        private void WaveCompleted()
        {
            Debug.Log($"Wave {currentWaveIndex + 1} Completed!");

            LevelData level = CampaignManager.Instance.GetCurrentLevel();
            if (level != null && currentWaveIndex < level.waves.Count - 1)
            {
                currentWaveIndex++;
                StartPreparationPhase();
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Victory);
            }
        }
    }
}
