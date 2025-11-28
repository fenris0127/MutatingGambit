using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.AI;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.UI;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Main game manager that controls the overall game flow.
    /// Handles turn management, win/loss conditions, and game state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        [Header("Core References")]
        [SerializeField]
        private Board board;

        [SerializeField]
        private ChessAI aiPlayer;

        [SerializeField]
        private RepairSystem repairSystem;

        [SerializeField]
        private RoomManager roomManager;

        [Header("Game State")]
        [SerializeField]
        private Team playerTeam = Team.White;

        [SerializeField]
        private Team currentTurn = Team.White;

        [SerializeField]
        private GameState state = GameState.NotStarted;

        [SerializeField]
        private int turnNumber = 0;

        [Header("Debug/Testing")]
        [SerializeField]
        private bool simulationMode = false;

        [Header("UI References")]
        [SerializeField]
        private TurnIndicator turnIndicator;

        [SerializeField]
        private GameOverScreen gameOverScreen;

        [Header("Events")]
        public UnityEvent<Team> OnTurnStart;
        public UnityEvent<Team> OnTurnEnd;
        public UnityEvent<Team> OnGameOver;
        public UnityEvent OnVictory;
        public UnityEvent OnDefeat;

        /// <summary>
        /// Game state enumeration.
        /// </summary>
        public enum GameState
        {
            NotStarted,
            PlayerTurn,
            AITurn,
            GameOver,
            Victory,
            Defeat
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the current game state.
        /// </summary>
        public GameState State => state;

        /// <summary>
        /// Gets the current turn team.
        /// </summary>
        public Team CurrentTurn => currentTurn;

        /// <summary>
        /// Gets the player's team.
        /// </summary>
        public Team PlayerTeam => playerTeam;

        /// <summary>
        /// Gets the current turn number.
        /// </summary>
        public int TurnNumber => turnNumber;

        /// <summary>
        /// Gets whether it's currently the player's turn.
        /// </summary>
        public bool IsPlayerTurn => currentTurn == playerTeam;

        /// <summary>
        /// Gets or sets whether the game is in simulation mode (no delays, no UI updates).
        /// </summary>
        public bool SimulationMode
        {
            get => simulationMode;
            set => simulationMode = value;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            // Find references if not assigned
            if (board == null) board = FindObjectOfType<Board>();
            if (repairSystem == null) repairSystem = FindObjectOfType<RepairSystem>();
            if (roomManager == null) roomManager = FindObjectOfType<RoomManager>();
            if (turnIndicator == null) turnIndicator = FindObjectOfType<TurnIndicator>();
            if (gameOverScreen == null) gameOverScreen = FindObjectOfType<GameOverScreen>();
        }

        private void Start()
        {
            if (state == GameState.NotStarted)
            {
                StartGame();
            }
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public void StartGame()
        {
            state = GameState.PlayerTurn;
            currentTurn = playerTeam;
            turnNumber = 0;

            StartTurn();

            Debug.Log("Game started!");
        }

        /// <summary>
        /// Starts a new turn.
        /// </summary>
        private void StartTurn()
        {
            turnNumber++;

            state = IsPlayerTurn ? GameState.PlayerTurn : GameState.AITurn;

            OnTurnStart?.Invoke(currentTurn);

            if (turnIndicator != null)
            {
                turnIndicator.SetCurrentTeam(currentTurn);
            }

            if (roomManager != null)
            {
                roomManager.StartTurn();
            }

            Debug.Log($"Turn {turnNumber}: {currentTurn}'s turn");

            // If AI turn, make AI move
            if (!IsPlayerTurn && aiPlayer != null)
            {
                if (simulationMode)
                {
                    ExecuteAITurn();
                }
                else
                {
                    Invoke(nameof(ExecuteAITurn), 0.5f); // Small delay for visual feedback
                }
            }
        }

        /// <summary>
        /// Executes the AI's turn.
        /// </summary>
        private void ExecuteAITurn()
        {
            if (board == null || aiPlayer == null)
            {
                Debug.LogError("Cannot execute AI turn - missing references!");
                return;
            }

            var move = aiPlayer.MakeMove(board);

            if (move.MovingPiece != null)
            {
                ExecuteMove(move.From, move.To);
            }
            else
            {
                Debug.LogWarning("AI could not find a valid move!");
                EndTurn();
            }
        }

        /// <summary>
        /// Executes a move on the board.
        /// </summary>
        public bool ExecuteMove(Vector2Int from, Vector2Int to)
        {
            if (board == null)
            {
                return false;
            }

            var movingPiece = board.GetPiece(from);
            var capturedPiece = board.GetPiece(to);

            bool success = board.MovePiece(from, to);

            if (success)
            {
                // Notify mutations
                if (MutatingGambit.Systems.Mutations.MutationManager.Instance != null)
                {
                    MutatingGambit.Systems.Mutations.MutationManager.Instance.NotifyMove(movingPiece, from, to, board);
                    
                    if (capturedPiece != null)
                    {
                        MutatingGambit.Systems.Mutations.MutationManager.Instance.NotifyCapture(movingPiece, capturedPiece, from, to, board);
                    }
                }

                // Handle captured piece
                if (capturedPiece != null)
                {
                    HandlePieceCapture(capturedPiece);
                }

                // Check victory/defeat conditions
                CheckGameConditions();

                // End turn if game not over
                if (state != GameState.GameOver && state != GameState.Victory && state != GameState.Defeat)
                {
                    EndTurn();
                }
            }

            return success;
        }

        /// <summary>
        /// Handles a piece being captured.
        /// </summary>
        private void HandlePieceCapture(Piece capturedPiece)
        {
            if (capturedPiece == null)
            {
                return;
            }

            // Break the piece if it has PieceHealth
            var pieceHealth = capturedPiece.GetComponent<PieceHealth>();
            if (pieceHealth != null && repairSystem != null)
            {
                repairSystem.BreakPiece(pieceHealth);

                // Check if king was broken
                if (capturedPiece.Type == PieceType.King)
                {
                    HandleKingBroken(capturedPiece.Team);
                }
            }
        }

        /// <summary>
        /// Handles a king being broken (game over).
        /// </summary>
        private void HandleKingBroken(Team team)
        {
            if (team == playerTeam)
            {
                GameOver(false); // Player lost
            }
            else
            {
                GameOver(true); // Player won
            }
        }

        /// <summary>
        /// Checks game win/loss conditions.
        /// </summary>
        private void CheckGameConditions()
        {
            // Check if player's king is broken
            if (repairSystem != null && repairSystem.IsKingBroken(playerTeam))
            {
                GameOver(false);
                return;
            }

            // Check if AI's king is broken
            Team aiTeam = playerTeam == Team.White ? Team.Black : Team.White;
            if (repairSystem != null && repairSystem.IsKingBroken(aiTeam))
            {
                GameOver(true);
                return;
            }

            // Check room-specific victory conditions
            if (roomManager != null)
            {
                roomManager.CheckConditions();

                if (roomManager.IsRoomCompleted)
                {
                    HandleRoomVictory();
                }
                else if (roomManager.IsRoomFailed)
                {
                    GameOver(false);
                }
            }
        }

        /// <summary>
        /// Handles victory in a room.
        /// </summary>
        private void HandleRoomVictory()
        {
            Debug.Log("Room completed!");
            OnVictory?.Invoke();

            // For now, just show victory screen
            // In full implementation, this would transition to reward selection
            if (gameOverScreen != null)
            {
                gameOverScreen.ShowVictory("Room Cleared!");
            }
        }

        /// <summary>
        /// Ends the current turn and switches to the next player.
        /// </summary>
        private void EndTurn()
        {
            OnTurnEnd?.Invoke(currentTurn);

            // Switch turn
            currentTurn = currentTurn == Team.White ? Team.Black : Team.White;

            StartTurn();
        }

        /// <summary>
        /// Ends the game with victory or defeat.
        /// </summary>
        public void GameOver(bool victory)
        {
            state = victory ? GameState.Victory : GameState.Defeat;

            OnGameOver?.Invoke(currentTurn);

            if (victory)
            {
                OnVictory?.Invoke();
                Debug.Log("Victory!");

                if (gameOverScreen != null)
                {
                    gameOverScreen.ShowVictory();
                }
            }
            else
            {
                OnDefeat?.Invoke();
                Debug.Log("Defeat!");

                if (gameOverScreen != null)
                {
                    gameOverScreen.ShowDefeat();
                }
            }
        }

        /// <summary>
        /// Restarts the current game/room.
        /// </summary>
        public void RestartGame()
        {
            state = GameState.NotStarted;
            currentTurn = playerTeam;
            turnNumber = 0;

            if (board != null)
            {
                board.Clear();
            }

            if (repairSystem != null)
            {
                repairSystem.Clear();
            }

            StartGame();
        }
        /// <summary>
        /// Sets the AI player reference (used for testing).
        /// </summary>
        public void SetAIPlayer(ChessAI ai)
        {
            aiPlayer = ai;
        }

        /// <summary>
        /// Sets the player team (used for testing).
        /// </summary>
        public void SetPlayerTeam(Team team)
        {
            playerTeam = team;
        }
    }
}
