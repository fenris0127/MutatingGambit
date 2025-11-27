using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.AI
{
    /// <summary>
    /// Evaluates board states and assigns scores.
    /// Higher scores are better for the AI.
    /// </summary>
    public class StateEvaluator
    {
        private AIConfig config;
        private Team aiTeam;
        private System.Random random;

        public StateEvaluator(AIConfig aiConfig, Team team, int seed = 0)
        {
            config = aiConfig;
            aiTeam = team;
            random = seed == 0 ? new System.Random() : new System.Random(seed);
        }

        /// <summary>
        /// Evaluates the current board state.
        /// Positive score = good for AI, negative = good for opponent.
        /// </summary>
        public float EvaluateBoard(Board board)
        {
            if (board == null)
            {
                return 0f;
            }

            float score = 0f;

            // Material evaluation
            score += EvaluateMaterial(board) * config.MaterialWeight;

            // Positional evaluation
            score += EvaluatePosition(board) * config.PositionalWeight;

            // King safety
            score += EvaluateKingSafety(board) * config.KingSafetyWeight;

            // Mobility (number of valid moves)
            score += EvaluateMobility(board) * config.MobilityWeight;

            // Add randomness to prevent deterministic play
            if (config.RandomnessFactor > 0)
            {
                float randomness = ((float)random.NextDouble() - 0.5f) * 2f * config.RandomnessFactor;
                score += randomness;
            }

            return score;
        }

        /// <summary>
        /// Evaluates material advantage (piece values).
        /// </summary>
        private float EvaluateMaterial(Board board)
        {
            float aiMaterial = 0f;
            float opponentMaterial = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            var aiPieces = board.GetPiecesByTeam(aiTeam);
            var opponentPieces = board.GetPiecesByTeam(opponentTeam);

            foreach (var piece in aiPieces)
            {
                aiMaterial += GetPieceValue(piece);
            }

            foreach (var piece in opponentPieces)
            {
                opponentMaterial += GetPieceValue(piece);
            }

            return aiMaterial - opponentMaterial;
        }

        /// <summary>
        /// Gets the value of a piece, considering mutations.
        /// </summary>
        private float GetPieceValue(Piece piece)
        {
            float baseValue = config.GetPieceValue(piece.Type);

            // Bonus for mutated pieces (they're more powerful)
            if (piece.HasMutations())
            {
                baseValue *= 1.2f;
            }

            return baseValue;
        }

        /// <summary>
        /// Evaluates positional advantage.
        /// Center control, piece development, etc.
        /// </summary>
        private float EvaluatePosition(Board board)
        {
            float score = 0f;

            // Center control bonus
            Vector2Int[] centerSquares = new Vector2Int[]
            {
                new Vector2Int(board.Width / 2 - 1, board.Height / 2 - 1),
                new Vector2Int(board.Width / 2, board.Height / 2 - 1),
                new Vector2Int(board.Width / 2 - 1, board.Height / 2),
                new Vector2Int(board.Width / 2, board.Height / 2)
            };

            foreach (var square in centerSquares)
            {
                if (!board.IsPositionValid(square))
                {
                    continue;
                }

                var piece = board.GetPiece(square);
                if (piece != null)
                {
                    if (piece.Team == aiTeam)
                    {
                        score += 0.5f;
                    }
                    else
                    {
                        score -= 0.5f;
                    }
                }
            }

            // Piece development (not on back rank)
            var aiPieces = board.GetPiecesByTeam(aiTeam);
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
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

        /// <summary>
        /// Evaluates king safety.
        /// </summary>
        private float EvaluateKingSafety(Board board)
        {
            float score = 0f;

            // Find kings
            Piece aiKing = FindKing(board, aiTeam);
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
            Piece opponentKing = FindKing(board, opponentTeam);

            if (aiKing != null)
            {
                // AI king is safer if it has friendly pieces nearby
                int friendlyNeighbors = CountFriendlyNeighbors(board, aiKing.Position, aiTeam);
                score += friendlyNeighbors * 0.3f;

                // Penalty if king is in check
                if (MoveValidator.IsPositionUnderAttack(board, aiKing.Position, opponentTeam))
                {
                    score -= 5.0f;
                }
            }

            if (opponentKing != null)
            {
                int opponentFriendlyNeighbors = CountFriendlyNeighbors(board, opponentKing.Position, opponentTeam);
                score -= opponentFriendlyNeighbors * 0.3f;

                // Bonus if opponent king is in check
                if (MoveValidator.IsPositionUnderAttack(board, opponentKing.Position, aiTeam))
                {
                    score += 5.0f;
                }
            }

            return score;
        }

        /// <summary>
        /// Evaluates mobility (number of valid moves).
        /// </summary>
        private float EvaluateMobility(Board board)
        {
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            int aiMoves = CountTotalMoves(board, aiTeam);
            int opponentMoves = CountTotalMoves(board, opponentTeam);

            return (aiMoves - opponentMoves) * 0.1f;
        }

        /// <summary>
        /// Finds the king of a specific team.
        /// </summary>
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

        /// <summary>
        /// Counts friendly pieces adjacent to a position.
        /// </summary>
        private int CountFriendlyNeighbors(Board board, Vector2Int position, Team team)
        {
            int count = 0;
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(1, 1), new Vector2Int(1, -1),
                new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };

            foreach (var offset in neighbors)
            {
                Vector2Int checkPos = position + offset;
                if (board.IsPositionValid(checkPos))
                {
                    var piece = board.GetPiece(checkPos);
                    if (piece != null && piece.Team == team && piece.Type != PieceType.King)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Counts total number of valid moves for a team.
        /// </summary>
        private int CountTotalMoves(Board board, Team team)
        {
            int totalMoves = 0;
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var moves = MoveValidator.GetValidMoves(board, piece.Position);
                totalMoves += moves.Count;
            }

            return totalMoves;
        }
    }
}
