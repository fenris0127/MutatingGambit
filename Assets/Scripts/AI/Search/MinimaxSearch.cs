using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI.Search
{
    /// <summary>
    /// 미니맥스 검색 알고리즘을 구현합니다.
    /// </summary>
    public class MinimaxSearch
    {
        private AIConfig config;
        private Team aiTeam;
        private StateEvaluator stateEvaluator;
        private System.Random random;

        private int nodesEvaluated = 0;
        private float searchStartTime = 0f;
        private bool timeExpired = false;

        public MinimaxSearch(AIConfig aiConfig, Team team, StateEvaluator evaluator, System.Random rng)
        {
            config = aiConfig;
            aiTeam = team;
            stateEvaluator = evaluator;
            random = rng;
        }

        public int NodesEvaluated => nodesEvaluated;

        /// <summary>
        /// 반복 심화 검색을 수행합니다.
        /// </summary>
        public Move IterativeDeepeningSearch(Board board)
        {
            searchStartTime = Time.realtimeSinceStartup;
            timeExpired = false;
            Move bestMove = new Move();
            float bestScore = float.NegativeInfinity;

            for (int depth = 1; depth <= config.SearchDepth; depth++)
            {
                if (IsTimeExpired())
                {
                    Debug.Log($"시간 초과! 깊이 {depth -1}에서 중단");
                    break;
                }

                var result = DepthLimitedSearch(board, depth);
                
                if (result.MovingPiece != null)
                {
                    bestMove = result;
                    bestScore = result.Score;
                }

                Debug.Log($"깊이 {depth}: 최선의 수 {bestMove.From} → {bestMove.To}, 점수: {bestScore:F2}");
            }

            return bestMove;
        }

        /// <summary>
        /// 깊이 제한 검색을 수행합니다.
        /// </summary>
        public Move DepthLimitedSearch(Board board, int depth)
        {
            searchStartTime = Time.realtimeSinceStartup;
            timeExpired = false;
            nodesEvaluated = 0;

            var allMoves = GetAllValidMoves(board, aiTeam);

            if (allMoves.Count == 0)
            {
                return new Move();
            }

            BoardState initialState = board.CloneAsState();

            float bestScore = float.NegativeInfinity;
            Move bestMove = allMoves[0];

            foreach (var move in allMoves)
            {
                if (IsTimeExpired()) break;

                BoardState clonedState = initialState.Clone();
                clonedState.SimulateMove(move.From, move.To);

                float score = MinimaxState(clonedState, depth - 1, float.NegativeInfinity, float.PositiveInfinity, false);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            bestMove.Score = bestScore;
            return bestMove;
        }

        /// <summary>
        /// 미니맥스 알고리즘 (BoardState 사용).
        /// </summary>
        public float MinimaxState(BoardState state, int depth, float alpha, float beta, bool maximizingPlayer)
        {
            nodesEvaluated++;

            if (IsTimeExpired())
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            if (depth == 0 || IsTerminalState(state))
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            Team currentTeam = maximizingPlayer ? aiTeam : GetOpponentTeam();
            var allMoves = GetAllValidMovesState(state, currentTeam);

            if (allMoves.Count == 0)
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            if (maximizingPlayer)
            {
                float maxEval = float.NegativeInfinity;
                foreach (var move in allMoves)
                {
                    if (IsTimeExpired()) break;

                    BoardState clonedState = state.Clone();
                    clonedState.SimulateMove(move.From, move.To);

                    float eval = MinimaxState(clonedState, depth - 1, alpha, beta, false);
                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);

                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                float minEval = float.PositiveInfinity;
                foreach (var move in allMoves)
                {
                    if (IsTimeExpired()) break;

                    BoardState clonedState = state.Clone();
                    clonedState.SimulateMove(move.From, move.To);

                    float eval = MinimaxState(clonedState, depth - 1, alpha, beta, true);
                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);

                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        private List<Move> GetAllValidMoves(Board board, Team team)
        {
            var moves = new List<Move>();
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var validMoves = MoveValidator.GetValidMoves(board, piece.Position);
                foreach (var targetPos in validMoves)
                {
                    moves.Add(new Move
                    {
                        MovingPiece = piece,
                        From = piece.Position,
                        To = targetPos
                    });
                }
            }

            return moves;
        }

        private List<Move> GetAllValidMovesState(BoardState state, Team team)
        {
            var moves = new List<Move>();
            var pieces = state.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var validMoves = state.GetValidMoves(piece);
                foreach (var targetPos in validMoves)
                {
                    moves.Add(new Move
                    {
                        From = piece.Position,
                        To = targetPos
                    });
                }
            }

            return moves;
        }

        private bool IsTerminalState(BoardState state)
        {
            var aiPieces = state.GetPiecesByTeam(aiTeam);
            var opponentPieces = state.GetPiecesByTeam(GetOpponentTeam());

            bool aiHasKing = aiPieces.Any(p => p.Type == PieceType.King);
            bool opponentHasKing = opponentPieces.Any(p => p.Type == PieceType.King);

            return !aiHasKing || !opponentHasKing;
        }

        private bool IsTimeExpired()
        {
            if (config.MaxTimePerMove <= 0) return false;
            
            float elapsed = (Time.realtimeSinceStartup - searchStartTime) * 1000f;
            return elapsed >= config.MaxTimePerMove;
        }

        private Team GetOpponentTeam() => aiTeam == Team.White ? Team.Black : Team.White;

        public void ResetCounters()
        {
            nodesEvaluated = 0;
            timeExpired = false;
        }
    }
}
