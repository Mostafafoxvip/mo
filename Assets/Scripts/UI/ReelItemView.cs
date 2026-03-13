using TMPro;
using UnityEngine;

namespace LuckyQuest.UI
{
    public sealed class ReelItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public void SetLabel(string value)
        {
            if (label != null)
                label.text = value;
        }
    }
}
