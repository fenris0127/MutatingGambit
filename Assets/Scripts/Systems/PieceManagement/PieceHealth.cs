using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.PieceManagement
{
    /// <summary>
    /// Tracks the health/damage state of a chess piece.
    /// Pieces can be Active or Broken.
    /// </summary>
    public class PieceHealth : MonoBehaviour
    {
        /// <summary>
        /// The current state of the piece.
        /// </summary>
        public enum PieceState
        {
            Active,     // Piece is on the board and functional
            Broken,     // Piece has been captured and needs repair
            Destroyed   // Piece is permanently lost (optional)
        }

        [Header("State")]
        [SerializeField]
        private PieceState currentState = PieceState.Active;

        [SerializeField]
        private Piece piece;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Can this piece be repaired after breaking?")]
        private bool canBeRepaired = true;

        [SerializeField]
        [Tooltip("Repair cost (if using currency system).")]
        private int repairCost = 0;

        [Header("Events")]
        public UnityEvent OnPieceBroken;
        public UnityEvent OnPieceRepaired;
        public UnityEvent OnPieceDestroyed;

        /// <summary>
        /// Gets the current state of the piece.
        /// </summary>
        public PieceState CurrentState => currentState;

        /// <summary>
        /// Gets whether the piece is currently active.
        /// </summary>
        public bool IsActive => currentState == PieceState.Active;

        /// <summary>
        /// Gets whether the piece is broken.
        /// </summary>
        public bool IsBroken => currentState == PieceState.Broken;

        /// <summary>
        /// Gets whether the piece is destroyed.
        /// </summary>
        public bool IsDestroyed => currentState == PieceState.Destroyed;

        /// <summary>
        /// Gets whether the piece can be repaired.
        /// </summary>
        public bool CanBeRepaired => canBeRepaired && currentState == PieceState.Broken;

        /// <summary>
        /// Gets the cost to repair this piece.
        /// </summary>
        public int RepairCost => repairCost;

        /// <summary>
        /// Gets the piece component.
        /// </summary>
        public Piece Piece
        {
            get
            {
                if (piece == null)
                {
                    piece = GetComponent<Piece>();
                }
                return piece;
            }
        }

        private void Awake()
        {
            if (piece == null)
            {
                piece = GetComponent<Piece>();
            }
        }

        /// <summary>
        /// Marks the piece as broken (captured).
        /// </summary>
        public void BreakPiece()
        {
            if (currentState == PieceState.Broken)
            {
                Debug.LogWarning($"Piece {Piece?.Type} is already broken!");
                return;
            }

            currentState = PieceState.Broken;
            OnPieceBroken?.Invoke();

            Debug.Log($"{Piece?.Team} {Piece?.Type} has been broken!");

            // Optionally disable visual representation
            SetVisualActive(false);
        }

        /// <summary>
        /// Repairs the piece, restoring it to active state.
        /// </summary>
        public bool RepairPiece()
        {
            if (currentState != PieceState.Broken)
            {
                Debug.LogWarning($"Cannot repair piece that is not broken (current state: {currentState})");
                return false;
            }

            if (!canBeRepaired)
            {
                Debug.LogWarning($"This piece cannot be repaired!");
                return false;
            }

            currentState = PieceState.Active;
            OnPieceRepaired?.Invoke();

            Debug.Log($"{Piece?.Team} {Piece?.Type} has been repaired!");

            // Re-enable visual representation
            SetVisualActive(true);

            return true;
        }

        /// <summary>
        /// Permanently destroys the piece.
        /// </summary>
        public void DestroyPiece()
        {
            currentState = PieceState.Destroyed;
            OnPieceDestroyed?.Invoke();

            Debug.Log($"{Piece?.Team} {Piece?.Type} has been destroyed!");

            SetVisualActive(false);
        }

        /// <summary>
        /// Sets the visual representation active or inactive.
        /// </summary>
        private void SetVisualActive(bool active)
        {
            // Hide/show sprite renderer
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = active;
            }

            // Disable colliders
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = active;
            }
        }

        /// <summary>
        /// Gets the data for serialization (save/load).
        /// </summary>
        public PieceHealthData GetSaveData()
        {
            return new PieceHealthData
            {
                pieceType = Piece?.Type ?? PieceType.Pawn,
                team = Piece?.Team ?? Team.White,
                state = currentState,
                position = Piece?.Position ?? Vector2Int.zero,
                canBeRepaired = canBeRepaired,
                repairCost = repairCost
            };
        }

        /// <summary>
        /// Loads data from serialization.
        /// </summary>
        public void LoadSaveData(PieceHealthData data)
        {
            currentState = data.state;
            canBeRepaired = data.canBeRepaired;
            repairCost = data.repairCost;

            SetVisualActive(currentState == PieceState.Active);
        }

        /// <summary>
        /// Resets the piece to active state.
        /// </summary>
        public void ResetToActive()
        {
            currentState = PieceState.Active;
            SetVisualActive(true);
        }
    }

    /// <summary>
    /// Serializable data for piece health state.
    /// </summary>
    [System.Serializable]
    public class PieceHealthData
    {
        public PieceType pieceType;
        public Team team;
        public PieceHealth.PieceState state;
        public Vector2Int position;
        public bool canBeRepaired;
        public int repairCost;
    }
}
