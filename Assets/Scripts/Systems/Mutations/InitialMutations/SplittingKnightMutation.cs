using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// 분열하는 나이트 변이: 나이트가 잡을 때 원래 위치에 폰을 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SplittingKnightMutation", menuName = "Mutations/Initial/Splitting Knight")]
    public class SplittingKnightMutation : Mutation
    {
        #region 변수
        [Header("Spawn Settings")]
        [SerializeField]
        [Tooltip("생성할 폰의 프리팹. null인 경우 보드의 기본 생성 방법을 사용합니다.")]
        private GameObject pawnPrefab;
        #endregion

        #region 공개 메서드
        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Knight)
            {
                Debug.LogWarning("SplittingKnightMutation은 나이트에만 적용할 수 있습니다.");
                return;
            }
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 정리 불필요
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            SpawnPawn(fromPos, mutatedPiece.Team, board);
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 지정된 위치에 폰을 생성합니다.
        /// </summary>
        private void SpawnPawn(Vector2Int position, Team team, Board board)
        {
            if (!board.IsPositionValid(position) || board.GetPiece(position) != null)
            {
                Debug.LogWarning($"SplittingKnight: {position}에 폰을 생성할 수 없습니다. 유효하지 않거나 점유되어 있습니다.");
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
            
            Debug.Log($"SplittingKnight: {position}에 폰 생성");
        }
        #endregion
    }
}
