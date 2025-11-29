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
        private static GameManager instance;

        [Header("Core References")]
        [SerializeField] private Board board;
        [SerializeField] private ChessAI aiPlayer;

        [Header("Components")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private VictoryConditionChecker victoryChecker;

        [Header("UI References")]
        [SerializeField] private TurnIndicator turnIndicator;

        [Header("Game State")]
        [SerializeField] private Team playerTeam = Team.White;
        [SerializeField] private GameState state = GameState.NotStarted;

        private bool simulationMode = false;

        [Header("Events")]
        public UnityEvent OnVictory;
        public UnityEvent OnDefeat;

        public enum GameState
        {
            NotStarted,
            PlayerTurn,
            AITurn,
            GameOver,
            Victory,
            Defeat
        }

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameManager.Instance;
                }
                return instance;
            }
        }

        public GameState State => state;
        public Team CurrentTurn => turnManager != null ? turnManager.CurrentTurn : playerTeam;
        public Team PlayerTeam => playerTeam;
        public int TurnNumber => turnManager != null ? turnManager.TurnNumber : 0;
        public bool IsPlayerTurn => turnManager != null && turnManager.IsPlayerTurn;
        public bool SimulationMode { get => simulationMode; set => simulationMode = value; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            InitializeComponents();
        }

        private void Start()
        {
            if (state == GameState.NotStarted)
            {
                StartGame();
            }
        }

        /// <summary>
        /// 새 게임을 시작합니다.
        /// </summary>
        public void StartGame()
        {
            state = GameState.PlayerTurn;

            if (turnManager != null)
            {
                turnManager.Initialize(playerTeam, aiPlayer);
                turnManager.OnTurnStart.AddListener(OnTurnStart);
                turnManager.OnTurnEnd.AddListener(OnTurnEnd);
                turnManager.StartTurn();
            }

            if (victoryChecker != null)
            {
                victoryChecker.Initialize(playerTeam);
            }

            Debug.Log("게임이 시작되었습니다!");
        }

        /// <summary>
        /// 보드에서 수를 실행합니다.
        /// </summary>
        public bool ExecuteMove(Vector2Int from, Vector2Int to)
        {
            if (board == null) return false;

            var movingPiece = board.GetPiece(from);
            var capturedPiece = board.GetPiece(to);

            bool success = board.MovePiece(from, to);

            if (success)
            {
                // 변이에 알림
                if (Systems.Mutations.MutationManager.Instance != null)
                {
                    Systems.Mutations.MutationManager.Instance.NotifyMove(movingPiece, from, to, board);
                    
                    if (capturedPiece != null)
                    {
                        Systems.Mutations.MutationManager.Instance.NotifyCapture(movingPiece, capturedPiece, from, to, board);
                    }
                }

                // 잡힌 기물 처리
                if (capturedPiece != null && victoryChecker != null)
                {
                    victoryChecker.HandlePieceCapture(capturedPiece);
                }

                // 승패 조건 확인
                if (victoryChecker != null)
                {
                    bool gameOver = victoryChecker.CheckGameConditions();
                    if (gameOver) return success;
                }

                // 게임이 끝나지 않았으면 턴 종료
                if (state != GameState.GameOver && state != GameState.Victory && state != GameState.Defeat)
                {
                    turnManager?.EndTurn();
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
            state = GameState.NotStarted;

            if (board != null)
            {
                board.Clear();
            }

            if (turnManager != null)
            {
                turnManager.Reset();
            }

            StartGame();
        }

        public void SetSimulationMode(bool value)
        {
            if (turnManager != null)
            {
                turnManager.SetSimulationMode(value);
            }
        }

        public void SetAIPlayer(ChessAI ai)
        {
            aiPlayer = ai;
        }

        public void SetPlayerTeam(Team team)
        {
            playerTeam = team;
        }

        private void InitializeComponents()
        {
            if (board == null) board = Board.Instance;
            if (turnIndicator == null) turnIndicator = FindObjectOfType<TurnIndicator>();
            if (turnManager == null) turnManager = GetComponent<TurnManager>();
            if (victoryChecker == null) victoryChecker = GetComponent<VictoryConditionChecker>();
        }

        private void OnTurnStart(Team team)
        {
            state = team == playerTeam ? GameState.PlayerTurn : GameState.AITurn;
            
            if (turnIndicator != null)
            {
                turnIndicator.SetCurrentTeam(team);
            }
        }

        private void OnTurnEnd(Team team)
        {
            // 턴 종료 시 추가 로직
        }
    }
}
