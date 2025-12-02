using System;
using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 움직임 규칙을 생성하고 캐싱하는 팩토리입니다.
    /// ScriptableObject 인스턴스를 반복적으로 생성하여 발생하는 메모리 누수를 방지합니다.
    /// </summary>
    public class MovementRuleFactory : MonoBehaviour
    {
        #region 싱글톤
        private static MovementRuleFactory instance;

        /// <summary>
        /// 싱글톤 인스턴스를 가져옵니다.
        /// </summary>
        public static MovementRuleFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    CreateInstance();
                }
                return instance;
            }
        }
        #endregion

        #region 캐시
        private readonly Dictionary<Type, MovementRule> ruleCache = new Dictionary<Type, MovementRule>();
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 싱글톤을 설정합니다.
        /// </summary>
        private void Awake()
        {
            if (!EnsureSingleInstance())
            {
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 캐시를 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            ClearCache();
            if (instance == this)
            {
                instance = null;
            }
        }
        #endregion

        #region 공개 메서드 - 규칙 가져오기
        /// <summary>
        /// 움직임 규칙 타입의 캐시된 인스턴스를 가져옵니다.
        /// 존재하지 않으면 생성합니다.
        /// </summary>
        public T GetRule<T>() where T : MovementRule
        {
            var type = typeof(T);
            
            if (!ruleCache.ContainsKey(type))
            {
                CreateAndCacheRule<T>(type);
            }

            return ruleCache[type] as T;
        }

        /// <summary>
        /// StraightLineRule 인스턴스를 가져옵니다.
        /// </summary>
        public StraightLineRule GetStraightLineRule()
        {
            return GetRule<StraightLineRule>();
        }

        /// <summary>
        /// DiagonalRule 인스턴스를 가져옵니다.
        /// </summary>
        public DiagonalRule GetDiagonalRule()
        {
            return GetRule<DiagonalRule>();
        }

        /// <summary>
        /// Queen의 움직임 규칙들을 가져옵니다 (직선 + 대각선).
        /// </summary>
        public MovementRule[] GetQueenRules()
        {
            return new MovementRule[]
            {
                GetStraightLineRule(),
                GetDiagonalRule()
            };
        }

        /// <summary>
        /// BackwardPawnRule 인스턴스를 가져옵니다.
        /// </summary>
        public BackwardPawnRule GetBackwardPawnRule()
        {
            return GetRule<BackwardPawnRule>();
        }
        #endregion

        #region 공개 메서드 - 캐시 관리
        /// <summary>
        /// 규칙 캐시를 정리합니다 (메모리 관리가 필요할 때 사용).
        /// </summary>
        public void ClearCache()
        {
            DestroyAllCachedRules();
            ruleCache.Clear();
        }
        #endregion

        #region 비공개 메서드 - 인스턴스 생성
        /// <summary>
        /// 팩토리 인스턴스를 생성합니다.
        /// </summary>
        private static void CreateInstance()
        {
            var go = new GameObject("MovementRuleFactory");
            instance = go.AddComponent<MovementRuleFactory>();
            DontDestroyOnLoad(go);
        }

        /// <summary>
        /// 싱글톤 인스턴스가 유일한지 확인합니다.
        /// </summary>
        private bool EnsureSingleInstance()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return false;
            }
            return true;
        }
        #endregion

        #region 비공개 메서드 - 규칙 생성 및 캐싱
        /// <summary>
        /// 규칙을 생성하고 캐시에 저장합니다.
        /// </summary>
        private void CreateAndCacheRule<T>(Type type) where T : MovementRule
        {
            var rule = ScriptableObject.CreateInstance<T>();
            ruleCache[type] = rule;
        }

        /// <summary>
        /// 캐시된 모든 규칙을 파괴합니다.
        /// </summary>
        private void DestroyAllCachedRules()
        {
            foreach (var rule in ruleCache.Values)
            {
                if (rule != null)
                {
                    Destroy(rule);
                }
            }
        }
        #endregion
    }
}
