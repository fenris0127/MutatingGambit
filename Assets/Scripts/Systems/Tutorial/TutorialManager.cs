using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField]
        private List<TutorialStep> steps;
        
        [Header("References")]
        [SerializeField]
        private Board board;
        [SerializeField]
        private TutorialUI tutorialUI;
        [SerializeField]
        private GameObject highlightPrefab;

        private int currentStepIndex = -1;
        private bool isTutorialActive = false;

        public bool IsTutorialActive => isTutorialActive;
        public TutorialStep CurrentStep => (isTutorialActive && currentStepIndex >= 0 && currentStepIndex < steps.Count) ? steps[currentStepIndex] : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (board == null) board = Board.Instance;
            if (tutorialUI == null) tutorialUI = FindObjectOfType<TutorialUI>();

            // Subscribe to board events
            if (board != null)
            {
                board.OnPieceMoved += HandlePieceMoved;
            }
        }

        private void OnDestroy()
        {
            if (board != null)
            {
                board.OnPieceMoved -= HandlePieceMoved;
            }
        }

        public void StartTutorial()
        {
            if (steps == null || steps.Count == 0) return;

            isTutorialActive = true;
            currentStepIndex = -1;
            NextStep();
        }

        public void NextStep()
        {
            currentStepIndex++;
            if (currentStepIndex >= steps.Count)
            {
                EndTutorial();
                return;
            }

            UpdateStep();
        }

        private void UpdateStep()
        {
            TutorialStep step = steps[currentStepIndex];
            
            // Update UI
            if (tutorialUI != null)
            {
                tutorialUI.ShowStep(step);
            }

            // Highlight tiles
            ClearHighlights();
            
            if (step.restrictMovement)
            {
                if (step.requiredMoveFrom.x >= 0) HighlightTile(step.requiredMoveFrom);
                if (step.requiredMoveTo.x >= 0) HighlightTile(step.requiredMoveTo);
            }
        }

        private List<GameObject> activeHighlights = new List<GameObject>();

        private void HighlightTile(Vector2Int position)
        {
            if (highlightPrefab == null) return;

            // Assuming board is at z=0 or similar
            Vector3 worldPos = new Vector3(position.x, position.y, 0);
            GameObject highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity);
            activeHighlights.Add(highlight);
        }

        private void ClearHighlights()
        {
            foreach (var highlight in activeHighlights)
            {
                if (highlight != null) Destroy(highlight);
            }
            activeHighlights.Clear();
        }

        public void EndTutorial()
        {
            isTutorialActive = false;
            if (tutorialUI != null)
            {
                tutorialUI.Hide();
            }
            ClearHighlights();
            Debug.Log("Tutorial Completed");
        }

        private void HandlePieceMoved(Piece piece, Vector2Int from, Vector2Int to, Piece captured)
        {
            if (!isTutorialActive) return;

            TutorialStep step = CurrentStep;
            if (step != null && step.completeOnMove)
            {
                // Check if move matches requirements
                if (step.restrictMovement)
                {
                    if (step.requiredMoveFrom.x >= 0 && from != step.requiredMoveFrom) return;
                    if (step.requiredMoveTo.x >= 0 && to != step.requiredMoveTo) return;
                }

                NextStep();
            }
        }

        /// <summary>
        /// Validates if a move is allowed during tutorial.
        /// Should be called by Board/GameManager before executing a move.
        /// </summary>
        public bool IsMoveAllowed(Vector2Int from, Vector2Int to)
        {
            if (!isTutorialActive) return true;

            TutorialStep step = CurrentStep;
            if (step == null) return true;

            if (step.restrictMovement)
            {
                if (step.requiredMoveFrom.x >= 0 && from != step.requiredMoveFrom) return false;
                if (step.requiredMoveTo.x >= 0 && to != step.requiredMoveTo) return false;
            }

            return true;
        }

        /// <summary>
        /// Called when a piece is selected.
        /// </summary>
        public void OnPieceSelected(Piece piece)
        {
            // Tutorial logic for piece selection can be added here
            if (!isTutorialActive) return;

            TutorialStep step = CurrentStep;
            if (step != null && step.restrictMovement && step.requiredMoveFrom.x >= 0)
            {
                // Validate if the correct piece was selected
                if (piece != null && piece.Position != step.requiredMoveFrom)
                {
                    Debug.Log("Tutorial: Please select the highlighted piece.");
                }
            }
        }
    }
}
