using System.Collections;
using LuckyQuest.Core;
using LuckyQuest.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuckyQuest.UI
{
    public sealed class SpinScreenController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button spinButton;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private ReelColumnAnimator reelA;
        [SerializeField] private ReelColumnAnimator reelB;
        [SerializeField] private ReelColumnAnimator reelC;
        [SerializeField] private HUDController hudController;
        [SerializeField] private RewardPopupController rewardPopup;

        [Header("Timing")]
        [SerializeField] private float columnDelay = 0.12f;

        private bool _isSpinInProgress;

        private void Start()
        {
            if (spinButton != null)
                spinButton.onClick.AddListener(OnSpinClicked);

            hudController.Refresh();
            resultText.text = "جاهز للدوران";
        }

        private void OnDestroy()
        {
            if (spinButton != null)
                spinButton.onClick.RemoveListener(OnSpinClicked);
        }

        private void OnSpinClicked()
        {
            if (_isSpinInProgress)
                return;

            if (gameManager == null)
            {
                Debug.LogError("GameManager reference is missing.");
                return;
            }

            StartCoroutine(SpinFlow());
        }

        private IEnumerator SpinFlow()
        {
            _isSpinInProgress = true;
            spinButton.interactable = false;
            resultText.text = "...يدور";

            SpinResult result = gameManager.ExecuteSpin();
            if (result.Symbols == null || result.Symbols.Count != 3)
            {
                resultText.text = result.RewardLabel;
                hudController.Refresh();
                spinButton.interactable = true;
                _isSpinInProgress = false;
                yield break;
            }

            bool aDone = false;
            bool bDone = false;
            bool cDone = false;

            reelA.Play(result.Symbols[0], () => aDone = true);
            yield return new WaitForSeconds(columnDelay);
            reelB.Play(result.Symbols[1], () => bDone = true);
            yield return new WaitForSeconds(columnDelay);
            reelC.Play(result.Symbols[2], () => cDone = true);

            yield return new WaitUntil(() => aDone && bDone && cDone);

            hudController.Refresh();

            if (result.IsWin)
            {
                resultText.text = $"فوز: {result.RewardLabel}";
                rewardPopup.Show("مكافأة", result.RewardLabel);
            }
            else
            {
                resultText.text = result.RewardLabel;
            }

            spinButton.interactable = true;
            _isSpinInProgress = false;
        }
    }
}
