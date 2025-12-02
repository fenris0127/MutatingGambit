using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// ExecuteMove의 사후 처리를 전담합니다.
    /// 단일 책임: 수 실행 후 변이 알림, 캡처 처리, 승패 확인만 담당
    /// </summary>
    public class MoveExecutionHandler
    {
        #region 의존성
        private readonly VictoryConditionChecker victoryChecker;
        #endregion

        #region 생성자
        /// <summary>
        /// MoveExecutionHandler를 초기화합니다.
        /// </summary>
        /// <param name="checker">승리 조건 체커</param>
        public MoveExecutionHandler(VictoryConditionChecker checker)
        {
            this.victoryChecker = checker;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 수 실행 후 처리를 수행합니다.
        /// </summary>
        /// <param name="movingPiece">이동한 기물</param>
        /// <param name="capturedPiece">잡힌 기물 (없으면 null)</param>
        /// <param name="from">시작 위치</param>
        /// <param name="to">목표 위치</param>
        /// <param name="board">보드</param>
        /// <returns>게임 종료 여부</returns>
        public bool HandlePostMove(Piece movingPiece, Piece capturedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            NotifyMutation(movingPiece, from, to, board);
            
            if (capturedPiece != null)
            {
                NotifyCapture(movingPiece, capturedPiece, from, to, board);
                HandlePieceCapture(capturedPiece);
            }

            return CheckGameOver();
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 변이 시스템에 이동을 알립니다.
        /// </summary>
        private void NotifyMutation(Piece movingPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (Systems.Mutations.MutationManager.Instance != null)
            {
                Systems.Mutations.MutationManager.Instance.NotifyMove(movingPiece, from, to, board);
            }
        }

        /// <summary>
        /// 변이 시스템에 캡처를 알립니다.
        /// </summary>
        private void NotifyCapture(Piece movingPiece, Piece capturedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (Systems.Mutations.MutationManager.Instance != null)
            {
                Systems.Mutations.MutationManager.Instance.NotifyCapture(movingPiece, capturedPiece, from, to, board);
            }
        }

        /// <summary>
        /// 기물 캡처를 처리합니다.
        /// </summary>
        private void HandlePieceCapture(Piece capturedPiece)
        {
            if (victoryChecker != null)
            {
                victoryChecker.HandlePieceCapture(capturedPiece);
            }
        }

        /// <summary>
        /// 게임 종료 조건을 확인합니다.
        /// </summary>
        private bool CheckGameOver()
        {
            if (victoryChecker != null)
            {
                return victoryChecker.CheckGameConditions();
            }
            return false;
        }
        #endregion
    }
}
