using UnityEngine;
using UnityEngine.SceneManagement;
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
        public event Action<int, int> OnWaveChanged; // (currentWave, totalWaves)

        [Header("Settings")]
        [SerializeField] private float prepPhaseDuration = 30f;
        [SerializeField] private AudioClip waveStartSFX;
        
        private GamePhase currentPhase;
        private float timer;
        private bool isTimerActive;
        private int currentWaveIndex = 0;
        private float combatStartTime;
        private bool matchLive;

        public int GetCurrentWaveIndex() => currentWaveIndex;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                if (waveStartSFX == null)
                {
#if UNITY_EDITOR
                    waveStartSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Gong.wav");
#endif
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            if (Instance != this) return;
            SceneManager.sceneUnloaded += OnBattleUnloading;
            Application.quitting += OnQuitting;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnBattleUnloading;
            Application.quitting -= OnQuitting;
        }

        private void OnQuitting()
        {
            matchLive = false;
            isTimerActive = false;
        }

        private void OnBattleUnloading(Scene scene)
        {
            if (scene.buildIndex <= 1) return;
            matchLive = false;
            isTimerActive = false;
            if (currentPhase == GamePhase.Combat)
                currentPhase = GamePhase.Preparation;
        }

        private void Update()
        {
            if (currentPhase == GamePhase.Preparation)
            {
                bool holdWave = TutorialDirector.Instance != null && TutorialDirector.Instance.HoldsOpeningWave(currentWaveIndex);
                if (holdWave) isTimerActive = false;
                else if (!isTimerActive && timer > 0f) isTimerActive = true;
            }

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

        private float ResolvePreparationDuration()
        {
            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level != null && level.waves != null && currentWaveIndex >= 0 && currentWaveIndex < level.waves.Count)
            {
                WaveData wave = level.waves[currentWaveIndex];
                if (wave != null && wave.timeBeforeNextWave > 0.01f)
                    return wave.timeBeforeNextWave;
            }

            return prepPhaseDuration;
        }

        public void StartPreparationPhase()
        {
            matchLive = true;
            currentPhase = GamePhase.Preparation;
            timer = ResolvePreparationDuration();
            bool holdWave = TutorialDirector.Instance != null && TutorialDirector.Instance.HoldsOpeningWave(currentWaveIndex);
            isTimerActive = !holdWave;
            OnPhaseChanged?.Invoke(currentPhase);
            NotifyWaveChanged();
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.OnPreparationBegan(currentWaveIndex);
            Debug.Log("Preparation Phase Started!");
        }

        public void StartCombatPhase()
        {
            currentPhase = GamePhase.Combat;
            isTimerActive = false;
            combatStartTime = Time.time;
            OnPhaseChanged?.Invoke(currentPhase);

            if (AudioManager.Instance != null && waveStartSFX != null)
            {
                AudioManager.Instance.PlaySFX(waveStartSFX);
            }

            Debug.Log("Combat Phase Started!");
            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.Notify("skip_wave");
        }

        public void SkipPreparation()
        {
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.HoldsOpeningWave(currentWaveIndex))
                return;

            if (currentPhase == GamePhase.Preparation)
            {
                // Erken Çağırma Bonusu (Early Call Bonus)
                if (timer > 0)
                {
                    int bonus = Mathf.CeilToInt(timer * 2f);
                    Side side = SideController.Instance != null ? SideController.Instance.GetPlayerSide() : Side.Light;
                    string icon = TowerDefence.UI.StatIcons.Price(side);

                    if (CurrencyManager.Instance != null && SideController.Instance != null)
                    {
                        CurrencyManager.Instance.AddCurrency(side, bonus);
                        Debug.Log($"<color=yellow>[Early Call Bonus]</color> +{bonus} {(side == Side.Dark ? "Soul" : "Gold")} awarded for calling wave {timer:F1}s early!");
                    }

                    if (bonus > 0)
                    {
                        TowerDefence.UI.WarningBannerUI.Show($"EARLY CALL BONUS! +{bonus} {icon}", 2.5f, new Color(1f, 0.85f, 0.2f, 1f), true);
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
            if (!matchLive) return;
            if (currentPhase != GamePhase.Combat) return;
            if (GameManager.Instance == null || GameManager.Instance.GetCurrentState() != GameState.Playing) return;

            // Senkronizasyon için çok kısa bir süre bekle (En az 1.5 saniye)
            if (Time.time < combatStartTime + 1.5f) return;

            // Spawner'lar hala üretim yapıyor mu VEYA henüz dalgaya başladılar mı kontrol et
            bool anySpawnerBusy = false;
            int enemySpawnerCount = 0;
            foreach (var spawner in Spawner.AllSpawners)
            {
                if (spawner == null || spawner.isPlayerSpawner || !spawner.gameObject.scene.isLoaded) continue;
                enemySpawnerCount++;

                if (spawner.CurrentWaveIndex < currentWaveIndex || spawner.IsSpawning)
                {
                    anySpawnerBusy = true;
                    break;
                }
            }

            if (enemySpawnerCount == 0 || anySpawnerBusy) return;

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
            NotifyWaveChanged();
            Debug.Log("Core: Wave Index Reset.");
        }

        private void WaveCompleted()
        {
            Debug.Log($"Wave {currentWaveIndex + 1} Completed!");

            LevelData level = CampaignManager.Instance != null ? CampaignManager.Instance.GetCurrentLevel() : null;
            if (level == null || level.waves == null || level.waves.Count == 0)
                return;

            int plannedWaves = CampaignManager.Instance != null
                ? CampaignManager.Instance.GetPlannedWaveCount(level)
                : level.waves.Count;
            if (currentWaveIndex < plannedWaves - 1)
            {
                currentWaveIndex++;
                NotifyWaveChanged();
                StartPreparationPhase();
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Victory);
            }
        }

        private void NotifyWaveChanged()
        {
            int totalWaves = 0;
            LevelData level = CampaignManager.Instance?.GetCurrentLevel();
            if (level != null)
                totalWaves = CampaignManager.Instance != null
                    ? CampaignManager.Instance.GetPlannedWaveCount(level)
                    : level.waves.Count;

            OnWaveChanged?.Invoke(currentWaveIndex + 1, totalWaves);
        }
    }
}
