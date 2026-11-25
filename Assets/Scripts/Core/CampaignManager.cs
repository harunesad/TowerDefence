using UnityEngine;
using UnityEngine.SceneManagement;
using TowerDefence.Data;
using TowerDefence.Combat;
namespace TowerDefence.Core
{
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        [SerializeField] private LevelData currentSelectedLevel;

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

        [Header("Environment & Background")]
        [SerializeField] private LevelThemeEnvironmentConfig environmentConfig;
        public void SetEnvironmentConfig(LevelThemeEnvironmentConfig config) => environmentConfig = config;

        [SerializeField] private System.Collections.Generic.List<LevelData> allLevels;

        public bool AllLevelsCleared(int difficulty)
        {
            if (allLevels == null || allLevels.Count == 0 || MetaProgressionManager.Instance == null) return false;
            bool any = false;
            for (int i = 0; i < allLevels.Count; i++)
            {
                LevelData level = allLevels[i];
                if (level == null || string.IsNullOrEmpty(level.levelID)) continue;
                any = true;
                if (!MetaProgressionManager.Instance.IsLevelCompleted(level.levelID, difficulty)) return false;
            }
            return any;
        }

        public LevelData FindLevel(string levelID)
        {
            if (allLevels == null || string.IsNullOrEmpty(levelID)) return null;
            return allLevels.Find(level => level != null && level.levelID == levelID);
        }

        public int GetPlannedWaveCount(LevelData level)
        {
            if (level == null || level.waves == null) return 0;
            int count = level.waves.Count;
            if (currentDifficultyLevel != 1) return count;
            if (level.levelID == "Level1") return Mathf.Min(3, count);
            if (level.levelID == "Level2") return Mathf.Min(4, count);
            return count;
        }

        public static int ScaleGroupCount(string levelID, int count)
        {
            if (count <= 1) return Mathf.Max(0, count);
            if (Instance == null || Instance.currentDifficultyLevel != 1) return count;
            if (levelID == "Level1") return Mathf.Max(1, count / 2);
            if (levelID == "Level2") return Mathf.Max(1, Mathf.RoundToInt(count * 0.7f));
            return count;
        }

        public void SelectLevel(LevelData level)
        {
            currentSelectedLevel = level;
        }

        private GameObject currentMapInstance;
        private bool rewardGrantedThisAttempt;
        private bool grantCrystalsOnSpawn = true;

