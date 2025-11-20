namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// King leaves an obstacle when moving
    /// </summary>
    public class KingsShadowArtifact : Artifact
    {
        public override string Name => "King's Shadow";
        public override string Description => "King leaves an obstacle when moving";
        public override int Cost => 100;
        public override ArtifactTrigger Trigger => ArtifactTrigger.OnMove;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (!IsActive || context.MovedPiece == null)
            {
                return;
            }

            // Only applies to kings
            if (context.MovedPiece.Type == PieceType.King)
            {
                // Create obstacle at original position
                if (context.FromPosition != null && board.IsValidPosition(context.FromPosition))
                {
                    var tile = board.GetTileAt(context.FromPosition);
                    if (tile.Piece == null) // Only if position is now empty
                    {
                        board.SetTileType(context.FromPosition, TileType.Wall);
                    }
                }
            }
        }
    }
}
