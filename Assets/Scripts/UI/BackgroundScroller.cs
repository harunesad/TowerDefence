using UnityEngine;
using UnityEngine.UI;

namespace TowerDefence.UI
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Scrolling Settings")]
        [SerializeField] private Vector2 scrollSpeed = new Vector2(0.5f, 0f);
        [SerializeField] private Vector2 scrollLimit = new Vector2(100f, -100f);
        [SerializeField] private bool loopScrolling = true;
        
        [Header("Target")]
        [SerializeField] private bool useThisGameObjectRect = true; // Use this gameobject's RectTransform
        
        private float scrollOffset;

        private void Awake()
        {
            // Initialize position at the right limit (start from one side)
            if (useThisGameObjectRect)
            {
                // Use the RectTransform of the gameObject this script is attached to
                RectTransform rt = GetComponent<RectTransform>();
                if (rt != null)
                {
                    scrollOffset = scrollLimit.y; // Start from the left/most negative limit
                    rt.anchoredPosition = new Vector2(scrollOffset, rt.anchoredPosition.y);
                }
            }
        }

        private void Update()
        {
            // Find the RectTransform to move
            RectTransform targetRT = useThisGameObjectRect ? GetComponent<RectTransform>() : null;
            
            // If not using this gameobject, try to find it
            if (targetRT == null)
            {
                targetRT = GetComponent<RectTransform>();
            }
            
            if (targetRT == null) return;

            scrollOffset += scrollSpeed.x * Time.deltaTime;
            
            // Limit the scrolling range
            float clampedOffset = Mathf.Clamp(scrollOffset, scrollLimit.x, scrollLimit.y);
            targetRT.anchoredPosition = new Vector2(clampedOffset, targetRT.anchoredPosition.y);
            
            // Reset offset if we hit the limit and looping is enabled
            if (loopScrolling)
            {
                if (scrollLimit.x > 0 && scrollOffset >= scrollLimit.y)
                    scrollOffset = scrollLimit.x;
                else if (scrollLimit.y < 0 && scrollOffset <= scrollLimit.y)
                    scrollOffset = scrollLimit.y;
            }
        }

        public void SetSpeed(Vector2 newSpeed) => scrollSpeed = newSpeed;
        public void SetLimit(Vector2 newLimit) => scrollLimit = newLimit;
        public void SetLooping(bool enable) => loopScrolling = enable;
        public void SetUseThisGameObjectRect(bool use) => useThisGameObjectRect = use;
    }
}