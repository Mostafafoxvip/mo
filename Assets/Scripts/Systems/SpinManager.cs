using System;
using System.Collections.Generic;
using LuckyQuest.Data;
using UnityEngine;

namespace LuckyQuest.Systems
{
    public sealed class SpinManager
    {
        private readonly IReadOnlyList<ReelSymbolDefinition> _symbolPool;
        private readonly RewardSystem _rewardSystem;
        private readonly Random _rng;
        private readonly int _energyCostPerSpin;

        public SpinManager(
            IReadOnlyList<ReelSymbolDefinition> symbolPool,
            RewardSystem rewardSystem,
            int energyCostPerSpin,
            int? seed = null)
        {
            _symbolPool = symbolPool ?? throw new ArgumentNullException(nameof(symbolPool));
            _rewardSystem = rewardSystem ?? throw new ArgumentNullException(nameof(rewardSystem));
            _energyCostPerSpin = Mathf.Max(1, energyCostPerSpin);
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();

            if (_symbolPool.Count == 0)
                throw new InvalidOperationException("Symbol pool cannot be empty.");
        }

        public SpinResult Spin(PlayerState player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.Energy < _energyCostPerSpin)
            {
                return new SpinResult
                {
                    IsWin = false,
                    RewardAmount = 0,
                    RewardLabel = "Not enough energy"
                };
            }

            player.Energy -= _energyCostPerSpin;
            player.TotalSpins += 1;

            List<SymbolType> rolled = new(3);
            for (int i = 0; i < 3; i++)
            {
                ReelSymbolDefinition symbol = WeightedRandom.Pick(_symbolPool, s => s.Weight, _rng);
                rolled.Add(symbol.Type);
            }

            SymbolType? winningSymbol = EvaluateWinningSymbol(rolled);
            if (winningSymbol.HasValue)
            {
                _rewardSystem.Grant(player, winningSymbol.Value);
                player.ClampToValidRanges();

                return new SpinResult
                {
                    Symbols = rolled,
                    IsWin = true,
                    RewardSymbol = winningSymbol.Value,
                    RewardAmount = _rewardSystem.GetRewardAmount(winningSymbol.Value),
                    RewardLabel = _rewardSystem.GetRewardLabel(winningSymbol.Value)
                };
            }

            player.ClampToValidRanges();
            return new SpinResult
            {
                Symbols = rolled,
                IsWin = false,
                RewardAmount = 0,
                RewardLabel = "No win"
            };
        }

        private static SymbolType? EvaluateWinningSymbol(IReadOnlyList<SymbolType> symbols)
        {
            if (symbols == null || symbols.Count != 3)
                return null;

            SymbolType a = symbols[0];
            SymbolType b = symbols[1];
            SymbolType c = symbols[2];

            bool triple = a == b && b == c;
            if (triple)
                return a == SymbolType.Wild ? SymbolType.Coin : a;

            if (a == b && c == SymbolType.Wild)
                return a;
            if (a == c && b == SymbolType.Wild)
                return a;
            if (b == c && a == SymbolType.Wild)
                return b;

            return null;
        }
    }
}
