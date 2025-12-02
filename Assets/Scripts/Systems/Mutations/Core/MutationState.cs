using System.Collections.Generic;
using MutatingGambit.Core.MovementRules;
using UnityEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Piece별 Mutation 상태를 관리하는 클래스
    /// ScriptableObject에서 상태를 분리하여 인스턴스별 독립적으로 관리합니다
    /// </summary>
    /// <remarks>
    /// ScriptableObject는 에셋이므로 모든 인스턴스가 공유됩니다.
    /// Mutation의 상태를 저장하려면 이 클래스를 사용하여 Piece별로 분리해야 합니다.
    ///
    /// 사용 예시:
    /// <code>
    /// var state = MutationManager.Instance.GetMutationState(piece, mutation);
    /// state.SetData("moveCount", 5);
    /// int count = state.GetData("moveCount", 0);
    /// </code>
    /// </remarks>
    public class MutationState
    {
        /// <summary>
        /// 이 상태가 관리하는 Mutation
        /// </summary>
        public Mutation Mutation { get; private set; }

        /// <summary>
        /// 상태 데이터 딕셔너리 (키-값 쌍)
        /// </summary>
        public Dictionary<string, object> Data { get; private set; }

        /// <summary>
        /// 이 Mutation이 추가한 MovementRule 목록
        /// 제거 시 자동으로 정리하기 위해 추적합니다
        /// </summary>
        public List<MovementRule> AddedRules { get; private set; }

        /// <summary>
        /// MutationState 생성자
        /// </summary>
        /// <param name="mutation">관리할 Mutation</param>
        public MutationState(Mutation mutation)
        {
            Mutation = mutation;
            Data = new Dictionary<string, object>();
            AddedRules = new List<MovementRule>();
        }

        /// <summary>
        /// 데이터를 가져옵니다. 존재하지 않으면 기본값을 반환합니다
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="key">데이터 키</param>
        /// <param name="defaultValue">기본값</param>
        /// <returns>저장된 값 또는 기본값</returns>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (Data.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        /// <summary>
        /// 데이터를 설정합니다
        /// </summary>
        /// <typeparam name="T">데이터 타입</typeparam>
        /// <param name="key">데이터 키</param>
        /// <param name="value">저장할 값</param>
        public void SetData<T>(string key, T value)
        {
            Data[key] = value;
        }

        /// <summary>
        /// 특정 키의 데이터가 존재하는지 확인합니다
        /// </summary>
        /// <param name="key">확인할 키</param>
        /// <returns>존재 여부</returns>
        public bool HasData(string key)
        {
            return Data.ContainsKey(key);
        }

        /// <summary>
        /// 특정 키의 데이터를 제거합니다
        /// </summary>
        /// <param name="key">제거할 키</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveData(string key)
        {
            return Data.Remove(key);
        }

        /// <summary>
        /// 모든 데이터를 초기화합니다
        /// </summary>
        public void ClearData()
        {
            Data.Clear();
        }

        /// <summary>
        /// MovementRule을 추적 목록에 추가합니다
        /// </summary>
        /// <param name="rule">추적할 Rule</param>
        public void TrackRule(MovementRule rule)
        {
            if (rule != null && !AddedRules.Contains(rule))
            {
                AddedRules.Add(rule);
            }
        }

        /// <summary>
        /// 추적 중인 모든 MovementRule을 제거합니다
        /// </summary>
        public void ClearTrackedRules()
        {
            AddedRules.Clear();
        }

        /// <summary>
        /// 디버깅용 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"MutationState[{Mutation?.MutationName}] - {Data.Count} data entries, {AddedRules.Count} rules";
        }
    }
}
