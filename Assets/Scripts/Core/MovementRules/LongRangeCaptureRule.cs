using UnityEngine;
using System.Collections.Generic;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 직선 방향으로 특정 거리만큼 떨어진 적을 잡을 수 있는 규칙
    /// Allows capturing enemies at a specific distance in cardinal directions
    /// </summary>
    public class LongRangeCaptureRule : MovementRule
    {
        [SerializeField]
        [Tooltip("잡을 수 있는 거리 (칸 수)")]
        private int captureRange = 2;

        public override List<Vector2Int> GetValidMoves(ChessEngine.Piece piece, ChessEngine.Board board)
        {
            var validMoves = new List<Vector2Int>();
            Vector2Int currentPos = piece.Position;

            // 4방향 (상, 하, 좌, 우)
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,      // 북
                Vector2Int.down,    // 남
                Vector2Int.left,    // 서
                Vector2Int.right    // 동
            };

            foreach (var direction in directions)
            {
                Vector2Int targetPos = currentPos + (direction * captureRange);

                // 보드 범위 확인
                if (!board.IsValidPosition(targetPos))
                    continue;

                // 목표 위치에 적 기물이 있는지 확인
                var targetPiece = board.GetPiece(targetPos);
                if (targetPiece != null && targetPiece.Team != piece.Team)
                {
                    validMoves.Add(targetPos);
                }
            }

            return validMoves;
        }

        public override bool IsValidMove(ChessEngine.Piece piece, Vector2Int targetPosition, ChessEngine.Board board)
        {
            var validMoves = GetValidMoves(piece, board);
            return validMoves.Contains(targetPosition);
        }
    }
}
