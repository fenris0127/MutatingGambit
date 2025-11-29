using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI.Evaluation
{
    /// <summary>
    /// 위치적 이점을 평가합니다 (중앙 지배, 기물 전개 등).
    /// </summary>
    public class PositionalEvaluator
    {
        private Team aiTeam;

        public PositionalEvaluator(Team team)
        {
            aiTeam = team;
        }

        /// <summary>
        /// 위치적 이점을 평가합니다.
        /// </summary>
        public float EvaluatePosition(Board board)
        {
            float score = 0f;

            // 중앙 지배 보너스
            score += EvaluateCenterControl(board);

            // 기물 전개 (뒷줄이 아닌 곳)
            score += EvaluatePieceDevelopment(board);

            return score;
        }

        /// <summary>
        /// BoardState에서 위치적 이점을 평가합니다.
        /// </summary>
        public float EvaluatePositionState(BoardState state)
        {
            float score = 0f;

            // 중앙 지배 보너스
            score += EvaluateCenterControlState(state);

            // 기물 전개
            score += EvaluatePieceDevelopmentState(state);

            return score;
        }

        private float EvaluateCenterControl(Board board)
        {
            float score = 0f;

            Vector2Int[] centerSquares = new Vector2Int[]
            {
                new Vector2Int(board.Width / 2 - 1, board.Height / 2 - 1),
                new Vector2Int(board.Width / 2, board.Height / 2 - 1),
                new Vector2Int(board.Width / 2 - 1, board.Height / 2),
                new Vector2Int(board.Width / 2, board.Height / 2)
            };

            foreach (var square in centerSquares)
            {
                if (!board.IsPositionValid(square)) continue;

                var piece = board.GetPiece(square);
                if (piece != null)
                {
                    score += piece.Team == aiTeam ? 0.5f : -0.5f;
                }
            }

            return score;
        }

        private float EvaluateCenterControlState(BoardState state)
        {
            float score = 0f;

            Vector2Int[] centerSquares = new Vector2Int[]
            {
                new Vector2Int(state.Width / 2 - 1, state.Height / 2 - 1),
                new Vector2Int(state.Width / 2, state.Height / 2 - 1),
                new Vector2Int(state.Width / 2 - 1, state.Height / 2),
                new Vector2Int(state.Width / 2, state.Height / 2)
            };

            foreach (var square in centerSquares)
            {
                if (!state.IsPositionValid(square)) continue;

                var piece = state.GetPieceData(square);
                if (piece != null)
                {
                    score += piece.Team == aiTeam ? 0.5f : -0.5f;
                }
            }

            return score;
        }

        private float EvaluatePieceDevelopment(Board board)
        {
            float score = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
            var aiPieces = board.GetPiecesByTeam(aiTeam);
            var opponentPieces = board.GetPiecesByTeam(opponentTeam);

            int aiBackRank = aiTeam == Team.White ? 0 : board.Height - 1;
            int opponentBackRank = opponentTeam == Team.White ? 0 : board.Height - 1;

            foreach (var piece in aiPieces)
            {
                if (piece.Type != PieceType.King && piece.Position.y != aiBackRank)
                {
                    score += 0.1f;
                }
            }

            foreach (var piece in opponentPieces)
            {
                if (piece.Type != PieceType.King && piece.Position.y != opponentBackRank)
                {
                    score -= 0.1f;
                }
            }

            return score;
        }

        private float EvaluatePieceDevelopmentState(BoardState state)
        {
            float score = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
            var aiPieces = state.GetPiecesByTeam(aiTeam);
            var opponentPieces = state.GetPiecesByTeam(opponentTeam);

            int aiBackRank = aiTeam == Team.White ? 0 : state.Height - 1;
            int opponentBackRank = opponentTeam == Team.White ? 0 : state.Height - 1;

            foreach (var piece in aiPieces)
            {
                if (piece.Type != PieceType.King && piece.Position.y != aiBackRank)
                {
                    score += 0.1f;
                }
            }

            foreach (var piece in opponentPieces)
            {
                if (piece.Type != PieceType.King && piece.Position.y != opponentBackRank)
                {
                    score -= 0.1f;
                }
            }

            return score;
        }
    }
}
