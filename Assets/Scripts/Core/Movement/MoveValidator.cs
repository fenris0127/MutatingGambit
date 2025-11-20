using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Validates and generates legal moves for chess pieces
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Gets all legal moves for a piece at the given position
        /// </summary>
        public static List<Position> GetLegalMoves(Board board, Position position)
        {
            var piece = board.GetPieceAt(position);
            if (piece == null || piece.IsBroken)
            {
                return new List<Position>();
            }

            IMoveRule moveRule = MoveRuleFactory.GetMoveRule(piece);
            var possibleMoves = moveRule.GetPossibleMoves(board, position, piece);

            // Apply mutations if any
            foreach (var mutation in piece.Mutations)
            {
                // Mutations will modify the move list in future implementations
                // For now, this is a placeholder for the mutation system
            }

            return possibleMoves;
        }

        /// <summary>
        /// Checks if a pawn will promote when moving to the target position
        /// </summary>
        public static bool WillPromote(Board board, Position from, Position to)
        {
            var piece = board.GetPieceAt(from);
            if (piece == null || piece.Type != PieceType.Pawn)
            {
                return false;
            }

            // White pawns promote on rank 8 (y = 7), black pawns on rank 1 (y = 0)
            if (piece.Color == PieceColor.White && to.Y == 7)
            {
                return true;
            }
            if (piece.Color == PieceColor.Black && to.Y == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if castling is possible
        /// </summary>
        public static bool CanCastle(Board board, Position kingPos, Position targetPos)
        {
            var king = board.GetPieceAt(kingPos);
            if (king == null || king.Type != PieceType.King || king.HasMoved)
            {
                return false;
            }

            // Determine if kingside or queenside castling
            int direction = targetPos.X > kingPos.X ? 1 : -1;
            int rookX = direction > 0 ? 7 : 0;
            var rookPos = PositionCache.Get(rookX, kingPos.Y);
            var rook = board.GetPieceAt(rookPos);

            if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
            {
                return false;
            }

            // Check if path is clear
            int start = System.Math.Min(kingPos.X, targetPos.X);
            int end = System.Math.Max(kingPos.X, targetPos.X);

            for (int x = start + 1; x < end; x++)
            {
                if (board.GetPieceAt(PositionCache.Get(x, kingPos.Y)) != null)
                {
                    return false;
                }
            }

            // Also check between rook and king
            int rookStart = System.Math.Min(kingPos.X, rookX);
            int rookEnd = System.Math.Max(kingPos.X, rookX);

            for (int x = rookStart + 1; x < rookEnd; x++)
            {
                if (board.GetPieceAt(PositionCache.Get(x, kingPos.Y)) != null)
                {
                    return false;
                }
            }

            // TODO: Check if king is in check or passes through check

            return true;
        }
    }
}
