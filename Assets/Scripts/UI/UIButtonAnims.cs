using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace TowerDefence.UI
{
    public class UIButtonAnims : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float clickScale = 0.95f;
        [SerializeField] private float transitionSpeed = 0.1f;

        private Vector3 originalScale;
        private Coroutine activeRoutine;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ScaleTo(originalScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ScaleTo(originalScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ScaleTo(originalScale * clickScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ScaleTo(originalScale * hoverScale);
        }

        private void ScaleTo(Vector3 target)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(ScaleRoutine(target));
        }

        private IEnumerator ScaleRoutine(Vector3 target)
        {
            float t = 0;
            Vector3 startScale = transform.localScale;
            while (t < 1)
            {
                t += Time.deltaTime / transitionSpeed;
                transform.localScale = Vector3.Lerp(startScale, target, t);
                yield return null;
            }
            transform.localScale = target;
        }
    }
}
