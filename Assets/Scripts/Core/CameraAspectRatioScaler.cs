using UnityEngine;

namespace TowerDefence.Core
{
    /// <summary>
    /// Ekranın en/boy oranına (Aspect Ratio) göre kameranın görüş alanını (FOV veya Orthographic Size) dinamik ayarlar.
    /// Referans 16:9 oranından daha dar ekranlarda (iPad, tablet vb.) haritanın kenarlarının kesilmesini engeller.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraAspectRatioScaler : MonoBehaviour
    {
        [Header("Aspect Ratio Reference")]
        [Tooltip("Oyunun tasarlandığı hedef en/boy oranı (Örn: 16:9 için 1.7778)")]
        [SerializeField] private float targetAspect = 16f / 9f;

        [Header("Orthographic Settings")]
        [Tooltip("16:9 referans ekranda kullanılan temel ortografik boyut")]
        [SerializeField] private float baseOrthographicSize = 28f;

        [Header("Perspective Settings (Opsiyonel)")]
        [Tooltip("16:9 referans ekranda kullanılan dikey FOV")]
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
            // Çözünürlük veya yön (Orientation) değiştiğinde anında güncelle
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
                // Cihaz 16:9'dan daha darsa (örn: iPad 4:3), haritayı kesmemek için kamerayı geriye çek
                if (currentAspect < targetAspect)
                {
                    cam.orthographicSize = baseOrthographicSize * (targetAspect / currentAspect);
                }
                else
                {
                    // 16:9 veya daha geniş ekranlarda temel boyutu koru
                    cam.orthographicSize = baseOrthographicSize;
                }
            }
            else
            {
                // Perspektif kamera için yatay FOV'u koru
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
        /// Sahneye yüklendiğinde ve her sahne geçişinde Main Camera üzerinde bu bileşen yoksa otomatik ekler.
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
