using System;

namespace LuckyQuest.Data
{
    [Serializable]
    public sealed class DailyMission
    {
        public string Id;
        public string Title;
        public int TargetValue;
        public int CurrentValue;
        public bool IsCompleted;
        public int RewardCoins;

        public void AddProgress(int amount)
        {
            if (IsCompleted)
                return;

            CurrentValue += amount;
            if (CurrentValue >= TargetValue)
            {
                CurrentValue = TargetValue;
                IsCompleted = true;
            }
        }
    }
}
