using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Leaping Rook Mutation: Rook can jump over ONE friendly piece.
    /// </summary>
    [CreateAssetMenu(fileName = "LeapingRookMutation", menuName = "Mutations/Initial/Leaping Rook")]
    public class LeapingRookMutation : Mutation
    {
        private LeapingRookRule leapingRule;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Rook)
            {
                Debug.LogWarning("LeapingRookMutation can only be applied to Rooks.");
                return;
            }

            // Create and add the leaping rook rule
            if (leapingRule == null)
            {
                leapingRule = ScriptableObject.CreateInstance<LeapingRookRule>();
            }

            piece.AddMovementRule(leapingRule);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            if (leapingRule != null)
            {
                piece.RemoveMovementRule(leapingRule);
            }
        }
    }

    /// <summary>
    /// Custom movement rule for leaping rook.
    /// Allows jumping over one friendly piece in straight lines.
    /// </summary>
    public class LeapingRookRule : MovementRule
    {
        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            Team pieceTeam)
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
            Team pieceTeam,
            List<Vector2Int> validMoves)
        {
            Vector2Int currentPos = fromPosition + direction;
            bool hasJumpedOnce = false;

            while (board.IsPositionValid(currentPos))
            {
                // Check for obstacles (can't jump obstacles)
                if (board.IsObstacle(currentPos))
                {
                    break;
                }

                // Empty square - can move here
                if (IsEmptyPosition(board, currentPos))
                {
                    validMoves.Add(currentPos);
                }
                // Friendly piece
                else if (IsFriendlyPiece(board, currentPos, pieceTeam))
                {
                    // If we haven't jumped yet, we can jump this piece
                    if (!hasJumpedOnce)
                    {
                        hasJumpedOnce = true;
                        currentPos += direction;
                        continue; // Continue to next square
                    }
                    else
                    {
                        // Already jumped once, can't jump again
                        break;
                    }
                }
                // Enemy piece - can capture but can't move beyond
                else if (IsEnemyPiece(board, currentPos, pieceTeam))
                {
                    validMoves.Add(currentPos);
                    break;
                }

                currentPos += direction;
            }
        }
    }
}
