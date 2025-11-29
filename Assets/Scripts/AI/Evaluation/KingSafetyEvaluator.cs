using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI.Evaluation
{
    /// <summary>
    /// 킹 안전을 평가합니다.
    /// </summary>
    public class KingSafetyEvaluator
    {
        private Team aiTeam;

        public KingSafetyEvaluator(Team team)
        {
            aiTeam = team;
        }

        /// <summary>
        /// 킹 안전을 평가합니다.
        /// </summary>
        public float EvaluateKingSafety(Board board)
        {
            float score = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            // AI 킹 찾기
            Piece aiKing = FindKing(board, aiTeam);
            if (aiKing != null)
            {
                // 안전하지 않으면 점수 감점
                if (MoveValidator.IsPositionUnderAttack(board, aiKing.Position, opponentTeam))
                {
                    score -= 2.0f;
                }

                // 보드 중앙에 가까우면 위험함
                int distanceFromCenter = GetDistanceFromCenter(aiKing.Position, board.Width, board.Height);
                score += distanceFromCenter * 0.1f;
            }

            // 적 킹 찾기
            Piece opponentKing = FindKing(board, opponentTeam);
            if (opponentKing != null)
            {
                // 적 킹이 안전하지 않으면 보너스
                if (MoveValidator.IsPositionUnderAttack(board, opponentKing.Position, aiTeam))
                {
                    score += 2.0f;
                }
            }

            return score;
        }

        /// <summary>
        /// BoardState에서 킹 안전을 평가합니다.
        /// </summary>
        public float EvaluateKingSafetyState(BoardState state)
        {
            float score = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            // 간단한 구현: 킹이 있는지 확인
            var aiPieces = state.GetPiecesByTeam(aiTeam);
            var opponentPieces = state.GetPiecesByTeam(opponentTeam);

            bool hasAiKing = false;
            bool hasOpponentKing = false;

            foreach (var piece in aiPieces)
            {
                if (piece.Type == PieceType.King)
                {
                    hasAiKing = true;
                    // 중앙에서 거리 보너스
                    int distanceFromCenter = GetDistanceFromCenter(piece.Position, state.Width, state.Height);
                    score += distanceFromCenter * 0.1f;
                }
            }

            foreach (var piece in opponentPieces)
            {
                if (piece.Type == PieceType.King)
                {
                    hasOpponentKing = true;
                }
            }

            // 킹이 없으면 큰 점수 변화
            if (!hasAiKing)
            {
                score -= 100f;
            }
            if (!hasOpponentKing)
            {
                score += 100f;
            }

            return score;
        }

        private Piece FindKing(Board board, Team team)
        {
            var pieces = board.GetPiecesByTeam(team);
            foreach (var piece in pieces)
            {
                if (piece.Type == PieceType.King)
                {
                    return piece;
                }
            }
            return null;
        }

        private int GetDistanceFromCenter(Vector2Int position, int width, int height)
        {
            int centerX = width / 2;
            int centerY = height / 2;
            
            return Mathf.Abs(position.x - centerX) + Mathf.Abs(position.y - centerY);
        }
    }
}
