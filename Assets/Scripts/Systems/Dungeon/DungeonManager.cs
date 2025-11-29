using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.BoardSystem;
using MutatingGambit.Systems.SaveLoad;
using MutatingGambit.UI;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Manages the dungeon progression, room transitions, and player state persistence.
    /// Coordinates between the dungeon map, room loading, and game state.
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        private static DungeonManager instance;

        [Header("Core References")]
        [SerializeField]
        private DungeonMapGenerator mapGenerator;

        [SerializeField]
        private Board gameBoard;

        [SerializeField]
        private RoomManager roomManager;

        [SerializeField]
        private RepairSystem repairSystem;

        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private BoardGenerator boardGenerator;

        [Header("UI References")]
        [SerializeField]
        private DungeonMapUI dungeonMapUI;

        [SerializeField]
        private RewardSelectionUI rewardUI;

        [SerializeField]
        private RepairUI repairUI;

        [SerializeField]
        private NotificationUI notificationUI;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject piecePrefab;

        [Header("Dungeon State")]
        [SerializeField]
        private PlayerState playerState;

        [SerializeField]
        private DungeonMap currentDungeonMap;

        [SerializeField]
        private RoomNode currentRoomNode;

        [Header("Events")]
        public UnityEvent<RoomNode> OnRoomEntered;
        public UnityEvent<RoomNode> OnRoomCleared;
        public UnityEvent OnDungeonCompleted;
        public UnityEvent OnDungeonFailed;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DungeonManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DungeonManager>();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the current dungeon map.
        /// </summary>
        public DungeonMap CurrentMap => currentDungeonMap;

        /// <summary>
        /// Gets the current room node.
        /// </summary>
        public RoomNode CurrentRoomNode => currentRoomNode;

        /// <summary>
        /// Gets the player state.
        /// </summary>
        public PlayerState PlayerState => playerState;

        /// <summary>
        /// Gets the current floor number.
        /// </summary>
        public int CurrentFloor => playerState != null ? playerState.FloorsCleared + 1 : 1;

        /// <summary>
        /// Gets the index of the current room in the map.
        /// </summary>
        public int CurrentRoomIndex
        {
            get
            {
                if (currentDungeonMap != null && currentRoomNode != null)
                {
                    return currentDungeonMap.AllNodes.IndexOf(currentRoomNode);
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets the dungeon generation seed.
        /// </summary>
        public int Seed
        {
            get
            {
                if (currentDungeonMap != null)
                {
                    return currentDungeonMap.Seed;
                }
                return 0;
            }
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
            if (mapGenerator == null) mapGenerator = FindFirstObjectByType<DungeonMapGenerator>();
            if (gameBoard == null) gameBoard = FindFirstObjectByType<Board>();
            if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
            if (repairSystem == null) repairSystem = FindFirstObjectByType<RepairSystem>();
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            if (boardGenerator == null) boardGenerator = FindFirstObjectByType<BoardGenerator>();
            if (dungeonMapUI == null) dungeonMapUI = FindFirstObjectByType<DungeonMapUI>();
            if (rewardUI == null) rewardUI = FindFirstObjectByType<RewardSelectionUI>();
            if (repairUI == null) repairUI = FindFirstObjectByType<RepairUI>();
            if (notificationUI == null) notificationUI = FindFirstObjectByType<NotificationUI>();
        }

        private void Start()
        {
            // Subscribe to UI events
            if (dungeonMapUI != null)
            {
                dungeonMapUI.OnNodeSelected.AddListener(OnNodeSelected);
            }

            if (rewardUI != null)
            {
                rewardUI.OnRewardSelected.AddListener(OnRewardSelected);
            }

            if (repairUI != null)
            {
                repairUI.OnRepairCompleted.AddListener(ContinueAfterRest);
            }

            // Subscribe to game manager events
            if (gameManager != null)
            {
                gameManager.OnVictory.AddListener(OnRoomVictory);
                gameManager.OnDefeat.AddListener(OnRoomDefeat);
            }
        }

        /// <summary>
        /// Starts a new dungeon run.
        /// </summary>
        public void StartNewRun()
        {
            // Create new player state with standard setup
            playerState = PlayerState.CreateStandardSetup(Team.White);

            // Generate dungeon map
            if (mapGenerator != null)
            {
                currentDungeonMap = mapGenerator.GenerateMap();
            }
            else
            {
                Debug.LogError("Cannot start dungeon - MapGenerator is null!");
                return;
            }

            // Show dungeon map UI
            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.ShowMap(currentDungeonMap);
            }

            Debug.Log("New dungeon run started!");
        }

        /// <summary>
        /// Loads a dungeon run from save data.
        /// </summary>
        public void LoadRun(Systems.SaveLoad.GameSaveData data)
        {
            if (data == null) return;

            // Restore player state
            if (playerState == null) playerState = new PlayerState();
            
            // We need references to libraries to resolve IDs
            var mutationLib = Resources.Load<MutatingGambit.Systems.Mutations.MutationLibrary>("MutationLibrary");
            var artifactLib = Resources.Load<MutatingGambit.Systems.Artifacts.ArtifactLibrary>("ArtifactLibrary");
            
            playerState.LoadFromSaveData(data.PlayerData, mutationLib, artifactLib);
            playerState.Currency = data.Gold;
            
            // Restore artifacts
            if (data.ActiveArtifactNames != null)
            {
                foreach (var artName in data.ActiveArtifactNames)
                {
                    var artifact = artifactLib.GetArtifactByName(artName);
                    if (artifact != null)
                    {
                        playerState.AddArtifact(artifact);
                    }
                }
            }

            // Generate/Restore Map
            // Use saved seed for deterministic map generation
            if (mapGenerator != null)
            {
                // Use saved seed if available, otherwise generate new one
                int seed = data.DungeonSeed != 0 ? data.DungeonSeed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                currentDungeonMap = mapGenerator.GenerateMap(5, 2, 4, seed);
                
                // Set current node
                // This is tricky without saving the whole graph structure or deterministic generation.
                // For now, let's just put them at the start of the floor or try to find the room index.
                if (currentDungeonMap != null && currentDungeonMap.AllNodes.Count > data.CurrentRoomIndex)
                {
                    currentRoomNode = currentDungeonMap.AllNodes[data.CurrentRoomIndex];
                }
            }

            // Show map
            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.ShowMap(currentDungeonMap);
                // If we are in a room, we might want to enter it immediately or show map.
                // Usually load -> show map is safer.
            }
            
            Debug.Log("Dungeon run loaded!");
        }

        /// <summary>
        /// Called when a node is selected from the dungeon map UI.
        /// </summary>
        private void OnNodeSelected(RoomNode node)
        {
            if (node == null || !node.IsAccessible)
            {
                Debug.LogWarning($"Cannot enter node {node?.NodeId}: not accessible");
                return;
            }

            EnterRoom(node);
        }

        /// <summary>
        /// Enters a room and sets up the board.
        /// </summary>
        public void EnterRoom(RoomNode roomNode)
        {
            if (roomNode == null || roomNode.Room == null)
            {
                Debug.LogError("Cannot enter room: node or room data is null");
                return;
            }

            // Save current room as cleared if transitioning
            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;
            }

            currentRoomNode = roomNode;

            // Handle different room types
            switch (roomNode.Type)
            {
                case RoomType.Rest:
                    EnterRestRoom(roomNode);
                    break;

                case RoomType.Treasure:
                    EnterTreasureRoom(roomNode);
                    break;

                case RoomType.NormalCombat:
                case RoomType.EliteCombat:
                case RoomType.Boss:
                case RoomType.Start:
                case RoomType.Tutorial:
                    EnterCombatRoom(roomNode);
                    break;

                case RoomType.Mystery:
                    EnterMysteryRoom(roomNode);
                    break;
            }

            OnRoomEntered?.Invoke(roomNode);
        }

        /// <summary>
        /// Enters a combat room (normal, elite, boss, or start).
        /// </summary>
        private void EnterCombatRoom(RoomNode roomNode)
        {
            RoomData roomData = roomNode.Room;

            // Clear and initialize board
            if (gameBoard != null)
            {
                gameBoard.Clear();

                // Generate board from room data
                if (roomData.BoardData != null)
                {
                    // Initialize board with specified dimensions
                    gameBoard.Initialize(roomData.BoardData.Width, roomData.BoardData.Height);
                    
                    // Place obstacles based on TileType
                    for (int y = 0; y < roomData.BoardData.Height; y++)
                    {
                        for (int x = 0; x < roomData.BoardData.Width; x++)
                        {
                            Vector2Int pos = new Vector2Int(x, y);
                            if (roomData.BoardData.GetTileType(pos) == MutatingGambit.Systems.BoardSystem.TileType.Obstacle)
                            {
                                if (gameBoard.IsPositionValid(pos))
                                {
                                    gameBoard.SetObstacle(pos, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Default 8x8 board
                    gameBoard.Initialize(8, 8);
                }
            }

            // Restore player pieces
            if (playerState != null)
            {
                playerState.RestoreBoardState(gameBoard, repairSystem, piecePrefab);
            }

            // Setup enemy pieces
            SetupEnemyPieces(roomData);

            // Apply artifacts
            ApplyArtifacts();

            // Enter room in RoomManager
            if (roomManager != null)
            {
                roomManager.SetBoard(gameBoard);
                roomManager.EnterRoom(roomData, playerState.PlayerTeam);
            }

            // Start the game
            if (gameManager != null)
            {
                gameManager.StartGame();
            }

            Debug.Log($"Entered combat room: {roomData.RoomName}");
        }

        /// <summary>
        /// Enters a rest room (repair pieces).
        /// </summary>
        private void EnterRestRoom(RoomNode roomNode)
        {
            Debug.Log("Entered rest room - showing repair UI");

            // Show repair UI
            if (repairUI != null && repairSystem != null)
            {
                repairUI.Show(repairSystem);
            }
            else
            {
                Debug.LogWarning("RepairUI or RepairSystem not found - skipping rest room");
                ContinueAfterRest();
            }
        }

        /// <summary>
        /// Enters a treasure room (immediate artifact reward).
        /// </summary>
        private void EnterTreasureRoom(RoomNode roomNode)
        {
            Debug.Log("Entered treasure room - showing reward selection");

            // Show reward selection UI
            ShowRewardSelection(roomNode.Room);
        }

        /// <summary>
        /// Enters a mystery room (random event).
        /// </summary>
        private void EnterMysteryRoom(RoomNode roomNode)
        {
            Debug.Log("Entered mystery room - random event");

            // Random event system - 40% treasure, 30% curse, 30% blessing
            int roll = UnityEngine.Random.Range(0, 100);
            
            if (roll < 40)
            {
                // Treasure
                Debug.Log("Mystery room reveals... Treasure!");
                EnterTreasureRoom(roomNode);
            }
            else if (roll < 70)
            {
                // Curse event
                Debug.Log("Mystery room reveals... A curse!");
                HandleCurseEvent();
            }
            else
            {
                // Blessing event
                Debug.Log("Mystery room reveals... A blessing!");
                HandleBlessingEvent();
            }
        }
        
        /// <summary>
        /// Handles curse event - lose currency or get negative effect.
        /// </summary>
        private void HandleCurseEvent()
        {
            int currencyLoss = UnityEngine.Random.Range(10, 31);
            playerState.Currency = Mathf.Max(0, playerState.Currency - currencyLoss);
            Debug.Log($"Curse! Lost {currencyLoss} currency. Remaining: {playerState.Currency}");
            
            // Add UI notification for curse event
            ShowNotification($"Curse! Lost {currencyLoss} currency.");
        }
        
        /// <summary>
        /// Handles blessing event - gain currency or heal broken piece.
        /// </summary>
        private void HandleBlessingEvent()
        {
            int currencyGain = UnityEngine.Random.Range(20, 51);
            playerState.Currency += currencyGain;
            Debug.Log($"Blessing! Gained {currencyGain} currency. Total: {playerState.Currency}");
            
            // Add UI notification for blessing event
            ShowNotification($"Blessing! Gained {currencyGain} currency.");
        }

        /// <summary>
        /// Shows a notification to the player.
        /// </summary>
        /// <summary>
        /// Shows a notification to the player.
        /// </summary>
        private void ShowNotification(string message)
        {
            Debug.Log($"[NOTIFICATION] {message}");
            
            if (notificationUI != null)
            {
                notificationUI.Show(message);
            }
        }

        /// <summary>
        /// Sets up enemy pieces from room data.
        /// </summary>
        private void SetupEnemyPieces(RoomData roomData)
        {
            if (roomData.EnemyPieces == null || roomData.EnemyPieces.Length == 0)
            {
                Debug.LogWarning($"Room '{roomData.RoomName}' has no enemy pieces defined");
                return;
            }

            foreach (var pieceData in roomData.EnemyPieces)
            {
                Piece piece = CreatePiece(pieceData.pieceType, pieceData.team, pieceData.position);
                if (piece != null && gameBoard != null)
                {
                    gameBoard.PlacePiece(piece, pieceData.position);
                }
            }

            // Apply enemy mutations
            if (roomData.EnemyMutations != null)
            {
                foreach (var mutationData in roomData.EnemyMutations)
                {
                    if (mutationData.mutation != null && gameBoard != null)
                    {
                        var piece = gameBoard.GetPiece(mutationData.piecePosition);
                        if (piece != null)
                        {
                            mutationData.mutation.ApplyToPiece(piece);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a piece GameObject.
        /// </summary>
        private Piece CreatePiece(PieceType type, Team team, Vector2Int position)
        {
            GameObject pieceObject;

            if (piecePrefab != null)
            {
                pieceObject = Instantiate(piecePrefab);
                pieceObject.name = $"{team}_{type}";
            }
            else
            {
                pieceObject = new GameObject($"{team}_{type}");
            }

            Piece piece = pieceObject.GetComponent<Piece>();
            if (piece == null)
            {
                piece = pieceObject.AddComponent<Piece>();
            }

            piece.Initialize(type, team, position);

            return piece;
        }

        /// <summary>
        /// Applies all collected artifacts to the board.
        /// </summary>
        private void ApplyArtifacts()
        {
            if (playerState == null || playerState.CollectedArtifacts == null)
            {
                return;
            }

            if (gameBoard == null || gameBoard.ArtifactManager == null)
            {
                return;
            }

            foreach (var artifact in playerState.CollectedArtifacts)
            {
                if (artifact != null)
                {
                    gameBoard.ArtifactManager.AddArtifact(artifact);
                }
            }

            Debug.Log($"Applied {playerState.CollectedArtifacts.Count} artifacts to board");
        }

        /// <summary>
        /// Called when a room is won.
        /// </summary>
        private void OnRoomVictory()
        {
            if (currentRoomNode == null)
            {
                return;
            }

            // Save player state
            if (playerState != null && gameBoard != null)
            {
                playerState.SaveBoardState(gameBoard, repairSystem);
                playerState.IncrementRoomsCleared();
            }

            // Mark room as cleared
            currentRoomNode.IsCleared = true;

            // Make connected rooms accessible
            foreach (var connectedNode in currentRoomNode.Connections)
            {
                connectedNode.IsAccessible = true;
            }

            OnRoomCleared?.Invoke(currentRoomNode);

            // Check if boss defeated (dungeon complete)
            if (currentRoomNode.Type == RoomType.Boss)
            {
                HandleDungeonComplete();
                return;
            }

            // Show reward selection
            if (currentRoomNode.Room != null)
            {
                ShowRewardSelection(currentRoomNode.Room);
            }
        }

        /// <summary>
        /// Handles dungeon completion (victory).
        /// </summary>
        private void HandleDungeonComplete()
        {
            Debug.Log("Dungeon Completed!");
            OnDungeonCompleted?.Invoke();
            
            // Record run
            if (GlobalDataManager.Instance != null)
            {
                GlobalDataManager.Instance.RecordRun(true, playerState);
            }

            // Show victory screen
            var gameOverScreen = FindFirstObjectByType<GameOverScreen>();
            if (gameOverScreen != null)
            {
                var stats = new UI.GameStats
                {
                    RoomsCleared = playerState.RoomsCleared,
                    ArtifactsCollected = playerState.CollectedArtifacts.Count,
                    PiecesLost = playerState.BrokenPieces.Count,
                    TotalMoves = playerState.TotalMoves
                };
                gameOverScreen.ShowDungeonComplete(stats);
            }
            
            // Clear save file
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSaveFile();
            }
        }

        /// <summary>
        /// Handles room defeat (game over).
        /// </summary>
        private void OnRoomDefeat()
        {
            Debug.Log("Room Failed - Game Over");
            OnDungeonFailed?.Invoke();

            // Record run
            if (GlobalDataManager.Instance != null)
            {
                GlobalDataManager.Instance.RecordRun(false, playerState);
            }

            // Show defeat screen
            var gameOverScreen = FindFirstObjectByType<GameOverScreen>();
            if (gameOverScreen != null)
            {
                gameOverScreen.ShowDefeat();
            }
            
            // Clear save file
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSaveFile();
            }
        }

        /// <summary>
        /// Shows reward selection UI.
        /// </summary>
        private void ShowRewardSelection(RoomData roomData)
        {
            if (rewardUI == null)
            {
                Debug.LogWarning("RewardSelectionUI not found - skipping reward selection");
                ShowDungeonMap();
                return;
            }

            // Get possible rewards
            List<Artifact> possibleRewards = new List<Artifact>();

            if (roomData.PossibleArtifactRewards != null && roomData.PossibleArtifactRewards.Length > 0)
            {
                possibleRewards.AddRange(roomData.PossibleArtifactRewards);
            }

            // Show reward UI
            if (possibleRewards.Count > 0)
            {
                rewardUI.ShowRewards(possibleRewards);
            }
            else
            {
                // No rewards, go straight to map
                ShowDungeonMap();
            }
        }

        /// <summary>
        /// Called when a reward is selected.
        /// </summary>
        private void OnRewardSelected(Artifact artifact)
        {
            if (artifact != null && playerState != null)
            {
                playerState.AddArtifact(artifact);
                Debug.Log($"Player selected artifact: {artifact.name}");
            }

            // Show dungeon map
            ShowDungeonMap();
        }

        /// <summary>
        /// Shows the dungeon map UI.
        /// </summary>
        private void ShowDungeonMap()
        {
            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.UpdateNodeStates();
                dungeonMapUI.ShowMap(currentDungeonMap);
            }
        }


        /// <summary>
        /// Continues after rest room.
        /// Called by RestUI or RepairUI.
        /// </summary>
        public void ContinueAfterRest()
        {
            // Mark room as cleared
            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;

                // Make connected rooms accessible
                foreach (var connectedNode in currentRoomNode.Connections)
                {
                    connectedNode.IsAccessible = true;
                }
            }

            // Show dungeon map
            ShowDungeonMap();
        }
    }
}
