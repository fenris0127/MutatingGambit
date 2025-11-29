using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.AI;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Systems.Testing
{
    /// <summary>
    /// Manages automated balance testing by running AI vs AI matches.
    /// </summary>
    public class BalanceTestManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private int numberOfGames = 10;

        [SerializeField]
        private AIConfig aiConfig;

        [Header("Player A (White)")]
        [SerializeField]
        private List<Mutation> playerAMutations;
        [SerializeField]
        private List<Artifact> playerAArtifacts;

        [Header("Player B (Black)")]
        [SerializeField]
        private List<Mutation> playerBMutations;
        [SerializeField]
        private List<Artifact> playerBArtifacts;

        // Results
        public int PlayerAWins { get; private set; }
        public int PlayerBWins { get; private set; }
        public int Draws { get; private set; }
        public int TotalGamesPlayed { get; private set; }
        public bool IsRunning { get; private set; }

        private GameManager gameManager;
        private Board board;
        private ChessAI aiWhite;
        private ChessAI aiBlack;
        private MutationManager mutationManager;
        private ArtifactManager artifactManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
            board = Board.Instance;
            mutationManager = MutationManager.Instance;
            artifactManager = ArtifactManager.Instance;
        }

        /// <summary>
        /// Starts the balance test simulation.
        /// </summary>
        public void RunTest()
        {
            if (IsRunning) return;

            StartCoroutine(RunTestRoutine());
        }

        private IEnumerator RunTestRoutine()
        {
            IsRunning = true;
            PlayerAWins = 0;
            PlayerBWins = 0;
            Draws = 0;
            TotalGamesPlayed = 0;

            // Setup AI components if needed
            SetupAI();

            // Configure GameManager for simulation
            gameManager.SimulationMode = true;

            for (int i = 0; i < numberOfGames; i++)
            {
                yield return StartCoroutine(RunSingleGame(i));
                TotalGamesPlayed++;
            }

            gameManager.SimulationMode = false;
            IsRunning = false;

            Debug.Log($"Balance Test Complete! White: {PlayerAWins}, Black: {PlayerBWins}, Draws: {Draws}");
        }

        private void SetupAI()
        {
            // Ensure we have AI components for both sides
            if (aiWhite == null)
            {
                GameObject go = new GameObject("AI_White");
                aiWhite = go.AddComponent<ChessAI>();
                aiWhite.Initialize(aiConfig, Team.White);
            }

            if (aiBlack == null)
            {
                GameObject go = new GameObject("AI_Black");
                aiBlack = go.AddComponent<ChessAI>();
                aiBlack.Initialize(aiConfig, Team.Black);
            }
        }

        private IEnumerator RunSingleGame(int gameIndex)
        {
            // Reset Game
            gameManager.RestartGame();
            
            // Wait a frame for initialization
            yield return null;

            // Apply configurations
            ApplyConfiguration(Team.White, playerAMutations, playerAArtifacts);
            ApplyConfiguration(Team.Black, playerBMutations, playerBArtifacts);

            // Set GameManager to use AI for both sides
            // We need to trick the GameManager. 
            // In simulation mode, we will manually trigger AI turns for both sides.
            
            // Actually, GameManager logic is: if (!IsPlayerTurn) -> AI Turn.
            // If we want AI vs AI, we can set PlayerTeam to None or handle it via a custom loop.
            // But modifying GameManager to support AI vs AI natively might be cleaner.
            // For now, let's toggle the "PlayerTeam" in GameManager to swap who is "AI".
            
            // Better approach: Let GameManager run its state machine.
            // If it's White's turn, we want White AI to move.
            // If it's Black's turn, we want Black AI to move.
            // The current GameManager only supports 1 AI.
            
            // Let's modify the loop here to drive the game manually if needed, 
            // OR we can swap the AI reference in GameManager each turn.
            
            bool gameActive = true;
            int moves = 0;
            int maxMoves = 100; // Prevent infinite games

            while (gameActive && moves < maxMoves)
            {
                Team currentTurn = gameManager.CurrentTurn;
                ChessAI currentAI = currentTurn == Team.White ? aiWhite : aiBlack;

                // Force AI move
                gameManager.SetAIPlayer(currentAI);
                gameManager.SetPlayerTeam(currentTurn == Team.White ? Team.Black : Team.White); // Make current turn "AI"

                // Wait for turn to complete
                // In simulation mode, GameManager executes immediately.
                // We just need to wait until turn number increases or game ends.
                int startTurn = gameManager.TurnNumber;
                
                // We need to trigger the loop. GameManager calls ExecuteAITurn automatically if !IsPlayerTurn.
                // By setting PlayerTeam to opposite of CurrentTurn, IsPlayerTurn becomes false.
                // So GameManager should pick it up.
                
                // Wait for state change
                while (gameManager.TurnNumber == startTurn && gameManager.State != GameManager.GameState.GameOver && 
                       gameManager.State != GameManager.GameState.Victory && gameManager.State != GameManager.GameState.Defeat)
                {
                    yield return null;
                }

                if (gameManager.State == GameManager.GameState.Victory || 
                    gameManager.State == GameManager.GameState.Defeat || 
                    gameManager.State == GameManager.GameState.GameOver)
                {
                    gameActive = false;
                    RecordResult(gameManager.State, gameManager.CurrentTurn); // Note: CurrentTurn might have switched if game ended
                }
                
                moves++;
            }

            if (moves >= maxMoves && gameActive)
            {
                Draws++;
            }
        }

        private void ApplyConfiguration(Team team, List<Mutation> mutations, List<Artifact> artifacts)
        {
            // Clear existing
            // This requires more robust MutationManager methods to clear by team,
            // but for MVP we can just assume we clear everything at start of game.
            if (team == Team.White) mutationManager.ClearAll(); 
            
            // Apply Mutations
            var pieces = board.GetPiecesByTeam(team);
            foreach (var piece in pieces)
            {
                foreach (var mutation in mutations)
                {
                    mutationManager.ApplyMutation(piece, mutation);
                }
            }

            // Apply Artifacts (Global)
            // Artifacts are usually global, but for testing we might want them to only affect one team?
            // The Artifact system design seems global. 
            // If we want team-specific artifacts, the Artifact class needs to check ownership.
            // For now, we'll just add them all.
            if (team == Team.White) artifactManager.ClearArtifacts();
            
            foreach (var artifact in artifacts)
            {
                artifactManager.AddArtifact(artifact);
            }
        }

        private void RecordResult(GameManager.GameState state, Team lastTurnTeam)
        {
            // If Victory, the current turn player WON (usually). 
            // Wait, GameManager.GameOver(true) means Player Won.
            // If AI vs AI, we need to know WHO won.
            
            // Let's rely on the event args if possible, or just check King status.
            // GameManager.HandleKingBroken passes the team of the BROKEN king.
            // If White King broken -> Black Wins.
            
            // Simpler: Check Kings on board.
            bool whiteKingAlive = HasKing(Team.White);
            bool blackKingAlive = HasKing(Team.Black);

            if (whiteKingAlive && !blackKingAlive) PlayerAWins++;
            else if (!whiteKingAlive && blackKingAlive) PlayerBWins++;
            else Draws++;
        }

        private bool HasKing(Team team)
        {
            var pieces = board.GetPiecesByTeam(team);
            foreach (var p in pieces)
            {
                if (p.Type == PieceType.King) return true;
            }
            return false;
        }
    }
}
