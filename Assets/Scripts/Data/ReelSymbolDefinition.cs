using System;
using UnityEngine;

namespace LuckyQuest.Data
{
    [Serializable]
    public sealed class ReelSymbolDefinition
    {
        [field: SerializeField] public SymbolType Type { get; private set; }
        [field: SerializeField] public int Weight { get; private set; } = 1;
        [field: SerializeField] public int RewardValue { get; private set; } = 0;

        // Parameterless constructor required by Unity serialisation
        public ReelSymbolDefinition() { }

        // Constructor used in tests and code-side pool construction
        public ReelSymbolDefinition(SymbolType type, int weight, int rewardValue = 0)
        {
            Type = type;
            Weight = weight;
            RewardValue = rewardValue;
        }

        public bool IsValid()
        {
            return Weight > 0;
        }
    }
}
