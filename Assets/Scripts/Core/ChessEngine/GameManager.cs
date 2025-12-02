using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.AI;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.UI;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 전체 게임 흐름을 제어하는 메인 게임 관리자.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region 싱글톤
        private static GameManager instance;

        /// <summary>
        /// GameManager 싱글톤 인스턴스입니다.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameManager>();
                }
                return instance;
            }
        }
        #endregion

        #region 핵심 참조
        [Header("Core References")]
        [SerializeField] private Board board;
        [SerializeField] private ChessAI aiPlayer;
        #endregion

        #region 컴포넌트 참조
        [Header("Components")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private VictoryConditionChecker victoryChecker;
        #endregion

        #region UI 참조
        [Header("UI References")]
        [SerializeField] private TurnIndicator turnIndicator;
        #endregion

        #region 게임 상태
        [Header("Game State")]
        [SerializeField] private Team playerTeam = Team.White;
        [SerializeField] private GameState state = GameState.NotStarted;
        private bool simulationMode = false;
        #endregion

        #region 헬퍼
        private MoveExecutionHandler moveHandler;
        #endregion

        #region 이벤트
        [Header("Events")]
        public UnityEvent OnVictory;
        public UnityEvent OnDefeat;
        #endregion

        #region 열거형
        public enum GameState
        {
            NotStarted,
            PlayerTurn,
            AITurn,
            GameOver,
            Victory,
            Defeat
        }
        #endregion

        #region 공개 속성
        /// <summary>현재 게임 상태를 가져옵니다.</summary>
        public GameState State => state;
        
        /// <summary>현재 턴을 가져옵니다.</summary>
        public Team CurrentTurn => turnManager != null ? turnManager.CurrentTurn : playerTeam;
        
        /// <summary>플레이어 팀을 가져옵니다.</summary>
        public Team PlayerTeam => playerTeam;
        
        /// <summary>현재 턴 번호를 가져옵니다.</summary>
        public int TurnNumber => turnManager != null ? turnManager.TurnNumber : 0;
        
        /// <summary>플레이어 턴인지 확인합니다.</summary>
        public bool IsPlayerTurn => turnManager != null && turnManager.IsPlayerTurn;
        
        /// <summary>시뮬레이션 모드 여부를 가져오거나 설정합니다.</summary>
        public bool SimulationMode { get => simulationMode; set => simulationMode = value; }
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 싱글톤 설정 및 초기화를 수행합니다.
        /// </summary>
        private void Awake()
        {
            if (!EnsureSingleInstance())
            {
                return;
            }

            instance = this;
            InitializeComponents();
            InitializeMoveHandler();
        }

        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        private void Start()
        {
            if (state == GameState.NotStarted)
            {
                StartGame();
            }
        }
        #endregion

        #region 공개 메서드 - 게임 제어
        /// <summary>
        /// 새 게임을 시작합니다.
        /// </summary>
        public void StartGame()
        {
            state = GameState.PlayerTurn;
            InitializeTurnManager();
            InitializeVictoryChecker();
            Debug.Log("게임이 시작되었습니다!");
        }

        /// <summary>
        /// 보드에서 수를 실행합니다.
        /// </summary>
        /// <param name="from">시작 위치</param>
        /// <param name="to">목표 위치</param>
        /// <returns>수 실행 성공 여부</returns>
        public bool ExecuteMove(Vector2Int from, Vector2Int to)
        {
            if (board == null) return false;

            var movingPiece = board.GetPiece(from);
            var capturedPiece = board.GetPiece(to);
            bool success = board.MovePiece(from, to);

            if (success)
            {
                bool gameOver = HandleMoveExecution(movingPiece, capturedPiece, from, to);
                if (!gameOver)
                {
                    EndTurnIfGameContinues();
                }
            }

            return success;
        }

        /// <summary>
        /// 승리를 트리거합니다.
        /// </summary>
        public void TriggerVictory()
        {
            state = GameState.Victory;
            OnVictory?.Invoke();
            Debug.Log("승리!");
        }

        /// <summary>
        /// 패배를 트리거합니다.
        /// </summary>
        public void TriggerDefeat()
        {
            state = GameState.Defeat;
            OnDefeat?.Invoke();
            Debug.Log("패배!");
        }

        /// <summary>
        /// 게임을 재시작합니다.
        /// </summary>
        public void RestartGame()
        {
            ResetGameState();
            ClearBoard();
            ResetTurnManager();
            StartGame();
        }
        #endregion

        #region 공개 메서드 - 설정
        /// <summary>
        /// 시뮬레이션 모드를 설정합니다.
        /// </summary>
        public void SetSimulationMode(bool value)
        {
            turnManager?.SetSimulationMode(value);
        }

        /// <summary>
        /// AI 플레이어를 설정합니다.
        /// </summary>
        public void SetAIPlayer(ChessAI ai)
        {
            aiPlayer = ai;
        }

        /// <summary>
        /// 플레이어 팀을 설정합니다.
        /// </summary>
        public void SetPlayerTeam(Team team)
        {
            playerTeam = team;
        }
        #endregion

        #region 비공개 메서드 - 초기화
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

        /// <summary>
        /// 필수 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            if (board == null) board = Board.Instance;
            if (turnIndicator == null) turnIndicator = FindFirstObjectByType<TurnIndicator>();
            if (turnManager == null) turnManager = GetComponent<TurnManager>();
            if (victoryChecker == null) victoryChecker = GetComponent<VictoryConditionChecker>();
        }

        /// <summary>
        /// MoveExecutionHandler를 초기화합니다.
        /// </summary>
        private void InitializeMoveHandler()
        {
            moveHandler = new MoveExecutionHandler(victoryChecker);
        }

        /// <summary>
        /// TurnManager를 초기화합니다.
        /// </summary>
        private void InitializeTurnManager()
        {
            if (turnManager != null)
            {
                turnManager.Initialize(playerTeam, aiPlayer);
                turnManager.OnTurnStart.AddListener(OnTurnStart);
                turnManager.OnTurnEnd.AddListener(OnTurnEnd);
                turnManager.StartTurn();
            }
        }

        /// <summary>
        /// VictoryChecker를 초기화합니다.
        /// </summary>
        private void InitializeVictoryChecker()
        {
            if (victoryChecker != null)
            {
                victoryChecker.Initialize(playerTeam);
            }
        }
        #endregion

        #region 비공개 메서드 - 수 실행
        /// <summary>
        /// 수 실행 후 처리를 수행합니다.
        /// </summary>
        private bool HandleMoveExecution(Piece movingPiece, Piece capturedPiece, Vector2Int from, Vector2Int to)
        {
            if (moveHandler != null)
            {
                return moveHandler.HandlePostMove(movingPiece, capturedPiece, from, to, board);
            }
            return false;
        }

        /// <summary>
        /// 게임이 계속되면 턴을 종료합니다.
        /// </summary>
        private void EndTurnIfGameContinues()
        {
            if (state != GameState.GameOver && state != GameState.Victory && state != GameState.Defeat)
            {
                turnManager?.EndTurn();
            }
        }
        #endregion

        #region 비공개 메서드 - 재시작
        /// <summary>
        /// 게임 상태를 리셋합니다.
        /// </summary>
        private void ResetGameState()
        {
            state = GameState.NotStarted;
        }

        /// <summary>
        /// 보드를 클리어합니다.
        /// </summary>
        private void ClearBoard()
        {
            if (board != null)
            {
                board.Clear();
            }
        }

        /// <summary>
        /// TurnManager를 리셋합니다.
        /// </summary>
        private void ResetTurnManager()
        {
            if (turnManager != null)
            {
                turnManager.Reset();
            }
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 턴 시작 이벤트를 처리합니다.
        /// </summary>
        private void OnTurnStart(Team team)
        {
            state = team == playerTeam ? GameState.PlayerTurn : GameState.AITurn;
            UpdateTurnIndicator(team);
        }

        /// <summary>
        /// 턴 종료 이벤트를 처리합니다.
        /// </summary>
        private void OnTurnEnd(Team team)
        {
            // 향후 확장 가능
        }

        /// <summary>
        /// 턴 인디케이터를 업데이트합니다.
        /// </summary>
        private void UpdateTurnIndicator(Team team)
        {
            if (turnIndicator != null)
            {
                turnIndicator.SetCurrentTeam(team);
            }
        }
        #endregion
    }
}
