namespace MutatingGambit.Core.Victory
{
    /// <summary>
    /// Victory condition: Capture a specific piece type
    /// </summary>
    public class CaptureSpecificPieceCondition : IVictoryCondition
    {
        private readonly PieceType _targetType;
        private readonly PieceColor _targetColor;

        public CaptureSpecificPieceCondition(PieceType targetType, PieceColor targetColor)
        {
            _targetType = targetType;
            _targetColor = targetColor;
        }

        public string GetDescription()
        {
            return $"Capture the {_targetColor} {_targetType}";
        }

        public bool IsMet(Board board)
        {
            // Check if the target piece no longer exists on the board
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var piece = board.GetPieceAt(PositionCache.Get(x, y));
                    if (piece != null &&
                        piece.Type == _targetType &&
                        piece.Color == _targetColor &&
                        !piece.IsBroken)
                    {
                        return false; // Piece still exists
                    }
                }
            }

            return true; // Piece has been captured
        }
    }
}
