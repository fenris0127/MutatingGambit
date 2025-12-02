using System;
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
        #region Singleton

        private static MutationManager instance;

        public static MutationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<MutationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("MutationManager");
                        instance = go.AddComponent<MutationManager>();
                        DontDestroyOnLoad(go);
                    }
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
        }

        #endregion

        #region Fields & Events

        private Dictionary<Piece, List<MutationState>> pieceMutationStates = new Dictionary<Piece, List<MutationState>>();

        /// <summary>
        /// 변이가 기물에 적용될 때 발생하는 이벤트
        /// </summary>
        public event Action<Piece, Mutation> OnMutationApplied;

        /// <summary>
        /// 변이가 기물에서 제거될 때 발생하는 이벤트
        /// </summary>
        public event Action<Piece, Mutation> OnMutationRemoved;

        #endregion

        #region Piece Registration

        /// <summary>
        /// 변이 추적을 위해 기물을 등록합니다.
        /// </summary>
        public void RegisterPiece(Piece piece)
        {
            if (piece != null && !pieceMutationStates.ContainsKey(piece))
            {
                pieceMutationStates[piece] = new List<MutationState>();
            }
        }

        /// <summary>
        /// 추적에서 기물을 등록 취소하고 정리합니다.
        /// </summary>
        public void UnregisterPiece(Piece piece)
        {
            if (piece != null && pieceMutationStates.ContainsKey(piece))
            {
                // 모든 상태의 추적된 Rule 정리
                foreach (var state in pieceMutationStates[piece])
                {
                    state.ClearTrackedRules();
                    state.ClearData();
                }
                pieceMutationStates.Remove(piece);
            }
        }

        #endregion

        #region Mutation Application

        /// <summary>
        /// 기물에 변이를 적용하고 추적합니다.
        /// </summary>
        public void ApplyMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null)
            {
                Debug.LogWarning("[MutationManager] Cannot apply null mutation or to null piece");
                return;
            }

            RegisterPiece(piece);

            // 이미 적용된 Mutation인지 확인
            var existingState = GetMutationState(piece, mutation);
            if (existingState != null)
            {
                Debug.LogWarning($"[MutationManager] {mutation.MutationName} is already applied to {piece.name}");
                return;
            }

            // 새 MutationState 생성
            var state = new MutationState(mutation);
            pieceMutationStates[piece].Add(state);

            // Mutation 적용
            mutation.ApplyToPiece(piece);
            Debug.Log($"{piece.name}에 변이 적용: {mutation.MutationName}");

            // 이벤트 발생
            OnMutationApplied?.Invoke(piece, mutation);
        }

        /// <summary>
        /// 기물에서 변이를 제거합니다.
        /// </summary>
        public void RemoveMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null) return;

            if (!pieceMutationStates.ContainsKey(piece)) return;

            // MutationState 찾기
            var state = GetMutationState(piece, mutation);
            if (state == null) return;

            // Mutation 제거
            mutation.RemoveFromPiece(piece);
            Debug.Log($"{piece.name}에서 변이 제거: {mutation.MutationName}");

            // 상태 정리
            state.ClearTrackedRules();
            state.ClearData();
            pieceMutationStates[piece].Remove(state);

            // 이벤트 발생
            OnMutationRemoved?.Invoke(piece, mutation);
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 기물의 변이 목록을 가져옵니다.
        /// </summary>
        public List<Mutation> GetMutations(Piece piece)
        {
            if (piece != null && pieceMutationStates.ContainsKey(piece))
            {
                var mutations = new List<Mutation>();
                foreach (var state in pieceMutationStates[piece])
                {
                    mutations.Add(state.Mutation);
                }
                return mutations;
            }
            return new List<Mutation>();
        }

        /// <summary>
        /// 특정 Piece의 특정 Mutation에 대한 MutationState를 가져옵니다
        /// </summary>
        /// <param name="piece">Piece 인스턴스</param>
        /// <param name="mutation">Mutation 인스턴스</param>
        /// <returns>MutationState 또는 null</returns>
        public MutationState GetMutationState(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null) return null;

            if (!pieceMutationStates.ContainsKey(piece)) return null;

            foreach (var state in pieceMutationStates[piece])
            {
                if (state.Mutation == mutation)
                {
                    return state;
                }
            }

            return null;
        }

        /// <summary>
        /// 특정 Piece의 모든 MutationState 목록을 가져옵니다
        /// </summary>
        /// <param name="piece">Piece 인스턴스</param>
        /// <returns>MutationState 리스트</returns>
        public List<MutationState> GetMutationStatesForPiece(Piece piece)
        {
            if (piece == null) return new List<MutationState>();

            if (!pieceMutationStates.ContainsKey(piece))
                return new List<MutationState>();

            return new List<MutationState>(pieceMutationStates[piece]);
        }

        /// <summary>
        /// 기물에 변이가 있는지 확인합니다.
        /// </summary>
        public bool HasMutations(Piece piece)
        {
            return piece != null && pieceMutationStates.ContainsKey(piece) && pieceMutationStates[piece].Count > 0;
        }

        /// <summary>
        /// 기물에 특정 변이가 있는지 확인합니다.
        /// </summary>
        public bool HasMutation(Piece piece, Mutation mutation)
        {
            return GetMutationState(piece, mutation) != null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 기물의 모든 변이를 제거합니다.
        /// </summary>
        public void ClearMutations(Piece piece)
        {
            if (piece == null || !pieceMutationStates.ContainsKey(piece)) return;

            var states = new List<MutationState>(pieceMutationStates[piece]);
            foreach (var state in states)
            {
                RemoveMutation(piece, state.Mutation);
            }
        }

        #endregion

        #region Notification Methods

        /// <summary>
        /// 기물 이동 시 변이에 알립니다.
        /// </summary>
        public void NotifyMove(Piece piece, Vector2Int from, Vector2Int to, Board board)
        {
            if (piece == null || board == null || !pieceMutationStates.ContainsKey(piece)) return;

            foreach (var state in pieceMutationStates[piece])
            {
                if (state?.Mutation != null)
                {
                    state.Mutation.OnMove(piece, from, to, board);
                }
            }
        }

        /// <summary>
        /// 기물 잡기 시 변이에 알립니다.
        /// </summary>
        public void NotifyCapture(Piece attacker, Piece captured, Vector2Int from, Vector2Int to, Board board)
        {
            if (attacker == null || captured == null || board == null) return;

            if (!pieceMutationStates.ContainsKey(attacker)) return;

            foreach (var state in pieceMutationStates[attacker])
            {
                if (state?.Mutation != null)
                {
                    state.Mutation.OnCapture(attacker, captured, from, to, board);
                }
            }
        }

        /// <summary>
        /// 모든 변이 데이터를 지웁니다.
        /// </summary>
        public void ClearAll()
        {
            // 모든 상태 정리
            foreach (var kvp in pieceMutationStates)
            {
                foreach (var state in kvp.Value)
                {
                    state.ClearTrackedRules();
                    state.ClearData();
                }
            }

            pieceMutationStates.Clear();
            Debug.Log("모든 변이 데이터 지움");
        }

        #endregion
    }
}
