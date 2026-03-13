using System;
using System.Collections.Generic;

namespace LuckyQuest.Data
{
    [Serializable]
    public sealed class SpinResult
    {
        public List<SymbolType> Symbols = new();
        public bool IsWin;
        public SymbolType? RewardSymbol;
        public int RewardAmount;
        public string RewardLabel = string.Empty;
    }
}
