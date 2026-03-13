using System;
using System.Collections.Generic;
using LuckyQuest.Data;
using LuckyQuest.Systems;
using UnityEngine;

namespace LuckyQuest.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [Header("Spin Settings")]
        [SerializeField] private int energyCostPerSpin = 1;
        [SerializeField] private int maxEnergy = 20;
        [SerializeField] private int minutesPerEnergy = 10;

        [Header("Symbol Pool")]
        [SerializeField] private List<ReelSymbolDefinition> symbolPool = new();

        public PlayerState PlayerState { get; private set; }
        public SpinResult LastSpinResult { get; private set; }
        public MissionSystem MissionSystem => _missionSystem;

        private SaveSystem _saveSystem;
        private RewardSystem _rewardSystem;
        private SpinManager _spinManager;
        private MissionSystem _missionSystem;
        private EnergySystem _energySystem;

        private void Awake()
        {
            _saveSystem = new SaveSystem();
            _rewardSystem = new RewardSystem();
            _missionSystem = new MissionSystem();
            _energySystem = new EnergySystem(maxEnergy, minutesPerEnergy);

            PlayerState = _saveSystem.LoadOrCreateDefault();
            if (PlayerState.LastEnergyTimestampUnix <= 0)
                PlayerState.LastEnergyTimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _energySystem.Regenerate(
                PlayerState,
                PlayerState.LastEnergyTimestampUnix,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            PlayerState.LastEnergyTimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _missionSystem.CreateDefaultMissions();
            _spinManager = new SpinManager(symbolPool, _rewardSystem, energyCostPerSpin);
            _saveSystem.Save(PlayerState);
        }

        public SpinResult ExecuteSpin()
        {
            LastSpinResult = _spinManager.Spin(PlayerState);
            _missionSystem.RegisterSpin();

            if (LastSpinResult.IsWin)
                _missionSystem.RegisterWin();

            if (LastSpinResult.RewardSymbol == SymbolType.Wood)
                _missionSystem.RegisterWoodGain(LastSpinResult.RewardAmount);

            PlayerState.LastEnergyTimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _saveSystem.Save(PlayerState);
            return LastSpinResult;
        }

        public int ClaimMissionRewards()
        {
            int claimed = _missionSystem.ClaimCompletedRewards(PlayerState);
            _saveSystem.Save(PlayerState);
            return claimed;
        }

        public void SaveNow()
        {
            PlayerState.LastEnergyTimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _saveSystem.Save(PlayerState);
        }

        public void ResetProgress()
        {
            _saveSystem.Reset();
            PlayerState = _saveSystem.LoadOrCreateDefault();
            PlayerState.LastEnergyTimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _missionSystem.CreateDefaultMissions();
            _saveSystem.Save(PlayerState);
        }
    }
}
