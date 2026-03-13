using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuckyQuest.UI
{
    public sealed class RewardPopupController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button collectButton;
        [SerializeField] private float showDuration = 0.2f;
        [SerializeField] private float hideDuration = 0.18f;

        private Tween _activeTween;

        private void Awake()
        {
            if (collectButton != null)
                collectButton.onClick.AddListener(Hide);

            SetVisibleInstant(false);
        }

        private void OnDestroy()
        {
            if (collectButton != null)
                collectButton.onClick.RemoveListener(Hide);

            _activeTween?.Kill();
        }

        public void Show(string title, string reward)
        {
            titleText.text = title;
            rewardText.text = reward;

            gameObject.SetActive(true);
            _activeTween?.Kill();

            canvasGroup.alpha = 0f;
            popupRoot.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, showDuration));
            sequence.Join(popupRoot.DOScale(1f, showDuration).SetEase(Ease.OutBack));
            _activeTween = sequence;
        }

        public void Hide()
        {
            _activeTween?.Kill();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0f, hideDuration));
            sequence.Join(popupRoot.DOScale(0.95f, hideDuration).SetEase(Ease.InBack));
            sequence.OnComplete(() => SetVisibleInstant(false));
            _activeTween = sequence;
        }

        private void SetVisibleInstant(bool visible)
        {
            gameObject.SetActive(visible);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }

            if (popupRoot != null)
                popupRoot.localScale = Vector3.one;
        }
    }
}
