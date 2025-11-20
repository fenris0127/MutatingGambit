using System;
using System.Collections.Generic;
using System.Linq;
using MutatingGambit.Core.Movement;

namespace MutatingGambit.Core.AI
{
    /// <summary>
    /// Chess AI that understands mutations and artifacts
    /// </summary>
    public class ChessAI
    {
        private readonly PieceColor _color;
        private readonly Difficulty _difficulty;
        private readonly Random _random;

        // Piece values
        private const float PAWN_VALUE = 1.0f;
        private const float KNIGHT_VALUE = 3.0f;
        private const float BISHOP_VALUE = 3.0f;
        private const float ROOK_VALUE = 5.0f;
        private const float QUEEN_VALUE = 9.0f;
        private const float KING_VALUE = 100.0f;

        // Positional bonuses
        private const float CENTER_BONUS = 0.3f;
        private const float MUTATION_BONUS = 1.0f;

        public Difficulty CurrentDifficulty => _difficulty;

        public ChessAI(PieceColor color, Difficulty difficulty = Difficulty.Normal)
        {
            _color = color;
            _difficulty = difficulty;
            _random = new Random();
        }

        /// <summary>
        /// Selects the best move for the current board state
        /// </summary>
        public AIMove SelectBestMove(Board board, int timeLimitMs = 5000)
        {
            var allMoves = GetAllPossibleMoves(board);

            if (allMoves.Count == 0)
            {
                return null;
            }

            // Easy difficulty: random move with slight preference for captures
            if (_difficulty == Difficulty.Easy)
            {
                var captureMoves = allMoves.Where(m => board.GetPieceAt(m.To) != null).ToList();
                if (captureMoves.Count > 0 && _random.Next(100) < 70)
                {
                    return captureMoves[_random.Next(captureMoves.Count)];
                }
                return allMoves[_random.Next(allMoves.Count)];
            }

            // Evaluate all moves
            foreach (var move in allMoves)
            {
                move.Evaluation = EvaluateMove(board, move);
            }

            // Add some randomness for non-master difficulties
            if (_difficulty != Difficulty.Master)
            {
                float randomness = _difficulty == Difficulty.Normal ? 0.5f : 0.2f;
                foreach (var move in allMoves)
                {
                    move.Evaluation += (float)(_random.NextDouble() * randomness - randomness / 2);
                }
            }

            // Return best move
            return allMoves.OrderByDescending(m => m.Evaluation).First();
        }

        /// <summary>
        /// Gets all possible moves for the AI's color
        /// </summary>
        public List<AIMove> GetAllPossibleMoves(Board board)
        {
            var moves = new List<AIMove>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var position = PositionCache.Get(x, y);
                    var piece = board.GetPieceAt(position);

                    if (piece != null && piece.Color == _color && !piece.IsBroken)
                    {
                        var pieceMoves = GetPossibleMovesForPiece(board, position);
                        foreach (var targetPos in pieceMoves)
                        {
                            moves.Add(new AIMove(position, targetPos));
                        }
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Gets possible moves for a specific piece (respects mutations)
        /// </summary>
        public List<Position> GetPossibleMovesForPiece(Board board, Position position)
        {
            return MoveValidator.GetLegalMoves(board, position);
        }

        /// <summary>
        /// Evaluates the current board state
        /// </summary>
        public float EvaluateBoard(Board board)
        {
            float evaluation = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var position = PositionCache.Get(x, y);
                    var piece = board.GetPieceAt(position);

                    if (piece != null && !piece.IsBroken)
                    {
                        float pieceValue = EvaluatePiece(piece, position);

                        // Add to evaluation based on color
                        if (piece.Color == _color)
                        {
                            evaluation += pieceValue;
                        }
                        else
                        {
                            evaluation -= pieceValue;
                        }
                    }
                }
            }

            return evaluation;
        }

        /// <summary>
        /// Evaluates a specific piece's value
        /// </summary>
        public float EvaluatePiece(Piece piece, Position position)
        {
            float value = GetBasePieceValue(piece.Type);

            // Positional bonus for center control
            float centerDistance = Math.Abs(position.X - 3.5f) + Math.Abs(position.Y - 3.5f);
            value += (7 - centerDistance) * CENTER_BONUS * 0.1f;

            // Mutation bonus
            if (piece.Mutations.Count > 0)
            {
                value += piece.Mutations.Count * MUTATION_BONUS;
            }

            // HP factor
            if (piece.HP < piece.MaxHP)
            {
                value *= (float)piece.HP / piece.MaxHP;
            }

            return value;
        }

        private float GetBasePieceValue(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn: return PAWN_VALUE;
                case PieceType.Knight: return KNIGHT_VALUE;
                case PieceType.Bishop: return BISHOP_VALUE;
                case PieceType.Rook: return ROOK_VALUE;
                case PieceType.Queen: return QUEEN_VALUE;
                case PieceType.King: return KING_VALUE;
                default: return 0;
            }
        }

        private float EvaluateMove(Board board, AIMove move)
        {
            // Simulate the move
            var simulatedBoard = board.Clone();
            var piece = simulatedBoard.GetPieceAt(move.From);
            var capturedPiece = simulatedBoard.GetPieceAt(move.To);

            // Make the move on simulated board
            simulatedBoard.PlacePiece(null, move.From);
            simulatedBoard.PlacePiece(piece, move.To);

            // Evaluate resulting position
            float evaluation = EvaluateBoard(simulatedBoard);

            // Bonus for captures
            if (capturedPiece != null)
            {
                evaluation += GetBasePieceValue(capturedPiece.Type) * 0.5f;
            }

            return evaluation;
        }
    }
}
