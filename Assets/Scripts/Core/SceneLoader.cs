using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using TowerDefence.Data;

namespace TowerDefence.Core
{
    /// <summary>
    /// Sahneler arası asenkron yükleme ve fade geçiş yöneticisi.
    /// DontDestroyOnLoad olarak yaşar. Tüm sahne geçişlerinde SceneLoader.Instance.LoadScene(...) çağrılır.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI tipsText;
        [SerializeField] private RectTransform spinnerIcon;

        [Header("Tips Data (ScriptableObject)")]
        [SerializeField] private LoadingTipsData tipsData;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private float minimumLoadTime = 0.6f;
        [SerializeField] private float spinnerSpeed = 200f;

        private bool isLoading = false;

        private static readonly string[] fallbackTips = new string[]
        {
            "Tip: Upgrade your towers to deal more damage!",
            "Tip: Place towers at choke points for maximum effectiveness.",
            "Tip: Use spells wisely — they have long cooldowns.",
            "Tip: Different tower types are strong against different enemies.",
            "Tip: Don't forget to check the Skill Tree for permanent upgrades!",
            "Tip: Watch ads for extra Fortune Wheel spins.",
            "Tip: Heroes can turn the tide of battle — equip them wisely!",
            "Tip: Earn Karma by completing levels to unlock new skills.",
            "Tip: Try harder difficulties for greater challenges and rewards!",
            "Tip: Daily rewards reset every day — don't miss them!"
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                // Başlangıçta gizle
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Spinner animasyonu
            if (spinnerIcon != null && isLoading)
            {
                spinnerIcon.Rotate(0f, 0f, -spinnerSpeed * Time.unscaledDeltaTime);
            }
        }

        /// <summary>
        /// Sahneyi build index ile yükler.
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneRoutine(sceneIndex, null));
        }

        /// <summary>
        /// Sahneyi isim ile yükler.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneRoutine(-1, sceneName));
        }

        /// <summary>
        /// Mevcut sahneyi yeniden yükler.
        /// </summary>
        public void ReloadCurrentScene()
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().buildIndex, null));
        }

        private IEnumerator LoadSceneRoutine(int sceneIndex, string sceneName)
        {
            isLoading = true;

            // Rastgele bir ipucu göster
            if (tipsText != null)
            {
                if (tipsData != null)
                    tipsText.text = tipsData.GetRandomTip();
                else
                    tipsText.text = fallbackTips[Random.Range(0, fallbackTips.Length)];
            }

            // Progress sıfırla
            if (progressBar != null) progressBar.value = 0f;
            if (progressText != null) progressText.text = "0%";

            // === FADE IN (Ekranı kapat) ===
            yield return StartCoroutine(Fade(0f, 1f));

            // Asenkron sahne yüklemeyi başlat
            AsyncOperation asyncOp;
            if (sceneName != null)
                asyncOp = SceneManager.LoadSceneAsync(sceneName);
            else
                asyncOp = SceneManager.LoadSceneAsync(sceneIndex);

            asyncOp.allowSceneActivation = false;

            float elapsed = 0f;
            float displayProgress = 0f;

            // Unity'de asyncOp.progress 0.9'a kadar gider, sonra allowSceneActivation bekler
            while (asyncOp.progress < 0.9f || elapsed < minimumLoadTime)
            {
                elapsed += Time.unscaledDeltaTime;

                // Gerçek ilerlemeyi 0-1 aralığına normalize et (0.9 = %100)
                float targetProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);

                // Minimum bekleme süresini de hesaba kat
                float timeProgress = Mathf.Clamp01(elapsed / minimumLoadTime);
                targetProgress = Mathf.Min(targetProgress, timeProgress);

                // Yumuşak ilerleme
                displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.unscaledDeltaTime * 2f);

                if (progressBar != null) progressBar.value = displayProgress;
                if (progressText != null) progressText.text = Mathf.RoundToInt(displayProgress * 100f) + "%";

                yield return null;
            }

            // %100'e tamamla
            displayProgress = 1f;
            if (progressBar != null) progressBar.value = 1f;
            if (progressText != null) progressText.text = "100%";

            yield return new WaitForSecondsRealtime(0.15f);

            // Sahneyi aktive et
            asyncOp.allowSceneActivation = true;

            // Sahnenin tam yüklenmesini bekle
            while (!asyncOp.isDone)
                yield return null;

            // Yeni sahnenin ilk frame'inin render olmasını bekle
            yield return null;

            // === FADE OUT (Ekranı aç) ===
            yield return StartCoroutine(Fade(1f, 0f));

            isLoading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (canvasGroup == null) yield break;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = to;

            if (to <= 0f)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        /// <summary>
        /// SceneLoader instance'ını kontrol eder. GameManager.Awake()'te prefab zaten instantiate edilmiş olmalıdır.
        /// </summary>
        public static void EnsureInstance()
        {
            if (Instance != null) return;
            Debug.LogWarning("[SceneLoader] Instance not found. Make sure LoadingScreenPrefab is assigned in GameManager Inspector.");
        }
    }
}
