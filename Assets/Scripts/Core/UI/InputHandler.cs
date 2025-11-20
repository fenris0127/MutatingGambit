using MutatingGambit.Core.Movement;

namespace MutatingGambit.Core.UI
{
    /// <summary>
    /// Handles user input and move validation
    /// </summary>
    public class InputHandler
    {
        private Position _selectedPosition;

        public void SelectPosition(Position position)
        {
            _selectedPosition = position;
        }

        public Position GetSelectedPosition()
        {
            return _selectedPosition;
        }

        public void ClearSelection()
        {
            _selectedPosition = null;
        }

        public bool ValidateMove(Board board, Position from, Position to)
        {
            var piece = board.GetPieceAt(from);
            if (piece == null || piece.IsBroken)
            {
                return false;
            }

            var legalMoves = MoveValidator.GetLegalMoves(board, from);
            return legalMoves.Contains(to);
        }

        public string GetMoveFeedback(Board board, Position from, Position to)
        {
            var piece = board.GetPieceAt(from);
            if (piece == null)
            {
                return "No piece at selected position";
            }

            if (piece.IsBroken)
            {
                return "Piece is broken and cannot move";
            }

            var legalMoves = MoveValidator.GetLegalMoves(board, from);
            if (!legalMoves.Contains(to))
            {
                return "Illegal move";
            }

            var targetPiece = board.GetPieceAt(to);
            if (targetPiece != null)
            {
                return $"Capture {targetPiece.Type}";
            }

            return "Valid move";
        }
    }
}
