using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Double Move Rook Mutation: Rook can move twice in a single turn.
    /// First move is normal, then gets a bonus move in perpendicular direction.
    /// </summary>
    [CreateAssetMenu(fileName = "DoubleMoveRookMutation", menuName = "Mutating Gambit/Mutations/Double Move Rook")]
    public class DoubleMoveRookMutation : Mutation
    {
        private bool hasBonusMove = false;
        private Vector2Int lastMoveDirection;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Rook)
            {
                Debug.LogWarning("Double Move Rook mutation can only be applied to rooks!");
                return;
            }

            Debug.Log($"Applied Double Move Rook mutation to {piece.Team} rook");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            hasBonusMove = false;
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (mutatedPiece.Type != PieceType.Rook)
            {
                return;
            }

            Vector2Int moveDirection = to - from;

            if (!hasBonusMove)
            {
                // First move - grant bonus move
                hasBonusMove = true;
                lastMoveDirection = moveDirection;

                Debug.Log($"Rook at {to} has a bonus move available!");
            }
            else
            {
                // Used bonus move
                hasBonusMove = false;
                Debug.Log($"Rook at {to} used bonus move");
            }
        }

        /// <summary>
        /// Call this to check if the rook has a bonus move available.
        /// </summary>
        public bool HasBonusMove => hasBonusMove;

        /// <summary>
        /// Resets the bonus move state (call at turn end).
        /// </summary>
        public void ResetBonusMove()
        {
            hasBonusMove = false;
        }
    }
}
