using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Interface for piece movement rules
    /// </summary>
    public interface IMoveRule
    {
        List<Position> GetPossibleMoves(Board board, Position position, Piece piece);
    }
}
