using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// Chess AI using minimax algorithm with alpha-beta pruning.
    /// Adapts to dynamic rule mutations.
    /// </summary>
    public class ChessAI : MonoBehaviour
    {
        [Header("AI Configuration")]
        [SerializeField]
        private AIConfig config;

        [SerializeField]
        private Team aiTeam;

        private StateEvaluator stateEvaluator;
        private MoveEvaluator moveEvaluator;
        private System.Random random;

        private int nodesEvaluated = 0;
        private float searchStartTime = 0f;
        private bool timeExpired = false;

        /// <summary>
        /// Gets the AI's team.
        /// </summary>
        public Team AITeam => aiTeam;

        /// <summary>
        /// Gets the AI configuration.
        /// </summary>
        public AIConfig Config => config;

        /// <summary>
        /// Initializes the AI with configuration.
        /// </summary>
        public void Initialize(AIConfig aiConfig, Team team, int seed = 0)
        {
            config = aiConfig;
            aiTeam = team;
            random = seed == 0 ? new System.Random() : new System.Random(seed);

            stateEvaluator = new StateEvaluator(config, team, seed);
            moveEvaluator = new MoveEvaluator(config, stateEvaluator, team);

            Debug.Log($"ChessAI initialized: Team={team}, Depth={config.SearchDepth}, Time={config.MaxTimePerMove}ms");
        }

        private void Awake()
        {
            if (config != null && stateEvaluator == null)
            {
                Initialize(config, aiTeam);
            }
        }

        /// <summary>
        /// Determines the best move for the AI on the given board.
        /// </summary>
        public Move MakeMove(Board board)
        {
            if (board == null)
            {
                Debug.LogError("ChessAI.MakeMove called with null board!");
                return default;
            }

            nodesEvaluated = 0;
            searchStartTime = Time.realtimeSinceStartup;
            timeExpired = false;

            Move bestMove;

            if (config.UseIterativeDeepening)
            {
                bestMove = IterativeDeepeningSearch(board);
            }
            else
            {
                bestMove = DepthLimitedSearch(board, config.SearchDepth);
            }

            float elapsedTime = (Time.realtimeSinceStartup - searchStartTime) * 1000f;
            Debug.Log($"AI move: {bestMove} | Nodes: {nodesEvaluated} | Time: {elapsedTime:F1}ms");

            return bestMove;
        }

        /// <summary>
        /// Iterative deepening search - searches incrementally deeper until time runs out.
        /// </summary>
        private Move IterativeDeepeningSearch(Board board)
        {
            Move bestMove = default;
            int maxDepth = config.SearchDepth;

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                if (IsTimeExpired())
                {
                    Debug.Log($"Iterative deepening stopped at depth {depth - 1}");
                    break;
                }

                Move currentBestMove = DepthLimitedSearch(board, depth);

                if (!IsTimeExpired())
                {
                    bestMove = currentBestMove;
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Performs a depth-limited minimax search.
        /// </summary>
        private Move DepthLimitedSearch(Board board, int depth)
        {
            List<Move> allMoves = GetAllMoves(board, aiTeam);

            if (allMoves.Count == 0)
            {
                Debug.LogWarning("No valid moves available for AI!");
                return default;
            }

            Move bestMove = allMoves[0];
            float bestScore = float.MinValue;

            // Convert to BoardState for efficient simulation
            BoardState initialState = board.CloneAsState();

            // Evaluate each move
            foreach (var move in allMoves)
            {
                if (IsTimeExpired())
                {
                    break;
                }

                // Clone state instead of board (much faster - no GameObjects)
                BoardState clonedState = initialState.Clone();
                clonedState.SimulateMove(move.From, move.To);

                // Run minimax from opponent's perspective
                float score = MinimaxState(clonedState, depth - 1, float.MinValue, float.MaxValue, false);

                // Track best move
                if (score > bestScore || (Mathf.Approximately(score, bestScore) && ShouldChooseRandomly()))
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            bestMove.Score = bestScore;
            return bestMove;
        }

        /// <summary>
        /// Minimax algorithm with alpha-beta pruning using lightweight BoardState.
        /// This is much faster than the Board-based version as it doesn't create/destroy GameObjects.
        /// </summary>
        private float MinimaxState(BoardState state, int depth, float alpha, float beta, bool maximizingPlayer)
        {
            nodesEvaluated++;

            // Time limit check
            if (IsTimeExpired())
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            // Terminal node (depth 0 or game over)
            if (depth == 0 || IsTerminalStateForState(state))
            {
                return stateEvaluator.EvaluateBoardState(state);
            }

            if (maximizingPlayer)
            {
                // AI's turn (maximize score)
                float maxEval = float.MinValue;
                List<Move> moves = GetAllMovesFromState(state, aiTeam);

                foreach (var move in moves)
                {
                    BoardState clonedState = state.Clone();
                    clonedState.SimulateMove(move.From, move.To);

                    float eval = MinimaxState(clonedState, depth - 1, alpha, beta, false);

                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);

                    // Alpha-beta pruning
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return maxEval;
            }
            else
            {
                // Opponent's turn (minimize score)
                float minEval = float.MaxValue;
                Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
                List<Move> moves = GetAllMovesFromState(state, opponentTeam);

                foreach (var move in moves)
                {
                    BoardState clonedState = state.Clone();
                    clonedState.SimulateMove(move.From, move.To);

                    float eval = MinimaxState(clonedState, depth - 1, alpha, beta, true);

                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);

                    // Alpha-beta pruning
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return minEval;
            }
        }

        /// <summary>
        /// Gets all valid moves for a team on the board.
        /// </summary>
        private List<Move> GetAllMoves(Board board, Team team)
        {
            var moves = new List<Move>();
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                if (piece == null) continue;

                var validMoves = MoveValidator.GetValidMoves(board, piece.Position);

                foreach (var targetPos in validMoves)
                {
                    var capturedPiece = board.GetPiece(targetPos);
                    moves.Add(new Move(piece.Position, targetPos, piece, capturedPiece));
                }
            }

            return moves;
        }

        /// <summary>
        /// Gets all valid moves for a team from a BoardState.
        /// </summary>
        private List<Move> GetAllMovesFromState(BoardState state, Team team)
        {
            var moves = new List<Move>();
            var pieces = state.GetPiecesByTeam(team);

            foreach (var pieceData in pieces)
            {
                if (pieceData == null) continue;

                var validMoves = state.GetValidMoves(pieceData);

                foreach (var targetPos in validMoves)
                {
                    var capturedPieceData = state.GetPieceData(targetPos);
                    // Create Move with null Piece references (not needed for simulation)
                    moves.Add(new Move(pieceData.Position, targetPos, null, null));
                }
            }

            return moves;
        }

        /// <summary>
        /// Checks if a BoardState is in a terminal state (game over).
        /// </summary>
        private bool IsTerminalStateForState(BoardState state)
        {
            // Check if either king is captured
            var aiKing = FindKingInState(state, aiTeam);
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
            var opponentKing = FindKingInState(state, opponentTeam);

            if (aiKing == null || opponentKing == null)
            {
                return true; // King captured = game over
            }

            // Check for stalemate (no valid moves)
            var aiMoves = GetAllMovesFromState(state, aiTeam);
            var opponentMoves = GetAllMovesFromState(state, opponentTeam);

            return aiMoves.Count == 0 || opponentMoves.Count == 0;
        }

        /// <summary>
        /// Finds the king of a specific team in BoardState.
        /// </summary>
        private BoardState.PieceData FindKingInState(BoardState state, Team team)
        {
            var pieces = state.GetPiecesByTeam(team);
            foreach (var pieceData in pieces)
            {
                if (pieceData != null && pieceData.Type == PieceType.King)
                {
                    return pieceData;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if the time limit has been exceeded.
        /// </summary>
        private bool IsTimeExpired()
        {
            if (timeExpired) return true;

            float elapsed = (Time.realtimeSinceStartup - searchStartTime) * 1000f;
            if (elapsed >= config.MaxTimePerMove)
            {
                timeExpired = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Randomly decides whether to choose a move with equal evaluation.
        /// </summary>
        private bool ShouldChooseRandomly()
        {
            return config.RandomnessFactor > 0 && random.NextDouble() < config.RandomnessFactor;
        }
    }
}
