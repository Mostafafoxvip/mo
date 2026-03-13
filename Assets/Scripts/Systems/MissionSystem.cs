using System.Collections.Generic;
using LuckyQuest.Data;

namespace LuckyQuest.Systems
{
    public sealed class MissionSystem
    {
        private readonly List<DailyMission> _missions = new();

        public IReadOnlyList<DailyMission> Missions => _missions;

        public void CreateDefaultMissions()
        {
            _missions.Clear();
            _missions.Add(new DailyMission
            {
                Id = "spin_5",
                Title = "قم بـ 5 دورات",
                TargetValue = 5,
                RewardCoins = 150
            });
            _missions.Add(new DailyMission
            {
                Id = "win_2",
                Title = "حقق فوزين",
                TargetValue = 2,
                RewardCoins = 200
            });
            _missions.Add(new DailyMission
            {
                Id = "collect_wood",
                Title = "اجمع 40 خشب",
                TargetValue = 40,
                RewardCoins = 250
            });
        }

        public void RegisterSpin()
        {
            AddProgress("spin_5", 1);
        }

        public void RegisterWin()
        {
            AddProgress("win_2", 1);
        }

        public void RegisterWoodGain(int amount)
        {
            AddProgress("collect_wood", amount);
        }

        public int ClaimCompletedRewards(PlayerState state)
        {
            int totalClaimed = 0;

            foreach (DailyMission mission in _missions)
            {
                if (!mission.IsCompleted || mission.RewardCoins <= 0)
                    continue;

                state.Coins += mission.RewardCoins;
                totalClaimed += mission.RewardCoins;
                mission.RewardCoins = 0;
            }

            state.ClampToValidRanges();
            return totalClaimed;
        }

        private void AddProgress(string missionId, int amount)
        {
            for (int i = 0; i < _missions.Count; i++)
            {
                if (_missions[i].Id == missionId)
                {
                    _missions[i].AddProgress(amount);
                    return;
                }
            }
        }
    }
}
