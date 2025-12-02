using UnityEngine;
using System.Collections.Generic;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 기물의 이동 범위를 확장하는 규칙
    /// Extends the movement range of a piece by allowing additional steps in all directions
    /// </summary>
    public class RangeExtensionRule : MovementRule
    {
        [SerializeField]
        [Tooltip("추가 이동 범위 (칸 수)")]
        private int extensionRange = 1;

        /// <summary>
        /// 추가 이동 범위를 설정합니다
        /// </summary>
        public int ExtensionRange
        {
            get => extensionRange;
            set => extensionRange = value;
        }

        public override List<Vector2Int> GetValidMoves(ChessEngine.Piece piece, ChessEngine.Board board)
        {
            var validMoves = new List<Vector2Int>();
            Vector2Int currentPos = piece.Position;

            // 8방향 모두에서 확장된 범위만큼 이동 가능
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new Vector2Int(1, 1), new Vector2Int(-1, 1),
                new Vector2Int(1, -1), new Vector2Int(-1, -1)
            };

            foreach (var direction in directions)
            {
                for (int distance = 1; distance <= extensionRange; distance++)
                {
                    Vector2Int targetPos = currentPos + (direction * distance);

                    // 보드 범위 확인
                    if (!board.IsValidPosition(targetPos))
                        break;

                    var targetPiece = board.GetPiece(targetPos);

                    // 빈 칸이면 이동 가능
                    if (targetPiece == null)
                    {
                        validMoves.Add(targetPos);
                    }
                    // 적 기물이면 잡기 가능하지만 그 이후는 막힘
                    else if (targetPiece.Team != piece.Team)
                    {
                        validMoves.Add(targetPos);
                        break;
                    }
                    // 아군 기물이면 막힘
                    else
                    {
                        break;
                    }
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
