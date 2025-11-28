using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays whose turn it is (Player vs AI).
    /// </summary>
    public class TurnIndicator : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private TextMeshProUGUI turnText;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private GameObject playerTurnIndicator;

        [SerializeField]
        private GameObject aiTurnIndicator;

        [Header("Visual Settings")]
        [SerializeField]
        private Color playerTurnColor = new Color(0.3f, 0.6f, 1f, 0.8f); // Blue

        [SerializeField]
        private Color aiTurnColor = new Color(1f, 0.4f, 0.4f, 0.8f); // Red

        [SerializeField]
        private string playerTurnText = "Your Turn";

        [SerializeField]
        private string aiTurnText = "AI Turn";

        private Team currentTeam = Team.White;
        private Team playerTeam = Team.White;
        private bool isPlayerTurn = true;

        private void Awake()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Sets the player's team.
        /// </summary>
        public void SetPlayerTeam(Team team)
        {
            playerTeam = team;
            UpdateDisplay();
        }

        /// <summary>
        /// Sets the current active team.
        /// </summary>
        public void SetCurrentTeam(Team team)
        {
            currentTeam = team;
            isPlayerTurn = (currentTeam == playerTeam);
            UpdateDisplay();
        }

        /// <summary>
        /// Sets whose turn it is directly.
        /// </summary>
        public void SetTurn(bool playerTurn)
        {
            isPlayerTurn = playerTurn;
            UpdateDisplay();
        }

        /// <summary>
        /// Updates the visual display.
        /// </summary>
        private void UpdateDisplay()
        {
            if (turnText != null)
            {
                turnText.text = isPlayerTurn ? playerTurnText : aiTurnText;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = isPlayerTurn ? playerTurnColor : aiTurnColor;
            }

            if (playerTurnIndicator != null)
            {
                playerTurnIndicator.SetActive(isPlayerTurn);
            }

            if (aiTurnIndicator != null)
            {
                aiTurnIndicator.SetActive(!isPlayerTurn);
            }
        }

        /// <summary>
        /// Shows the turn indicator.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the turn indicator.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Animates the turn transition (optional enhancement).
        /// </summary>
        public void AnimateTurnChange()
        {
            if (backgroundImage != null)
            {
                StartCoroutine(PulseRoutine());
            }
        }

        private System.Collections.IEnumerator PulseRoutine()
        {
            Color originalColor = backgroundImage.color;
            Color flashColor = Color.white;
            float duration = 0.5f;
            float halfDuration = duration / 2f;
            float elapsed = 0f;

            // Flash to white
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                backgroundImage.color = Color.Lerp(originalColor, flashColor, elapsed / halfDuration);
                yield return null;
            }

            elapsed = 0f;
            // Fade back
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                backgroundImage.color = Color.Lerp(flashColor, originalColor, elapsed / halfDuration);
                yield return null;
            }

            backgroundImage.color = originalColor;
        }
    }
}
