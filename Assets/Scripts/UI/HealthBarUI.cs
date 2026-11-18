using UnityEngine;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject container;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (container == null) container = gameObject;
        }

        public void UpdateHealth(float current, float max)
        {
            if (fillImage != null)
            {
                float fillAmount = Mathf.Clamp01(current / max);
                fillImage.fillAmount = fillAmount;

                // Color change removed. 
                // fillImage always keeps its original color (e.g. Green).
                // Background Image underneath remains red in the background.
            }
        }

        public void SetColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }

        private void LateUpdate()
        {
            // Billboard efekti: Her zaman kameraya bak
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }

        public void SetVisible(bool visible)
        {
            if (container != null) container.SetActive(visible);
        }
    }
}
