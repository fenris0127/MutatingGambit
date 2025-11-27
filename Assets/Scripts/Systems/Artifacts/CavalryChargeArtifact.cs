using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Cavalry Charge Artifact: Knights can move one additional square after capturing.
    /// </summary>
    [CreateAssetMenu(fileName = "CavalryChargeArtifact", menuName = "Artifacts/Cavalry Charge")]
    public class CavalryChargeArtifact : Artifact
    {
        // Track if a knight just captured (to enable bonus move)
        private Piece knightThatCaptured = null;
        private Vector2Int capturePosition;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // This is triggered when a piece captures
            if (context.MovedPiece == null || context.CapturedPiece == null)
            {
                return;
            }

            // Check if the attacker was a knight
            if (context.MovedPiece.Type == PieceType.Knight)
            {
                knightThatCaptured = context.MovedPiece;
                capturePosition = context.ToPosition;

                Debug.Log($"Cavalry Charge: Knight at {capturePosition.ToNotation()} captured! Bonus move available.");

                // Note: Actual bonus move implementation would require integration with
                // turn/move management system. For now we just mark it.
                // A full implementation would:
                // 1. Set a flag that this knight can move again
                // 2. Wait for the player to make the bonus move
                // 3. Clear the flag after the bonus move
            }
        }

        /// <summary>
        /// Checks if a knight has a pending bonus move.
        /// </summary>
        public bool HasBonusMove(Piece piece)
        {
            return piece != null && piece == knightThatCaptured;
        }

        /// <summary>
        /// Clears the bonus move state.
        /// Should be called after the bonus move is made or turn ends.
        /// </summary>
        public void ClearBonusMove()
        {
            knightThatCaptured = null;
            capturePosition = Vector2Int.zero;
        }

        public override void OnRemoved(Board board)
        {
            ClearBonusMove();
        }
    }
}
