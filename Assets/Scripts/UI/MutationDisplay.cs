using UnityEngine;
using UnityEngine.EventSystems;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Mutations;
using System.Collections.Generic;
using System.Linq;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays mutation indicators on chess pieces and handles hover tooltips.
    /// Attach this to piece GameObjects that can have mutations.
    /// </summary>
    public class MutationDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField]
        private Piece piece;

        [Header("Visual Indicators")]
        [SerializeField]
        private GameObject mutationIndicator;

        [SerializeField]
        private SpriteRenderer pieceRenderer;

        [SerializeField]
        private Color mutatedOutlineColor = new Color(0.5f, 1f, 0.5f, 1f); // Green glow

        [Header("Settings")]
        [SerializeField]
        private bool showTooltipOnHover = true;

        private List<Mutation> activeMutations = new List<Mutation>();
        private Material originalMaterial;
        private Material outlineMaterial;

        private void Awake()
        {
            if (piece == null)
            {
                piece = GetComponent<Piece>();
            }

            if (pieceRenderer == null)
            {
                pieceRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (pieceRenderer != null)
            {
                originalMaterial = pieceRenderer.material;
            }
        }

        private void Start()
        {
            UpdateMutationDisplay();
        }

        /// <summary>
        /// Updates the visual display based on active mutations.
        /// </summary>
        public void UpdateMutationDisplay()
        {
            if (piece == null)
            {
                return;
            }

            // Get mutations from MutationManager
            activeMutations = MutationManager.Instance.GetMutations(piece);

            bool hasMutations = activeMutations.Count > 0;

            // Show/hide mutation indicator
            if (mutationIndicator != null)
            {
                mutationIndicator.SetActive(hasMutations);
            }

            // Apply outline effect if mutated
            if (hasMutations && pieceRenderer != null)
            {
                ApplyMutationOutline();
            }
            else if (pieceRenderer != null && originalMaterial != null)
            {
                pieceRenderer.material = originalMaterial;
            }
        }

        /// <summary>
        /// Applies a colored outline to indicate mutation.
        /// </summary>
        private void ApplyMutationOutline()
        {
            // For outline effect, you would typically use a shader
            // For now, we'll just tint the sprite slightly
            if (pieceRenderer != null)
            {
                Color currentColor = pieceRenderer.color;
                pieceRenderer.color = Color.Lerp(currentColor, mutatedOutlineColor, 0.3f);
            }
        }

        /// <summary>
        /// Called when mouse enters the piece.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!showTooltipOnHover)
            {
                return;
            }

            string tooltipText = BuildTooltipText();
            TooltipManager.Instance.ShowTooltip(GetPieceTitle(), tooltipText);
        }

        /// <summary>
        /// Called when mouse exits the piece.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTooltip();
        }

        /// <summary>
        /// Builds the tooltip text showing piece info and mutations.
        /// </summary>
        private string BuildTooltipText()
        {
            if (piece == null)
            {
                return "";
            }

            var sb = new System.Text.StringBuilder();

            // Movement information
            sb.AppendLine($"<b>Movement Rules:</b>");
            if (piece.MovementRules.Count > 0)
            {
                foreach (var rule in piece.MovementRules)
                {
                    sb.AppendLine($"• {rule.name}");
                }
            }
            else
            {
                sb.AppendLine("• No movement rules");
            }

            // Mutation information
            if (activeMutations.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"<b>Active Mutations:</b>");
                foreach (var mutation in activeMutations)
                {
                    sb.AppendLine($"• <color=green>{mutation.MutationName}</color>");
                    sb.AppendLine($"  {mutation.Description}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the title for the tooltip.
        /// </summary>
        private string GetPieceTitle()
        {
            if (piece == null)
            {
                return "Unknown Piece";
            }

            string teamColor = piece.Team == Team.White ? "White" : "Black";
            return $"{teamColor} {piece.Type}";
        }

        /// <summary>
        /// Manually refreshes the mutation display.
        /// Call this when mutations are added/removed.
        /// </summary>
        public void Refresh()
        {
            UpdateMutationDisplay();
        }

        /// <summary>
        /// Sets the piece reference.
        /// </summary>
        public void SetPiece(Piece targetPiece)
        {
            piece = targetPiece;
            UpdateMutationDisplay();
        }
    }
}
