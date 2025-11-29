using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// 게임 전체의 변이를 추적하고 관리합니다.
    /// </summary>
    public class MutationManager : MonoBehaviour
    {
        private static MutationManager instance;

        private Dictionary<Piece, List<Mutation>> pieceMutations = new Dictionary<Piece, List<Mutation>>();
        private MutationApplicator applicator;

        public static MutationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MutationManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            applicator = new MutationApplicator();
        }

        /// <summary>
        /// 변이 추적을 위해 기물을 등록합니다.
        /// </summary>
        public void RegisterPiece(Piece piece)
        {
            if (piece != null && !pieceMutations.ContainsKey(piece))
            {
                pieceMutations[piece] = new List<Mutation>();
            }
        }

        /// <summary>
        /// 추적에서 기물을 등록 취소하고 정리합니다.
        /// </summary>
        public void UnregisterPiece(Piece piece)
        {
            if (piece != null && pieceMutations.ContainsKey(piece))
            {
                pieceMutations.Remove(piece);
            }
        }

        /// <summary>
        /// 기물에 변이를 적용하고 추적합니다.
        /// </summary>
        public void ApplyMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null) return;

            RegisterPiece(piece);
            
            if (!pieceMutations[piece].Contains(mutation))
            {
                applicator.ApplyMutation(piece, mutation);
                pieceMutations[piece].Add(mutation);
            }
        }

        /// <summary>
        /// 기물에서 변이를 제거합니다.
        /// </summary>
        public void RemoveMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null) return;

            if (pieceMutations.ContainsKey(piece) && pieceMutations[piece].Contains(mutation))
            {
                applicator.RemoveMutation(piece, mutation);
                pieceMutations[piece].Remove(mutation);
            }
        }

        /// <summary>
        /// 기물의 변이 목록을 가져옵니다.
        /// </summary>
        public List<Mutation> GetMutationsForPiece(Piece piece)
        {
            if (piece != null && pieceMutations.ContainsKey(piece))
            {
                return new List<Mutation>(pieceMutations[piece]);
            }
            return new List<Mutation>();
        }

        /// <summary>
        /// 기물 이동 시 변이에 알립니다.
        /// </summary>
        public void NotifyMove(Piece piece, Vector2Int from, Vector2Int to, Board board)
        {
            if (piece == null || !pieceMutations.ContainsKey(piece)) return;

            foreach (var mutation in pieceMutations[piece])
            {
                mutation.OnPieceMoved(piece, from,to, board);
            }
        }

        /// <summary>
        /// 기물 잡기 시 변이에 알립니다.
        /// </summary>
        public void NotifyCapture(Piece attacker, Piece captured, Vector2Int from, Vector2Int to, Board board)
        {
            if (attacker != null && pieceMutations.ContainsKey(attacker))
            {
                foreach (var mutation in pieceMutations[attacker])
                {
                    mutation.OnPieceCaptured(attacker, captured, from, to, board);
                }
            }
        }

        /// <summary>
        /// 모든 변이 데이터를 지웁니다.
        /// </summary>
        public void ClearAll()
        {
            pieceMutations.Clear();
            Debug.Log("모든 변이 데이터 지움");
        }
    }
}
