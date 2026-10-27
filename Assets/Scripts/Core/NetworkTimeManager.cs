using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TowerDefence.Core
{
    public class NetworkTimeManager : MonoBehaviour
    {
        public static NetworkTimeManager Instance { get; private set; }

        public event Action OnTimeSynced;

        [Header("State")]
        [SerializeField] private bool isTimeSynced = false;
        public bool IsTimeSynced => isTimeSynced;

        private DateTime networkBaseUtcTime;
        private float syncRealtimeSinceStartup = 0f;

        // İnternet olmasa dahi cihaz saatinin geriye veya kontrolsüz ileri alınmasını engellemek için
        private long lastKnownSafeUtcTicks = 0;
        private const string PREF_SAFE_TIME_KEY = "TD_LAST_SAFE_TIME_TICKS";

        // Güvenilir, hafif HTTP head / time endpoint'leri
        private readonly string[] timeServers = new string[]
        {
            "https://www.google.com",
            "https://www.cloudflare.com",
            "https://www.apple.com"
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                LoadSafeTime();
                StartCoroutine(SyncTimeRoutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSafeTime()
        {
            string savedTicks = PlayerPrefs.GetString(PREF_SAFE_TIME_KEY, "0");
            if (long.TryParse(savedTicks, out long ticks))
            {
                lastKnownSafeUtcTicks = ticks;
            }

            // Başlangıçta fallback olarak cihaz UTC saatini güvenli referansla karşılaştır
            DateTime deviceUtc = DateTime.UtcNow;
            if (deviceUtc.Ticks < lastKnownSafeUtcTicks)
            {
                // Cihaz saati geriye alınmış! Güvenli son zamanı temel al.
                networkBaseUtcTime = new DateTime(lastKnownSafeUtcTicks, DateTimeKind.Utc);
            }
            else
            {
                networkBaseUtcTime = deviceUtc;
                lastKnownSafeUtcTicks = deviceUtc.Ticks;
            }
            syncRealtimeSinceStartup = Time.realtimeSinceStartup;
        }

        public void SaveCurrentAsSafeTime()
        {
            DateTime current = GetCurrentUtcTime();
            if (current.Ticks > lastKnownSafeUtcTicks)
            {
                lastKnownSafeUtcTicks = current.Ticks;
                PlayerPrefs.SetString(PREF_SAFE_TIME_KEY, lastKnownSafeUtcTicks.ToString());
                PlayerPrefs.Save();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveCurrentAsSafeTime();
            }
            else
            {
                // Uygulamaya geri dönüldüğünde zamanı tekrar senkronize etmeyi dene
                StartCoroutine(SyncTimeRoutine());
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentAsSafeTime();
        }

        public IEnumerator SyncTimeRoutine()
        {
            foreach (string server in timeServers)
            {
                using (UnityWebRequest req = UnityWebRequest.Head(server))
                {
                    req.timeout = 5;
                    yield return req.SendWebRequest();

                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        string dateStr = req.GetResponseHeader("date");
                        if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime serverTime))
                        {
                            networkBaseUtcTime = serverTime.ToUniversalTime();
                            syncRealtimeSinceStartup = Time.realtimeSinceStartup;
                            isTimeSynced = true;

                            if (networkBaseUtcTime.Ticks > lastKnownSafeUtcTicks)
                            {
                                lastKnownSafeUtcTicks = networkBaseUtcTime.Ticks;
                                PlayerPrefs.SetString(PREF_SAFE_TIME_KEY, lastKnownSafeUtcTicks.ToString());
                                PlayerPrefs.Save();
                            }

                            Debug.Log($"[NetworkTimeManager] Time synced successfully with {server}: {networkBaseUtcTime} UTC");
                            OnTimeSynced?.Invoke();
                            yield break;
                        }
                    }
                }
            }

            Debug.LogWarning("[NetworkTimeManager] Could not sync with network servers. Using monotonic local clock.");
        }

        /// <summary>
        /// Saat hilelerine karşı korumalı anlık UTC zamanını döndürür.
        /// Cihaz saati ileri veya geri alınsa bile, son bilinen gerçek zamandan donanımsal geçen süre (realtimeSinceStartup) kadar ilerler.
        /// </summary>
        public DateTime GetCurrentUtcTime()
        {
            // Senkronize olunan andan beri donanımsal olarak geçen saniye
            float elapsedSeconds = Time.realtimeSinceStartup - syncRealtimeSinceStartup;
            if (elapsedSeconds < 0f) elapsedSeconds = 0f;

            DateTime calculatedTime = networkBaseUtcTime.AddSeconds(elapsedSeconds);

            // Eğer cihaz saati bilinen en güvenli andan gerideyse asla geriye gitme
            if (calculatedTime.Ticks < lastKnownSafeUtcTicks)
            {
                calculatedTime = new DateTime(lastKnownSafeUtcTicks, DateTimeKind.Utc);
            }

            return calculatedTime;
        }

        /// <summary>
        /// Yerel saat dilimine dönüştürülmüş anlık güvenli zaman.
        /// </summary>
        public DateTime GetCurrentLocalTime()
        {
            return GetCurrentUtcTime().ToLocalTime();
        }
    }
}
