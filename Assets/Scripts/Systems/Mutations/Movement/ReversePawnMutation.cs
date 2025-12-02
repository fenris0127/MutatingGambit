using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Systems.Mutations.Movement
{
    /// <summary>
    /// 폰이 뒤로 이동할 수 있게 하여 전통적인 폰의 제약을 깹니다.
    /// Pawn can move backwards, breaking the traditional pawn limitation.
    /// </summary>
    [CreateAssetMenu(fileName = "ReversePawnMutation", menuName = "MutatingGambit/Mutations/Movement/Reverse Pawn")]
    public class ReversePawnMutation : Mutation
    {
        public override void ApplyToPiece(Piece piece)
        {
            // Add reverse movement (backward by 1)
            var reverseRule = ScriptableObject.CreateInstance<BackwardPawnRule>();
            AddAndTrackRule(piece, reverseRule);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            RemoveTrackedRules(piece);
        }
    }
}
