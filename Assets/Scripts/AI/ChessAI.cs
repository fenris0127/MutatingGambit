using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.AI.Search;

namespace MutatingGambit.AI
{
    /// <summary>
    /// 미니맥스 알고리즘과 알파-베타 가지치기를 사용하는 체스 AI.
    /// 동적 규칙 변이에 적응합니다.
    /// </summary>
    public class ChessAI : MonoBehaviour
    {
        [Header("AI Configuration")]
        [SerializeField] private AIConfig config;
        [SerializeField] private Team aiTeam;

        private StateEvaluator stateEvaluator;
        private MoveEvaluator moveEvaluator;
        private MinimaxSearch minimaxSearch;
        private MoveSelector moveSelector;
        private System.Random random;

        public Team AITeam => aiTeam;
        public AIConfig Config => config;

        private void Awake()
        {
            if (config != null && stateEvaluator == null)
            {
                Initialize(config, aiTeam);
            }
        }

        /// <summary>
        /// AI를 초기화합니다.
        /// </summary>
        public void Initialize(AIConfig aiConfig, Team team, int seed = 0)
        {
            config = aiConfig;
            aiTeam = team;
            random = seed == 0 ? new System.Random() : new System.Random(seed);

            stateEvaluator = new StateEvaluator(config, team, seed);
            moveEvaluator = new MoveEvaluator(stateEvaluator, config);
            minimaxSearch = new MinimaxSearch(config, team, stateEvaluator, random);
            moveSelector = new MoveSelector(config, random);

            Debug.Log($"{team} AI 초기화 완료 (깊이: {config.SearchDepth})");
        }

        /// <summary>
        /// AI가 수를 선택합니다.
        /// </summary>
        public Move MakeMove(Board board)
        {
            if (board == null)
            {
                Debug.LogError("보드가 null입니다!");
                return new Move();
            }

            Debug.Log($"{aiTeam} AI 사고 중...");
            minimaxSearch.ResetCounters();

            Move bestMove;

            if (config.UseIterativeDeepening)
            {
                bestMove = minimaxSearch.IterativeDeepeningSearch(board);
            }
            else
            {
                bestMove = minimaxSearch.DepthLimitedSearch(board, config.SearchDepth);
            }

            if (bestMove.MovingPiece == null)
            {
                Debug.LogWarning("AI가 유효한 수를 찾지 못했습니다!");
                return new Move();
            }

            Debug.Log($"AI 선택: {bestMove.From} → {bestMove.To} (점수: {bestMove.Score:F2}, 노드: {minimaxSearch.NodesEvaluated})");

            return bestMove;
        }

        /// <summary>
        /// 테스트용: 주어진 보드 상태를 평가합니다.
        /// </summary>
        public float EvaluatePosition(Board board)
        {
            if (stateEvaluator == null)
            {
                Debug.LogError("StateEvaluator가 초기화되지 않았습니다!");
                return 0f;
            }

            return stateEvaluator.EvaluateBoard(board);
        }

        /// <summary>
        /// AI 설정을 업데이트합니다.
        /// </summary>
        public void UpdateConfig(AIConfig newConfig)
        {
            config = newConfig;
            
            if (stateEvaluator != null)
            {
                stateEvaluator = new StateEvaluator(config, aiTeam);
            }
            
            if (moveEvaluator != null)
            {
                moveEvaluator = new MoveEvaluator(stateEvaluator, config);
            }

            if (minimaxSearch != null)
            {
                minimaxSearch = new MinimaxSearch(config, aiTeam, stateEvaluator, random);
            }

            if (moveSelector != null)
            {
                moveSelector = new MoveSelector(config, random);
            }

            Debug.Log($"AI 설정 업데이트됨: {config.ConfigName}");
        }
    }
}
