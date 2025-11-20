using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Null movement rule for pieces with no valid moves
    /// </summary>
    public class NullMoveRule : IMoveRule
    {
        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            return new List<Position>();
        }
    }
}
