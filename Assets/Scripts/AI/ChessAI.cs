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
        #region 설정 및 기본 속성
        [Header("AI Configuration")]
        [SerializeField] private AIConfig config;
        [SerializeField] private Team aiTeam;
        #endregion

        #region 평가자 및 검색 컴포넌트
        private StateEvaluator stateEvaluator;
        private MoveEvaluator moveEvaluator;
        private MinimaxSearch minimaxSearch;
        private MoveSelector moveSelector;
        private System.Random random;
        #endregion

        #region 공개 속성
        /// <summary>AI 팀을 가져옵니다.</summary>
        public Team AITeam => aiTeam;
        
        /// <summary>AI 설정을 가져옵니다.</summary>
        public AIConfig Config => config;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// AI를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            if (config != null && stateEvaluator == null)
            {
                Initialize(config, aiTeam);
            }
        }
        #endregion

        #region 공개 메서드 - 초기화
        /// <summary>
        /// AI를 초기화합니다.
        /// </summary>
        /// <param name="aiConfig">AI 설정</param>
        /// <param name="team">AI 팀</param>
        /// <param name="seed">난수 시드 (0이면 랜덤)</param>
        public void Initialize(AIConfig aiConfig, Team team, int seed = 0)
        {
            config = aiConfig;
            aiTeam = team;
            InitializeRandom(seed);
            ConfigureEvaluators();
            LogInitialization();
        }
        #endregion

        #region 공개 메서드 - 수 선택
        /// <summary>
        /// AI가 수를 선택합니다.
        /// </summary>
        /// <param name="board">현재 보드</param>
        /// <returns>선택된 수</returns>
        public Move MakeMove(Board board)
        {
            if (!ValidateBoard(board))
            {
                return new Move();
            }

            LogThinking();
            minimaxSearch.ResetCounters();

            Move bestMove = SelectBestMove(board);

            if (bestMove.MovingPiece == null)
            {
                LogNoValidMove();
                return new Move();
            }

            LogSelectedMove(bestMove);
            return bestMove;
        }
        #endregion

        #region 공개 메서드 - 평가
        /// <summary>
        /// 테스트용: 주어진 보드 상태를 평가합니다.
        /// </summary>
        /// <param name="board">평가할 보드</param>
        /// <returns>보드 평가 점수</returns>
        public float EvaluatePosition(Board board)
        {
            if (stateEvaluator == null)
            {
                Debug.LogError("StateEvaluator가 초기화되지 않았습니다!");
                return 0f;
            }

            return stateEvaluator.EvaluateBoard(board);
        }
        #endregion

        #region 공개 메서드 - 설정 업데이트
        /// <summary>
        /// AI 설정을 업데이트합니다.
        /// </summary>
        /// <param name="newConfig">새 설정</param>
        public void UpdateConfig(AIConfig newConfig)
        {
            config = newConfig;
            ConfigureEvaluators();
            LogConfigUpdate();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 난수 생성기를 초기화합니다.
        /// </summary>
        private void InitializeRandom(int seed)
        {
            random = seed == 0 ? new System.Random() : new System.Random(seed);
        }

        /// <summary>
        /// 평가자 및 검색 컴포넌트를 구성합니다.
        /// </summary>
        private void ConfigureEvaluators()
        {
            stateEvaluator = new StateEvaluator(config, aiTeam, random);
            moveEvaluator = new MoveEvaluator(config, stateEvaluator, aiTeam);
            minimaxSearch = new MinimaxSearch(config, aiTeam, stateEvaluator, random);
            moveSelector = new MoveSelector(config, random);
        }

        /// <summary>
        /// 초기화 완료를 로그에 출력합니다.
        /// </summary>
        private void LogInitialization()
        {
            Debug.Log($"{aiTeam} AI 초기화 완료 (깊이: {config.SearchDepth})");
        }

        /// <summary>
        /// 설정 업데이트를 로그에 출력합니다.
        /// </summary>
        private void LogConfigUpdate()
        {
            Debug.Log($"AI 설정 업데이트됨: {config.ConfigName}");
        }
        #endregion

        #region 비공개 메서드 - 수 선택
        /// <summary>
        /// 보드의 유효성을 검증합니다.
        /// </summary>
        private bool ValidateBoard(Board board)
        {
            if (board == null)
            {
                Debug.LogError("보드가 null입니다!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 최선의 수를 선택합니다.
        /// </summary>
        private Move SelectBestMove(Board board)
        {
            if (config.UseIterativeDeepening)
            {
                return minimaxSearch.IterativeDeepeningSearch(board);
            }
            else
            {
                return minimaxSearch.DepthLimitedSearch(board, config.SearchDepth);
            }
        }

        /// <summary>
        /// AI 사고 중임을 로그에 출력합니다.
        /// </summary>
        private void LogThinking()
        {
            Debug.Log($"{aiTeam} AI 사고 중...");
        }

        /// <summary>
        /// 유효한 수를 찾지 못했음을 로그에 출력합니다.
        /// </summary>
        private void LogNoValidMove()
        {
            Debug.LogWarning("AI가 유효한 수를 찾지 못했습니다!");
        }

        /// <summary>
        /// 선택된 수를 로그에 출력합니다.
        /// </summary>
        private void LogSelectedMove(Move bestMove)
        {
            Debug.Log($"AI 선택: {bestMove.From} → {bestMove.To} (점수: {bestMove.Score:F2}, 노드: {minimaxSearch.NodesEvaluated})");
        }
        #endregion
    }
}
