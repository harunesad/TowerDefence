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
        private float combatStartTime;

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
            combatStartTime = Time.time;
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log("Combat Phase Started!");
        }

        public void SkipPreparation()
        {
            if (currentPhase == GamePhase.Preparation)
            {
                // Erken Çağırma Bonusu (Early Call Bonus)
                if (timer > 0)
                {
                    int bonusGold = Mathf.CeilToInt(timer * 2f); // Saniye başına 2 Altın
                    int bonusKarma = Mathf.CeilToInt(timer * 0.5f); // 2 Saniyede 1 Karma

                    if (CurrencyManager.Instance != null && SideController.Instance != null)
                    {
                        CurrencyManager.Instance.AddCurrency(SideController.Instance.GetPlayerSide(), bonusGold);
                        Debug.Log($"<color=yellow>[Early Call Bonus]</color> +{bonusGold} Gold awarded for calling wave {timer:F1}s early!");
                    }
                    
                    if (MetaProgressionManager.Instance != null && bonusKarma > 0)
                    {
                        MetaProgressionManager.Instance.AddKarma(bonusKarma);
                        Debug.Log($"<color=magenta>[Early Call Bonus]</color> +{bonusKarma} Karma awarded!");
                    }
                }

                timer = 0;
                OnTimerUpdated?.Invoke(timer);
                StartCombatPhase();
            }
        }

        public GamePhase GetCurrentPhase() => currentPhase;
        public float GetRemainingTime() => timer;

        private void CheckWaveCompletion()
        {
            if (currentPhase != GamePhase.Combat) return;
            if (GameManager.Instance.GetCurrentState() != GameState.Playing) return;

            // Senkronizasyon için çok kısa bir süre bekle (En az 1.5 saniye)
            if (Time.time < combatStartTime + 1.5f) return;

            // Spawner'lar hala üretim yapıyor mu VEYA henüz dalgaya başladılar mı kontrol et
            bool anySpawnerBusy = false;
            foreach (var spawner in Spawner.AllSpawners)
            {
                if (spawner == null || spawner.isPlayerSpawner) continue;

                // Spawner henüz PhaseManager'ın dalga index'ine ulaşmamışsa VEYA şu an aktif olarak doğuruyorsa
                if (spawner.CurrentWaveIndex < currentWaveIndex || spawner.IsSpawning)
                {
                    anySpawnerBusy = true;
                    break;
                }
            }

            if (anySpawnerBusy) return;

            // Sadece DÜŞMAN birimlerini kontrol et (Oyuncunun kendi askerleri dalgayı bloklamasın)
            Side playerSide = SideController.Instance.GetPlayerSide();
            int enemyCount = 0;

            foreach (var unit in Unit.AllUnits)
            {
                if (unit != null && !unit.IsDead && unit.GetSide() != playerSide)
                {
                    enemyCount++;
                }
            }
            
            // Eğer sahada düşman kalmadıysa ve spawner'lar bittiyse dalga bitmiştir
            if (enemyCount == 0)
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
