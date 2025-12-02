using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI.Search
{
    /// <summary>
    /// 미니맥스 검색 알고리즘을 구현합니다.
    /// 알파-베타 가지치기를 사용하여 성능을 최적화합니다.
    /// </summary>
    public class MinimaxSearch
    {
        #region 필드
        private AIConfig config;
        private Team aiTeam;
        private StateEvaluator stateEvaluator;
        private System.Random random;
        #endregion

        #region 검색 상태
        private int nodesEvaluated = 0;
        private float searchStartTime = 0f;
        private bool timeExpired = false;
        #endregion

        #region 공개 속성
        /// <summary>평가된 노드 수를 가져옵니다.</summary>
        public int NodesEvaluated => nodesEvaluated;
        #endregion

        #region 생성자
        /// <summary>
        /// MinimaxSearch를 초기화합니다.
        /// </summary>
        public MinimaxSearch(AIConfig aiConfig, Team team, StateEvaluator evaluator, System.Random rng)
        {
            config = aiConfig;
            aiTeam = team;
            stateEvaluator = evaluator;
            random = rng;
        }
        #endregion

        #region 공개 메서드 - 반복 심화 검색
        /// <summary>
        /// 반복 심화 검색을 수행합니다.
        /// </summary>
        /// <param name="board">검색할 보드</param>
        /// <returns>최선의 수</returns>
        public Move IterativeDeepeningSearch(Board board)
        {
            InitializeSearch();
            Move bestMove = new Move();

            for (int depth = 1; depth <= config.SearchDepth; depth++)
            {
                if (IsTimeExpired())
                {
                    LogDepthInterruption(depth);
                    break;
                }

                var result = DepthLimitedSearch(board, depth);
                
                if (result.MovingPiece != null)
                {
                    bestMove = result;
                }

                LogDepthResult(depth, bestMove);
            }

            return bestMove;
        }
        #endregion

        #region 공개 메서드 - 깊이 제한 검색
        /// <summary>
        /// 깊이 제한 검색을 수행합니다.
        /// </summary>
        /// <param name="board">검색할 보드</param>
        /// <param name="depth">검색 깊이</param>
        /// <returns>최선의 수</returns>
        public Move DepthLimitedSearch(Board board, int depth)
        {
            InitializeSearch();
            var allMoves = GetAllValidMoves(board, aiTeam);

            if (allMoves.Count == 0)
            {
                return new Move();
            }

            BoardState initialState = board.CloneAsState();
            return FindBestMove(initialState, allMoves, depth);
        }
        #endregion

        #region 공개 메서드 - 유틸리티
        /// <summary>
        /// 카운터를 리셋합니다.
        /// </summary>
        public void ResetCounters()
        {
            nodesEvaluated = 0;
            timeExpired = false;
        }
        #endregion

        #region 비공개 메서드 - 검색 초기화
        /// <summary>
        /// 검색을 초기화합니다.
        /// </summary>
        private void InitializeSearch()
        {
            searchStartTime = Time.realtimeSinceStartup;
            timeExpired = false;
            nodesEvaluated = 0;
        }
        #endregion

        #region 비공개 메서드 - 최선의 수 찾기
        /// <summary>
        /// 모든 가능한 수 중 최선의 수를 찾습니다.
        /// </summary>
        private Move FindBestMove(BoardState initialState, List<Move> allMoves, int depth)
        {
            float bestScore = float.NegativeInfinity;
            Move bestMove = allMoves[0];

            foreach (var move in allMoves)
            {
                if (IsTimeExpired()) break;

                float score = EvaluateMove(initialState, move, depth);

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
        /// 수를 평가합니다.
        /// </summary>
        private float EvaluateMove(BoardState initialState, Move move, int depth)
        {
            BoardState clonedState = initialState.Clone();
            clonedState.SimulateMove(move.From, move.To);
            return MinimaxState(clonedState, depth - 1, float.NegativeInfinity, float.PositiveInfinity, false);
        }
        #endregion

        #region 비공개 메서드 - 미니맥스 알고리즘
        /// <summary>
        /// 미니맥스 알고리즘 (BoardState 사용).
        /// </summary>
        private float MinimaxState(BoardState state, int depth, float alpha, float beta, bool maximizingPlayer)
        {
            nodesEvaluated++;

            if (ShouldStopSearch(state, depth))
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            Team currentTeam = maximizingPlayer ? aiTeam : GetOpponentTeam();
            var allMoves = GetAllValidMovesState(state, currentTeam);

            if (allMoves.Count == 0)
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            return maximizingPlayer 
                ? MaximizeScore(state, allMoves, depth, alpha, beta)
                : MinimizeScore(state, allMoves, depth, alpha, beta);
        }

        /// <summary>
        /// 검색을 중단해야 하는지 확인합니다.
        /// </summary>
        private bool ShouldStopSearch(BoardState state, int depth)
        {
            return IsTimeExpired() || depth == 0 || IsTerminalState(state);
        }

        /// <summary>
        /// 최대 점수를 찾습니다 (AI 턴).
        /// </summary>
        private float MaximizeScore(BoardState state, List<Move> moves, int depth, float alpha, float beta)
        {
            float maxEval = float.NegativeInfinity;

            foreach (var move in moves)
            {
                if (IsTimeExpired()) break;

                float eval = EvaluateMoveRecursive(state, move, depth, alpha, beta, false);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);

                if (beta <= alpha) break; // 알파-베타 가지치기
            }

            return maxEval;
        }

        /// <summary>
        /// 최소 점수를 찾습니다 (상대 턴).
        /// </summary>
        private float MinimizeScore(BoardState state, List<Move> moves, int depth, float alpha, float beta)
        {
            float minEval = float.PositiveInfinity;

            foreach (var move in moves)
            {
                if (IsTimeExpired()) break;

                float eval = EvaluateMoveRecursive(state, move, depth, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);

                if (beta <= alpha) break; // 알파-베타 가지치기
            }

            return minEval;
        }

        /// <summary>
        /// 수를 재귀적으로 평가합니다.
        /// </summary>
        private float EvaluateMoveRecursive(BoardState state, Move move, int depth, float alpha, float beta, bool maximizing)
        {
            BoardState clonedState = state.Clone();
            clonedState.SimulateMove(move.From, move.To);
            return MinimaxState(clonedState, depth - 1, alpha, beta, maximizing);
        }
        #endregion

        #region 비공개 메서드 - 수 생성
        /// <summary>
        /// Board에서 모든 유효한 수를 가져옵니다.
        /// </summary>
        private List<Move> GetAllValidMoves(Board board, Team team)
        {
            var moves = new List<Move>();
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                AddMovesForPiece(moves, board, piece);
            }

            return moves;
        }

        /// <summary>
        /// 기물의 모든 유효한 수를 추가합니다.
        /// </summary>
        private void AddMovesForPiece(List<Move> moves, Board board, Piece piece)
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

        /// <summary>
        /// BoardState에서 모든 유효한 수를 가져옵니다.
        /// </summary>
        private List<Move> GetAllValidMovesState(BoardState state, Team team)
        {
            var moves = new List<Move>();
            var pieces = state.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                AddMovesForPieceState(moves, state, piece);
            }

            return moves;
        }

        /// <summary>
        /// BoardState에서 기물의 모든 유효한 수를 추가합니다.
        /// </summary>
        private void AddMovesForPieceState(List<Move> moves, BoardState state, BoardState.PieceData piece)
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
        #endregion

        #region 비공개 메서드 - 상태 확인
        /// <summary>
        /// 종료 상태인지 확인합니다.
        /// </summary>
        private bool IsTerminalState(BoardState state)
        {
            var aiPieces = state.GetPiecesByTeam(aiTeam);
            var opponentPieces = state.GetPiecesByTeam(GetOpponentTeam());

            bool aiHasKing = aiPieces.Any(p => p.Type == PieceType.King);
            bool opponentHasKing = opponentPieces.Any(p => p.Type == PieceType.King);

            return !aiHasKing || !opponentHasKing;
        }

        /// <summary>
        /// 시간 초과 여부를 확인합니다.
        /// </summary>
        private bool IsTimeExpired()
        {
            if (config.MaxTimePerMove <= 0) return false;
            
            float elapsed = (Time.realtimeSinceStartup - searchStartTime) * 1000f;
            return elapsed >= config.MaxTimePerMove;
        }

        /// <summary>
        /// 상대 팀을 가져옵니다.
        /// </summary>
        private Team GetOpponentTeam() => aiTeam == Team.White ? Team.Black : Team.White;
        #endregion

        #region 비공개 메서드 - 로깅
        /// <summary>
        /// 깊이 중단을 로그에 출력합니다.
        /// </summary>
        private void LogDepthInterruption(int depth)
        {
            Debug.Log($"시간 초과! 깊이 {depth - 1}에서 중단");
        }

        /// <summary>
        /// 깊이별 결과를 로그에 출력합니다.
        /// </summary>
        private void LogDepthResult(int depth, Move bestMove)
        {
            Debug.Log($"깊이 {depth}: 최선의 수 {bestMove.From} → {bestMove.To}, 점수: {bestMove.Score:F2}");
        }
        #endregion
    }
}
