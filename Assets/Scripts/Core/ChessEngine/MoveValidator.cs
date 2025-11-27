using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Validates chess moves based on current board state and piece rules.
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Gets all valid moves for a piece at the given position.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="position">The position of the piece</param>
        /// <returns>List of valid destination positions</returns>
        public static List<Vector2Int> GetValidMoves(Board board, Vector2Int position)
        {
            if (board == null || !board.IsPositionValid(position))
            {
                return new List<Vector2Int>();
            }

            Piece piece = board.GetPiece(position);
            if (piece == null)
            {
                return new List<Vector2Int>();
            }

            return piece.GetValidMoves(board);
        }

        /// <summary>
        /// Checks if a move from one position to another is valid.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="from">Starting position</param>
        /// <param name="to">Destination position</param>
        /// <returns>True if the move is valid</returns>
        public static bool IsValidMove(Board board, Vector2Int from, Vector2Int to)
        {
            if (board == null || !board.IsPositionValid(from) || !board.IsPositionValid(to))
            {
                return false;
            }

            // Can't move to the same position
            if (from == to)
            {
                return false;
            }

            var validMoves = GetValidMoves(board, from);
            return validMoves.Contains(to);
        }

        /// <summary>
        /// Checks if a piece at the given position can capture an enemy piece.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="attackerPos">Position of the attacking piece</param>
        /// <param name="targetPos">Position of the target piece</param>
        /// <returns>True if capture is valid</returns>
        public static bool CanCapture(Board board, Vector2Int attackerPos, Vector2Int targetPos)
        {
            if (!IsValidMove(board, attackerPos, targetPos))
            {
                return false;
            }

            Piece attacker = board.GetPiece(attackerPos);
            Piece target = board.GetPiece(targetPos);

            if (attacker == null || target == null)
            {
                return false;
            }

            // Can only capture enemy pieces
            return attacker.Team != target.Team;
        }

        /// <summary>
        /// Gets all positions that a piece can attack (including empty squares for some pieces).
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="position">Position of the piece</param>
        /// <returns>List of positions under attack</returns>
        public static List<Vector2Int> GetAttackedPositions(Board board, Vector2Int position)
        {
            // For now, attacked positions are the same as valid moves
            // This might differ for pawns in standard chess (attack diagonal but move forward)
            return GetValidMoves(board, position);
        }

        /// <summary>
        /// Checks if a position is under attack by any enemy piece.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="position">Position to check</param>
        /// <param name="enemyTeam">The team that might be attacking</param>
        /// <returns>True if position is under attack</returns>
        public static bool IsPositionUnderAttack(Board board, Vector2Int position, Team enemyTeam)
        {
            var enemyPieces = board.GetPiecesByTeam(enemyTeam);

            foreach (var enemyPiece in enemyPieces)
            {
                var attackedPositions = GetAttackedPositions(board, enemyPiece.Position);
                if (attackedPositions.Contains(position))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all pieces that can move to a specific position.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="team">Team of pieces to check</param>
        /// <returns>List of pieces that can move to target position</returns>
        public static List<Piece> GetPiecesAttackingPosition(Board board, Vector2Int targetPosition, Team team)
        {
            var attackingPieces = new List<Piece>();
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                if (IsValidMove(board, piece.Position, targetPosition))
                {
                    attackingPieces.Add(piece);
                }
            }

            return attackingPieces;
        }

        /// <summary>
        /// Checks if a team has any valid moves available.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="team">Team to check</param>
        /// <returns>True if team has at least one valid move</returns>
        public static bool HasValidMoves(Board board, Team team)
        {
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var validMoves = GetValidMoves(board, piece.Position);
                if (validMoves.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
