using UnityEngine;
using TMPro;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Represents a single entry in the move history panel.
    /// </summary>
    public class MoveHistoryEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI moveText;

        /// <summary>
        /// Sets the text for this history entry.
        /// </summary>
        public void SetText(string text)
        {
            if (moveText != null)
            {
                moveText.text = text;
            }
        }
    }
}
