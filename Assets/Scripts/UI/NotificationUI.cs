using UnityEngine;
using TMPro;
using System.Collections;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays temporary notifications to the player.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private float displayDuration = 2f;

        [SerializeField]
        private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Shows a notification message.
        /// </summary>
        public void Show(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (canvasGroup == null) yield break;

            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Wait
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
}
