using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// 폭발하는 킹 변이: 킹이 이동할 때 인접한 모든 적 기물에게 피해를 줍니다.
    /// 공격적이고 적극적인 킹 전략을 만듭니다.
    /// Explosive King Mutation: King damages all adjacent enemy pieces when moving.
    /// Creates an offensive, aggressive king strategy.
    /// </summary>
    [CreateAssetMenu(fileName = "ExplosiveKingMutation", menuName = "Mutating Gambit/Mutations/Explosive King")]
    public class ExplosiveKingMutation : Mutation
    {
        [SerializeField]
        [Tooltip("Damage dealt to adjacent enemies (currently just marks for capture).")]
        private int explosionDamage = 1;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.King)
            {
                Debug.LogWarning("Explosive King mutation can only be applied to kings!");
                return;
            }

            Debug.Log($"Applied Explosive King mutation to {piece.Team} king");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // No special cleanup needed
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (mutatedPiece.Type != PieceType.King)
            {
                return;
            }

            // Find all adjacent enemy pieces and damage them
            Vector2Int[] adjacentOffsets = new Vector2Int[]
            {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 1), new Vector2Int(1, -1),
                new Vector2Int(-1, 1), new Vector2Int(-1, -1)
            };

            List<Piece> damagedPieces = new List<Piece>();

            foreach (var offset in adjacentOffsets)
            {
                Vector2Int checkPos = to + offset;

                if (board.IsPositionValid(checkPos))
                {
                    var piece = board.GetPiece(checkPos);

                    if (piece != null && piece.Team != mutatedPiece.Team)
                    {
                        damagedPieces.Add(piece);
                    }
                }
            }

            // Apply damage (in MVP, we'll just log it)
            if (damagedPieces.Count > 0)
            {
                Debug.Log($"Explosive King damaged {damagedPieces.Count} adjacent enemies!");

                // In full implementation, this would reduce piece health
                // For now, we'll mark them visually or apply a damage effect
            }
        }
    }
}
