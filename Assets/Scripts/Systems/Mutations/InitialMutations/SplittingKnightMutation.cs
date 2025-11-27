using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Splitting Knight Mutation: When knight captures, spawns a pawn at its original position.
    /// </summary>
    [CreateAssetMenu(fileName = "SplittingKnightMutation", menuName = "Mutations/Initial/Splitting Knight")]
    public class SplittingKnightMutation : Mutation
    {
        [Header("Spawn Settings")]
        [SerializeField]
        [Tooltip("Prefab for the pawn to spawn. If null, will try to find from resources.")]
        private GameObject pawnPrefab;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Knight)
            {
                Debug.LogWarning("SplittingKnightMutation can only be applied to Knights.");
                return;
            }

            // No movement rule changes needed - effect happens on capture
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // No cleanup needed
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Board board)
        {
            // This should be called from outside when a capture happens
            // For now we just log it - actual spawning would need GameManager integration
            Debug.Log($"SplittingKnight: {mutatedPiece} captured {capturedPiece}. Would spawn pawn at original position.");

            // Future implementation would:
            // 1. Get the position before the knight moved (need to track this)
            // 2. Check if that position is now empty
            // 3. Spawn a pawn there
        }

        /// <summary>
        /// Spawns a pawn at the specified position.
        /// This is a placeholder - actual implementation needs piece instantiation system.
        /// </summary>
        private void SpawnPawn(Vector2Int position, Team team, Board board)
        {
            // TODO: Implement when piece instantiation system is ready
            Debug.Log($"Would spawn {team} pawn at {position.ToNotation()}");

            /* Future implementation:
            if (pawnPrefab != null)
            {
                GameObject pawnObject = Instantiate(pawnPrefab);
                Piece pawn = pawnObject.GetComponent<Piece>();
                pawn.Initialize(PieceType.Pawn, team, position);
                board.PlacePiece(pawn, position);
            }
            */
        }
    }
}
