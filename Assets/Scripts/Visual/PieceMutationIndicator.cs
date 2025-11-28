using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Mutations;

namespace MutatingGambit.Visual
{
    /// <summary>
    /// Visual indicator that shows when a piece has mutations applied.
    /// Displays a small icon or glow effect above the piece.
    /// </summary>
    public class PieceMutationIndicator : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField]
        private GameObject mutationIconPrefab;
        
        [SerializeField]
        private Transform iconContainer;
        
        [SerializeField]
        private Color mutationGlowColor = new Color(0.6f, 0.3f, 0.9f, 0.7f);
        
        [SerializeField]
        private ParticleSystem mutationParticles;
        
        [SerializeField]
        private SpriteRenderer glowRenderer;

        private Piece piece;
        private bool hasMutations;

        private void Start()
        {
            piece = GetComponent<Piece>();
            if (piece == null)
            {
                Debug.LogError("PieceMutationIndicator requires a Piece component!");
                return;
            }

            // Subscribe to mutation events
            if (MutationManager.Instance != null)
            {
                MutationManager.Instance.OnMutationApplied += OnMutationChange;
                MutationManager.Instance.OnMutationRemoved += OnMutationChange;
            }

            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (MutationManager.Instance != null)
            {
                MutationManager.Instance.OnMutationApplied -= OnMutationChange;
                MutationManager.Instance.OnMutationRemoved -= OnMutationChange;
            }
        }

        private void OnMutationChange(Piece affectedPiece, Mutation mutation)
        {
            if (affectedPiece == piece)
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (piece == null || MutationManager.Instance == null) return;

            var mutations = MutationManager.Instance.GetMutations(piece);
            hasMutations = mutations.Count > 0;

            // Update glow
            if (glowRenderer != null)
            {
                glowRenderer.enabled = hasMutations;
                if (hasMutations)
                {
                    glowRenderer.color = mutationGlowColor;
                }
            }

            // Update particles
            if (mutationParticles != null)
            {
                if (hasMutations && !mutationParticles.isPlaying)
                {
                    mutationParticles.Play();
                }
                else if (!hasMutations && mutationParticles.isPlaying)
                {
                    mutationParticles.Stop();
                }
            }

            // Update mutation icons
            if (iconContainer != null)
            {
                // Clear existing icons
                foreach (Transform child in iconContainer)
                {
                    Destroy(child.gameObject);
                }

                // Create icons for each mutation (max 3 visible)
                int displayCount = Mathf.Min(mutations.Count, 3);
                for (int i = 0; i < displayCount; i++)
                {
                    if (mutationIconPrefab != null)
                    {
                        GameObject iconObj = Instantiate(mutationIconPrefab, iconContainer);
                        MutationIcon icon = iconObj.GetComponent<MutationIcon>();
                        if (icon != null)
                        {
                            icon.Setup(mutations[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Plays a visual effect when a mutation is applied.
        /// </summary>
        public void PlayMutationAppliedEffect()
        {
            if (mutationParticles != null)
            {
                mutationParticles.Play();
            }

            // Could add additional effects like:
            // - Screen shake
            // - Flash effect
            // - Sound effect (call AudioManager)
        }
    }

    /// <summary>
    /// Simple component for displaying a mutation icon.
    /// </summary>
    public class MutationIcon : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer iconRenderer;

        private Mutation mutation;

        public void Setup(Mutation mut)
        {
            mutation = mut;
            if (iconRenderer != null && mut != null && mut.Icon != null)
            {
                iconRenderer.sprite = mut.Icon;
            }
        }

        // Could add tooltip on mouse hover
        private void OnMouseEnter()
        {
            if (mutation != null && UI.TooltipManager.Instance != null)
            {
                // Show tooltip with mutation name and description
                UI.TooltipManager.Instance.ShowTooltip(mutation.MutationName, mutation.Description);
            }
        }

        private void OnMouseExit()
        {
            if (UI.TooltipManager.Instance != null)
            {
                UI.TooltipManager.Instance.HideTooltip();
            }
        }
    }
}
