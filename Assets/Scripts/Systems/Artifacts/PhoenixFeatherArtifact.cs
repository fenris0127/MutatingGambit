using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Phoenix Feather Artifact: Automatically repairs one broken piece per room.
    /// Provides passive piece recovery.
    /// </summary>
    [CreateAssetMenu(fileName = "PhoenixFeatherArtifact", menuName = "Mutating Gambit/Artifacts/Phoenix Feather")]
    public class PhoenixFeatherArtifact : Artifact
    {
        private bool usedThisRoom = false;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (usedThisRoom || context == null)
            {
                return;
            }

            // Get repair system from context
            var repairSystem = context.RepairSystem as RepairSystem;

            if (repairSystem != null && repairSystem.BrokenPieceCount > 0)
            {
                var brokenPieces = repairSystem.BrokenPieces;

                if (brokenPieces.Count > 0)
                {
                    // Auto-repair first broken piece
                    repairSystem.RepairPiece(brokenPieces[0]);
                    usedThisRoom = true;

                    Debug.Log("Phoenix Feather auto-repaired a piece!");
                }
            }
        }

        public override void OnAcquired(Board board)
        {
            Debug.Log("Phoenix Feather acquired! One piece will auto-repair each room.");
        }

        public void ResetForNewRoom()
        {
            usedThisRoom = false;
        }
    }
}
