using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using MutatingGambit.AI;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays dramatic boss introduction sequence.
    /// Shows boss name, description, and abilities.
    /// </summary>
    public class BossIntroUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject introPanel;

        [SerializeField]
        private TextMeshProUGUI bossNameText;

        [SerializeField]
        private TextMeshProUGUI bossDescriptionText;

        [SerializeField]
        private Image bossPortrait;

        [SerializeField]
        private Button startBattleButton;

        [Header("Animation")]
        [SerializeField]
        private float introAnimationDuration = 2f;

        [SerializeField]
        private bool useAnimations = true;

        [Header("Events")]
        public UnityEvent OnBattleStart;

        private void Awake()
        {
            if (introPanel != null)
            {
                introPanel.SetActive(false);
            }

            if (startBattleButton != null)
            {
                startBattleButton.onClick.AddListener(OnStartBattleClicked);
            }
        }

        /// <summary>
        /// Shows the boss introduction with given boss AI.
        /// </summary>
        public void ShowIntro(BossAI boss)
        {
            if (boss == null)
            {
                Debug.LogWarning("Cannot show boss intro - boss is null!");
                return;
            }

            if (bossNameText != null)
            {
                bossNameText.text = boss.BossName;
            }

            if (bossDescriptionText != null)
            {
                bossDescriptionText.text = boss.BossDescription;
            }

            if (introPanel != null)
            {
                introPanel.SetActive(true);

                if (useAnimations)
                {
                    AnimateIntro();
                }
            }

            Debug.Log($"Boss intro: {boss.BossName}");
        }

        /// <summary>
        /// Animates the boss introduction.
        /// </summary>
        private void AnimateIntro()
        {
            if (introPanel == null) return;

            // Fade in animation
            CanvasGroup canvasGroup = introPanel.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = introPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;

            LeanTween.alphaCanvas(canvasGroup, 1f, introAnimationDuration)
                .setEaseInOutQuad();

            // Scale animation
            RectTransform rectTransform = introPanel.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.zero;

                LeanTween.scale(rectTransform, Vector3.one, introAnimationDuration)
                    .setEaseOutBack();
            }
        }

        /// <summary>
        /// Hides the boss introduction.
        /// </summary>
        public void Hide()
        {
            if (introPanel != null)
            {
                if (useAnimations)
                {
                    CanvasGroup canvasGroup = introPanel.GetComponent<CanvasGroup>();

                    if (canvasGroup != null)
                    {
                        LeanTween.alphaCanvas(canvasGroup, 0f, 0.5f)
                            .setOnComplete(() => introPanel.SetActive(false));
                    }
                    else
                    {
                        introPanel.SetActive(false);
                    }
                }
                else
                {
                    introPanel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Called when start battle button is clicked.
        /// </summary>
        private void OnStartBattleClicked()
        {
            OnBattleStart?.Invoke();
            Hide();
        }

        /// <summary>
        /// Sets the boss portrait image.
        /// </summary>
        public void SetPortrait(Sprite portrait)
        {
            if (bossPortrait != null)
            {
                bossPortrait.sprite = portrait;
            }
        }
    }
}
