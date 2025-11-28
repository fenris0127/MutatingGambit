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

            // Simple fade in (requires LeanTween or DOTween for animation)
            // TODO: Implement with Unity's built-in animation or coroutine
            CanvasGroup canvasGroup = introPanel.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = introPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f; // Instantly show for now
        }

        /// <summary>
        /// Hides the boss introduction.
        /// </summary>
        public void Hide()
        {
            if (introPanel != null)
            {
                // Simple hide (requires animation library for fade out)
                // TODO: Implement fade out with coroutine or Unity Animation
                introPanel.SetActive(false);
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
