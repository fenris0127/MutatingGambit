using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Gravity Mirror Artifact: At the end of each turn, all pieces are pulled 1 square toward the bottom of the board.
    /// Pieces in the bottom rows (0-3) are affected.
    /// </summary>
    [CreateAssetMenu(fileName = "GravityMirrorArtifact", menuName = "Artifacts/Gravity Mirror")]
    public class GravityMirrorArtifact : Artifact
    {
        [Header("Gravity Settings")]
        [SerializeField]
        [Tooltip("How many squares pieces are pulled down.")]
        private int pullDistance = 1;

        [SerializeField]
        [Tooltip("Minimum row that pieces can be pulled to (0-indexed).")]
        private int minimumRow = 0;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // This is triggered at turn end
            ApplyGravity(board);
        }

        /// <summary>
        /// Pulls all pieces toward the bottom of the board.
        /// </summary>
        private void ApplyGravity(Board board)
        {
            if (board == null)
            {
                return;
            }

            // Get all pieces
            var allPieces = board.GetAllPieces();

            // Create a list of moves to apply (piece, from, to)
            var gravityMoves = new System.Collections.Generic.List<(Piece piece, Vector2Int from, Vector2Int to)>();

            foreach (var piece in allPieces)
            {
                if (piece == null)
                {
                    continue;
                }

                Vector2Int currentPos = piece.Position;
                int targetY = Mathf.Max(minimumRow, currentPos.y - pullDistance);

                // Only pull if position changes
                if (targetY < currentPos.y)
                {
                    Vector2Int targetPos = new Vector2Int(currentPos.x, targetY);

                    // Check if target position is valid and empty
                    if (board.IsPositionValid(targetPos) &&
                        !board.IsObstacle(targetPos) &&
                        board.GetPiece(targetPos) == null)
                    {
                        gravityMoves.Add((piece, currentPos, targetPos));
                    }
                }
            }

            // Apply all gravity moves
            foreach (var (piece, from, to) in gravityMoves)
            {
                board.MovePiece(from, to);
                Debug.Log($"Gravity Mirror: Pulled {piece.Type} from {from.ToNotation()} to {to.ToNotation()}");
            }

            if (gravityMoves.Count > 0)
            {
                Debug.Log($"Gravity Mirror: Pulled {gravityMoves.Count} pieces downward.");
            }
        }
    }
}
