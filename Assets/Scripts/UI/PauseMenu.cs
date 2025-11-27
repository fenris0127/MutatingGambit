using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Pause menu with resume, restart, and quit options.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject pausePanel;

        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button restartButton;

        [SerializeField]
        private Button mainMenuButton;

        [SerializeField]
        private Button quitButton;

        [Header("Settings")]
        [SerializeField]
        private KeyCode pauseKey = KeyCode.Escape;

        [SerializeField]
        private bool pauseTimeOnOpen = true;

        [Header("Events")]
        public UnityEvent OnPause;
        public UnityEvent OnResume;
        public UnityEvent OnRestart;
        public UnityEvent OnMainMenu;
        public UnityEvent OnQuit;

        private bool isPaused = false;

        private void Awake()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            SetupButtons();
        }

        private void Update()
        {
            // Toggle pause with keyboard
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }

        /// <summary>
        /// Sets up button listeners.
        /// </summary>
        private void SetupButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(Restart);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        /// <summary>
        /// Toggles pause state.
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void Pause()
        {
            isPaused = true;

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }

            if (pauseTimeOnOpen)
            {
                Time.timeScale = 0f;
            }

            OnPause?.Invoke();
        }

        /// <summary>
        /// Resumes the game.
        /// </summary>
        public void Resume()
        {
            isPaused = false;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (pauseTimeOnOpen)
            {
                Time.timeScale = 1f;
            }

            OnResume?.Invoke();
        }

        /// <summary>
        /// Restarts the current scene.
        /// </summary>
        public void Restart()
        {
            OnRestart?.Invoke();

            // Unpause before reloading
            if (pauseTimeOnOpen)
            {
                Time.timeScale = 1f;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            OnMainMenu?.Invoke();

            // Unpause before changing scene
            if (pauseTimeOnOpen)
            {
                Time.timeScale = 1f;
            }

            SceneManager.LoadScene(0); // Assumes main menu is scene 0
        }

        /// <summary>
        /// Quits the game.
        /// </summary>
        public void QuitGame()
        {
            OnQuit?.Invoke();

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Gets whether the game is currently paused.
        /// </summary>
        public bool IsPaused => isPaused;

        private void OnDestroy()
        {
            // Ensure time is unpaused when destroyed
            if (pauseTimeOnOpen && isPaused)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
