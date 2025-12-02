using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// L자 모양의 나이트 점프 움직임 규칙입니다.
    /// 나이트는 다른 기물을 뛰어넘을 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "KnightJumpRule", menuName = "Movement Rules/Knight Jump Rule")]
    public class KnightJumpRule : MovementRule
    {
        #region 공개 메서드
        /// <summary>
        /// 나이트의 모든 유효한 L자 이동을 계산합니다.
        /// </summary>
        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();
            var knightMoves = GetKnightMoveOffsets();

            foreach (var move in knightMoves)
            {
                ProcessKnightMove(board, fromPosition, move, pieceTeam, validMoves);
            }

            return validMoves;
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 나이트의 8가지 L자 이동 오프셋을 가져옵니다.
        /// </summary>
        private Vector2Int[] GetKnightMoveOffsets()
        {
            return new Vector2Int[]
            {
                new Vector2Int(2, 1),    // 우 2, 상 1
                new Vector2Int(2, -1),   // 우 2, 하 1
                new Vector2Int(-2, 1),   // 좌 2, 상 1
                new Vector2Int(-2, -1),  // 좌 2, 하 1
                new Vector2Int(1, 2),    // 우 1, 상 2
                new Vector2Int(1, -2),   // 우 1, 하 2
                new Vector2Int(-1, 2),   // 좌 1, 상 2
                new Vector2Int(-1, -2)   // 좌 1, 하 2
            };
        }

        /// <summary>
        /// 단일 나이트 이동을 처리합니다.
        /// </summary>
        private void ProcessKnightMove(
            IBoard board,
            Vector2Int fromPosition,
            Vector2Int moveOffset,
            ChessEngine.Team pieceTeam,
            List<Vector2Int> validMoves)
        {
            Vector2Int targetPos = fromPosition + moveOffset;

            if (!IsValidKnightDestination(board, targetPos, pieceTeam))
            {
                return;
            }

            validMoves.Add(targetPos);
        }

        /// <summary>
        /// 나이트 목적지가 유효한지 확인합니다.
        /// </summary>
        private bool IsValidKnightDestination(IBoard board, Vector2Int position, ChessEngine.Team pieceTeam)
        {
            if (!board.IsPositionValid(position)) return false;
            if (board.IsObstacle(position)) return false;

            return IsEmptyPosition(board, position) || IsEnemyPiece(board, position, pieceTeam);
        }
        #endregion
    }
}
