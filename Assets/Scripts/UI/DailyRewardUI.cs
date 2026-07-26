using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class DailyRewardUI : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Transform gridContainer;

        [Header("Refs")]
        [SerializeField] private GameObject backdrop;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button closeButton;

        [Header("Icons")]
        public Sprite karmaSprite;
        public Sprite crystalSprite;

        [Header("Cell Colors")]
        [SerializeField] private Color claimedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color availableColor = new Color(0.2f, 0.6f, 0.2f, 0.9f);
        [SerializeField] private Color waitingColor = new Color(0.4f, 0.4f, 0.2f, 0.8f);
        [SerializeField] private Color lockedColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);

        private GameObject[] cells;
        private DailyRewardManager drm;

        private void Start()
        {
            drm = DailyRewardManager.Instance;
            if (drm == null) return;

            if (claimButton != null) claimButton.onClick.AddListener(ClaimReward);
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            if (backdrop != null) backdrop.AddComponent<Button>().onClick.AddListener(ClosePanel);

            GenerateGrid();
            RefreshUI();
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        private void ClosePanel()
        {
            gameObject.SetActive(false);
            if (backdrop != null) backdrop.SetActive(false);
        }

        private void GenerateGrid()
        {
            if (gridContainer == null) return;

            cells = new GameObject[30];
            for (int i = 0; i < 30; i++)
            {
                int day = i + 1;
                GameObject cell = new GameObject("DayCell_" + day, typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(gridContainer, false);
                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.sizeDelta = Vector2.zero;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;

                ColorBlock colors = new ColorBlock
                {
                    colorMultiplier = 1,
                    fadeDuration = 0.1f
                };

                GameObject dayTextGO = new GameObject("DayText", typeof(RectTransform), typeof(TextMeshProUGUI));
                dayTextGO.transform.SetParent(cell.transform, false);
                TextMeshProUGUI dayText = dayTextGO.GetComponent<TextMeshProUGUI>();
                dayText.fontSize = 14;
                dayText.alignment = TextAlignmentOptions.Top;
                dayText.text = "Day " + day;
                dayText.color = Color.white;
                RectTransform dayRT = dayTextGO.GetComponent<RectTransform>();
                dayRT.sizeDelta = new Vector2(100, 20);
                dayRT.anchorMin = new Vector2(0.5f, 0.5f);
                dayRT.anchorMax = new Vector2(0.5f, 0.5f);
                dayRT.anchoredPosition = new Vector2(0, 10);

                GameObject iconGO = new GameObject("RewardIcon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(cell.transform, false);
                Image iconImg = iconGO.GetComponent<Image>();
                RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(40, 40);
                iconRT.anchorMin = new Vector2(0.5f, 0.5f);
                iconRT.anchorMax = new Vector2(0.5f, 0.5f);
                iconRT.anchoredPosition = new Vector2(0, -5);

                GameObject amountGO = new GameObject("RewardText", typeof(RectTransform), typeof(TextMeshProUGUI));
                amountGO.transform.SetParent(cell.transform, false);
                TextMeshProUGUI amountText = amountGO.GetComponent<TextMeshProUGUI>();
                amountText.fontSize = 14;
                amountText.alignment = TextAlignmentOptions.Center;
                amountText.color = Color.yellow;
                RectTransform amountRT = amountGO.GetComponent<RectTransform>();
                amountRT.sizeDelta = new Vector2(100, 20);
                amountRT.anchorMin = new Vector2(0.5f, 0f);
                amountRT.anchorMax = new Vector2(0.5f, 0f);
                amountRT.anchoredPosition = new Vector2(0, 15);

                GameObject statusGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
                statusGO.transform.SetParent(cell.transform, false);
                TextMeshProUGUI statusText = statusGO.GetComponent<TextMeshProUGUI>();
                statusText.fontSize = 10;
                statusText.alignment = TextAlignmentOptions.Bottom;
                statusText.text = "";
                statusText.color = Color.white;
                RectTransform statusRT = statusGO.GetComponent<RectTransform>();
                statusRT.sizeDelta = new Vector2(80, 16);
                statusRT.anchorMin = new Vector2(0.5f, 0);
                statusRT.anchorMax = new Vector2(0.5f, 0);
                statusRT.anchoredPosition = new Vector2(0, 2);

                cells[i] = cell;
            }
        }

        public void RefreshUI()
        {
            if (drm == null) return;

            int currentDay = drm.GetCurrentDayIndex();
            bool available = drm.IsRewardAvailable();

            if (cells != null)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    UpdateCell(i, cells[i]);
                }
            }

            if (timerText != null)
            {
                if (currentDay >= 30)
                    timerText.text = "ALL REWARDS COLLECTED!";
                else if (available)
                    timerText.text = "CLAIM NOW!";
                else
                    timerText.text = drm.GetTimeToNextReward();
            }

            if (infoText != null)
            {
                if (currentDay >= 30)
                    infoText.text = "You have completed all 30 days!";
                else if (available)
                    infoText.text = $"Day {currentDay + 1} reward is ready!";
                else
                    infoText.text = $"Day {currentDay + 1} - Next reward in:";
            }

            if (claimButton != null)
            {
                claimButton.interactable = available && currentDay < 30;
            }
        }

        private void UpdateCell(int index, GameObject cell)
        {
            DailyRewardManager.DailyRewardTier tier = drm.GetTier(index);
            if (tier == null) return;

            string status = drm.GetDayStatus(index);

            TextMeshProUGUI dayText = cell.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
            if (dayText != null) dayText.text = $"Day {tier.dayNumber}";

            Image iconImg = cell.transform.Find("RewardIcon")?.GetComponent<Image>();
            TextMeshProUGUI amountText = cell.transform.Find("RewardText")?.GetComponent<TextMeshProUGUI>();

            if (iconImg != null && amountText != null)
            {
                if (tier.crystalReward > 0)
                {
                    iconImg.sprite = crystalSprite;
                    amountText.text = $"{tier.crystalReward} C";
                    amountText.color = Color.cyan;
                }
                else
                {
                    iconImg.sprite = karmaSprite;
                    amountText.text = $"{tier.karmaReward} K";
                    amountText.color = Color.yellow;
                }
            }

            TextMeshProUGUI statusText = cell.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                switch (status)
                {
                    case "claimed":   statusText.text = "DONE"; break;
                    case "available": statusText.text = "CLAIM"; break;
                    case "waiting":   statusText.text = "WAIT"; break;
                    default:          statusText.text = "LOCK"; break;
                }
            }

            Image bg = cell.GetComponent<Image>();
            if (bg != null)
            {
                switch (status)
                {
                    case "claimed":   bg.color = claimedColor; break;
                    case "available": bg.color = availableColor; break;
                    case "waiting":   bg.color = waitingColor; break;
                    default:          bg.color = lockedColor; break;
                }
            }
        }

        private void ClaimReward()
        {
            if (drm == null) return;

            if (drm.ClaimReward(out int karma, out int crystal))
            {
                RefreshUI();
            }
        }
    }
}
