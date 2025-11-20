namespace MutatingGambit.Core.Victory
{
    /// <summary>
    /// Victory condition: Move king to a specific position
    /// </summary>
    public class KingToPositionCondition : IVictoryCondition
    {
        private readonly PieceColor _kingColor;
        private readonly Position _targetPosition;

        public KingToPositionCondition(PieceColor kingColor, Position targetPosition)
        {
            _kingColor = kingColor;
            _targetPosition = targetPosition;
        }

        public string GetDescription()
        {
            return $"Move your {_kingColor} King to {_targetPosition.ToNotation()}";
        }

        public bool IsMet(Board board)
        {
            var piece = board.GetPieceAt(_targetPosition);
            return piece != null &&
                   piece.Type == PieceType.King &&
                   piece.Color == _kingColor;
        }
    }
}
