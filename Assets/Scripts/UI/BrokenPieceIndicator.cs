using UnityEngine;
using UnityEngine.UI;
using MutatingGambit.Systems.PieceManagement;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Visual indicator for broken pieces.
    /// Shows cracks, damage effects, or other visual feedback.
    /// </summary>
    public class BrokenPieceIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PieceHealth pieceHealth;

        [SerializeField]
        private SpriteRenderer pieceRenderer;

        [SerializeField]
        private GameObject brokenOverlay;

        [SerializeField]
        private ParticleSystem brokenParticles;

        [Header("Visual Settings")]
        [SerializeField]
        private Color brokenTint = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        [SerializeField]
        private Color normalTint = Color.white;

        [SerializeField]
        private Sprite brokenSprite;

        [SerializeField]
        private Sprite normalSprite;

        [Header("Animation")]
        [SerializeField]
        private bool animateBreaking = true;

        [SerializeField]
        private float breakAnimationDuration = 0.5f;

        private bool isBroken = false;

        private void Awake()
        {
            if (pieceHealth == null)
            {
                pieceHealth = GetComponent<PieceHealth>();
            }

            if (pieceRenderer == null)
            {
                pieceRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (pieceHealth != null)
            {
                pieceHealth.OnPieceBroken.AddListener(OnPieceBroken);
                pieceHealth.OnPieceRepaired.AddListener(OnPieceRepaired);
            }
        }

        private void Start()
        {
            UpdateVisuals(pieceHealth != null && pieceHealth.IsBroken);
        }

        /// <summary>
        /// Called when the piece is broken.
        /// </summary>
        private void OnPieceBroken()
        {
            if (animateBreaking)
            {
                AnimateBreaking();
            }
            else
            {
                UpdateVisuals(true);
            }

            // Play particles
            if (brokenParticles != null)
            {
                brokenParticles.Play();
            }
        }

        /// <summary>
        /// Called when the piece is repaired.
        /// </summary>
        private void OnPieceRepaired()
        {
            UpdateVisuals(false);

            // Animate repair
            StartCoroutine(RepairAnimationRoutine());
        }

        private System.Collections.IEnumerator RepairAnimationRoutine()
        {
             if (pieceRenderer != null)
             {
                 Color original = normalTint;
                 float duration = 0.5f;
                 float elapsed = 0f;
                 while (elapsed < duration)
                 {
                     elapsed += Time.deltaTime;
                     float t = elapsed / duration;
                     pieceRenderer.color = Color.Lerp(Color.green, original, t);
                     yield return null;
                 }
                 pieceRenderer.color = original;
             }
        }

        /// <summary>
        /// Animates the piece breaking.
        /// </summary>
        private void AnimateBreaking()
        {
            UpdateVisuals(true);
            StartCoroutine(ShakeRoutine());
        }

        private System.Collections.IEnumerator ShakeRoutine()
        {
            Vector3 originalPos = transform.localPosition;
            float duration = 0.5f;
            float magnitude = 0.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = originalPos + new Vector3(x, y, 0);
                yield return null;
            }
            transform.localPosition = originalPos;
        }

        /// <summary>
        /// Updates the visual state.
        /// </summary>
        private void UpdateVisuals(bool broken)
        {
            isBroken = broken;

            // Update sprite tint
            if (pieceRenderer != null)
            {
                pieceRenderer.color = broken ? brokenTint : normalTint;
            }

            // Show/hide broken overlay
            if (brokenOverlay != null)
            {
                brokenOverlay.SetActive(broken);
            }

            // Change sprite if available
            if (brokenSprite != null && normalSprite != null && pieceRenderer != null)
            {
                pieceRenderer.sprite = broken ? brokenSprite : normalSprite;
            }
        }

        /// <summary>
        /// Forces a visual update based on current piece health.
        /// </summary>
        public void RefreshVisuals()
        {
            if (pieceHealth != null)
            {
                UpdateVisuals(pieceHealth.IsBroken);
            }
        }

        private void OnDestroy()
        {
            if (pieceHealth != null)
            {
                pieceHealth.OnPieceBroken.RemoveListener(OnPieceBroken);
                pieceHealth.OnPieceRepaired.RemoveListener(OnPieceRepaired);
            }
        }
    }
}
