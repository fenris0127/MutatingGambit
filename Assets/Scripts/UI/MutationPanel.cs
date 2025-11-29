using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays all active mutations in the left sidebar.
    /// Automatically updates when mutations are applied/removed.
    /// </summary>
    public class MutationPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject mutationCardPrefab;
        [SerializeField]
        private Transform contentContainer;
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private Board board;

        [Header("Settings")]
        [SerializeField]
        private Color normalColor = new Color(0.3f, 0.2f, 0.5f);
        [SerializeField]
        private Color highlightColor = new Color(0.5f, 0.3f, 0.7f);

        private List<MutationCardUI> activeCards = new List<MutationCardUI>();
        private Dictionary<Mutation, MutationCardUI> mutationToCard = new Dictionary<Mutation, MutationCardUI>();

        private void Start()
        {
            // Cache board reference if not assigned
            if (board == null)
            {
                board = Board.Instance;
            }

            if (titleText != null)
            {
                titleText.text = "Active Mutations";
            }

            // Subscribe to MutationManager events
            if (MutationManager.Instance != null)
            {
                // Note: We need to add events to MutationManager
                // For now, we'll poll or manually update
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes the mutation display.
        /// Call this after mutations change.
        /// </summary>
        public void RefreshDisplay()
        {
            // Clear existing cards
            ClearCards();

            if (MutationManager.Instance == null || board == null)
            {
                return;
            }

            // Get all unique mutations from all pieces
            HashSet<Mutation> uniqueMutations = new HashSet<Mutation>();
            var allPieces = board.GetAllPieces();
            foreach (var piece in allPieces)
            {
                if (piece != null && piece.Team == Team.White) // Assuming player is White
                {
                    var mutations = MutationManager.Instance.GetMutations(piece);
                    foreach (var mutation in mutations)
                    {
                        uniqueMutations.Add(mutation);
                    }
                }
            }

            // Create cards for each unique mutation
            foreach (var mutation in uniqueMutations)
            {
                CreateMutationCard(mutation);
            }
        }

        private void CreateMutationCard(Mutation mutation)
        {
            if (mutationCardPrefab == null || contentContainer == null)
            {
                return;
            }

            GameObject cardObj = Instantiate(mutationCardPrefab, contentContainer);
            MutationCardUI card = cardObj.GetComponent<MutationCardUI>();
            
            if (card == null)
            {
                card = cardObj.AddComponent<MutationCardUI>();
            }

            card.Setup(mutation);
            activeCards.Add(card);
            mutationToCard[mutation] = card;
        }

        private void ClearCards()
        {
            foreach (var card in activeCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            activeCards.Clear();
            mutationToCard.Clear();
        }

        /// <summary>
        /// Highlights a specific mutation (e.g., when hovering over a piece with that mutation).
        /// </summary>
        public void HighlightMutation(Mutation mutation, bool highlight)
        {
            if (mutationToCard.TryGetValue(mutation, out MutationCardUI card))
            {
                card.SetHighlight(highlight);
            }
        }
    }

    /// <summary>
    /// Individual mutation card UI element.
    /// </summary>
    public class MutationCardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private Image iconImage;
        [SerializeField]
        private TextMeshProUGUI nameText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;
        [SerializeField]
        private Image backgroundImage;

        private Mutation mutation;
        private Color normalColor = new Color(0.3f, 0.2f, 0.5f, 0.8f);
        private Color highlightColor = new Color(0.5f, 0.3f, 0.7f, 1f);

        public void Setup(Mutation mut)
        {
            mutation = mut;

            if (mut == null) return;

            if (nameText != null)
            {
                nameText.text = mut.MutationName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = mut.Description;
            }

            if (iconImage != null && mut.Icon != null)
            {
                iconImage.sprite = mut.Icon;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        public void SetHighlight(bool highlight)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = highlight ? highlightColor : normalColor;
            }
        }

        // Tooltip support
        public void OnPointerEnter()
        {
            if (mutation != null && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ShowTooltip(mutation.MutationName, mutation.Description);
            }
        }

        public void OnPointerExit()
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltip();
            }
        }
    }
}
