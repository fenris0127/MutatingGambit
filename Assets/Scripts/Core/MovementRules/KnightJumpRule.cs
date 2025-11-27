using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Movement rule for L-shaped knight jumps.
    /// Knights can jump over other pieces.
    /// </summary>
    [CreateAssetMenu(fileName = "KnightJumpRule", menuName = "Movement Rules/Knight Jump Rule")]
    public class KnightJumpRule : MovementRule
    {
        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();

            // All 8 possible L-shaped knight moves
            Vector2Int[] knightMoves = new Vector2Int[]
            {
                new Vector2Int(2, 1),    // Right 2, Up 1
                new Vector2Int(2, -1),   // Right 2, Down 1
                new Vector2Int(-2, 1),   // Left 2, Up 1
                new Vector2Int(-2, -1),  // Left 2, Down 1
                new Vector2Int(1, 2),    // Right 1, Up 2
                new Vector2Int(1, -2),   // Right 1, Down 2
                new Vector2Int(-1, 2),   // Left 1, Up 2
                new Vector2Int(-1, -2)   // Left 1, Down 2
            };

            foreach (var move in knightMoves)
            {
                Vector2Int targetPos = fromPosition + move;

                // Check if position is valid
                if (!board.IsPositionValid(targetPos))
                {
                    continue;
                }

                // Check if position is an obstacle
                if (board.IsObstacle(targetPos))
                {
                    continue;
                }

                // Can move to empty squares or capture enemy pieces
                if (IsEmptyPosition(board, targetPos) || IsEnemyPiece(board, targetPos, pieceTeam))
                {
                    validMoves.Add(targetPos);
                }
            }

            return validMoves;
        }
    }
}
