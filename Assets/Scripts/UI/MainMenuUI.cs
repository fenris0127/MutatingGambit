using UnityEngine;
using UnityEngine.UI;
using MutatingGambit.Systems.SaveLoad;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
                
                // Disable continue if no save file
                if (SaveManager.Instance != null && !SaveManager.Instance.HasSaveFile())
                {
                    continueButton.interactable = false;
                }
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnNewGameClicked()
        {
            // Clear old save
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSaveFile();
            }

            // Start new run
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.StartNewRun();
                gameObject.SetActive(false); // Hide main menu
            }
        }

        private void OnContinueClicked()
        {
            if (SaveManager.Instance != null && DungeonManager.Instance != null)
            {
                var data = SaveManager.Instance.LoadGame();
                if (data != null)
                {
                    DungeonManager.Instance.LoadRun(data);
                    gameObject.SetActive(false); // Hide main menu
                }
            }
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
