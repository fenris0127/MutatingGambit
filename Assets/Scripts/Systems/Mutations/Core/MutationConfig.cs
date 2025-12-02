using UnityEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// 전역 Mutation 설정을 관리하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MutationConfig", menuName = "MutatingGambit/Config/Mutation Config")]
    public class MutationConfig : ScriptableObject
    {
        [Header("드롭 확률")]
        [Tooltip("일반 등급 변이의 드롭 확률")]
        [Range(0f, 1f)]
        public float commonDropRate = 0.5f;

        [Tooltip("희귀 등급 변이의 드롭 확률")]
        [Range(0f, 1f)]
        public float rareDropRate = 0.3f;

        [Tooltip("영웅 등급 변이의 드롭 확률")]
        [Range(0f, 1f)]
        public float epicDropRate = 0.15f;

        [Tooltip("전설 등급 변이의 드롭 확률")]
        [Range(0f, 1f)]
        public float legendaryDropRate = 0.05f;

        [Header("제한")]
        [Tooltip("기물 하나당 최대 변이 개수")]
        public int maxMutationsPerPiece = 3;

        [Tooltip("동일한 변이를 중복으로 적용할 수 있는지 여부")]
        public bool allowDuplicateMutations = false;

        [Header("사용 가능한 Mutation 목록")]
        [Tooltip("게임에서 사용 가능한 모든 변이 목록")]
        public List<Mutation> availableMutations = new List<Mutation>();

        [Header("비용 설정")]
        [Tooltip("등급별 기본 비용 배율")]
        public float commonCostMultiplier = 1.0f;
        public float rareCostMultiplier = 2.0f;
        public float epicCostMultiplier = 3.5f;
        public float legendaryCostMultiplier = 5.0f;

        /// <summary>
        /// 드롭 확률의 합이 1.0인지 검증합니다
        /// </summary>
        private void OnValidate()
        {
            float totalRate = commonDropRate + rareDropRate + epicDropRate + legendaryDropRate;
            if (Mathf.Abs(totalRate - 1.0f) > 0.01f)
            {
                Debug.LogWarning($"[MutationConfig] 드롭 확률의 합이 1.0이 아닙니다: {totalRate:F2}");
            }

            if (maxMutationsPerPiece < 1)
            {
                Debug.LogWarning("[MutationConfig] maxMutationsPerPiece는 최소 1 이상이어야 합니다");
                maxMutationsPerPiece = 1;
            }
        }

        /// <summary>
        /// 등급에 따른 드롭 확률을 반환합니다
        /// </summary>
        public float GetDropRate(MutationRarity rarity)
        {
            switch (rarity)
            {
                case MutationRarity.Common:
                    return commonDropRate;
                case MutationRarity.Rare:
                    return rareDropRate;
                case MutationRarity.Epic:
                    return epicDropRate;
                case MutationRarity.Legendary:
                    return legendaryDropRate;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 등급에 따른 비용 배율을 반환합니다
        /// </summary>
        public float GetCostMultiplier(MutationRarity rarity)
        {
            switch (rarity)
            {
                case MutationRarity.Common:
                    return commonCostMultiplier;
                case MutationRarity.Rare:
                    return rareCostMultiplier;
                case MutationRarity.Epic:
                    return epicCostMultiplier;
                case MutationRarity.Legendary:
                    return legendaryCostMultiplier;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// 특정 등급의 사용 가능한 변이 목록을 반환합니다
        /// </summary>
        public List<Mutation> GetMutationsByRarity(MutationRarity rarity)
        {
            List<Mutation> filtered = new List<Mutation>();
            foreach (var mutation in availableMutations)
            {
                if (mutation != null && mutation.Rarity == rarity)
                {
                    filtered.Add(mutation);
                }
            }
            return filtered;
        }

        /// <summary>
        /// 특정 기물 타입과 호환되는 변이 목록을 반환합니다
        /// </summary>
        public List<Mutation> GetCompatibleMutations(Core.ChessEngine.PieceType pieceType)
        {
            List<Mutation> compatible = new List<Mutation>();
            foreach (var mutation in availableMutations)
            {
                if (mutation != null && mutation.IsCompatibleWith(pieceType))
                {
                    compatible.Add(mutation);
                }
            }
            return compatible;
        }

        /// <summary>
        /// 랜덤한 등급을 드롭 확률에 따라 선택합니다
        /// </summary>
        public MutationRarity GetRandomRarity()
        {
            float roll = Random.value;
            float cumulative = 0f;

            cumulative += commonDropRate;
            if (roll < cumulative) return MutationRarity.Common;

            cumulative += rareDropRate;
            if (roll < cumulative) return MutationRarity.Rare;

            cumulative += epicDropRate;
            if (roll < cumulative) return MutationRarity.Epic;

            return MutationRarity.Legendary;
        }
    }
}
