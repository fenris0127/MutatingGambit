using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI.Search
{
    /// <summary>
    /// AI의 수 선택을 담당합니다.
    /// </summary>
    public class MoveSelector
    {
        private AIConfig config;
        private System.Random random;

        public MoveSelector(AIConfig aiConfig, System.Random rng)
        {
            config = aiConfig;
            random = rng;
        }

        /// <summary>
        /// 무작위 인자를 고려하여 최선의 수를 선택합니다.
        /// </summary>
        public Move SelectBestMove(List<Move> moves)
        {
            if (moves == null || moves.Count == 0)
            {
                return new Move();
            }

            // 무작위성 확인
            if (ShouldChooseRandomly())
            {
                int randomIndex = random.Next(moves.Count);
                Debug.Log($"무작위 수 선택: {moves[randomIndex].From} → {moves[randomIndex].To}");
                return moves[randomIndex];
            }

            // 최고 점수 찾기
            float bestScore = moves.Max(m => m.Score);

            // 비슷한 점수의 수들 중에서 선택
            var goodMoves = moves.Where(m => Mathf.Abs(m.Score - bestScore) < 0.1f).ToList();

            if (goodMoves.Count > 1)
            {
                int index = random.Next(goodMoves.Count);
                return goodMoves[index];
            }

            return moves.OrderByDescending(m => m.Score).First();
        }

        /// <summary>
        /// 무작위로 수를 선택해야 하는지 확인합니다.
        /// </summary>
        private bool ShouldChooseRandomly() => 
            config.RandomnessFactor > 0 && random.NextDouble() < config.RandomnessFactor;
    }
}
