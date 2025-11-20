namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// Knights move again after capturing
    /// </summary>
    public class CavalryChargeArtifact : Artifact
    {
        public override string Name => "Cavalry Charge";
        public override string Description => "Knights move again after capturing";
        public override int Cost => 120;
        public override ArtifactTrigger Trigger => ArtifactTrigger.OnCapture;

        public override bool AllowsExtraMove(Board board, ArtifactContext context)
        {
            if (!IsActive || context.MovedPiece == null)
            {
                return false;
            }

            // Only applies to knights
            if (context.MovedPiece.Type != PieceType.Knight)
            {
                return false;
            }

            // Only when capturing
            if (context.CapturedPiece == null)
            {
                return false;
            }

            return true;
        }
    }
}
