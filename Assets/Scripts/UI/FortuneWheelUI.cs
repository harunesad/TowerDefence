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
        
        [Header("Spin Settings")]
        public float spinDuration = 4f;
        public int spinRevolutions = 8;
        
        private bool isSpinning = false;
        
        // 8 slices in clockwise order (assuming slice 0 is at top)
        private int[] karmaRewards = { 10, 50, 100, 250, 0, 0, 0, 0 };
        private int[] crystalRewards = { 0, 0, 0, 0, 5, 10, 25, 50 };
        private string[] rewardNames = { "10 Karma", "50 Karma", "100 Karma", "250 Karma", "5 Crystals", "10 Crystals", "25 Crystals", "50 Crystals" };
        
        private void Awake()
        {
            if (spinButton != null) spinButton.onClick.AddListener(Spin);
            if (closeButtonObj != null)
            {
                Button cb = closeButtonObj.GetComponent<Button>();
                if (cb != null) cb.onClick.AddListener(ClosePanel);
            }
        }
        
        private void OnEnable()
        {
            UpdateUI();
            isSpinning = false;
        }

        public bool IsSpinAvailable()
        {
            if (MetaProgressionManager.Instance == null) return true;

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelTime();
            if (lastTimeTicks == 0) return true; // Hiç çevirmemiş

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date; // Ertesi günün başlangıcı
            return DateTime.Now >= nextTime;
        }

        public string GetTimeToNextSpin()
        {
            if (MetaProgressionManager.Instance == null) return "";

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelTime();
            if (lastTimeTicks == 0) return "Available Now!";

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
            TimeSpan remaining = nextTime - DateTime.Now;

            if (remaining.Ticks <= 0) return "Available Now!";

            return string.Format("{0:D2}h {1:D2}m {2:D2}s", remaining.Hours, remaining.Minutes, remaining.Seconds);
        }

        private void UpdateUI()
        {
            bool available = IsSpinAvailable();

            if (spinButton != null) spinButton.interactable = available;

            if (rewardText != null)
            {
                if (available)
                    rewardText.text = "Spin to Win!";
                else
                    rewardText.text = "Next spin: " + GetTimeToNextSpin();
            }

            if (closeButtonObj != null) closeButtonObj.SetActive(true);
        }

        private void Update()
        {
            // Sürekli kalan süreyi güncelle (çark dönmüyorsa ve spin hakkı yoksa)
            if (!isSpinning && !IsSpinAvailable())
            {
                if (rewardText != null)
                    rewardText.text = "Next spin: " + GetTimeToNextSpin();
            }
        }
        
        public void Spin()
        {
            if (isSpinning) return;
            if (!IsSpinAvailable())
            {
                if (rewardText != null)
                    rewardText.text = "Next spin: " + GetTimeToNextSpin();
                return;
            }
            StartCoroutine(SpinRoutine());
        }
        
        private void ClosePanel()
        {
            if (isSpinning) return;
            gameObject.SetActive(false);
        }
        
        private IEnumerator SpinRoutine()
        {
            isSpinning = true;
            if (spinButton != null) spinButton.interactable = false;
            if (closeButtonObj != null) closeButtonObj.SetActive(false);
            if (rewardText != null) rewardText.text = "Spinning...";
            
            // Random slice to land on (0 to 7)
            int randomSlice = UnityEngine.Random.Range(0, 8);
            
            // Add some randomness so it doesn't land exactly on the line
            float offset = UnityEngine.Random.Range(-15f, 15f);
            
            float totalRotation = (360f * spinRevolutions) + (randomSlice * 45f) + offset;
            
            float elapsed = 0f;
            float startAngle = wheelTransform.eulerAngles.z;
            
            // EaseOutQuart
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spinDuration;
                float easeOut = 1 - Mathf.Pow(1 - t, 4);
                
                float currentAngle = Mathf.Lerp(startAngle, startAngle - totalRotation, easeOut);
                wheelTransform.rotation = Quaternion.Euler(0, 0, currentAngle);
                yield return null;
            }
            
            wheelTransform.rotation = Quaternion.Euler(0, 0, startAngle - totalRotation);
            
            // Give reward
            if (karmaRewards[randomSlice] > 0)
            {
                if (MetaProgressionManager.Instance != null)
                    MetaProgressionManager.Instance.AddKarma(karmaRewards[randomSlice]);
            }
            if (crystalRewards[randomSlice] > 0)
            {
                if (MetaProgressionManager.Instance != null)
                    MetaProgressionManager.Instance.AddCrystals(crystalRewards[randomSlice]);
            }

            // Çevirme zamanını kaydet (günde 1 kez)
            if (MetaProgressionManager.Instance != null)
                MetaProgressionManager.Instance.SetLastFortuneWheelTime(DateTime.Now.Ticks);
            
            if (rewardText != null) rewardText.text = "You won " + rewardNames[randomSlice] + "!";
            
            isSpinning = false;
            // Çevirdikten sonra buton artık devre dışı kalmalı (yarına kadar)
            if (spinButton != null) spinButton.interactable = false;
            if (closeButtonObj != null) closeButtonObj.SetActive(true);
        }
    }
}
