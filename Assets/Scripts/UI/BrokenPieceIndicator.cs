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

            // Animate repair (requires animation system or DOTween)
            // TODO: Implement with Unity's built-in animation or coroutine
        }

        /// <summary>
        /// Animates the piece breaking.
        /// </summary>
        private void AnimateBreaking()
        {
            // Simple visual update without animation (requires LeanTween or DOTween)
            // TODO: Implement shake and fade with coroutine or Unity Animation
            UpdateVisuals(true);

            if (pieceRenderer != null)
            {
                pieceRenderer.color = brokenTint;
            }
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
