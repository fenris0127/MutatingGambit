using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI.Evaluation
{
    /// <summary>
    /// 기동성(유효한 수의 수)을 평가합니다.
    /// </summary>
    public class MobilityEvaluator
    {
        private Team aiTeam;

        public MobilityEvaluator(Team team)
        {
            aiTeam = team;
        }

        /// <summary>
        /// 기동성을 평가합니다.
        /// </summary>
        public float EvaluateMobility(Board board)
        {
            float score = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            // AI 팀의 기동성
            int aiMobility = GetTotalMobility(board, aiTeam);
            int opponentMobility = GetTotalMobility(board, opponentTeam);

            // 더 많은 수를 사용할 수 있으면 유리
            score = (aiMobility - opponentMobility) * 0.1f;

            return score;
        }

        /// <summary>
        /// BoardState에서 기동성을 평가합니다.
        /// </summary>
        public float EvaluateMobilityState(BoardState state)
        {
            float score = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            // AI 팀의 기동성
            int aiMobility = GetTotalMobilityState(state, aiTeam);
            int opponentMobility = GetTotalMobilityState(state, opponentTeam);

            score = (aiMobility - opponentMobility) * 0.1f;

            return score;
        }

        private int GetTotalMobility(Board board, Team team)
        {
            int totalMoves = 0;
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var validMoves = MoveValidator.GetValidMoves(board, piece.Position);
                totalMoves += validMoves.Count;
            }

            return totalMoves;
        }

        private int GetTotalMobilityState(BoardState state, Team team)
        {
            int totalMoves = 0;
            var pieces = state.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var validMoves = state.GetValidMoves(piece);
                totalMoves += validMoves.Count;
            }

            return totalMoves;
        }
    }
}
