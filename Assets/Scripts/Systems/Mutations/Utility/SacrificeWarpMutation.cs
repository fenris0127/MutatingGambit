using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Utility
{
    /// <summary>
    /// 게임당 한 번, 이 기물을 희생하여 아군 기물을 순간이동시킬 수 있습니다.
    /// Once per game, this piece can be sacrificed to teleport any friendly piece.
    /// </summary>
    [CreateAssetMenu(fileName = "SacrificeWarpMutation", menuName = "MutatingGambit/Mutations/Utility/Sacrifice Warp")]
    public class SacrificeWarpMutation : Mutation
    {
        private const string KEY_USED = "used";

        public override void ApplyToPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            state?.SetData(KEY_USED, false);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 상태는 MutationManager에서 자동으로 정리됨
        }

        public void ActivateSacrifice(Piece sacrificePiece, Piece targetPiece, Vector2Int destination, Board board)
        {
            var state = MutationManager.Instance.GetMutationState(sacrificePiece, this);
            if (state == null) return;

            bool used = state.GetData(KEY_USED, false);
            if (used) return;

            // Remove sacrifice piece
            board.RemovePiece(sacrificePiece.Position);

            // Teleport target
            board.MovePiece(targetPiece.Position, destination);

            state.SetData(KEY_USED, true);
            Debug.Log($"Sacrifice Warp: {sacrificePiece.Type} sacrificed to teleport {targetPiece.Type}");
        }
    }
}
