namespace MutatingGambit.Core.Victory
{
    /// <summary>
    /// Victory condition: Checkmate the opponent within N moves
    /// </summary>
    public class CheckmateInNMovesCondition : IVictoryCondition
    {
        private readonly int _maxMoves;
        private int _currentMoves;

        public CheckmateInNMovesCondition(int maxMoves)
        {
            _maxMoves = maxMoves;
            _currentMoves = 0;
        }

        public string GetDescription()
        {
            return $"Checkmate the opponent within {_maxMoves} moves";
        }

        public bool IsMet(Board board)
        {
            // TODO: Implement checkmate detection
            // For now, this is a placeholder
            return false;
        }

        public void IncrementMoves()
        {
            _currentMoves++;
        }

        public bool IsOutOfMoves()
        {
            return _currentMoves >= _maxMoves;
        }
    }
}
