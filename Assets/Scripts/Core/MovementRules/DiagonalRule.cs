using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Movement rule for diagonal movement.
    /// Used by Bishop and Queen pieces.
    /// </summary>
    [CreateAssetMenu(fileName = "DiagonalRule", menuName = "Movement Rules/Diagonal Rule")]
    public class DiagonalRule : MovementRule
    {
        [SerializeField]
        [Tooltip("Maximum distance the piece can move. -1 for unlimited.")]
        private int maxDistance = -1;

        [SerializeField]
        [Tooltip("If true, piece must move exactly the max distance (e.g., Glass Bishop mutation).")]
        private bool exactDistance = false;

        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();

            // Four diagonal directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 1),    // Up-Right
                new Vector2Int(1, -1),   // Down-Right
                new Vector2Int(-1, 1),   // Up-Left
                new Vector2Int(-1, -1)   // Down-Left
            };

            foreach (var direction in directions)
            {
                AddMovesInDirection(board, fromPosition, direction, pieceTeam, validMoves);
            }

            return validMoves;
        }

        private void AddMovesInDirection(
            IBoard board,
            Vector2Int fromPosition,
            Vector2Int direction,
            ChessEngine.Team pieceTeam,
            List<Vector2Int> validMoves)
        {
            Vector2Int currentPos = fromPosition + direction;
            int distanceMoved = 1;

            while (board.IsPositionValid(currentPos))
            {
                // Check if we've reached max distance
                if (maxDistance > 0 && distanceMoved > maxDistance)
                {
                    break;
                }

                // Check for obstacles
                if (board.IsObstacle(currentPos))
                {
                    break;
                }

                bool isValidSquare = false;

                // Empty square
                if (IsEmptyPosition(board, currentPos))
                {
                    isValidSquare = true;
                }
                // Friendly piece - blocked
                else if (IsFriendlyPiece(board, currentPos, pieceTeam))
                {
                    // If exact distance required, check if we've reached it
                    if (exactDistance && maxDistance > 0 && distanceMoved == maxDistance)
                    {
                        // Can't land on friendly piece even at exact distance
                    }
                    break;
                }
                // Enemy piece - can capture but can't move beyond
                else if (IsEnemyPiece(board, currentPos, pieceTeam))
                {
                    isValidSquare = true;

                    // Add this move if it meets distance requirements
                    if (!exactDistance || (maxDistance <= 0 || distanceMoved == maxDistance))
                    {
                        validMoves.Add(currentPos);
                    }
                    break;
                }

                // Add move if it meets distance requirements
                if (isValidSquare)
                {
                    if (!exactDistance || (maxDistance <= 0 || distanceMoved == maxDistance))
                    {
                        validMoves.Add(currentPos);
                    }
                }

                currentPos += direction;
                distanceMoved++;
            }
        }
    }
}
