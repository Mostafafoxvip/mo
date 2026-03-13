using System;
using System.Collections;
using System.Collections.Generic;
using LuckyQuest.Data;
using UnityEngine;

namespace LuckyQuest.UI
{
    public sealed class ReelColumnAnimator : MonoBehaviour
    {
        [SerializeField] private ReelItemView itemView;
        [SerializeField] private List<SymbolType> previewSymbols = new();
        [SerializeField] private float spinInterval = 0.06f;
        [SerializeField] private int minSpinSteps = 12;
        [SerializeField] private int maxSpinSteps = 18;

        private Coroutine _spinRoutine;
        private readonly System.Random _rng = new();

        public bool IsSpinning => _spinRoutine != null;

        public void Play(SymbolType finalSymbol, Action onComplete)
        {
            if (_spinRoutine != null)
                StopCoroutine(_spinRoutine);

            _spinRoutine = StartCoroutine(PlayRoutine(finalSymbol, onComplete));
        }

        private IEnumerator PlayRoutine(SymbolType finalSymbol, Action onComplete)
        {
            int totalSteps = _rng.Next(minSpinSteps, maxSpinSteps + 1);
            if (previewSymbols == null || previewSymbols.Count == 0)
                previewSymbols = new List<SymbolType>
                {
                    SymbolType.Coin,
                    SymbolType.Gem,
                    SymbolType.Wood,
                    SymbolType.Energy,
                    SymbolType.Wild,
                    SymbolType.Chest
                };

            for (int i = 0; i < totalSteps; i++)
            {
                SymbolType preview = previewSymbols[_rng.Next(0, previewSymbols.Count)];
                itemView.SetLabel(preview.ToString());
                yield return new WaitForSeconds(spinInterval + i * 0.003f);
            }

            itemView.SetLabel(finalSymbol.ToString());
            _spinRoutine = null;
            onComplete?.Invoke();
        }
    }
}
