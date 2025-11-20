using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Movement rules for knights
    /// </summary>
    public class KnightMoveRule : IMoveRule
    {
        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            var moves = new List<Position>();

            // All 8 possible L-shaped moves
            int[][] offsets = new int[][]
            {
                new int[] { -2, -1 }, new int[] { -2, 1 },
                new int[] { -1, -2 }, new int[] { -1, 2 },
                new int[] { 1, -2 },  new int[] { 1, 2 },
                new int[] { 2, -1 },  new int[] { 2, 1 }
            };

            foreach (var offset in offsets)
            {
                var newPos = PositionCache.Get(position.X + offset[0], position.Y + offset[1]);
                if (board.IsValidPosition(newPos))
                {
                    var targetPiece = board.GetPieceAt(newPos);
                    if (targetPiece == null || targetPiece.Color != piece.Color)
                    {
                        moves.Add(newPos);
                    }
                }
            }

            return moves;
        }
    }
}
