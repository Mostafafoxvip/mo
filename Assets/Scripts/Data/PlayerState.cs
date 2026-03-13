using System;

namespace LuckyQuest.Data
{
    [Serializable]
    public sealed class PlayerState
    {
        public int Coins = 0;
        public int Gems = 0;
        public int Wood = 0;
        public int Energy = 20;
        public int Level = 1;
        public int TotalSpins = 0;
        public int ChestWins = 0;
        public long LastEnergyTimestampUnix = 0;

        public void ClampToValidRanges()
        {
            Coins = Math.Max(0, Coins);
            Gems = Math.Max(0, Gems);
            Wood = Math.Max(0, Wood);
            Energy = Math.Max(0, Energy);
            Level = Math.Max(1, Level);
            TotalSpins = Math.Max(0, TotalSpins);
            ChestWins = Math.Max(0, ChestWins);
            LastEnergyTimestampUnix = Math.Max(0, LastEnergyTimestampUnix);
        }
    }
}
