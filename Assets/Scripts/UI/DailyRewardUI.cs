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
        [SerializeField] private GameObject cellPrefab;

        [Header("Icons")]
        public Sprite karmaSprite;
        public Sprite crystalSprite;

        [Header("Cell Colors")]
        [SerializeField] private Color claimedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color availableColor = new Color(0.55f, 0.42f, 0.12f, 0.95f);
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

        private void LateUpdate()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial == null || tutorial.Current() != "daily_tell") return;
            bool closing = tutorial.WaitsDailyClose();
            if (claimButton != null && closing)
                claimButton.interactable = false;
            Button target = closing ? closeButton : claimButton;
            if (target == null) return;
            target.interactable = true;
            RaiseAboveShade(target.gameObject);
        }

        static void RaiseAboveShade(GameObject go)
        {
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null) canvas = go.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 560;
            if (go.GetComponent<GraphicRaycaster>() == null)
                go.AddComponent<GraphicRaycaster>();
        }

        private void ClosePanel()
        {
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.Current() == "daily_tell")
            {
                if (!TutorialDirector.Instance.WaitsDailyClose()) return;
                TutorialDirector.Instance.OnDailyClosed();
                return;
            }
            gameObject.SetActive(false);
            if (backdrop != null) backdrop.SetActive(false);
        }

        private void GenerateGrid()
        {
            if (gridContainer == null) return;

            cells = new GameObject[30];
            if (cellPrefab == null) return;

            for (int i = 0; i < 30; i++)
            {
                int day = i + 1;
                GameObject cell = Instantiate(cellPrefab, gridContainer);
                cell.name = "DayCell_" + day;
                cells[i] = cell;
            }
        }

        public RectTransform ClaimTarget => claimButton != null ? claimButton.transform as RectTransform : null;
        public RectTransform CloseTarget => closeButton != null ? closeButton.transform as RectTransform : null;

        public void RefreshUI()
        {
            if (drm == null) return;

            int currentDay = drm.GetCurrentDayIndex();
            int pending = drm.GetPendingCount();
            bool monthComplete = drm.IsMonthComplete();

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
                else if (monthComplete)
                    timerText.text = "30 DAYS OVER";
                else if (pending > 0)
                    timerText.text = "CLAIM NOW!";
                else
                    timerText.text = drm.GetTimeToNextReward();
            }

            if (infoText != null)
            {
                if (currentDay >= 30)
                    infoText.text = "You have completed all 30 days!";
                else if (monthComplete)
                    infoText.text = "The 30 days from your first claim are over.";
                else if (pending > 1)
                    infoText.text = $"Days {currentDay + 1}-{currentDay + pending} rewards are ready!";
                else if (pending == 1)
                    infoText.text = $"Day {currentDay + 1} reward is ready!";
                else
                    infoText.text = $"Day {currentDay + 1} - Next reward at midnight:";
            }

            if (claimButton != null)
                claimButton.interactable = pending > 0;
        }

        private void UpdateCell(int index, GameObject cell)
        {
            DailyRewardManager.DailyRewardTier tier = drm.GetTier(index);
            if (tier == null) return;

            string status = drm.GetDayStatus(index);

            TextMeshProUGUI dayText = cell.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
            if (dayText != null) dayText.text = $"Day {tier.dayNumber}";

            TextMeshProUGUI amountText = cell.transform.Find("RewardText")?.GetComponent<TextMeshProUGUI>();

            if (amountText != null)
            {
                Transform karmaIcon = cell.transform.Find("KarmaIcon");
                Transform crystalIcon = cell.transform.Find("CrystalIcon");
                bool crystalReward = tier.crystalReward > 0;
                if (karmaIcon != null) karmaIcon.gameObject.SetActive(!crystalReward);
                if (crystalIcon != null) crystalIcon.gameObject.SetActive(crystalReward);
                amountText.text = crystalReward ? tier.crystalReward.ToString() : tier.karmaReward.ToString();
                amountText.color = crystalReward ? Color.cyan : Color.yellow;
            }

            TextMeshProUGUI statusText = cell.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                switch (status)
                {
                    case "claimed":   statusText.text = "DONE"; break;
                    case "available": statusText.text = "TODAY"; break;
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
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial != null && tutorial.Current() == "daily_tell" && tutorial.WaitsDailyClose())
                return;

            if (drm.ClaimReward(out int karma, out int crystal))
            {
                RefreshUI();
                WarningBannerUI.Show("DAILY REWARD CLAIMED!", 2.5f, new Color(0.2f, 1f, 0.4f, 1f), true);
                if (TutorialDirector.Instance != null)
                    TutorialDirector.Instance.OnDailyClaimed();
            }
        }
    }
}
