using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// Manages game speed (x1, x2, x3) and pause state via Time.timeScale.
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

        // Event for UI to listen when speed changes. Returns -1 if paused.
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

        private const float DefaultFixedDelta = 0.02f;

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = DefaultFixedDelta;
        }

        /// <summary>0=x1, 1=x2, 2=x3</summary>
        public void SetSpeed(int index)
        {
            if (index < 0 || index >= speedLevels.Length) return;
            currentSpeedIndex = index;
            isPaused = false; // Automatically unpause when speed button is pressed
            Time.timeScale = speedLevels[index];
            Time.fixedDeltaTime = DefaultFixedDelta * Time.timeScale;
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
                Time.fixedDeltaTime = DefaultFixedDelta * savedTimeScale;
                OnSpeedChanged?.Invoke(-1);
            }
            else
            {
                Time.timeScale = savedTimeScale;
                Time.fixedDeltaTime = DefaultFixedDelta * savedTimeScale;
                OnSpeedChanged?.Invoke(currentSpeedIndex);
            }
        }

        public bool IsPaused() => isPaused;

        /// <summary>Returns the current speed index (0=x1, 1=x2, 2=x3)</summary>
        public int GetCurrentSpeedIndex() => currentSpeedIndex;

        /// <summary>Returns the current Time.timeScale value</summary>
        public float GetCurrentSpeed() => isPaused ? 0f : speedLevels[currentSpeedIndex];
    }
}
