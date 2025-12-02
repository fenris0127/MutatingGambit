using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Movement
{
    /// <summary>
    /// 턴당 한 번, 인접한 아군 기물과 위치를 교환할 수 있습니다.
    /// Piece can swap places with an adjacent friendly piece once per turn.
    /// </summary>
    [CreateAssetMenu(fileName = "SwapPositionMutation", menuName = "MutatingGambit/Mutations/Movement/Swap Position")]
    public class SwapPositionMutation : Mutation
    {
        private const string KEY_USED_THIS_TURN = "usedThisTurn";

        public override void ApplyToPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            state?.SetData(KEY_USED_THIS_TURN, false);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 상태는 MutationManager에서 자동으로 정리됨
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            var state = MutationManager.Instance.GetMutationState(mutatedPiece, this);
            state?.SetData(KEY_USED_THIS_TURN, false); // Reset for next turn
        }
    }
}
