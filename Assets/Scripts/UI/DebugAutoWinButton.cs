using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;
using TowerDefence.Combat;

namespace TowerDefence.UI
{
    /// <summary>
    /// Debug component that enables testing by finishing levels with a single click and random success (1-3 stars) on mobile/devices.
    /// Automatically created at runtime by HUDController.
    /// </summary>
    public class DebugAutoWinButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();

            // Place in a safe area in the top-right corner of the screen next to other buttons
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-15f, -80f);
            rt.sizeDelta = new Vector2(110f, 40f);

            // Background (Semi-transparent eye-catching dark amber/gold)
            Image bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.85f, 0.45f, 0.05f, 0.75f);

            // Button component
            button = gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.85f, 0.45f, 0.05f, 0.75f);
            colors.highlightedColor = new Color(1f, 0.6f, 0.1f, 0.95f);
            colors.pressedColor = new Color(0.6f, 0.3f, 0.0f, 1f);
            button.colors = colors;
            button.onClick.AddListener(OnAutoWinClicked);

            // Label (TMP)
            GameObject textObj = new GameObject("Label", typeof(RectTransform));
            textObj.transform.SetParent(transform, false);

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚡ WIN";
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
        }

        private void OnAutoWinClicked()
        {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            return;
#else
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.GetCurrentState() == GameState.Victory || 
                GameManager.Instance.GetCurrentState() == GameState.Defeat)
            {
                return; // Game already ended
            }

            Debug.Log("[DebugAutoWin] Test button pressed! Calculating random outcome...");

            // 1. Determine random stars (1, 2, or 3 stars)
            // 25% chance for 1 star, 35% chance for 2 stars, 40% chance for 3 stars
            int randomRoll = Random.Range(0, 100);
            int targetStars;
            if (randomRoll < 25) targetStars = 1;
            else if (randomRoll < 60) targetStars = 2;
            else targetStars = 3;

            // 2. Health simulation: LevelResultUI calculates stars via (livesRemaining / totalLives) formula:
            // lifePercent >= 1.0f -> 3 Stars
            // lifePercent >= 0.5f -> 2 Stars
            // lifePercent < 0.5f  -> 1 Star
            if (LivesManager.Instance != null)
            {
                int maxLives = LivesManager.Instance.GetMaxLives();
                int simulatedLives;

                switch (targetStars)
                {
                    case 3:
                        simulatedLives = maxLives; // 100% health
                        break;
                    case 2:
                        simulatedLives = Mathf.Max(1, Mathf.RoundToInt(maxLives * Random.Range(0.55f, 0.85f))); // 55%-85% health
                        break;
                    case 1:
                    default:
                        simulatedLives = Mathf.Max(1, Mathf.RoundToInt(maxLives * Random.Range(0.15f, 0.40f))); // 15%-40% health
                        break;
                }

                LivesManager.Instance.SetCurrentLivesForDebug(simulatedLives);
                Debug.Log($"[DebugAutoWin] Target Stars: {targetStars}★ | Simulated Lives: {simulatedLives}/{maxLives}");
            }

            // 3. Stop/clear active enemies on the field
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                if (unit != null)
                {
                    unit.TakeDamage(999999);
                }
            }

            // 4. Trigger victory state
            GameManager.Instance.ChangeState(GameState.Victory);

            // Hide/disable test button to prevent multiple clicks
            gameObject.SetActive(false);
#endif
        }
    }
}
