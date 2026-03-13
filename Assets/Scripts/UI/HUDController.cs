using LuckyQuest.Core;
using LuckyQuest.Data;
using TMPro;
using UnityEngine;

namespace LuckyQuest.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text gemsText;
        [SerializeField] private TMP_Text woodText;
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private TMP_Text levelText;

        public void Refresh()
        {
            if (gameManager == null || gameManager.PlayerState == null)
                return;

            PlayerState state = gameManager.PlayerState;
            coinsText.text = state.Coins.ToString();
            gemsText.text = state.Gems.ToString();
            woodText.text = state.Wood.ToString();
            energyText.text = state.Energy.ToString();
            levelText.text = $"Lv. {state.Level}";
        }
    }
}
