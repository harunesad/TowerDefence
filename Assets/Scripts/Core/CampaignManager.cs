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

        [SerializeField] private System.Collections.Generic.List<LevelData> allLevels;

        public void SelectLevel(LevelData level)
        {
            currentSelectedLevel = level;
            Debug.Log($"Level Selected: {level.levelName}");
        }

        private GameObject currentMapInstance;

        public void LoadSelectedLevel()
        {
            if (currentSelectedLevel != null)
            {
                // 1. Temizlik: Eski harita varsa yok et
                if (currentMapInstance != null)
                {
                    Destroy(currentMapInstance);
                }

                // 2. Sahne Yükleme Kontrolü
                if (SceneManager.GetActiveScene().buildIndex != currentSelectedLevel.sceneIndex)
                {
                    // Sahne yüklendiğinde spawn yapmak için event'e abone ol
                    SceneManager.sceneLoaded += OnLevelSceneLoaded;
                    SceneManager.LoadScene(currentSelectedLevel.sceneIndex);
                }
                else
                {
                    // Zaten sahnedeyiz, direkt spawn et
                    SpawnMap();
                }
            }
        }

        public void RestartLevel()
        {
            if (currentSelectedLevel != null)
            {
                // Her şeyi temizle
                if (currentMapInstance != null) Destroy(currentMapInstance);
                
                // Sahne yüklendiğinde haritayı tekrar kurması için abone ol
                SceneManager.sceneLoaded += OnLevelSceneLoaded;
                SceneManager.LoadScene(currentSelectedLevel.sceneIndex);
                
                Debug.Log($"[CampaignManager] Restarting Level: {currentSelectedLevel.levelName}");
            }
        }

        private void OnLevelSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Sadece bir kez çalışması için çıkar
            SceneManager.sceneLoaded -= OnLevelSceneLoaded;
            
            // Sahne yükleme bitince spawn et
            SpawnMap();
        }

        private void SpawnMap()
        {
            if (currentSelectedLevel == null) return;

            // 3. Spawning: Haritayı oluştur
            if (currentSelectedLevel.mapPrefab != null)
            {
                currentMapInstance = Instantiate(currentSelectedLevel.mapPrefab, Vector3.zero, Quaternion.identity);
                currentMapInstance.name = "[MAP] " + currentSelectedLevel.levelName;
                Debug.Log($"CampaignManager: Map spawned: {currentMapInstance.name}");
            }
            else
            {
                Debug.LogError($"CampaignManager: Map Prefab missing on {currentSelectedLevel.levelName}!");
            }

            // 4. Sistem Resetleri
            CurrencyManager.Instance.InitializeFromLevel(currentSelectedLevel);
            PhaseManager.Instance.ResetWaveIndex();
            GameManager.Instance.ChangeState(GameState.Playing);
            PhaseManager.Instance.StartPreparationPhase();

            // Yeni: Haritayı ekrana tam ortala (HUD payı dahil)
            if (currentMapInstance != null)
            {
                CenterCameraAtRuntime(currentMapInstance);
            }
        }

        public void CompleteCurrentLevel()
        {
            if (currentSelectedLevel == null) return;

            Debug.Log($"Level Completed: {currentSelectedLevel.levelName}");

            // Seviye bitiş ödülü (Karma)
            int reward = 50; // Temel ödül, LevelData'dan çekilebilir
            MetaProgressionManager.Instance.AddKarma(reward);

            // Bir sonraki seviyenin kilidini aç (Yazılımsal mantık)
            UnlockNextLevel();

            // Veriyi kaydet
            // SaveManager.Instance.SaveGame(); // Gelecek için
        }

        private void UnlockNextLevel()
        {
            int currentIndex = allLevels.IndexOf(currentSelectedLevel);
            int nextIndex = currentIndex + 1;
            
            if (nextIndex < allLevels.Count)
            {
                // Eğer açılan yeni bölüm, kayıttaki en yüksek bölümden büyükse kaydı güncelle
                if (nextIndex > MetaProgressionManager.Instance.GetHighestUnlockedLevel())
                {
                    MetaProgressionManager.Instance.UpdateHighestLevel(nextIndex);
                    Debug.Log($"Next Level Unlocked and Saved: {allLevels[nextIndex].levelName}");
                }
            }
        }

        public bool IsLevelUnlocked(LevelData level)
        {
            if (allLevels == null || allLevels.Count == 0) return true;
            
            int index = allLevels.IndexOf(level);
            if (index <= 0) return true; // İlk bölüm veya liste dışı (hata koruması) her zaman açık
            
            // Kayıtlı en yüksek bölüm indeksinden küçük veya eşitse açıktır
            return index <= MetaProgressionManager.Instance.GetHighestUnlockedLevel();
        }

        private void CenterCameraAtRuntime(GameObject mapInstance)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Harita sınırlarını tam oynanabilir alana göre hesapla (Waypointler üzerinden)
            Bounds bounds = new Bounds();
            bool first = true;
            PathWaypoints[] allPaths = mapInstance.GetComponentsInChildren<PathWaypoints>();

            foreach (var path in allPaths)
            {
                if (path.GetWaypoints() == null) continue;
                foreach (var wp in path.GetWaypoints())
                {
                    if (wp == null) continue;
                    if (first) { bounds = new Bounds(wp.position, Vector3.zero); first = false; }
                    else bounds.Encapsulate(wp.position);
                }
            }

            if (first) return; 

            Vector3 center = bounds.center;
            float maxDim = Mathf.Max(bounds.size.x, bounds.size.z);
            
            float camAngle = 65f;
            // Dinamik yükseklik: Harita boyutuna göre mesafeyi ayarla
            float height = Mathf.Max(30, maxDim * 0.7f); 
            
            // HUD Panelini (Alt Kısım) telafi etmek için fokus noktasını biraz yukarı kaydır
            // Trigonometrik ofset + Sabit birim ofset
            float uiOffsetZ = -height / Mathf.Tan(camAngle * Mathf.Deg2Rad); 
            float hudCompensation = 10f; // Haritayı dikeyde tam merkeze (HUD üstüne) taşır

            cam.transform.position = new Vector3(center.x, height, center.z + uiOffsetZ + hudCompensation);
            cam.transform.rotation = Quaternion.Euler(camAngle, 0, 0);
            
            if (cam.orthographic) cam.orthographicSize = maxDim * 0.45f;
            else cam.fieldOfView = 40;

            Debug.Log($"[CampaignManager] Camera centered on {mapInstance.name}. Center: {center}, Height: {height}");
        }

        public LevelData GetCurrentLevel() => currentSelectedLevel;
    }
}
