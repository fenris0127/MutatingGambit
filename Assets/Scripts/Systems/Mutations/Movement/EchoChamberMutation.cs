using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations.Movement
{
    /// <summary>
    /// 기물이 이동한 시작 위치에 2턴 동안 잔상을 남깁니다 (이동 불가, 이동 차단).
    /// Piece leaves a clone at its starting position for 2 turns (cannot move, blocks movement).
    /// </summary>
    [CreateAssetMenu(fileName = "EchoChamberMutation", menuName = "MutatingGambit/Mutations/Movement/Echo Chamber")]
    public class EchoChamberMutation : Mutation
    {
        private const string KEY_ECHO_POSITIONS = "echoPositions";

        public override void ApplyToPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            state?.SetData(KEY_ECHO_POSITIONS, new Dictionary<Vector2Int, int>());
        }

        public override void RemoveFromPiece(Piece piece)
        {
            var state = MutationManager.Instance.GetMutationState(piece, this);
            var echoPositions = state?.GetData<Dictionary<Vector2Int, int>>(KEY_ECHO_POSITIONS);

            if (echoPositions != null)
            {
                echoPositions.Clear();
            }
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            var state = MutationManager.Instance.GetMutationState(mutatedPiece, this);
            var echoPositions = state?.GetData<Dictionary<Vector2Int, int>>(KEY_ECHO_POSITIONS);
            if (echoPositions == null) return;

            // Leave an "echo" obstacle at the from position
            board.SetObstacle(from, true);
            echoPositions[from] = 2; // Lasts 2 turns

            // Cleanup old echoes
            var toRemove = new List<Vector2Int>();
            foreach (var kvp in echoPositions)
            {
                echoPositions[kvp.Key]--;
                if (echoPositions[kvp.Key] <= 0)
                {
                    board.SetObstacle(kvp.Key, false);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var pos in toRemove)
            {
                echoPositions.Remove(pos);
            }
        }
    }
}
