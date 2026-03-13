using LuckyQuest.Data;
using UnityEngine;

namespace LuckyQuest.Systems
{
    public sealed class SaveSystem
    {
        private const string SaveKey = "lucky_quest_player_state";

        public void Save(PlayerState state)
        {
            if (state == null)
            {
                Debug.LogError("Cannot save null PlayerState.");
                return;
            }

            string json = JsonUtility.ToJson(state);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public PlayerState LoadOrCreateDefault()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return CreateDefault();

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return CreateDefault();

            PlayerState state = JsonUtility.FromJson<PlayerState>(json);
            if (state == null)
                return CreateDefault();

            state.ClampToValidRanges();
            return state;
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        private static PlayerState CreateDefault()
        {
            return new PlayerState
            {
                Coins = 500,
                Gems = 10,
                Wood = 0,
                Energy = 20,
                Level = 1,
                TotalSpins = 0,
                ChestWins = 0
            };
        }
    }
}
