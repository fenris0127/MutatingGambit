using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Movement rule for one-square king movement in all directions.
    /// </summary>
    [CreateAssetMenu(fileName = "KingStepRule", menuName = "Movement Rules/King Step Rule")]
    public class KingStepRule : MovementRule
    {
        [SerializeField]
        [Tooltip("Number of squares the king can move (default 1).")]
        private int stepDistance = 1;

        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();

            // All 8 directions (including diagonals)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),    // Up
                new Vector2Int(0, -1),   // Down
                new Vector2Int(-1, 0),   // Left
                new Vector2Int(1, 0),    // Right
                new Vector2Int(1, 1),    // Up-Right
                new Vector2Int(1, -1),   // Down-Right
                new Vector2Int(-1, 1),   // Up-Left
                new Vector2Int(-1, -1)   // Down-Left
            };

            foreach (var direction in directions)
            {
                Vector2Int targetPos = fromPosition + (direction * stepDistance);

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
