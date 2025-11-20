using MutatingGambit.Core.Movement;

namespace MutatingGambit.Core.Mutations
{
    /// <summary>
    /// Knight spawns a pawn at its original position when capturing
    /// </summary>
    public class SplittingKnightMutation : Mutation
    {
        public override string Name => "Splitting Knight";
        public override string Description => "Spawns a pawn when capturing";
        public override int Cost => 75;

        public override IMoveRule ApplyToMoveRule(IMoveRule baseRule, Piece piece)
        {
            // Knight movement doesn't change, but capture behavior does
            return baseRule;
        }

        /// <summary>
        /// Called when the knight captures a piece
        /// </summary>
        public void OnCapture(Board board, Position originalPos, Position targetPos, Piece knight)
        {
            // Check if original position is empty
            if (board.GetPieceAt(originalPos) == null)
            {
                // Spawn a pawn of the same color
                var spawnedPawn = new Piece(knight.Color, PieceType.Pawn);
                board.PlacePiece(spawnedPawn, originalPos);
            }
        }
    }
}
