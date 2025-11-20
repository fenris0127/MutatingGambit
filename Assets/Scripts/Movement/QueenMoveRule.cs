using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Movement rules for queens (combines rook and bishop)
    /// </summary>
    public class QueenMoveRule : IMoveRule
    {
        private readonly RookMoveRule _rookRule = new RookMoveRule();
        private readonly BishopMoveRule _bishopRule = new BishopMoveRule();

        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            var moves = new List<Position>();

            // Combine rook and bishop moves
            moves.AddRange(_rookRule.GetPossibleMoves(board, position, piece));
            moves.AddRange(_bishopRule.GetPossibleMoves(board, position, piece));

            return moves;
        }
    }
}
