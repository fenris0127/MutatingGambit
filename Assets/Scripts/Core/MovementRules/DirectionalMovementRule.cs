using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 방향성 움직임 규칙의 기반 클래스입니다.
    /// StraightLineRule과 DiagonalRule의 공통 로직을 제공합니다.
    /// </summary>
    public abstract class DirectionalMovementRule : MovementRule
    {
        #region 필드
        [SerializeField]
        [Tooltip("기물이 이동할 수 있는 최대 거리. -1은 무제한.")]
        protected int maxDistance = -1;
        #endregion

        #region 추상 메서드
        /// <summary>
        /// 하위 클래스가 사용할 방향들을 가져옵니다.
        /// </summary>
        protected abstract Vector2Int[] GetDirections();
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 모든 방향으로 유효한 수를 계산합니다.
        /// </summary>
        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();
            var directions = GetDirections();

            foreach (var direction in directions)
            {
                AddMovesInDirection(board, fromPosition, direction, pieceTeam, validMoves);
            }

            return validMoves;
        }
        #endregion

        #region 보호된 메서드 - 방향별 이동
        /// <summary>
        /// 특정 방향으로 유효한 수를 추가합니다.
        /// </summary>
        protected virtual void AddMovesInDirection(
            IBoard board,
            Vector2Int fromPosition,
            Vector2Int direction,
            ChessEngine.Team pieceTeam,
            List<Vector2Int> validMoves)
        {
            Vector2Int currentPos = fromPosition + direction;
            int distanceMoved = 1;

            while (board.IsPositionValid(currentPos))
            {
                if (ShouldStopAtDistance(distanceMoved)) break;
                if (board.IsObstacle(currentPos)) break;

                if (ProcessSquare(board, currentPos, pieceTeam, distanceMoved, validMoves))
                {
                    break; // 기물을 만나면 중단
                }

                currentPos += direction;
                distanceMoved++;
            }
        }

        /// <summary>
        /// 현재 칸을 처리하고 기물을 만났는지 반환합니다.
        /// </summary>
        protected virtual bool ProcessSquare(
            IBoard board,
            Vector2Int position,
            ChessEngine.Team pieceTeam,
            int distance,
            List<Vector2Int> validMoves)
        {
            if (IsEmptyPosition(board, position))
            {
                if (CanMoveToEmptySquare(distance))
                {
                    validMoves.Add(position);
                }
                return false; // 계속 진행
            }
            else if (IsFriendlyPiece(board, position, pieceTeam))
            {
                return true; // 아군 기물 - 중단
            }
            else if (IsEnemyPiece(board, position, pieceTeam))
            {
                if (CanCaptureAtDistance(distance))
                {
                    validMoves.Add(position);
                }
                return true; // 적 기물 - 캡처 후 중단
            }

            return false;
        }
        #endregion

        #region 보호된 메서드 - 거리 및 조건 검증
        /// <summary>
        /// 최대 거리에 도달하여 중단해야 하는지 확인합니다.
        /// </summary>
        protected virtual bool ShouldStopAtDistance(int distance)
        {
            return maxDistance > 0 && distance > maxDistance;
        }

        /// <summary>
        /// 빈 칸으로 이동할 수 있는지 확인합니다.
        /// </summary>
        protected virtual bool CanMoveToEmptySquare(int distance)
        {
            return true; // 기본적으로 모든 빈 칸으로 이동 가능
        }

        /// <summary>
        /// 해당 거리에서 캡처할 수 있는지 확인합니다.
        /// </summary>
        protected virtual bool CanCaptureAtDistance(int distance)
        {
            return true; // 기본적으로 모든 거리에서 캡처 가능
        }
        #endregion
    }
}
