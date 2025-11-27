using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Movement rule for straight line movement (horizontal and vertical).
    /// Used by Rook and Queen pieces.
    /// </summary>
    [CreateAssetMenu(fileName = "StraightLineRule", menuName = "Movement Rules/Straight Line Rule")]
    public class StraightLineRule : MovementRule
    {
        [SerializeField]
        [Tooltip("Maximum distance the piece can move. -1 for unlimited.")]
        private int maxDistance = -1;

        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();

            // Four directions: up, down, left, right
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
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

                // Empty square - can move here
                if (IsEmptyPosition(board, currentPos))
                {
                    validMoves.Add(currentPos);
                }
                // Friendly piece - blocked
                else if (IsFriendlyPiece(board, currentPos, pieceTeam))
                {
                    break;
                }
                // Enemy piece - can capture but can't move beyond
                else if (IsEnemyPiece(board, currentPos, pieceTeam))
                {
                    validMoves.Add(currentPos);
                    break;
                }

                currentPos += direction;
                distanceMoved++;
            }
        }
    }
}
