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

        // internet olmasa dahi cihaz saatinin geriye veya kontrolsuz ileri alinmasini engellemek icin
        private long lastKnownSafeUtcTicks = 0;
        private const string PREF_SAFE_TIME_KEY = "TD_LAST_SAFE_TIME_TICKS";

        // Guvenilir, hafif HTTP head / time endpoint'leri
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

            // Baslangicta fallback olarak cihaz UTC saatini guvenli referansla karsilastir
            DateTime deviceUtc = DateTime.UtcNow;
            if (lastKnownSafeUtcTicks <= 0)
            {
                networkBaseUtcTime = deviceUtc;
                lastKnownSafeUtcTicks = deviceUtc.Ticks;
            }
            else
            {
                // Cihaz saati ileri alinsa da oturum, son kayitli andan ve gercek gecen sureden yurur.
                // Geri alinan saat de bu andin gerisine dusemez.
                networkBaseUtcTime = new DateTime(lastKnownSafeUtcTicks, DateTimeKind.Utc);
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
                // Uygulamaya geri donuldugunde zamani tekrar senkronize etmeyi dene
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
                            lastKnownSafeUtcTicks = networkBaseUtcTime.Ticks;
                            PlayerPrefs.SetString(PREF_SAFE_TIME_KEY, lastKnownSafeUtcTicks.ToString());
                            PlayerPrefs.Save();

                            OnTimeSynced?.Invoke();
                            yield break;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Saat hilelerine karsi korumali anlik UTC zamanini dondurur.
        /// Cihaz saati ileri veya geri alinsa bile, son bilinen gercek zamandan donanimsal gecen sure (realtimeSinceStartup) kadar ilerler.
        /// </summary>
        public DateTime GetCurrentUtcTime()
        {
            // Senkronize olunan andan beri donanimsal olarak gecen saniye
            float elapsedSeconds = Time.realtimeSinceStartup - syncRealtimeSinceStartup;
            if (elapsedSeconds < 0f) elapsedSeconds = 0f;

            DateTime calculatedTime = networkBaseUtcTime.AddSeconds(elapsedSeconds);

            // Eger cihaz saati bilinen en guvenli andan gerideyse asla geriye gitme
            if (calculatedTime.Ticks < lastKnownSafeUtcTicks)
            {
                calculatedTime = new DateTime(lastKnownSafeUtcTicks, DateTimeKind.Utc);
            }

            return calculatedTime;
        }

        /// <summary>
        /// Yerel saat dilimine donusturulmus anlik guvenli zaman.
        /// </summary>
        public DateTime GetCurrentLocalTime()
        {
            return GetCurrentUtcTime().ToLocalTime();
        }
    }
}
