using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Systems.Mutations.Attack
{
    /// <summary>
    /// 기물을 잡을 때마다 영구적으로 이동 범위가 +1 증가합니다 (중첩 가능).
    /// Capturing a piece grants +1 movement range permanently (stacks).
    /// </summary>
    [CreateAssetMenu(fileName = "BloodthirstMutation", menuName = "MutatingGambit/Mutations/Attack/Bloodthirst")]
    public class BloodthirstMutation : Mutation
    {
        private const string KEY_CAPTURE_COUNT = "captureCount";
        private const string KEY_RANGE_RULE = "rangeRule";

        public override void ApplyToPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            if (state == null) return;

            state.SetData(KEY_CAPTURE_COUNT, 0);

            // 초기 범위 확장 규칙 추가 (시작은 0칸 추가)
            var rangeRule = ScriptableObject.CreateInstance<RangeExtensionRule>();
            rangeRule.ExtensionRange = 0;
            AddAndTrackRule(piece, rangeRule);
            state.SetData(KEY_RANGE_RULE, rangeRule);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            RemoveTrackedRules(piece);
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            var state = MutationManager.Instance.GetMutationState(mutatedPiece, this);
            if (state == null) return;

            // 캡처 카운트 증가
            int captureCount = state.GetData(KEY_CAPTURE_COUNT, 0);
            captureCount++;
            state.SetData(KEY_CAPTURE_COUNT, captureCount);

            // 범위 확장 규칙 업데이트
            var rangeRule = state.GetData<RangeExtensionRule>(KEY_RANGE_RULE);
            if (rangeRule != null)
            {
                rangeRule.ExtensionRange = captureCount;
                Debug.Log($"Bloodthirst: {captureCount} kills, range extended to +{captureCount}");
            }
        }
    }
}
