using System.Collections.Generic;

namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Movement rules for rooks
    /// </summary>
    public class RookMoveRule : IMoveRule
    {
        public List<Position> GetPossibleMoves(Board board, Position position, Piece piece)
        {
            var moves = new List<Position>();

            // Horizontal and vertical directions
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },   // Up
                new int[] { 0, -1 },  // Down
                new int[] { 1, 0 },   // Right
                new int[] { -1, 0 }   // Left
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
