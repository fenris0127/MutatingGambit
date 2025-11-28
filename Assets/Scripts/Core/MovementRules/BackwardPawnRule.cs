using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Allows a piece to move one square backwards.
    /// </summary>
    [CreateAssetMenu(fileName = "BackwardPawnRule", menuName = "Movement Rules/Backward Pawn Rule")]
    public class BackwardPawnRule : MovementRule
    {
        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();

            // Determine backward direction based on team
            // White moves up (+1), so backward is down (-1)
            // Black moves down (-1), so backward is up (+1)
            int directionY = (pieceTeam == Team.White) ? -1 : 1;
            Vector2Int backwardMove = new Vector2Int(0, directionY);
            Vector2Int targetPos = fromPosition + backwardMove;

            if (board.IsPositionValid(targetPos))
            {
                // Check for obstacles
                if (board.IsObstacle(targetPos))
                {
                    return validMoves;
                }

                // Can move if empty
                if (IsEmptyPosition(board, targetPos))
                {
                    validMoves.Add(targetPos);
                }
                // Note: Pawns typically do not capture vertically.
                // This rule adds backward movement, similar to how they move forward.
            }

            return validMoves;
        }
    }
}
