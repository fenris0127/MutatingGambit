using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Movement rules for kings
    /// </summary>
    public class KingMoveRule : IMoveRule
    {
        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            var moves = new List<Position>();

            // All 8 adjacent squares
            int[][] offsets = new int[][]
            {
                new int[] { -1, -1 }, new int[] { 0, -1 }, new int[] { 1, -1 },
                new int[] { -1, 0 },                       new int[] { 1, 0 },
                new int[] { -1, 1 },  new int[] { 0, 1 },  new int[] { 1, 1 }
            };

            foreach (var offset in offsets)
            {
                var newPos = PositionCache.Get(position.X + offset[0], position.Y + offset[1]);
                if (board.IsValidPosition(newPos))
                {
                    var targetPiece = board.GetPieceAt(newPos);
                    if (targetPiece == null || targetPiece.Color != piece.Color)
                    {
                        // TODO: Check if move would put king in check
                        moves.Add(newPos);
                    }
                }
            }

            // TODO: Add castling logic

            return moves;
        }
    }
}