        public void LoadSelectedLevel()
        {
            if (currentSelectedLevel != null
                && TutorialDirector.Instance != null
                && TutorialDirector.Instance.LevelClosed(currentSelectedLevel.levelID))
                return;

            grantCrystalsOnSpawn = true;
            if (currentSelectedLevel != null)
            {
                // Sahne yuklendiginde yeni haritayi spawn etmek icin abone ol
                SceneManager.sceneLoaded += OnLevelSceneLoaded;
                SceneLoader.EnsureInstance();
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadScene(currentSelectedLevel.sceneIndex);
                }
            }
        }

        public void RestartLevel()
        {
            grantCrystalsOnSpawn = false;
            if (currentSelectedLevel != null)
            {
                // Sahne yuklendiginde haritayi tekrar kurmasi icin abone ol
                SceneManager.sceneLoaded += OnLevelSceneLoaded;
                SceneLoader.EnsureInstance();
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadScene(currentSelectedLevel.sceneIndex);
                }
                
            }
        }

        private void OnLevelSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Sadece bir kez calismasi icin cikar
            SceneManager.sceneLoaded -= OnLevelSceneLoaded;
            
            // Sahne yukleme bitince spawn et
            SpawnMap();
        }

        private void SpawnMap()
        {
            if (currentSelectedLevel == null) return;

            // 3. Spawning: Generate map
            if (currentSelectedLevel.mapPrefab != null)
            {
                currentMapInstance = Instantiate(currentSelectedLevel.mapPrefab, currentSelectedLevel.mapPrefab.transform.position, currentSelectedLevel.mapPrefab.transform.rotation);
                currentMapInstance.name = "[MAP] " + currentSelectedLevel.levelName;
            }
            else
            {
                Debug.LogError($"CampaignManager: Map Prefab missing on {currentSelectedLevel.levelName}!");
            }

            // 4. Sistem Resetleri
            rewardGrantedThisAttempt = false;
            CurrencyManager.Instance.InitializeFromLevel(currentSelectedLevel);
            if (grantCrystalsOnSpawn)
                GrantLevelStartCrystals();
            grantCrystalsOnSpawn = true;
            PhaseManager.Instance.ResetWaveIndex();
            
            if (TowerDefence.Combat.LivesManager.Instance != null)
                TowerDefence.Combat.LivesManager.Instance.ResetLives();

            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.SetSpeedX1();

            GameManager.Instance.ChangeState(GameState.Playing);
            PhaseManager.Instance.StartPreparationPhase();

            // Yeni: Haritayi ekrana tam ortala (HUD payi dahil)
            if (currentMapInstance != null)
            {
                CenterCameraAtRuntime(currentMapInstance);
            }

            if (HeroManager.Instance != null)
                HeroManager.Instance.InitializeForLevel();

            ApplyLevelEnvironment(currentSelectedLevel);
        }

        private int currentDifficultyLevel = 1;
        public int CurrentDifficultyLevel => currentDifficultyLevel;

        public float GetCombatDifficultyMultiplier()
        {
            if (currentDifficultyLevel == 2) return 1.5f;
            if (currentDifficultyLevel >= 3) return 2f;
            return 1f;
        }

        public static float RewardDifficultyFactor(int globalDifficulty)
        {
            if (globalDifficulty >= 3) return 2f;
            if (globalDifficulty == 2) return 1.5f;
            return 1f;
        }

        public static void CalculateClearReward(int stars, int levelDifficulty, int globalDifficulty, out int karma, out int crystals)
        {
            float scale = Mathf.Max(1, levelDifficulty) * RewardDifficultyFactor(globalDifficulty);
            int safeStars = Mathf.Max(0, stars);
            karma = Mathf.RoundToInt(25f * safeStars * scale);
            crystals = Mathf.RoundToInt(2f * safeStars * scale);
        }

        public static void CalculateManualPayout(int oldStars, int newStars, int levelDifficulty, int globalDifficulty, out int karma, out int crystals, out bool offerAdDouble)
        {
            CalculateClearReward(newStars, levelDifficulty, globalDifficulty, out int earnedKarma, out int earnedCrystals);
            if (oldStars <= 0)
            {
                karma = earnedKarma;
                crystals = earnedCrystals;
                offerAdDouble = false;
                return;
            }

            if (newStars > oldStars)
            {
                CalculateClearReward(oldStars, levelDifficulty, globalDifficulty, out int previousKarma, out int previousCrystals);
                karma = Mathf.Max(0, earnedKarma - previousKarma);
                crystals = Mathf.Max(0, earnedCrystals - previousCrystals);
                offerAdDouble = false;
                return;
            }

            karma = Mathf.RoundToInt(earnedKarma * 0.4f);
            crystals = Mathf.RoundToInt(earnedCrystals * 0.4f);
            offerAdDouble = karma > 0 || crystals > 0;
        }

        public void SetDifficulty(int difficulty)
        {
            if (difficulty >= 1 && difficulty <= MetaProgressionManager.Instance.GetHighestUnlockedGlobalDifficulty())
            {
                currentDifficultyLevel = difficulty;
            }
        }

        public void AllowAnotherCompletionReward()
        {
            rewardGrantedThisAttempt = false;
        }

        public (int karma, int crystals) CompleteCurrentLevel(int stars, int karmaReward, int crystalReward)
        {
            if (currentSelectedLevel == null || MetaProgressionManager.Instance == null)
                return (0, 0);
            if (rewardGrantedThisAttempt)
                return (0, 0);
            rewardGrantedThisAttempt = true;


            int karmaToGrant = karmaReward;
            int crystalsToGrant = crystalReward;
            string levelID = currentSelectedLevel.levelID;

            if (karmaToGrant > 0)
                MetaProgressionManager.Instance.AddKarma(karmaToGrant);
            if (crystalsToGrant > 0)
                MetaProgressionManager.Instance.AddCrystals(crystalsToGrant);

            // Bolumu tamamlandi olarak kaydet
            MetaProgressionManager.Instance.SaveLevelProgress(levelID, stars, currentDifficultyLevel);

            // Bir sonraki seviyenin kilidini ac (Yazilimsal mantik)
            UnlockNextLevel();

            // Tum bolumler bittiyse bir sonraki zorlugu ac
            CheckDifficultyUnlock();
            return (karmaToGrant, crystalsToGrant);
        }

        private void GrantLevelStartCrystals()
        {
            if (MetaProgressionManager.Instance == null || currentSelectedLevel == null) return;
            int crystals = Mathf.RoundToInt(MetaProgressionManager.Instance.GetTotalAdditiveBonus(UpgradeType.CrystalStartFlatBonus, Side.Neutral));
            if (crystals <= 0) return;
            MetaProgressionManager.Instance.TryGrantCrystalVault(currentSelectedLevel.levelID, currentDifficultyLevel, crystals);
        }

        private void UnlockNextLevel()
        {
            int currentIndex = allLevels.IndexOf(currentSelectedLevel);
            int nextIndex = currentIndex + 1;
            
            if (nextIndex < allLevels.Count)
            {
                // Eger acilan yeni bolum, kayittaki en yuksek bolumden buyukse kaydi guncelle
                if (nextIndex > MetaProgressionManager.Instance.GetHighestUnlockedLevel(currentDifficultyLevel))
                {
                    MetaProgressionManager.Instance.UpdateHighestLevel(nextIndex, currentDifficultyLevel);
                }
            }
        }

        private void CheckDifficultyUnlock()
        {
            bool allCompleted = true;
            foreach (var level in allLevels)
            {
                if (!MetaProgressionManager.Instance.IsLevelCompleted(level.levelID, currentDifficultyLevel))
                {
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted && currentDifficultyLevel == MetaProgressionManager.Instance.GetHighestUnlockedGlobalDifficulty())
            {
                MetaProgressionManager.Instance.UnlockNextGlobalDifficulty();
            }
        }

        public bool IsLevelUnlocked(LevelData level)
        {
            if (allLevels == null || allLevels.Count == 0) return true;
            
            int index = allLevels.IndexOf(level);
            if (index < 0) return false;
            if (index == 0) return true;
            
            // Kayitli en yuksek bolum indeksinden kucuk veya esitse aciktir
            return index <= MetaProgressionManager.Instance.GetHighestUnlockedLevel(currentDifficultyLevel);
        }

        private void CenterCameraAtRuntime(GameObject mapInstance)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Harita sinirlarini tam oynanabilir alana gore hesapla (Waypointler uzerinden)
            Bounds bounds = new Bounds();
            bool first = true;
            PathWaypoints[] allPaths = mapInstance.GetComponentsInChildren<PathWaypoints>();

            foreach (var path in allPaths)
            {
                var pts = path.GetPathPoints();
                if (pts == null) continue;
                foreach (var wp in pts)
                {
                    if (first) { bounds = new Bounds(wp, Vector3.zero); first = false; }
                    else bounds.Encapsulate(wp);
                }
            }

            if (first) return; 

            Vector3 center = bounds.center;
            float maxDim = Mathf.Max(bounds.size.x, bounds.size.z);
            
            float camAngle = 65f;
            // Dinamik yukseklik: Harita boyutuna gore mesafeyi ayarla
            float height = Mathf.Max(30, maxDim * 0.7f); 
            
            // HUD Panelini (Alt Kisim) telafi etmek icin fokus noktasini biraz yukari kaydir
            // Trigonometrik ofset + Sabit birim ofset
            float uiOffsetZ = -height / Mathf.Tan(camAngle * Mathf.Deg2Rad); 
            float hudCompensation = 10f; // Haritayi dikeyde tam merkeze (HUD ustune) tasir

            // cam.transform.position = new Vector3(center.x, height, center.z + uiOffsetZ + hudCompensation);
            // cam.transform.rotation = Quaternion.Euler(camAngle, 0, 0);
            
            // orthographicSize artik kod tarafindan degistirilmiyor, editordeki deger korunuyor.
            if (!cam.orthographic) cam.fieldOfView = 40;

        }


        public void ApplyLevelEnvironment(LevelData level)
        {
            if (level == null) return;

            Camera cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();

            // Solid Color Modu (Varsayilan ve hafif mod)
            if (environmentConfig == null || environmentConfig.backgroundMode == LevelThemeEnvironmentConfig.BackgroundMode.SolidColor)
            {
                Color targetColor = Color.clear;

                if (level.customBackgroundColor.a > 0.01f)
                {
                    targetColor = level.customBackgroundColor;
                }
                else if (environmentConfig != null)
                {
                    targetColor = environmentConfig.GetColorForTheme(level.theme);
                }
                else
                {
                    targetColor = new Color(0.18f, 0.24f, 0.20f, 1f);
                }

                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = targetColor;
                }

            }
            else // Skybox Material Modu
            {
                Material targetSkybox = null;

                if (level.customSkybox != null)
                {
                    targetSkybox = level.customSkybox;
                }
                else if (environmentConfig != null)
                {
                    targetSkybox = environmentConfig.GetSkyboxForTheme(level.theme);
                }

                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                }

                if (targetSkybox != null)
                {
                    RenderSettings.skybox = targetSkybox;
                    DynamicGI.UpdateEnvironment();
                }
            }
        }

        public LevelData GetCurrentLevel() => currentSelectedLevel;

        /// <summary>Bir sonraki seviyeyi sec ve yukle. Son seviyedeyse false doner.</summary>
        public bool LoadNextLevel()
        {
            if (allLevels == null || allLevels.Count == 0 || currentSelectedLevel == null)
                return false;

            int currentIndex = allLevels.IndexOf(currentSelectedLevel);
            int nextIndex = currentIndex + 1;
            while (nextIndex < allLevels.Count
                && TutorialDirector.Instance != null
                && TutorialDirector.Instance.LevelClosed(allLevels[nextIndex].levelID))
                nextIndex++;

            if (nextIndex >= allLevels.Count)
            {
                return false;
            }

            SelectLevel(allLevels[nextIndex]);
            LoadSelectedLevel();
            return true;
        }
    }
}
