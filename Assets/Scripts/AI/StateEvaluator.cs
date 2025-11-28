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
        /// Evaluates a lightweight BoardState (optimized for AI simulation).
        /// Positive score = good for AI, negative = good for opponent.
        /// </summary>
        public float EvaluateBoardState(BoardState state)
        {
            if (state == null)
            {
                return 0f;
            }

            float score = 0f;

            // Material evaluation
            score += EvaluateMaterialState(state) * config.MaterialWeight;

            // Positional evaluation
            score += EvaluatePositionState(state) * config.PositionalWeight;

            // King safety
            score += EvaluateKingSafetyState(state) * config.KingSafetyWeight;

            // Mobility (number of valid moves)
            score += EvaluateMobilityState(state) * config.MobilityWeight;

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

        // ========== BoardState Evaluation Methods (Optimized for AI) ==========

        /// <summary>
        /// Evaluates material advantage from BoardState.
        /// </summary>
        private float EvaluateMaterialState(BoardState state)
        {
            float aiMaterial = 0f;
            float opponentMaterial = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            var aiPieces = state.GetPiecesByTeam(aiTeam);
            var opponentPieces = state.GetPiecesByTeam(opponentTeam);

            foreach (var pieceData in aiPieces)
            {
                aiMaterial += config.GetPieceValue(pieceData.Type);
            }

            foreach (var pieceData in opponentPieces)
            {
                opponentMaterial += config.GetPieceValue(pieceData.Type);
            }

            return aiMaterial - opponentMaterial;
        }

        /// <summary>
        /// Evaluates positional advantage from BoardState.
        /// </summary>
        private float EvaluatePositionState(BoardState state)
        {
            float score = 0f;

            // Center control bonus
            Vector2Int[] centerSquares = new Vector2Int[]
            {
                new Vector2Int(state.Width / 2 - 1, state.Height / 2 - 1),
                new Vector2Int(state.Width / 2, state.Height / 2 - 1),
                new Vector2Int(state.Width / 2 - 1, state.Height / 2),
                new Vector2Int(state.Width / 2, state.Height / 2)
            };

            foreach (var square in centerSquares)
            {
                if (!state.IsPositionValid(square))
                {
                    continue;
                }

                var pieceData = state.GetPieceData(square);
                if (pieceData != null)
                {
                    if (pieceData.Team == aiTeam)
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
            var aiPieces = state.GetPiecesByTeam(aiTeam);
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
            var opponentPieces = state.GetPiecesByTeam(opponentTeam);

            int aiBackRank = aiTeam == Team.White ? 0 : state.Height - 1;
            int opponentBackRank = opponentTeam == Team.White ? 0 : state.Height - 1;

            foreach (var pieceData in aiPieces)
            {
                if (pieceData.Type != PieceType.King && pieceData.Position.y != aiBackRank)
                {
                    score += 0.1f;
                }
            }

            foreach (var pieceData in opponentPieces)
            {
                if (pieceData.Type != PieceType.King && pieceData.Position.y != opponentBackRank)
                {
                    score -= 0.1f;
                }
            }

            return score;
        }

        /// <summary>
        /// Evaluates king safety from BoardState.
        /// </summary>
        private float EvaluateKingSafetyState(BoardState state)
        {
            float score = 0f;

            // Find kings
            BoardState.PieceData aiKing = FindKingInState(state, aiTeam);
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;
            BoardState.PieceData opponentKing = FindKingInState(state, opponentTeam);

            if (aiKing != null)
            {
                // AI king is safer if it has friendly pieces nearby
                int friendlyNeighbors = CountFriendlyNeighborsState(state, aiKing.Position, aiTeam);
                score += friendlyNeighbors * 0.3f;
            }

            if (opponentKing != null)
            {
                int opponentFriendlyNeighbors = CountFriendlyNeighborsState(state, opponentKing.Position, opponentTeam);
                score -= opponentFriendlyNeighbors * 0.3f;
            }

            return score;
        }

        /// <summary>
        /// Evaluates mobility from BoardState.
        /// </summary>
        private float EvaluateMobilityState(BoardState state)
        {
            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            int aiMoves = CountTotalMovesState(state, aiTeam);
            int opponentMoves = CountTotalMovesState(state, opponentTeam);

            return (aiMoves - opponentMoves) * 0.1f;
        }

        /// <summary>
        /// Finds the king in BoardState.
        /// </summary>
        private BoardState.PieceData FindKingInState(BoardState state, Team team)
        {
            var pieces = state.GetPiecesByTeam(team);
            foreach (var pieceData in pieces)
            {
                if (pieceData.Type == PieceType.King)
                {
                    return pieceData;
                }
            }
            return null;
        }

        /// <summary>
        /// Counts friendly pieces adjacent to a position in BoardState.
        /// </summary>
        private int CountFriendlyNeighborsState(BoardState state, Vector2Int position, Team team)
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
                if (state.IsPositionValid(checkPos))
                {
                    var pieceData = state.GetPieceData(checkPos);
                    if (pieceData != null && pieceData.Team == team && pieceData.Type != PieceType.King)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Counts total number of valid moves for a team in BoardState.
        /// </summary>
        private int CountTotalMovesState(BoardState state, Team team)
        {
            int totalMoves = 0;
            var pieces = state.GetPiecesByTeam(team);

            foreach (var pieceData in pieces)
            {
                var moves = state.GetValidMoves(pieceData);
                totalMoves += moves.Count;
            }

            return totalMoves;
        }
    }
}
