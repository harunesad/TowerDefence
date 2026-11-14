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
            EnsureNetworkTimeManager();
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

        public int GetPendingCount()
        {
            if (!TryGetWindow(out int claimed, out int unlocked, out bool closed)) return 0;
            if (closed || claimed >= 30) return 0;
            return Mathf.Max(0, unlocked - claimed);
        }

        public bool IsMonthComplete()
        {
            if (GetCurrentDayIndex() >= 30) return true;
            return TryGetWindow(out _, out _, out bool closed) && closed;
        }

        public bool IsRewardAvailable()
        {
            return GetPendingCount() > 0;
        }

        public void PrepareLessonClaim()
        {
            if (MetaProgressionManager.Instance == null) return;
            if (!TryGetTrustedNow(out DateTime now)) return;
            int claimed = GetCurrentDayIndex();
            if (claimed <= 0 || claimed >= 30) return;
            long lastTicks = MetaProgressionManager.Instance.GetLastDailyRewardTime();
            if (lastTicks == 0 || new DateTime(lastTicks).Date != now.Date) return;
            if (GetPendingCount() <= 0) return;
            long pinned = now.Date.AddDays(1 - claimed).Ticks;
            if (MetaProgressionManager.Instance.GetDailyRewardStartTicks() == pinned) return;
            MetaProgressionManager.Instance.SetDailyRewardStartTicks(pinned);
        }

        public bool ClaimReward(out int karma, out int crystal)
        {
            karma = 0;
            crystal = 0;

            if (!TryGetWindow(out int claimed, out int unlocked, out bool closed)) return false;
            if (closed || claimed >= unlocked || claimed >= 30) return false;
            if (!TryGetTrustedNow(out DateTime now)) return false;

            int lastDay = Mathf.Min(unlocked, 30);
            for (int i = claimed; i < lastDay; i++)
            {
                DailyRewardTier tier = rewardTiers[i];
                if (tier == null) continue;
                karma += tier.karmaReward;
                crystal += tier.crystalReward;
            }

            long startTicks = MetaProgressionManager.Instance.GetDailyRewardStartTicks();
            if (startTicks == 0)
                startTicks = now.Date.Ticks;

            MetaProgressionManager.Instance.GrantDailyReward(karma, crystal, now.Ticks, lastDay, startTicks);
            Debug.Log($"[DailyReward] Claimed days {claimed + 1}-{lastDay}: {karma} Karma, {crystal} Crystals");
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
            if (IsMonthComplete()) return "Completed!";
            if (GetPendingCount() > 0) return "Available Now!";
            if (!TryGetTrustedNow(out DateTime now)) return "Syncing time...";

            TimeSpan remaining = now.Date.AddDays(1) - now;
            if (remaining.Ticks <= 0) return "Available Now!";

            int hours = (int)remaining.TotalHours;
            return string.Format("{0:D2}h {1:D2}m {2:D2}s", hours, remaining.Minutes, remaining.Seconds);
        }

        private static bool TryGetTrustedNow(out DateTime now)
        {
            now = default;
            if (NetworkTimeManager.Instance == null) return false;
            now = NetworkTimeManager.Instance.GetCurrentLocalTime();
            return true;
        }

        private bool TryGetWindow(out int claimed, out int unlocked, out bool closed)
        {
            claimed = 0;
            unlocked = 0;
            closed = false;
            if (MetaProgressionManager.Instance == null) return false;

            claimed = Mathf.Clamp(MetaProgressionManager.Instance.GetDailyRewardDayIndex(), 0, 30);
            long startTicks = MetaProgressionManager.Instance.GetDailyRewardStartTicks();
            if (startTicks == 0)
            {
                unlocked = claimed > 0 ? claimed : 1;
                closed = claimed >= 30;
                return true;
            }

            if (!TryGetTrustedNow(out DateTime now)) return false;

            DateTime start = new DateTime(startTicks).Date;
            int elapsed = (now.Date - start).Days;
            if (elapsed < 0) elapsed = 0;
            if (elapsed >= 30)
            {
                closed = true;
                unlocked = claimed;
                return true;
            }

            unlocked = Mathf.Min(30, elapsed + 1);
            return true;
        }

        public string GetDayStatus(int dayIndex)
        {
            if (!TryGetWindow(out int claimed, out int unlocked, out bool closed))
                return dayIndex < GetCurrentDayIndex() ? "claimed" : "locked";

            if (dayIndex < claimed) return "claimed";
            if (!closed && dayIndex < unlocked) return "available";
            if (!closed && dayIndex == unlocked && unlocked < 30) return "waiting";
            return "locked";
        }

        private void EnsureNetworkTimeManager()
        {
            if (NetworkTimeManager.Instance == null)
            {
                GameObject go = new GameObject("NetworkTimeManager", typeof(NetworkTimeManager));
            }
        }
    }
}
