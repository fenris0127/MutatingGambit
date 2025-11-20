using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Movement rules for pawns
    /// </summary>
    public class PawnMoveRule : IMoveRule
    {
        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            var moves = new List<Position>();
            int direction = piece.Color == PieceColor.White ? 1 : -1; // White moves up, black moves down

            // Forward one square
            var oneForward = PositionCache.Get(position.X, position.Y + direction);
            if (board.IsValidPosition(oneForward) && board.GetPieceAt(oneForward) == null)
            {
                moves.Add(oneForward);

                // Forward two squares on first move
                if (!piece.HasMoved)
                {
                    var twoForward = PositionCache.Get(position.X, position.Y + (direction * 2));
                    if (board.IsValidPosition(twoForward) && board.GetPieceAt(twoForward) == null)
                    {
                        moves.Add(twoForward);
                    }
                }
            }

            // Diagonal captures
            var diagonalLeft = PositionCache.Get(position.X - 1, position.Y + direction);
            if (board.IsValidPosition(diagonalLeft))
            {
                var targetPiece = board.GetPieceAt(diagonalLeft);
                if (targetPiece != null && targetPiece.Color != piece.Color)
                {
                    moves.Add(diagonalLeft);
                }
            }

            var diagonalRight = PositionCache.Get(position.X + 1, position.Y + direction);
            if (board.IsValidPosition(diagonalRight))
            {
                var targetPiece = board.GetPieceAt(diagonalRight);
                if (targetPiece != null && targetPiece.Color != piece.Color)
                {
                    moves.Add(diagonalRight);
                }
            }

            return moves;
        }
    }
}
