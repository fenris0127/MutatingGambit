using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Utility
{
    /// <summary>
    /// 적 기물을 3개 잡으면 무작위 다른 기물 종류로 변신합니다.
    /// Piece transforms into a random different piece type after capturing 3 enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "EvolutionMutation", menuName = "MutatingGambit/Mutations/Utility/Evolution")]
    public class EvolutionMutation : Mutation
    {
        private const string KEY_CAPTURE_COUNT = "captureCount";

        public override void ApplyToPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            state?.SetData(KEY_CAPTURE_COUNT, 0);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 상태는 MutationManager에서 자동으로 정리됨
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            var state = MutationManager.Instance.GetMutationState(mutatedPiece, this);
            if (state == null) return;

            int captureCount = state.GetData(KEY_CAPTURE_COUNT, 0);
            captureCount++;
            state.SetData(KEY_CAPTURE_COUNT, captureCount);

            if (captureCount >= 3)
            {
                // Transform piece (would need piece transformation system)
                Debug.Log($"{mutatedPiece.Type} is evolving!");
                state.SetData(KEY_CAPTURE_COUNT, 0);
            }
        }
    }
}
