using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.AI.Evaluation;

namespace MutatingGambit.AI
{
    /// <summary>
    /// 보드 상태를 평가하고 점수를 할당합니다.
    /// AI에게 높은 점수가 더 좋습니다.
    /// </summary>
    public class StateEvaluator
    {
        private AIConfig config;
        private Team aiTeam;
        private System.Random random;

        // 평가 전략 컴포넌트
        private MaterialEvaluator materialEvaluator;
        private PositionalEvaluator positionalEvaluator;
        private KingSafetyEvaluator kingSafetyEvaluator;
        private MobilityEvaluator mobilityEvaluator;

        public StateEvaluator(AIConfig aiConfig, Team team, int seed = 0)
        {
            config = aiConfig;
            aiTeam = team;
            random = seed == 0 ? new System.Random() : new System.Random(seed);

            // 평가자 초기화
            materialEvaluator = new MaterialEvaluator(config, team);
            positionalEvaluator = new PositionalEvaluator(team);
            kingSafetyEvaluator = new KingSafetyEvaluator(team);
            mobilityEvaluator = new MobilityEvaluator(team);
        }

        /// <summary>
        /// 현재 보드 상태를 평가합니다.
        /// 양수 점수 = AI에게 유리, 음수 = 상대에게 유리.
        /// </summary>
        public float EvaluateBoard(Board board)
        {
            if (board == null)
            {
                return 0f;
            }

            float score = 0f;

            // 물질 평가
            score += materialEvaluator.EvaluateMaterial(board) * config.MaterialWeight;

            // 위치 평가
            score += positionalEvaluator.EvaluatePosition(board) * config.PositionalWeight;

            // 킹 안전
            score += kingSafetyEvaluator.EvaluateKingSafety(board) * config.KingSafetyWeight;

            // 기동성
            score += mobilityEvaluator.EvaluateMobility(board) * config.MobilityWeight;

            // 무작위성 추가하여 결정론적 플레이 방지
            if (config.RandomnessFactor > 0)
            {
                float randomness = ((float)random.NextDouble() - 0.5f) * 2f * config.RandomnessFactor;
                score += randomness;
            }

            return score;
        }

        /// <summary>
        /// 경량 BoardState를 평가합니다 (AI 시뮬레이션에 최적화됨).
        /// 양수 점수 = AI에게 유리, 음수 = 상대에게 유리.
        /// </summary>
        public float EvaluateBoardState(BoardState state)
        {
            if (state == null)
            {
                return 0f;
            }

            float score = 0f;

            // 물질 평가
            score += materialEvaluator.EvaluateMaterialState(state) * config.MaterialWeight;

            // 위치 평가
            score += positionalEvaluator.EvaluatePositionState(state) * config.PositionalWeight;

            // 킹 안전
            score += kingSafetyEvaluator.Evalu ateKingSafetyState(state) * config.KingSafetyWeight;

            // 기동성
            score += mobilityEvaluator.EvaluateMobilityState(state) * config.MobilityWeight;

            // 무작위성 추가
            if (config.RandomnessFactor > 0)
            {
                float randomness = ((float)random.NextDouble() - 0.5f) * 2f * config.RandomnessFactor;
                score += randomness;
            }

            return score;
        }
    }
}
