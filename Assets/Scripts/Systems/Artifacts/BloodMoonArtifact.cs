using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Blood Moon Artifact: Capturing pieces grants bonus power.
    /// Encourages aggressive play.
    /// </summary>
    [CreateAssetMenu(fileName = "BloodMoonArtifact", menuName = "Mutating Gambit/Artifacts/Blood Moon")]
    public class BloodMoonArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context != null && context.CapturedPiece != null)
            {
                Debug.Log($"Blood Moon: {context.MovedPiece?.Type} gains power from capture!");
                // In full implementation: boost the capturing piece's stats
            }
        }
    }
}
