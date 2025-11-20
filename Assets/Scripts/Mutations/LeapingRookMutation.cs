using System.Collections.Generic;
using MutatingGambit.Core.Movement;

namespace MutatingGambit.Core.Mutations
{
    /// <summary>
    /// Allows a rook to jump over ONE friendly piece
    /// </summary>
    public class LeapingRookMutation : Mutation
    {
        public override string Name => "Leaping Rook";
        public override string Description => "Can jump over ONE friendly piece";
        public override int Cost => 50;

        public override IMoveRule ApplyToMoveRule(IMoveRule baseRule, Piece piece)
        {
            if (piece.Type != PieceType.Rook)
            {
                return baseRule;
            }

            return new LeapingRookMoveRule(baseRule);
        }
    }

    /// <summary>
    /// Move rule for leaping rooks
    /// </summary>
    public class LeapingRookMoveRule : IMoveRule
    {
        private readonly IMoveRule _baseRule;

        public LeapingRookMoveRule(IMoveRule baseRule)
        {
            _baseRule = baseRule;
        }

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
            int friendlyPiecesJumped = 0;

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
                else if (targetPiece.Color == piece.Color)
                {
                    // Friendly piece
                    if (friendlyPiecesJumped == 0)
                    {
                        // Can jump over the first friendly piece
                        friendlyPiecesJumped++;
                    }
                    else
                    {
                        // Can't jump over two friendly pieces
                        break;
                    }
                }
                else
                {
                    // Enemy piece - can capture but can't jump
                    moves.Add(newPos);
                    break;
                }

                x += dx;
                y += dy;
            }
        }
    }
}
