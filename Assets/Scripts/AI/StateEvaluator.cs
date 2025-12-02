using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.AI.Evaluation;

namespace MutatingGambit.AI
{
    /// <summary>
    /// 보드 상태를 평가하고 점수를 할당합니다.
    /// AI에게높은 점수가 더 좋습니다.
    /// </summary>
    public class StateEvaluator
    {
        #region 필드
        private AIConfig config;
        private Team aiTeam;
        private System.Random random;
        #endregion

        #region 평가자 컴포넌트
        private MaterialEvaluator materialEvaluator;
        private PositionalEvaluator positionalEvaluator;
        private KingSafetyEvaluator kingSafetyEvaluator;
        private MobilityEvaluator mobilityEvaluator;
        #endregion

        #region 생성자
        /// <summary>
        /// StateEvaluator를 초기화합니다.
        /// </summary>
        /// <param name="aiConfig">AI 설정</param>
        /// <param name="team">평가할 팀</param>
        /// <param name="rng">난수 생성기 (null이면 새로 생성)</param>
        public StateEvaluator(AIConfig aiConfig, Team team, System.Random rng = null)
        {
            config = aiConfig;
            aiTeam = team;
            random = rng ?? new System.Random();
            InitializeEvaluators();
        }
        #endregion

        #region 공개 메서드 - Board 평가
        /// <summary>
        /// 현재 보드 상태를 평가합니다.
        /// 양수 점수 = AI에게 유리, 음수 = 상대에게 유리.
        /// </summary>
        /// <param name="board">평가할 보드</param>
        /// <returns>보드 평가 점수</returns>
        public float EvaluateBoard(Board board)
        {
            if (board == null) return 0f;

            float materialScore = materialEvaluator.EvaluateMaterial(board) * config.MaterialWeight;
            float positionalScore = positionalEvaluator.EvaluatePosition(board) * config.PositionalWeight;
            float kingSafetyScore = kingSafetyEvaluator.EvaluateKingSafety(board) * config.KingSafetyWeight;
            float mobilityScore = mobilityEvaluator.EvaluateMobility(board) * config.MobilityWeight;

            return CalculateFinalScore(materialScore, positionalScore, kingSafetyScore, mobilityScore);
        }
        #endregion

        #region 공개 메서드 - BoardState 평가
        /// <summary>
        /// 경량 BoardState를 평가합니다 (AI 시뮬레이션에 최적화됨).
        /// 양수 점수 = AI에게 유리, 음수 = 상대에게 유리.
        /// </summary>
        /// <param name="state">평가할 보드 상태</param>
        /// <returns>보드 상태 평가 점수</returns>
        public float EvaluateBoardState(BoardState state)
        {
            if (state == null) return 0f;

            float materialScore = materialEvaluator.EvaluateMaterialState(state) * config.MaterialWeight;
            float positionalScore = positionalEvaluator.EvaluatePositionState(state) * config.PositionalWeight;
            float kingSafetyScore = kingSafetyEvaluator.EvaluateKingSafetyState(state) * config.KingSafetyWeight;
            float mobilityScore = mobilityEvaluator.EvaluateMobilityState(state) * config.MobilityWeight;

            return CalculateFinalScore(materialScore, positionalScore, kingSafetyScore, mobilityScore);
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 모든 평가자를 초기화합니다.
        /// </summary>
        private void InitializeEvaluators()
        {
            materialEvaluator = new MaterialEvaluator(config, aiTeam);
            positionalEvaluator = new PositionalEvaluator(aiTeam);
            kingSafetyEvaluator = new KingSafetyEvaluator(aiTeam);
            mobilityEvaluator = new MobilityEvaluator(aiTeam);
        }
        #endregion

        #region 비공개 메서드 - 점수 계산
        /// <summary>
        /// 모든 평가 점수를 합산하고 무작위성을 추가합니다.
        /// </summary>
        private float CalculateFinalScore(float material, float positional, float kingSafety, float mobility)
        {
            float baseScore = material + positional + kingSafety + mobility;
            return AddRandomness(baseScore);
        }

        /// <summary>
        /// 결정론적 플레이를 방지하기 위해 무작위성을 추가합니다.
        /// </summary>
        private float AddRandomness(float score)
        {
            if (config.RandomnessFactor > 0)
            {
                float randomness = ((float)random.NextDouble() - 0.5f) * 2f * config.RandomnessFactor;
                return score + randomness;
            }
            return score;
        }
        #endregion
    }
}
