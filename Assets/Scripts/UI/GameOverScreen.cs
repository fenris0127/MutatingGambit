using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays the game over screen with victory or defeat status.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject gameOverPanel;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private TextMeshProUGUI statsText;

        [SerializeField]
        private Button restartButton;

        [SerializeField]
        private Button mainMenuButton;

        [SerializeField]
        private Button quitButton;

        [Header("Visual Settings")]
        [SerializeField]
        private Color victoryColor = new Color(0.3f, 1f, 0.3f); // Green

        [SerializeField]
        private Color defeatColor = new Color(1f, 0.3f, 0.3f); // Red

        [SerializeField]
        private string victoryTitle = "Victory!";

        [SerializeField]
        private string defeatTitle = "Defeat";

        [Header("Events")]
        public UnityEvent OnRestart;
        public UnityEvent OnMainMenu;
        public UnityEvent OnQuit;

        private bool isVictory = false;

        private void Awake()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            SetupButtons();
        }

        /// <summary>
        /// Sets up button listeners.
        /// </summary>
        private void SetupButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestart);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(HandleMainMenu);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(HandleQuit);
            }
        }

        /// <summary>
        /// Shows the game over screen with victory status.
        /// </summary>
        public void ShowVictory(string message = "", GameStats stats = null)
        {
            isVictory = true;
            Show(message, stats);
        }

        /// <summary>
        /// Shows the game over screen with defeat status.
        /// </summary>
        public void ShowDefeat(string message = "", GameStats stats = null)
        {
            isVictory = false;
            Show(message, stats);
        }

        /// <summary>
        /// Shows the game over screen for dungeon completion.
        /// </summary>
        public void ShowDungeonComplete(GameStats stats = null)
        {
            isVictory = true;
            Show("Dungeon Conquered!", stats);
            
            if (titleText != null)
            {
                titleText.text = "DUNGEON CLEARED";
            }
        }

        /// <summary>
        /// Shows the game over screen.
        /// </summary>
        private void Show(string message, GameStats stats)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = isVictory ? victoryTitle : defeatTitle;
                titleText.color = isVictory ? victoryColor : defeatColor;
            }

            // Set message
            if (messageText != null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = isVictory
                        ? "You have conquered the dungeon!"
                        : "Your pieces have fallen...";
                }
                messageText.text = message;
            }

            // Set stats
            if (statsText != null && stats != null)
            {
                statsText.text = FormatStats(stats);
            }
        }

        /// <summary>
        /// Formats game stats for display.
        /// </summary>
        private string FormatStats(GameStats stats)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Rooms Cleared: {stats.RoomsCleared}");
            sb.AppendLine($"Artifacts Collected: {stats.ArtifactsCollected}");
            sb.AppendLine($"Pieces Lost: {stats.PiecesLost}");
            sb.AppendLine($"Total Moves: {stats.TotalMoves}");
            return sb.ToString();
        }

        /// <summary>
        /// Hides the game over screen.
        /// </summary>
        public void Hide()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Handles restart button click.
        /// </summary>
        private void HandleRestart()
        {
            OnRestart?.Invoke();

            // Default behavior: reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Handles main menu button click.
        /// </summary>
        private void HandleMainMenu()
        {
            OnMainMenu?.Invoke();

            // Default behavior: load scene 0 (assumed to be main menu)
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Handles quit button click.
        /// </summary>
        private void HandleQuit()
        {
            OnQuit?.Invoke();

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Checks if the screen is currently visible.
        /// </summary>
        public bool IsVisible => gameOverPanel != null && gameOverPanel.activeSelf;
    }

    /// <summary>
    /// Container for game statistics.
    /// </summary>
    [System.Serializable]
    public class GameStats
    {
        public int RoomsCleared;
        public int ArtifactsCollected;
        public int PiecesLost;
        public int TotalMoves;
        public float PlayTime;

        public GameStats()
        {
            RoomsCleared = 0;
            ArtifactsCollected = 0;
            PiecesLost = 0;
            TotalMoves = 0;
            PlayTime = 0f;
        }
    }
}
