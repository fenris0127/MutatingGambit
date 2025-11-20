namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// Context information for artifact effects
    /// </summary>
    public class ArtifactContext
    {
        public Piece MovedPiece { get; set; }
        public Piece CapturedPiece { get; set; }
        public Position FromPosition { get; set; }
        public Position ToPosition { get; set; }
        public PieceColor CurrentPlayer { get; set; }
        public int TurnNumber { get; set; }
    }
}
