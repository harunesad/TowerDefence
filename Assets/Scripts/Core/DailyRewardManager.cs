using UnityEngine;
using System;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class DailyRewardManager : MonoBehaviour
    {
        public static DailyRewardManager Instance { get; private set; }

        [Header("Rewards")]
        [SerializeField] private int karmaReward = 100;
        [SerializeField] private int crystalReward = 20;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsRewardAvailable()
        {
            long lastTimeTicks = MetaProgressionManager.Instance.GetLastDailyRewardTime();
            if (lastTimeTicks == 0) return true;

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date; // Bir sonraki günün başlangıcı (00:00)

            return DateTime.Now >= nextTime;
        }

        public bool ClaimReward(out int karma, out int crystal)
        {
            karma = 0;
            crystal = 0;

            if (IsRewardAvailable())
            {
                karma = karmaReward;
                crystal = crystalReward;

                MetaProgressionManager.Instance.AddKarma(karma);
                MetaProgressionManager.Instance.AddCrystals(crystal);
                MetaProgressionManager.Instance.SetLastDailyRewardTime(DateTime.Now.Ticks);

                Debug.Log($"[DailyReward] Claimed: {karma} Karma, {crystal} Crystals");
                return true;
            }

            return false;
        }

        public string GetTimeToNextReward()
        {
            long lastTimeTicks = MetaProgressionManager.Instance.GetLastDailyRewardTime();
            if (lastTimeTicks == 0) return "Available Now!";

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
            TimeSpan remaining = nextTime - DateTime.Now;

            if (remaining.Ticks <= 0) return "Available Now!";

            return string.Format("{0:D2}h {1:D2}m {2:D2}s", remaining.Hours, remaining.Minutes, remaining.Seconds);
        }
    }
}
