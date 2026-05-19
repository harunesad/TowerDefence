using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// Oyun hızını (x1, x2, x3) ve duraklatma (Pause) durumunu Time.timeScale üzerinden yönetir.
    /// </summary>
    public class GameSpeedManager : MonoBehaviour
    {
        public static GameSpeedManager Instance { get; private set; }

        [Header("Speed Settings")]
        [SerializeField] private float[] speedLevels = { 1f, 2f, 3f };
        [SerializeField] private int defaultSpeedIndex = 0;

        private int currentSpeedIndex;
        private bool isPaused = false;
        private float savedTimeScale = 1f;

        // Hız değiştiğinde UI'ın dinleyebileceği event. Duraklatıldıysa -1 döner.
        public System.Action<int> OnSpeedChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetSpeed(defaultSpeedIndex);
        }

        private void OnDestroy()
        {
            // Sahne kapanırken timeScale'i sıfırla
            Time.timeScale = 1f;
        }

        /// <summary>0=x1, 1=x2, 2=x3</summary>
        public void SetSpeed(int index)
        {
            if (index < 0 || index >= speedLevels.Length) return;
            currentSpeedIndex = index;
            isPaused = false; // Hız butonuna basılınca duraklatmayı otomatik kapat
            Time.timeScale = speedLevels[index];
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Fiziği de senkronize et
            OnSpeedChanged?.Invoke(currentSpeedIndex);
        }

        public void SetSpeedX1() => SetSpeed(0);
        public void SetSpeedX2() => SetSpeed(1);
        public void SetSpeedX3() => SetSpeed(2);

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                savedTimeScale = speedLevels[currentSpeedIndex];
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0f;
                OnSpeedChanged?.Invoke(-1); // -1: Duraklatıldı
            }
            else
            {
                Time.timeScale = savedTimeScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                OnSpeedChanged?.Invoke(currentSpeedIndex);
            }
        }

        public bool IsPaused() => isPaused;

        /// <summary>Mevcut hız indeksini döndürür (0=x1, 1=x2, 2=x3)</summary>
        public int GetCurrentSpeedIndex() => currentSpeedIndex;

        /// <summary>Mevcut Time.timeScale değerini döndürür</summary>
        public float GetCurrentSpeed() => isPaused ? 0f : speedLevels[currentSpeedIndex];
    }
}
