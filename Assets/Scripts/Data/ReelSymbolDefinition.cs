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

        public bool IsValid()
        {
            return Weight > 0;
        }
    }
}
