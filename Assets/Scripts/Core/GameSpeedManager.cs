using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// Oyun hızını (x1, x2, x3) Time.timeScale üzerinden yönetir.
    /// Time.timeScale tüm Unity sistemlerini (hareket, animasyon, fizik, zamanlayıcılar) etkiler.
    /// </summary>
    public class GameSpeedManager : MonoBehaviour
    {
        public static GameSpeedManager Instance { get; private set; }

        [Header("Speed Settings")]
        [SerializeField] private float[] speedLevels = { 1f, 2f, 3f };
        [SerializeField] private int defaultSpeedIndex = 0;

        private int currentSpeedIndex;

        // Hız değiştiğinde UI'ın dinleyebileceği event
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
            Time.timeScale = speedLevels[index];
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Fiziği de senkronize et
            OnSpeedChanged?.Invoke(currentSpeedIndex);
        }

        public void SetSpeedX1() => SetSpeed(0);
        public void SetSpeedX2() => SetSpeed(1);
        public void SetSpeedX3() => SetSpeed(2);

        /// <summary>Mevcut hız indeksini döndürür (0=x1, 1=x2, 2=x3)</summary>
        public int GetCurrentSpeedIndex() => currentSpeedIndex;

        /// <summary>Mevcut Time.timeScale değerini döndürür</summary>
        public float GetCurrentSpeed() => speedLevels[currentSpeedIndex];
    }
}
