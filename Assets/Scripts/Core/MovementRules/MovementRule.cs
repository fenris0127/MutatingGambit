using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 모든 움직임 규칙의 추상 기반 클래스입니다.
    /// 움직임 규칙은 기물이 보드에서 어떻게 움직일 수 있는지 정의합니다.
    /// 전략 패턴을 사용하여 동적 규칙 수정을 허용합니다.
    /// </summary>
    public abstract class MovementRule : ScriptableObject
    {
        #region 필드
        [SerializeField]
        private string ruleName;

        [SerializeField]
        [TextArea(2, 4)]
        private string ruleDescription;
        #endregion

        #region 공개 속성
        /// <summary>
        /// 이 움직임 규칙의 이름을 가져옵니다.
        /// </summary>
        public string RuleName => ruleName;

        /// <summary>
        /// 이 움직임 규칙의 설명을 가져옵니다.
        /// </summary>
        public string RuleDescription => ruleDescription;
        #endregion

        #region 추상 메서드
        /// <summary>
        /// 주어진 위치의 기물에 대한 모든 유효한 수를 계산합니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="fromPosition">기물의 위치</param>
        /// <param name="pieceTeam">기물의 팀</param>
        /// <returns>유효한 목적지 위치 목록</returns>
        public abstract List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam
        );
        #endregion

        #region 보호된 헬퍼 메서드
        /// <summary>
        /// 위치에 적 기물이 있는지 확인합니다.
        /// </summary>
        protected bool IsEnemyPiece(IBoard board, Vector2Int position, ChessEngine.Team pieceTeam)
        {
            var piece = board.GetPieceAt(position);
            return piece != null && piece.Team != pieceTeam;
        }

        /// <summary>
        /// 위치에 아군 기물이 있는지 확인합니다.
        /// </summary>
        protected bool IsFriendlyPiece(IBoard board, Vector2Int position, ChessEngine.Team pieceTeam)
        {
            var piece = board.GetPieceAt(position);
            return piece != null && piece.Team == pieceTeam;
        }

        /// <summary>
        /// 위치가 비어있는지 확인합니다.
        /// </summary>
        protected bool IsEmptyPosition(IBoard board, Vector2Int position)
        {
            return board.GetPieceAt(position) == null;
        }
        #endregion
    }

    /// <summary>
    /// 보드 상태 접근을 위한 인터페이스입니다.
    /// MovementRule이 강한 결합 없이 보드를 쿼리할 수 있도록 합니다.
    /// </summary>
    public interface IBoard
    {
        /// <summary>보드의 너비</summary>
        int Width { get; }
        
        /// <summary>보드의 높이</summary>
        int Height { get; }
        
        /// <summary>지정된 위치의 기물을 가져옵니다</summary>
        IPiece GetPieceAt(Vector2Int position);
        
        /// <summary>위치가 유효한지 확인합니다</summary>
        bool IsPositionValid(Vector2Int position);
        
        /// <summary>위치에 장애물이 있는지 확인합니다</summary>
        bool IsObstacle(Vector2Int position);
    }

    /// <summary>
    /// 기물 정보를 위한 인터페이스입니다.
    /// </summary>
    public interface IPiece
    {
        /// <summary>기물의 팀</summary>
        ChessEngine.Team Team { get; }
        
        /// <summary>기물의 타입</summary>
        ChessEngine.PieceType Type { get; }
        
        /// <summary>기물의 위치</summary>
        Vector2Int Position { get; }
    }
}
