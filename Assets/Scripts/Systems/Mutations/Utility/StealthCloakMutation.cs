using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Utility
{
    /// <summary>
    /// 첫 이동 후 3턴 동안 적 AI에게 보이지 않습니다.
    /// Piece is invisible to enemy AI for 3 turns after first move.
    /// </summary>
    [CreateAssetMenu(fileName = "StealthCloakMutation", menuName = "MutatingGambit/Mutations/Utility/Stealth Cloak")]
    public class StealthCloakMutation : Mutation
    {
        private const string KEY_STEALTH_TURNS_REMAINING = "stealthTurnsRemaining";
        private const string KEY_ACTIVATED = "activated";

        public override void ApplyToPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            if (state != null)
            {
                state.SetData(KEY_ACTIVATED, false);
                state.SetData(KEY_STEALTH_TURNS_REMAINING, 0);
            }
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 상태는 MutationManager에서 자동으로 정리됨
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            var state = MutationManager.Instance.GetMutationState(mutatedPiece, this);
            if (state == null) return;

            bool activated = state.GetData(KEY_ACTIVATED, false);
            int stealthTurnsRemaining = state.GetData(KEY_STEALTH_TURNS_REMAINING, 0);

            if (!activated)
            {
                state.SetData(KEY_ACTIVATED, true);
                state.SetData(KEY_STEALTH_TURNS_REMAINING, 3);
                Debug.Log($"{mutatedPiece.Type} is now stealthed for 3 turns");
            }
            else if (stealthTurnsRemaining > 0)
            {
                state.SetData(KEY_STEALTH_TURNS_REMAINING, stealthTurnsRemaining - 1);
            }
        }

        public bool IsStealthed(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            if (state == null) return false;

            int stealthTurnsRemaining = state.GetData(KEY_STEALTH_TURNS_REMAINING, 0);
            return stealthTurnsRemaining > 0;
        }
    }
}
