using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// 기물에 변이를 적용하고 제거하는 작업을 처리합니다.
    /// </summary>
    public class MutationApplicator
    {
        /// <summary>
        /// 기물에 변이를 적용합니다.
        /// </summary>
        public void ApplyMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null)
            {
                Debug.LogWarning("기물 또는 변이가 null입니다");
                return;
            }

            mutation.ApplyToPiece(piece);
            Debug.Log($"{piece.name}에 변이 적용: {mutation.MutationName}");
        }

        /// <summary>
        /// 기물에서 변이를 제거합니다.
        /// </summary>
        public void RemoveMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null)
            {
                Debug.LogWarning("기물 또는 변이가 null입니다");
                return;
            }

            mutation.RemoveFromPiece(piece);
            Debug.Log($"{piece.name}에서 변이 제거: {mutation.MutationName}");
        }

        /// <summary>
        /// 기물에서 모든 변이를 제거합니다.
        /// </summary>
        public void RemoveAllMutations(Piece piece, List<Mutation> mutations)
        {
            if (piece == null || mutations == null) return;

            foreach (var mutation in mutations)
            {
                if (mutation != null)
                {
                    mutation.RemoveFromPiece(piece);
                }
            }

            Debug.Log($"{piece.name}에서 모든 변이 제거됨");
        }

        /// <summary>
        /// 기물 목록에 변이를 적용합니다.
        /// </summary>
        public void ApplyMutationToMultiplePieces(List<Piece> pieces, Mutation mutation)
        {
            if (pieces == null || mutation == null) return;

            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    ApplyMutation(piece, mutation);
                }
            }
        }
    }
}
