using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// 체스 수를 나타냅니다.
    /// </summary>
    public struct Move
    {
        public Vector2Int From;
        public Vector2Int To;
        public Piece MovingPiece;
        public Piece CapturedPiece;
        public float Score;

        public Move(Vector2Int from, Vector2Int to, Piece movingPiece = null, Piece capturedPiece = null)
        {
            From = from;
            To = to;
            MovingPiece = movingPiece;
            CapturedPiece = capturedPiece;
            Score = 0f;
        }

        public override string ToString()
        {
            string capture = CapturedPiece != null ? $"x{CapturedPiece.Type}" : "";
            return $"{MovingPiece?.Type}{From} -> {To}{capture} (Score: {Score:F2})";
        }
    }

    /// <summary>
    /// 개별 수를 평가하고 점수를 할당합니다.
    /// </summary>
    public class MoveEvaluator
    {
        #region 변수
        private AIConfig config;
        private StateEvaluator stateEvaluator;
        private Team aiTeam;
        #endregion

        #region 공개 메서드
        public MoveEvaluator(AIConfig aiConfig, StateEvaluator evaluator, Team team)
        {
            config = aiConfig;
            stateEvaluator = evaluator;
            aiTeam = team;
        }

        /// <summary>
        /// 수를 시뮬레이션하고 결과 보드 상태를 평가하여 수를 평가합니다.
        /// </summary>
        public float EvaluateMove(Board board, Move move)
        {
            if (board == null)
            {
                return 0f;
            }

            // 수를 시뮬레이션하기 위한 경량 상태 생성
            BoardState clonedState = board.CloneAsState();

            // 복제된 상태에서 수 실행
            clonedState.SimulateMove(move.From, move.To);

            // 결과 위치 평가
            float score = stateEvaluator.EvaluateBoardState(clonedState);

            // 포획 보너스
            if (move.CapturedPiece != null)
            {
                score += config.GetPieceValue(move.CapturedPiece.Type) * 0.1f;
            }

            // 중앙 지배 수 보너스
            if (IsControllingCenter(move.To, board.Width, board.Height))
            {
                score += 0.2f;
            }

            // 전진하는 기물 보너스 (공격적인 플레이)
            int forwardDirection = aiTeam == Team.White ? 1 : -1;
            int yDelta = (move.To.y - move.From.y) * forwardDirection;
            if (yDelta > 0)
            {
                score += yDelta * 0.05f;
            }

            return score;
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 위치가 보드의 중앙을 지배하는지 확인합니다.
        /// </summary>
        private bool IsControllingCenter(Vector2Int position, int boardWidth, int boardHeight)
        {
            int centerX = boardWidth / 2;
            int centerY = boardHeight / 2;

            // 중앙 4칸 내에 있는지 확인
            return (position.x == centerX - 1 || position.x == centerX) &&
                   (position.y == centerY - 1 || position.y == centerY);
        }
        #endregion
    }
}
