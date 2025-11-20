using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Movement rules for bishops
    /// </summary>
    public class BishopMoveRule : IMoveRule
    {
        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            var moves = new List<Position>();

            // Diagonal directions
            int[][] directions = new int[][]
            {
                new int[] { 1, 1 },    // Up-right
                new int[] { 1, -1 },   // Down-right
                new int[] { -1, 1 },   // Up-left
                new int[] { -1, -1 }   // Down-left
            };

            foreach (var dir in directions)
            {
                AddMovesInDirection(board, position, piece, dir[0], dir[1], moves);
            }

            return moves;
        }

        private void AddMovesInDirection(Board board, Position position, Piece piece, int dx, int dy, List<Position> moves)
        {
            int x = position.X + dx;
            int y = position.Y + dy;

            while (true)
            {
                var newPos = PositionCache.Get(x, y);
                if (!board.IsValidPosition(newPos))
                {
                    break;
                }

                var targetPiece = board.GetPieceAt(newPos);
                if (targetPiece == null)
                {
                    moves.Add(newPos);
                }
                else
                {
                    // Can capture enemy piece
                    if (targetPiece.Color != piece.Color)
                    {
                        moves.Add(newPos);
                    }
                    break; // Can't move past any piece
                }

                x += dx;
                y += dy;
            }
        }
    }
}
