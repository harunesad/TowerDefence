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
        
        private bool isSpinning = false;
        
        // 8 slices in clockwise order (assuming slice 0 is at top)
        private int[] karmaRewards = { 10, 0, 50, 0, 100, 0, 250, 0 };
        private int[] crystalRewards = { 0, 5, 0, 10, 0, 25, 0, 50 };
        private string[] rewardNames = { "10 Karma", "5 Crystals", "50 Karma", "10 Crystals", "100 Karma", "25 Crystals", "250 Karma", "50 Crystals" };
        
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

        public bool IsSpinAvailable()
        {
            if (MetaProgressionManager.Instance == null) return true;

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelTime();
            if (lastTimeTicks == 0) return true; // Hiç çevirmemiş

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date; // Ertesi günün başlangıcı
            return DateTime.Now >= nextTime;
        }

        public bool IsAdSpinAvailable()
        {
            if (MetaProgressionManager.Instance == null) return true;

            long lastAdTimeTicks = MetaProgressionManager.Instance.GetLastFortuneWheelAdTime();
            if (lastAdTimeTicks == 0) return true;

            DateTime lastTime = new DateTime(lastAdTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
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
            bool freeAvailable = IsSpinAvailable();
            bool adAvailable = IsAdSpinAvailable();
            TextMeshProUGUI btnText = spinButton != null ? spinButton.GetComponentInChildren<TextMeshProUGUI>() : null;

            if (freeAvailable)
            {
                if (spinButton != null) spinButton.interactable = true;
                if (btnText != null) btnText.text = "SPIN";
                if (rewardText != null) rewardText.text = "Spin to Win!";
            }
            else if (adAvailable)
            {
                // Free hakkı yok ama Reklam izleyerek çevirebilir
                if (spinButton != null) spinButton.interactable = true;
                if (btnText != null) btnText.text = "WATCH AD";
                if (rewardText != null) rewardText.text = "Watch Ad for Extra Spin!";
            }
            else
            {
                // İki hak da bitti
                if (spinButton != null) spinButton.interactable = false;
                if (btnText != null) btnText.text = "NO SPINS";
                if (rewardText != null) rewardText.text = "Next Spin: " + GetTimeToNextSpin();
            }

            if (closeButtonObj != null) closeButtonObj.SetActive(true);
        }

        private void Update()
        {
            // Sürekli kalan süreyi güncelle (çark dönmüyorsa ve ne free ne de ad spin yoksa)
            if (!isSpinning && !IsSpinAvailable() && !IsAdSpinAvailable())
            {
                if (rewardText != null)
                    rewardText.text = "Next Spin: " + GetTimeToNextSpin();
            }
        }
        
        public void Spin()
        {
            if (isSpinning) return;

            bool isFreeSpin = IsSpinAvailable();
            bool isAdSpin = IsAdSpinAvailable();

            if (isFreeSpin)
            {
                // Ücretsiz çevirme
                StartCoroutine(SpinRoutine(isFreeSpin: true));
            }
            else if (isAdSpin)
            {
                // Reklamlı çevirme
                if (AdManager.Instance != null)
                {
                    if (rewardText != null) rewardText.text = "Loading Ad...";
                    AdManager.Instance.ShowRewardedAd(
                        onSuccess: () => { StartCoroutine(SpinRoutine(isFreeSpin: false)); },
                        onFailed: () => { 
                            if (rewardText != null) rewardText.text = "Ad Failed. Try again."; 
                        }
                    );
                }
                else
                {
                    if (rewardText != null) rewardText.text = "Ad System Offline";
                }
            }
            else
            {
                // Hiçbir hak yok (Buraya normalde tıklanamaz ama önlem)
                if (rewardText != null)
                    rewardText.text = "Next Spin: " + GetTimeToNextSpin();
            }
        }
        
        private void ClosePanel()
        {
            if (isSpinning) return;
            gameObject.SetActive(false);
            if (backdrop != null) backdrop.SetActive(false);
        }
        
        private IEnumerator SpinRoutine(bool isFreeSpin)
        {
            isSpinning = true;
            if (spinButton != null) spinButton.interactable = false;
            if (closeButtonObj != null) closeButtonObj.SetActive(false);
            if (rewardText != null) rewardText.text = "Spinning...";
            
            // Random slice to land on (0 to 7)
            int randomSlice = UnityEngine.Random.Range(0, 8);
            
            // +22.5f shifts from bar edge to slice center (bars are at 0,45,90... icons are between them)
            // Since the wheel rotates clockwise (angle decreases), to bring slice 'N' to the top,
            // we need to rotate by 360 - (N * 45) - 22.5 degrees.
            float jitter = UnityEngine.Random.Range(-10f, 10f);
            float targetAngle = 360f - (randomSlice * 45f) - 22.5f;
            float totalRotation = (360f * spinRevolutions) + targetAngle + jitter;
            
            float elapsed = 0f;
            float startAngle = wheelTransform.eulerAngles.z;
            int lastSlice = Mathf.FloorToInt((wheelTransform.eulerAngles.z % 360) / 45f);
            
            // EaseOutQuart
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spinDuration;
                float easeOut = 1 - Mathf.Pow(1 - t, 4);
                
                float currentAngle = Mathf.Lerp(startAngle, startAngle - totalRotation, easeOut);
                wheelTransform.rotation = Quaternion.Euler(0, 0, currentAngle);

                int currentSlice = Mathf.FloorToInt((Mathf.Abs(currentAngle) % 360) / 45f);
                if (currentSlice != lastSlice)
                {
                    lastSlice = currentSlice;
                    if (AudioManager.Instance != null && tickSound != null)
                        AudioManager.Instance.PlaySFX(tickSound, 0.5f, UnityEngine.Random.Range(0.9f, 1.1f));
                }

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

            // Çevirme süresini kaydet.
            if (MetaProgressionManager.Instance != null)
            {
                if (isFreeSpin)
                    MetaProgressionManager.Instance.SetLastFortuneWheelTime(DateTime.Now.Ticks);
                else
                    MetaProgressionManager.Instance.SetLastFortuneWheelAdTime(DateTime.Now.Ticks);
            }
            
            // Ödül metni: Slice numarasına göre sabit metin
            if (rewardText != null)
            {
                if (karmaRewards[randomSlice] > 0)
                    rewardText.text = "You won " + karmaRewards[randomSlice] + " Karma!";
                else if (crystalRewards[randomSlice] > 0)
                    rewardText.text = "You won " + crystalRewards[randomSlice] + " Crystals!";
                else
                    rewardText.text = "No reward this spin.";
            }
            
            isSpinning = false;
            
            // UI'ı mevcut duruma göre (reklamlı çevirmeye uygun şekilde) geri yükle
            UpdateUI();
            if (closeButtonObj != null) closeButtonObj.SetActive(true);
        }
    }
}
