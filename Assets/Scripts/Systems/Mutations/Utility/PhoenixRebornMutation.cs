using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Utility
{
    /// <summary>
    /// 이 기물이 잡히면 2턴 후 무작위 빈 칸에 부활합니다.
    /// When this piece is captured, it respawns at a random empty square after 2 turns.
    /// </summary>
    [CreateAssetMenu(fileName = "PhoenixRebornMutation", menuName = "MutatingGambit/Mutations/Utility/Phoenix Reborn")]
    public class PhoenixRebornMutation : Mutation
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

        // Would need to implement respawn logic in game manager
    }
}
