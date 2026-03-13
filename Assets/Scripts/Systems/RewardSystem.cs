using LuckyQuest.Data;

namespace LuckyQuest.Systems
{
    public sealed class RewardSystem
    {
        public void Grant(PlayerState player, SymbolType symbol)
        {
            switch (symbol)
            {
                case SymbolType.Coin:
                    player.Coins += 100;
                    break;
                case SymbolType.Gem:
                    player.Gems += 5;
                    break;
                case SymbolType.Wood:
                    player.Wood += 20;
                    break;
                case SymbolType.Energy:
                    player.Energy += 3;
                    break;
                case SymbolType.Chest:
                    player.Coins += 250;
                    player.Gems += 2;
                    player.ChestWins += 1;
                    break;
                case SymbolType.Wild:
                    player.Coins += 50;
                    break;
            }

            player.ClampToValidRanges();
        }

        public int GetRewardAmount(SymbolType symbol)
        {
            return symbol switch
            {
                SymbolType.Coin => 100,
                SymbolType.Gem => 5,
                SymbolType.Wood => 20,
                SymbolType.Energy => 3,
                SymbolType.Chest => 252,
                SymbolType.Wild => 50,
                _ => 0
            };
        }

        public string GetRewardLabel(SymbolType symbol)
        {
            return symbol switch
            {
                SymbolType.Coin => "100 Coins",
                SymbolType.Gem => "5 Gems",
                SymbolType.Wood => "20 Wood",
                SymbolType.Energy => "3 Energy",
                SymbolType.Chest => "Chest Bonus",
                SymbolType.Wild => "50 Coins",
                _ => "Unknown Reward"
            };
        }
    }
}
