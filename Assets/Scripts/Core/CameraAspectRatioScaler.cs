using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// Ekranin en/boy oranina (Aspect Ratio) gore kameranin gorus alanini (FOV veya Orthographic Size) dinamik ayarlar.
    /// Referans 16:9 oranindan daha dar ekranlarda (iPad, tablet vb.) haritanin kenarlarinin kesilmesini engeller.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraAspectRatioScaler : MonoBehaviour
    {
        [Header("Aspect Ratio Reference")]
        [Tooltip("Target aspect ratio the game is designed for (e.g. 1.7778 for 16:9)")]
        [SerializeField] private float targetAspect = 16f / 9f;

        [Header("Orthographic Settings")]
        [Tooltip("Base orthographic size used on 16:9 reference screen")]
        [SerializeField] private float baseOrthographicSize = 28f;

        [Header("Perspective Settings (Opsiyonel)")]
        [Tooltip("Vertical FOV used on 16:9 reference screen")]
        [SerializeField] private float baseFieldOfView = 60f;

        private Camera cam;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null && cam.orthographic && baseOrthographicSize <= 0f)
            {
                baseOrthographicSize = cam.orthographicSize;
            }
            AdjustCamera();
        }

        private void OnEnable()
        {
            AdjustCamera();
        }

        private void Update()
        {
            // cozunurluk veya yon (Orientation) degistiginde aninda guncelle
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                AdjustCamera();
            }
        }

        public void AdjustCamera()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null || Screen.height <= 0) return;

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            float currentAspect = (float)Screen.width / Screen.height;

            if (cam.orthographic)
            {
                // Cihaz 16:9'dan daha darsa (orn: iPad 4:3), haritayi kesmemek icin kamerayi geriye cek
                if (currentAspect < targetAspect)
                {
                    cam.orthographicSize = baseOrthographicSize * (targetAspect / currentAspect);
                }
                else
                {
                    // 16:9 veya daha genis ekranlarda temel boyutu koru
                    cam.orthographicSize = baseOrthographicSize;
                }
            }
            else
            {
                // Perspektif kamera icin yatay FOV'u koru
                if (currentAspect < targetAspect)
                {
                    float horizontalFOVRad = 2f * Mathf.Atan(Mathf.Tan(baseFieldOfView * 0.5f * Mathf.Deg2Rad) * targetAspect);
                    float newVerticalFOVRad = 2f * Mathf.Atan(Mathf.Tan(horizontalFOVRad * 0.5f) / currentAspect);
                    cam.fieldOfView = newVerticalFOVRad * Mathf.Rad2Deg;
                }
                else
                {
                    cam.fieldOfView = baseFieldOfView;
                }
            }
        }

        /// <summary>
        /// Sahneye yuklendiginde ve her sahne gecisinde Main Camera uzerinde bu bilesen yoksa otomatik ekler.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoAttachToMainCamera()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
            {
                CheckAndAttach();
            };
            CheckAndAttach();
        }

        private static void CheckAndAttach()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<CameraAspectRatioScaler>() == null)
            {
                mainCam.gameObject.AddComponent<CameraAspectRatioScaler>();
            }
        }
    }
}
