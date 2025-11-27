using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MutatingGambit.Systems.Tutorial
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject panel;
        [SerializeField]
        private TextMeshProUGUI messageText;
        [SerializeField]
        private Button nextButton;
        [SerializeField]
        private TextMeshProUGUI nextButtonText;

        private void Start()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }
            Hide();
        }

        public void ShowStep(TutorialStep step)
        {
            if (step == null) return;

            panel.SetActive(true);
            messageText.text = step.message;

            if (step.showButton)
            {
                nextButton.gameObject.SetActive(true);
                if (nextButtonText != null) nextButtonText.text = step.buttonText;
            }
            else
            {
                nextButton.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void OnNextClicked()
        {
            TutorialManager.Instance.NextStep();
        }
    }
}
