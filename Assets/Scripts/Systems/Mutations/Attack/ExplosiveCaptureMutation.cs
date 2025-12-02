using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Attack
{
    /// <summary>
    /// 기물을 잡을 때, 잡힌 기물 주변의 모든 기물에게도 피해를 줍니다.
    /// When capturing, also damage all pieces adjacent to the captured piece.
    /// </summary>
    [CreateAssetMenu(fileName = "ExplosiveCaptureMutation", menuName = "MutatingGambit/Mutations/Attack/Explosive Capture")]
    public class ExplosiveCaptureMutation : Mutation
    {
        [SerializeField]
        private int splashDamage = 1;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            // Deal splash damage to adjacent pieces
            Vector2Int capturePos = capturedPiece.Position;
            Vector2Int[] adjacentOffsets = new Vector2Int[]
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
            };

            foreach (var offset in adjacentOffsets)
            {
                Vector2Int checkPos = capturePos + offset;
                var adjacentPiece = board.GetPiece(checkPos);
                if (adjacentPiece != null)
                {
                    // Apply damage (would need PieceHealth system)
                    Debug.Log($"Splash damage to {adjacentPiece.Type} at {checkPos}");
                }
            }
        }
    }
}
