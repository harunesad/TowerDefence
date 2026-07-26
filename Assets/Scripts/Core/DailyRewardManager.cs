using UnityEngine;
using System;
using TowerDefence.Core;

namespace TowerDefence.UI
{
    public class DailyRewardManager : MonoBehaviour
    {
        public static DailyRewardManager Instance { get; private set; }

        [System.Serializable]
        public class DailyRewardTier
        {
            public int dayNumber;
            public int karmaReward;
            public int crystalReward;
        }

        [Header("30-Day Rewards")]
        [SerializeField] private DailyRewardTier[] rewardTiers = new DailyRewardTier[30];

        public int TotalDays => 30;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultRewards();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDefaultRewards()
        {
            bool needsInit = rewardTiers == null || rewardTiers.Length == 0
                || rewardTiers[0] == null || rewardTiers[0].dayNumber == 0;
            if (needsInit)
            {
                rewardTiers = new DailyRewardTier[30];
                for (int i = 0; i < 30; i++)
                {
                    int day = i + 1;
                    int karma = 0;
                    int crystal = 0;

                    // Her gün için tek bir ödül türü (genelde Karma, her 5. gün veya özel günlerde Kristal)
                    if (day % 7 == 0 || day % 10 == 0) 
                    {
                        crystal = (day / 5) * 5 + 5; // Örn: 7. gün 5 kristal, 10. gün 15 kristal vs.
                    }
                    else
                    {
                        karma = day * 10 + 50; // Örn: 1. gün 60 karma, 2. gün 70 karma
                    }

                    if (day == 30)
                    {
                        crystal = 50;
                        karma = 0;
                    }

                    rewardTiers[i] = new DailyRewardTier
                    {
                        dayNumber = day,
                        karmaReward = karma,
                        crystalReward = crystal
                    };
                }
            }
        }

        public int GetCurrentDayIndex()
        {
            return MetaProgressionManager.Instance.GetDailyRewardDayIndex();
        }

        public bool IsRewardAvailable()
        {
            int currentDay = GetCurrentDayIndex();
            if (currentDay >= 30) return false; // All 30 completed

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastDailyRewardTime();
            if (lastTimeTicks == 0) return true; // First time ever

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
            return DateTime.Now >= nextTime;
        }

        public bool ClaimReward(out int karma, out int crystal)
        {
            karma = 0;
            crystal = 0;

            if (!IsRewardAvailable()) return false;

            int currentDay = GetCurrentDayIndex();
            if (currentDay >= 30) return false;

            DailyRewardTier tier = rewardTiers[currentDay];
            karma = tier.karmaReward;
            crystal = tier.crystalReward;

            MetaProgressionManager.Instance.AddKarma(karma);
            MetaProgressionManager.Instance.AddCrystals(crystal);
            MetaProgressionManager.Instance.SetLastDailyRewardTime(DateTime.Now.Ticks);
            MetaProgressionManager.Instance.SetDailyRewardDayIndex(currentDay + 1);

            Debug.Log($"[DailyReward] Day {tier.dayNumber} Claimed: {karma} Karma, {crystal} Crystals");
            return true;
        }

        public DailyRewardTier GetTier(int dayIndex)
        {
            if (dayIndex >= 0 && dayIndex < 30)
                return rewardTiers[dayIndex];
            return null;
        }

        public string GetTimeToNextReward()
        {
            int currentDay = GetCurrentDayIndex();
            if (currentDay >= 30) return "Completed!";

            long lastTimeTicks = MetaProgressionManager.Instance.GetLastDailyRewardTime();
            if (lastTimeTicks == 0) return "Available Now!";

            DateTime lastTime = new DateTime(lastTimeTicks);
            DateTime nextTime = lastTime.AddDays(1).Date;
            TimeSpan remaining = nextTime - DateTime.Now;

            if (remaining.Ticks <= 0) return "Available Now!";

            return string.Format("{0:D2}h {1:D2}m {2:D2}s", remaining.Hours, remaining.Minutes, remaining.Seconds);
        }

        public string GetDayStatus(int dayIndex)
        {
            int currentDay = GetCurrentDayIndex();
            if (dayIndex < currentDay) return "claimed";
            if (dayIndex == currentDay && IsRewardAvailable()) return "available";
            if (dayIndex == currentDay) return "waiting";
            return "locked";
        }
    }
}
