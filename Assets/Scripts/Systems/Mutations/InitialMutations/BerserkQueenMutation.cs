using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Berserk Queen Mutation: Queen must move to the maximum distance in chosen direction.
    /// Cannot stop mid-way - all or nothing movement.
    /// </summary>
    [CreateAssetMenu(fileName = "BerserkQueenMutation", menuName = "Mutating Gambit/Mutations/Berserk Queen")]
    public class BerserkQueenMutation : Mutation
    {
        private BerserkQueenRule berserkRule;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Queen)
            {
                Debug.LogWarning("Berserk Queen mutation can only be applied to queens!");
                return;
            }

            // Remove standard queen rules
            piece.ClearMovementRules();

            // Add berserk rule that forces maximum distance movement
            berserkRule = ScriptableObject.CreateInstance<BerserkQueenRule>();
            piece.AddMovementRule(berserkRule);

            Debug.Log($"Applied Berserk Queen mutation to {piece.Team} queen");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            if (berserkRule != null)
            {
                piece.RemoveMovementRule(berserkRule);
            }
        }
    }

    /// <summary>
    /// Custom movement rule for Berserk Queen - moves to maximum distance only.
    /// </summary>
    public class BerserkQueenRule : MovementRule
    {
        public override System.Collections.Generic.List<Vector2Int> GetValidMoves(
            IBoard board, Vector2Int fromPosition, Team pieceTeam)
        {
            var moves = new System.Collections.Generic.List<Vector2Int>();

            // 8 directions (straight + diagonal)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 1), new Vector2Int(1, -1),
                new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };

            foreach (var direction in directions)
            {
                // Find maximum distance in this direction
                Vector2Int furthestMove = FindFurthestMove(board, fromPosition, direction, pieceTeam);

                if (furthestMove != fromPosition)
                {
                    moves.Add(furthestMove);
                }
            }

            return moves;
        }

        private Vector2Int FindFurthestMove(IBoard board, Vector2Int start, Vector2Int direction, Team team)
        {
            Vector2Int current = start;
            Vector2Int furthest = start;

            for (int distance = 1; distance < 8; distance++)
            {
                Vector2Int next = start + direction * distance;

                if (!board.IsPositionValid(next) || board.IsObstacle(next))
                {
                    break;
                }

                var piece = board.GetPieceAt(next);

                if (piece == null)
                {
                    furthest = next;
                }
                else if (piece.Team != team)
                {
                    // Can capture enemy
                    furthest = next;
                    break;
                }
                else
                {
                    // Blocked by friendly piece
                    break;
                }
            }

            return furthest;
        }
    }
}
