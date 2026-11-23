using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class FortuneWheelUI : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform wheelTransform;
        public Button spinButton;
        public TextMeshProUGUI rewardText;
        public GameObject closeButtonObj;
        public GameObject backdrop;
        
        [Header("Spin Settings")]
        public float spinDuration = 4f;
        public int spinRevolutions = 8;
        public AudioClip tickSound;
        public AudioClip rewardSound;
        
        private bool isSpinning = false;
        private int lastWonKarma;
        private int lastWonCrystals;
        
        // 8 slices in clockwise order (assuming slice 0 is at top)
        private int[] karmaRewards = { 5, 0, 15, 0, 25, 0, 40, 0 };
        private int[] crystalRewards = { 0, 2, 0, 4, 0, 6, 0, 10 };
        
        private void Awake()
        {
            if (spinButton != null) spinButton.onClick.AddListener(Spin);
            if (closeButtonObj != null)
            {
                Button cb = closeButtonObj.GetComponent<Button>();
                if (cb != null) cb.onClick.AddListener(ClosePanel);
            }
            if (backdrop == null && transform.parent != null)
            {
                Transform bd = transform.parent.Find("FortuneWheelBackdrop");
                if (bd != null) backdrop = bd.gameObject;
            }
            if (backdrop != null)
            {
                Button bb = backdrop.GetComponent<Button>();
                if (bb == null) bb = backdrop.AddComponent<Button>();
                bb.onClick.AddListener(ClosePanel);
            }
        }
        
        private void OnEnable()
        {
            UpdateUI();
            isSpinning = false;
        }

        public RectTransform SpinTarget => spinButton != null ? spinButton.transform as RectTransform : null;
        public RectTransform CloseTarget => closeButtonObj != null ? closeButtonObj.transform as RectTransform : null;

        private void LateUpdate()
        {
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial == null || tutorial.Current() != "fortune_tell") return;
            if (tutorial.WaitsFortuneClose())
            {
                if (spinButton != null)
                {
                    spinButton.interactable = false;
                    DropShade(spinButton.gameObject);
                }
                if (closeButtonObj == null) return;
                closeButtonObj.SetActive(true);
                Button close = closeButtonObj.GetComponent<Button>();
                if (close != null) close.interactable = true;
                RaiseAboveShade(closeButtonObj);
                return;
            }
            if (spinButton == null) return;
            if (!isSpinning) spinButton.interactable = true;
            RaiseAboveShade(spinButton.gameObject);
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

        static void DropShade(GameObject go)
        {
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null) return;
            canvas.overrideSorting = false;
        }

        public void PrepareLessonSpin()
        {
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.WaitsFortuneClose())
                return;
            if (IsSpinAvailable()) return;
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.SetLastFortuneWheelTime(0, true);
            UpdateUI();
        }

        public bool IsSpinAvailable()
        {
            if (MetaProgressionManager.Instance == null) return true;

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelTime();
            if (lastTimeTicks == 0) return true; // Has never spun

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date; // Start of next day
            DateTime now = NetworkTimeManager.Instance != null ? NetworkTimeManager.Instance.GetCurrentLocalTime() : DateTime.Now;
            return now >= nextTime;
        }

        public bool IsAdSpinAvailable()
        {
            if (MetaProgressionManager.Instance == null) return true;

            long lastAdTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelAdTime();
            if (lastAdTimeTicks == 0) return true;

            DateTime lastTime = new DateTime(lastAdTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
            DateTime now = NetworkTimeManager.Instance != null ? NetworkTimeManager.Instance.GetCurrentLocalTime() : DateTime.Now;
            return now >= nextTime;
        }

        public string GetTimeToNextSpin()
        {
            if (MetaProgressionManager.Instance == null) return "";

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelTime();
            if (lastTimeTicks == 0) return "Available Now!";

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
            DateTime now = NetworkTimeManager.Instance != null ? NetworkTimeManager.Instance.GetCurrentLocalTime() : DateTime.Now;
            TimeSpan remaining = nextTime - now;

            if (remaining.Ticks <= 0) return "Available Now!";

            return string.Format("{0:D2}h {1:D2}m {2:D2}s", remaining.Hours, remaining.Minutes, remaining.Seconds);
        }

        private void UpdateUI()
        {
            bool freeAvailable = IsSpinAvailable();
            bool adAvailable = IsAdSpinAvailable();
            TextMeshProUGUI btnText = spinButton != null ? spinButton.GetComponentInChildren<TextMeshProUGUI>() : null;

            string statusMsg = "";
            if (freeAvailable)
            {
                if (spinButton != null) spinButton.interactable = true;
                if (btnText != null) btnText.text = "SPIN";
                statusMsg = "Spin to Win!";
            }
            else if (adAvailable)
            {
                // No free spin, but can spin by watching an ad
                if (spinButton != null) spinButton.interactable = true;
                if (btnText != null) btnText.text = "WATCH AD";
                statusMsg = "Watch Ad for Extra Spin!";
            }
            else
            {
                // Both spins consumed
                if (spinButton != null) spinButton.interactable = false;
                if (btnText != null) btnText.text = "NO SPINS";
                statusMsg = "Next Spin: " + GetTimeToNextSpin();
            }

            SetCombinedRewardText(statusMsg);

            if (closeButtonObj != null) closeButtonObj.SetActive(true);
        }

        TMP_FontAsset rewardFont;

        void ShowRewardWarning(string message)
        {
            if (rewardText == null) return;
            if (rewardFont == null) rewardFont = rewardText.font;
            if (GameManager.Instance != null && GameManager.Instance.WarningFont != null)
                rewardText.font = GameManager.Instance.WarningFont;
            rewardText.text = message;
        }

        void RestoreRewardFont()
        {
            if (rewardText != null && rewardFont != null)
                rewardText.font = rewardFont;
        }

        private void SetCombinedRewardText(string statusMsg)
        {
            if (rewardText == null) return;

            RestoreRewardFont();
            rewardText.text = statusMsg;
            CurrencyRowView.Apply(rewardText.transform, lastWonKarma, lastWonCrystals, false);
        }

        private void Update()
        {
            // Continuously update remaining time (if wheel isn't spinning and neither free nor ad spin is available)
            if (!isSpinning && !IsSpinAvailable() && !IsAdSpinAvailable())
            {
                string statusMsg = "Next Spin: " + GetTimeToNextSpin();
                SetCombinedRewardText(statusMsg);
            }
        }
        
        public void Spin()
        {
            if (isSpinning) return;
            if (TutorialDirector.Instance != null && TutorialDirector.Instance.WaitsFortuneClose())
                return;

            bool isFreeSpin = IsSpinAvailable();
            bool isAdSpin = IsAdSpinAvailable();

            if (isFreeSpin)
            {
                // Free spin
                StartCoroutine(SpinRoutine(isFreeSpin: true));
            }
            else if (isAdSpin)
            {
                // Ad-rewarded spin
                if (AdManager.Instance != null)
                {
                    lastWonKarma = 0;
                    lastWonCrystals = 0;
                    if (rewardText != null)
                    {
                        RestoreRewardFont();
                        rewardText.text = "Loading Ad...";
                        CurrencyRowView.Apply(rewardText.transform, 0, 0, false);
                    }
                    AdManager.Instance.ShowRewardedAd(
                        onSuccess: () => { StartCoroutine(SpinRoutine(isFreeSpin: false)); },
                        onFailed: () => ShowRewardWarning("Ad Failed. Try again.")
                    );
                }
                else
                {
                    ShowRewardWarning("Ad System Offline");
                }
            }
            else
            {
                // No spins available (normally unreachable button click safeguard)
                if (rewardText != null)
                    rewardText.text = "Next Spin: " + GetTimeToNextSpin();
            }
        }
        
        private void ClosePanel()
        {
            if (isSpinning) return;
            TutorialDirector tutorial = TutorialDirector.Instance;
            if (tutorial != null && tutorial.Current() == "fortune_tell")
            {
                if (!tutorial.WaitsFortuneClose()) return;
                tutorial.OnFortuneClosed();
                return;
            }
            gameObject.SetActive(false);
            if (backdrop != null) backdrop.SetActive(false);
        }
        
        private IEnumerator SpinRoutine(bool isFreeSpin)
        {
            isSpinning = true;
            lastWonKarma = 0;
            lastWonCrystals = 0;

            if (spinButton != null) spinButton.interactable = false;
            if (closeButtonObj != null) closeButtonObj.SetActive(false);
            if (rewardText != null)
            {
                RestoreRewardFont();
                rewardText.text = "Spinning...";
                CurrencyRowView.Apply(rewardText.transform, 0, 0, false);
            }
            
            // Random slice to land on (0 to 7)
            int randomSlice = UnityEngine.Random.Range(0, 8);
            
            // Absolute angle required for slice N (0..7) to align directly under the top pointer:
            // Slices are arranged clockwise:
            // Slice 0 (5 Karma): +22.5° | Slice 6 (40 Karma): +292.5° | Slice 7 (10 Crystal): +337.5°
            // When wheel spins clockwise (Euler Z decreasing), this angle lands exactly at the top.
            float targetEndAngle = (randomSlice * 45f) + 22.5f;
            float jitter = UnityEngine.Random.Range(-6f, 6f); // Slight random offset within the slice
            targetEndAngle += jitter;

            float startAngle = wheelTransform.eulerAngles.z;
            // Starting from current angle, rotate clockwise by at least 'spinRevolutions' turns to land exactly on target angle:
            // Normalize startAngle relative to targetEndAngle
            float delta = Mathf.Repeat(startAngle - targetEndAngle, 360f);
            float totalRotation = (360f * spinRevolutions) + delta;
            float endAngle = startAngle - totalRotation;
            
            float elapsed = 0f;
            int lastTickIndex = -1;
            
            // EaseOutQuart
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spinDuration;
                float easeOut = 1 - Mathf.Pow(1 - t, 4);
                
                float currentAngle = Mathf.Lerp(startAngle, endAngle, easeOut);
                wheelTransform.rotation = Quaternion.Euler(0, 0, currentAngle);

                int currentTickIndex = Mathf.FloorToInt(Mathf.Abs(currentAngle) / 45f);
                if (currentTickIndex != lastTickIndex)
                {
                    lastTickIndex = currentTickIndex;
                    if (AudioManager.Instance != null && tickSound != null)
                        AudioManager.Instance.PlaySFX(tickSound, 0.5f, UnityEngine.Random.Range(0.9f, 1.1f));
                }

                yield return null;
            }
            
            wheelTransform.rotation = Quaternion.Euler(0, 0, endAngle);

            // Spin eligibility is saved alongside reward. If app closes before spin ends, eligibility remains intact.
            if (MetaProgressionManager.Instance != null)
            {
                DateTime now = NetworkTimeManager.Instance != null ? NetworkTimeManager.Instance.GetCurrentLocalTime() : DateTime.Now;
                int karma = karmaRewards[randomSlice];
                int crystals = crystalRewards[randomSlice];
                bool persistWithReward = karma > 0 || crystals > 0;
                if (isFreeSpin)
                    MetaProgressionManager.Instance.SetLastFortuneWheelTime(now.Ticks, !persistWithReward);
                else
                    MetaProgressionManager.Instance.SetLastFortuneWheelAdTime(now.Ticks, !persistWithReward);

                if (karma > 0)
                    MetaProgressionManager.Instance.AddKarma(karma);
                if (crystals > 0)
                    MetaProgressionManager.Instance.AddCrystals(crystals);
            }

            // Reward text
            lastWonKarma = karmaRewards[randomSlice];
            lastWonCrystals = crystalRewards[randomSlice];
            if (AudioManager.Instance != null && rewardSound != null)
                AudioManager.Instance.PlaySFX(rewardSound);
            
            isSpinning = false;

            if (TutorialDirector.Instance != null)
                TutorialDirector.Instance.OnFortuneReward();

            // Restore UI based on current state (ready for ad spin)
            UpdateUI();
            if (closeButtonObj != null) closeButtonObj.SetActive(true);
        }
    }
}
