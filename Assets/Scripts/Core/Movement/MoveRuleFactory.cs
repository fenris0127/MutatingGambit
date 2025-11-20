namespace MutatingGambit.Core.Movement
{
    /// <summary>
    /// Factory for creating move rules based on piece type
    /// </summary>
    public static class MoveRuleFactory
    {
        public static IMoveRule GetMoveRule(Piece piece)
        {
            switch (piece.Type)
            {
                case PieceType.Pawn:
                    return new PawnMoveRule();
                case PieceType.Rook:
                    return new RookMoveRule();
                case PieceType.Knight:
                    return new KnightMoveRule();
                case PieceType.Bishop:
                    return new BishopMoveRule();
                case PieceType.Queen:
                    return new QueenMoveRule();
                case PieceType.King:
                    return new KingMoveRule();
                default:
                    return new NullMoveRule();
            }
        }
    }
}
