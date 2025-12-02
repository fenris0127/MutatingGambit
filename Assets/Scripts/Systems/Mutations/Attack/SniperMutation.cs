using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Systems.Mutations.Attack
{
    /// <summary>
    /// 기물이 직선 방향으로 2칸 떨어진 적을 잡을 수 있습니다 (저격수).
    /// Piece can capture pieces 2 squares away in cardinal directions (sniper).
    /// </summary>
    [CreateAssetMenu(fileName = "SniperMutation", menuName = "MutatingGambit/Mutations/Attack/Sniper")]
    public class SniperMutation : Mutation
    {
        public override void ApplyToPiece(Piece piece)
        {
            // 2칸 거리 원거리 포획 규칙 추가
            var longRangeCaptureRule = ScriptableObject.CreateInstance<LongRangeCaptureRule>();
            AddAndTrackRule(piece, longRangeCaptureRule);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            RemoveTrackedRules(piece);
        }
    }
}
