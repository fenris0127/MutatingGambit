using System.Collections.Generic;
using MutatingGambit.Core.Movement;

namespace MutatingGambit.Core.Mutations
{
    /// <summary>
    /// Bishop can only move exactly 3 squares diagonally
    /// </summary>
    public class GlassBishopMutation : Mutation
    {
        public override string Name => "Glass Bishop";
        public override string Description => "Moves exactly 3 squares (no more, no less)";
        public override int Cost => 60;

        public override IMoveRule ApplyToMoveRule(IMoveRule baseRule, Piece piece)
        {
            if (piece.Type != PieceType.Bishop)
            {
                return baseRule;
            }

            return new GlassBishopMoveRule();
        }
    }

    /// <summary>
    /// Move rule for glass bishops
    /// </summary>
    public class GlassBishopMoveRule : IMoveRule
    {
        private const int EXACT_DISTANCE = 3;

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
                // Move exactly 3 squares in this direction
                var newPos = PositionCache.Get(
                    position.X + (dir[0] * EXACT_DISTANCE),
                    position.Y + (dir[1] * EXACT_DISTANCE)
                );

                if (board.IsValidPosition(newPos))
                {
                    var targetPiece = board.GetPieceAt(newPos);

                    // Can move if empty or enemy piece
                    if (targetPiece == null || targetPiece.Color != piece.Color)
                    {
                        // Check if path is clear (still can't jump over pieces)
                        if (IsPathClear(board, position, newPos, dir[0], dir[1]))
                        {
                            moves.Add(newPos);
                        }
                    }
                }
            }

            return moves;
        }

        private bool IsPathClear(Board board, Position from, Position to, int dx, int dy)
        {
            int x = from.X + dx;
            int y = from.Y + dy;

            // Check squares 1 and 2 (not the destination square 3)
            for (int i = 0; i < EXACT_DISTANCE - 1; i++)
            {
                var checkPos = PositionCache.Get(x, y);
                if (board.GetPieceAt(checkPos) != null)
                {
                    return false; // Path is blocked
                }
                x += dx;
                y += dy;
            }

            return true;
        }
    }
}
