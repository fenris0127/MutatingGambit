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
        #region 변수
        [Header("Spawn Settings")]
        [SerializeField]
        [Tooltip("Prefab for the pawn to spawn. If null, will use the board's default spawn method.")]
        private GameObject pawnPrefab;
        #endregion

        #region 공개 메서드
        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Knight)
            {
                Debug.LogWarning("SplittingKnightMutation can only be applied to Knights.");
                return;
            }
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // No cleanup needed
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            SpawnPawn(fromPos, mutatedPiece.Team, board);
        }

        public override bool IsCompatibleWith(PieceType pieceType) => pieceType == PieceType.Knight;
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// Spawns a pawn at the specified position.
        /// </summary>
        private void SpawnPawn(Vector2Int position, Team team, Board board)
        {
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
                board.SpawnPiece(PieceType.Pawn, team, position);
            }
            
            Debug.Log($"SplittingKnight: Spawned pawn at {position}");
        }
        #endregion
    }
}
