using System.Collections.Generic;
using LuckyQuest.Data;
using LuckyQuest.Systems;
using NUnit.Framework;

namespace LuckyQuest.Tests
{
    public sealed class WeightedRandomTests
    {
        [Test]
        public void Pick_AlwaysReturnsItemFromList()
        {
            var items = new List<string> { "A", "B", "C" };
            var rng = new System.Random(42);
            for (int i = 0; i < 100; i++)
            {
                string result = WeightedRandom.Pick(items, _ => 1, rng);
                Assert.IsTrue(items.Contains(result));
            }
        }

        [Test]
        public void Pick_SingleItemAlwaysReturnsThatItem()
        {
            var items = new List<string> { "Only" };
            var rng = new System.Random(0);
            Assert.AreEqual("Only", WeightedRandom.Pick(items, _ => 10, rng));
        }

        [Test]
        public void Pick_ZeroWeightItemNeverSelected()
        {
            var items = new List<string> { "Zero", "Always" };
            var weights = new Dictionary<string, int> { ["Zero"] = 0, ["Always"] = 100 };
            var rng = new System.Random(1);
            for (int i = 0; i < 200; i++)
            {
                string result = WeightedRandom.Pick(items, s => weights[s], rng);
                Assert.AreEqual("Always", result);
            }
        }
    }

    public sealed class RewardSystemTests
    {
        private RewardSystem _rewards;
        private PlayerState _player;

        [SetUp]
        public void Setup()
        {
            _rewards = new RewardSystem();
            _player = new PlayerState { Coins = 0, Gems = 0, Wood = 0, Energy = 10 };
        }

        [Test]
        public void Grant_Coin_Adds100Coins()
        {
            _rewards.Grant(_player, SymbolType.Coin);
            Assert.AreEqual(100, _player.Coins);
        }

        [Test]
        public void Grant_Gem_Adds5Gems()
        {
            _rewards.Grant(_player, SymbolType.Gem);
            Assert.AreEqual(5, _player.Gems);
        }

        [Test]
        public void Grant_Wood_Adds20Wood()
        {
            _rewards.Grant(_player, SymbolType.Wood);
            Assert.AreEqual(20, _player.Wood);
        }

        [Test]
        public void Grant_Chest_GivesBonusCoinsAndGems()
        {
            _rewards.Grant(_player, SymbolType.Chest);
            Assert.AreEqual(250, _player.Coins);
            Assert.AreEqual(2, _player.Gems);
            Assert.AreEqual(1, _player.ChestWins);
        }

        [Test]
        public void Grant_Wild_Adds50Coins()
        {
            _rewards.Grant(_player, SymbolType.Wild);
            Assert.AreEqual(50, _player.Coins);
        }
    }

    public sealed class SpinManagerTests
    {
        private static List<ReelSymbolDefinition> BuildPool()
        {
            return new List<ReelSymbolDefinition>
            {
                new ReelSymbolDefinition(SymbolType.Coin,   10, 100),
                new ReelSymbolDefinition(SymbolType.Gem,    5,  5),
                new ReelSymbolDefinition(SymbolType.Wood,   8,  20),
                new ReelSymbolDefinition(SymbolType.Energy, 6,  3),
                new ReelSymbolDefinition(SymbolType.Wild,   3,  50),
                new ReelSymbolDefinition(SymbolType.Chest,  2,  252)
            };
        }

        [Test]
        public void Spin_DeductsEnergy()
        {
            var player = new PlayerState { Energy = 5 };
            var manager = new SpinManager(BuildPool(), new RewardSystem(), 1, seed: 0);
            manager.Spin(player);
            Assert.AreEqual(4, player.Energy);
        }

        [Test]
        public void Spin_NoEnergy_ReturnsNoWinWithMessage()
        {
            var player = new PlayerState { Energy = 0 };
            var manager = new SpinManager(BuildPool(), new RewardSystem(), 1, seed: 0);
            SpinResult result = manager.Spin(player);
            Assert.IsFalse(result.IsWin);
            Assert.AreEqual("Not enough energy", result.RewardLabel);
        }

        [Test]
        public void Spin_IncrementsTotalSpins()
        {
            var player = new PlayerState { Energy = 10, TotalSpins = 0 };
            var manager = new SpinManager(BuildPool(), new RewardSystem(), 1, seed: 0);
            manager.Spin(player);
            Assert.AreEqual(1, player.TotalSpins);
        }

        [Test]
        public void Spin_ResultAlwaysHas3SymbolsWhenEnergyAvailable()
        {
            var player = new PlayerState { Energy = 20 };
            var manager = new SpinManager(BuildPool(), new RewardSystem(), 1, seed: 42);
            for (int i = 0; i < 20; i++)
            {
                SpinResult result = manager.Spin(player);
                if (result.Symbols != null)
                    Assert.AreEqual(3, result.Symbols.Count);
            }
        }
    }

    public sealed class EnergySystemTests
    {
        [Test]
        public void Regenerate_FullEnergy_DoesNothing()
        {
            var system = new EnergySystem(20, 10);
            var state = new PlayerState { Energy = 20 };
            system.Regenerate(state, 0, 3600);
            Assert.AreEqual(20, state.Energy);
        }

        [Test]
        public void Regenerate_1Hour_10MinPerEnergy_Adds6()
        {
            var system = new EnergySystem(20, 10);
            var state = new PlayerState { Energy = 0 };
            system.Regenerate(state, 0, 3600); // 60 minutes → 6 energy
            Assert.AreEqual(6, state.Energy);
        }

        [Test]
        public void Regenerate_DoesNotExceedMax()
        {
            var system = new EnergySystem(20, 10);
            var state = new PlayerState { Energy = 18 };
            system.Regenerate(state, 0, 7200); // 120 minutes → 12 energy gained, capped at 20
            Assert.AreEqual(20, state.Energy);
        }

        [Test]
        public void Regenerate_NegativeElapsed_DoesNothing()
        {
            var system = new EnergySystem(20, 10);
            var state = new PlayerState { Energy = 5 };
            system.Regenerate(state, 1000, 500);
            Assert.AreEqual(5, state.Energy);
        }
    }
}
