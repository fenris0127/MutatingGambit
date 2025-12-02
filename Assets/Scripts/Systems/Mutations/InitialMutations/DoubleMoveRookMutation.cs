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
        private const string KEY_HAS_BONUS_MOVE = "hasBonusMove";
        private const string KEY_LAST_MOVE_DIR = "lastMoveDirection";

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Rook)
            {
                Debug.LogWarning("Double Move Rook mutation can only be applied to rooks!");
                return;
            }

            // 상태 초기화
            var state = MutationManager.Instance.GetMutationState(piece, this);
            if (state != null)
            {
                state.SetData(KEY_HAS_BONUS_MOVE, false);
                state.SetData(KEY_LAST_MOVE_DIR, Vector2Int.zero);
            }

            Debug.Log($"Applied Double Move Rook mutation to {piece.Team} rook");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 상태는 MutationManager에서 자동으로 정리됨
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (mutatedPiece.Type != PieceType.Rook)
            {
                return;
            }

            var state = MutationManager.Instance.GetMutationState(mutatedPiece, this);
            if (state == null) return;

            Vector2Int moveDirection = to - from;
            bool hasBonusMove = state.GetData(KEY_HAS_BONUS_MOVE, false);

            if (!hasBonusMove)
            {
                // First move - grant bonus move
                state.SetData(KEY_HAS_BONUS_MOVE, true);
                state.SetData(KEY_LAST_MOVE_DIR, moveDirection);

                Debug.Log($"Rook at {to} has a bonus move available!");
            }
            else
            {
                // Used bonus move
                state.SetData(KEY_HAS_BONUS_MOVE, false);
                Debug.Log($"Rook at {to} used bonus move");
            }
        }

        /// <summary>
        /// Call this to check if the rook has a bonus move available.
        /// </summary>
        public bool GetHasBonusMove(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            return state?.GetData(KEY_HAS_BONUS_MOVE, false) ?? false;
        }

        /// <summary>
        /// Resets the bonus move state (call at turn end).
        /// </summary>
        public void ResetBonusMove(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            state?.SetData(KEY_HAS_BONUS_MOVE, false);
        }
    }
}
