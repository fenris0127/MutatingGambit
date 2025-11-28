using System;
using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Factory for creating and caching movement rules.
    /// Prevents memory leaks from repeatedly creating ScriptableObject instances.
    /// </summary>
    public class MovementRuleFactory : MonoBehaviour
    {
        private static MovementRuleFactory instance;

        // Cache for rule instances
        private readonly Dictionary<Type, MovementRule> ruleCache = new Dictionary<Type, MovementRule>();

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static MovementRuleFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("MovementRuleFactory");
                    instance = go.AddComponent<MovementRuleFactory>();
                    DontDestroyOnLoad(go);
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
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Gets a cached instance of a movement rule type.
        /// Creates it if it doesn't exist yet.
        /// </summary>
        public T GetRule<T>() where T : MovementRule
        {
            var type = typeof(T);
            
            if (!ruleCache.ContainsKey(type))
            {
                var rule = ScriptableObject.CreateInstance<T>();
                ruleCache[type] = rule;
            }

            return ruleCache[type] as T;
        }

        /// <summary>
        /// Gets a StraightLineRule instance.
        /// </summary>
        public StraightLineRule GetStraightLineRule()
        {
            return GetRule<StraightLineRule>();
        }

        /// <summary>
        /// Gets a DiagonalRule instance.
        /// </summary>
        public DiagonalRule GetDiagonalRule()
        {
            return GetRule<DiagonalRule>();
        }

        /// <summary>
        /// Gets the movement rules for a Queen (straight + diagonal).
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
        /// Gets a BackwardPawnRule instance.
        /// </summary>
        public BackwardPawnRule GetBackwardPawnRule()
        {
            return GetRule<BackwardPawnRule>();
        }

        /// <summary>
        /// Clears the rule cache (use when needed for memory management).
        /// </summary>
        public void ClearCache()
        {
            foreach (var rule in ruleCache.Values)
            {
                if (rule != null)
                {
                    Destroy(rule);
                }
            }
            ruleCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
