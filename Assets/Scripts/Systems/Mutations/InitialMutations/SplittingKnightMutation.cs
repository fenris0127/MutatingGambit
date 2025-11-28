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

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            // Spawn pawn at original position
            SpawnPawn(fromPos, mutatedPiece.Team, board);
        }

        /// <summary>
        /// Spawns a pawn at the specified position.
        /// </summary>
        private void SpawnPawn(Vector2Int position, Team team, Board board)
        {
            // Check if position is valid and empty
            if (!board.IsPositionValid(position) || board.GetPiece(position) != null)
            {
                Debug.LogWarning($"SplittingKnight: Cannot spawn pawn at {position}, invalid or occupied.");
                return;
            }

            if (pawnPrefab != null)
            {
                GameObject pawnObject = Instantiate(pawnPrefab);
                Piece pawn = pawnObject.GetComponent<Piece>();
                if (pawn == null) pawn = pawnObject.AddComponent<Piece>();
                
                pawn.Initialize(PieceType.Pawn, team, position);
                board.PlacePiece(pawn, position);
            }
            else
            {
                // Fallback to board's spawn method
                board.SpawnPiece(PieceType.Pawn, team, position);
            }
            
            Debug.Log($"SplittingKnight: Spawned pawn at {position}");
        }
    }
}
