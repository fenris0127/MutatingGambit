using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// Represents a chess move.
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
    /// Evaluates individual moves and assigns scores.
    /// </summary>
    public class MoveEvaluator
    {
        private AIConfig config;
        private StateEvaluator stateEvaluator;
        private Team aiTeam;

        public MoveEvaluator(AIConfig aiConfig, StateEvaluator evaluator, Team team)
        {
            config = aiConfig;
            stateEvaluator = evaluator;
            aiTeam = team;
        }

        /// <summary>
        /// Evaluates a move by simulating it and scoring the resulting board state.
        /// </summary>
        public float EvaluateMove(Board board, Move move)
        {
            if (board == null)
            {
                return 0f;
            }

            // Create lightweight state to simulate move
            BoardState clonedState = board.CloneAsState();

            // Execute move on cloned state
            clonedState.SimulateMove(move.From, move.To);

            // Evaluate resulting position
            float score = stateEvaluator.EvaluateBoardState(clonedState);

            // Bonus for captures
            if (move.CapturedPiece != null)
            {
                score += config.GetPieceValue(move.CapturedPiece.Type) * 0.1f;
            }

            // Bonus for center control moves
            if (IsControllingCenter(move.To, board.Width, board.Height))
            {
                score += 0.2f;
            }

            // Bonus for moving pieces forward (offensive play)
            int forwardDirection = aiTeam == Team.White ? 1 : -1;
            int yDelta = (move.To.y - move.From.y) * forwardDirection;
            if (yDelta > 0)
            {
                score += yDelta * 0.05f;
            }

            return score;
        }

        /// <summary>
        /// Checks if a position controls the center of the board.
        /// </summary>
        private bool IsControllingCenter(Vector2Int position, int boardWidth, int boardHeight)
        {
            int centerX = boardWidth / 2;
            int centerY = boardHeight / 2;

            // Check if within center 4 squares
            return (position.x == centerX - 1 || position.x == centerX) &&
                   (position.y == centerY - 1 || position.y == centerY);
        }
    }
}
