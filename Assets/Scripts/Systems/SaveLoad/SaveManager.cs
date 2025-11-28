using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.Systems.SaveLoad
{
    /// <summary>
    /// Manages saving and loading of game data.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "savegame.json";

        [Header("References")]
        [SerializeField] private MutationLibrary mutationLibrary;
        [SerializeField] private ArtifactLibrary artifactLibrary;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Saves the current game state.
        /// </summary>
        public void SaveGame()
        {
            GameSaveData data = new GameSaveData();
            data.SaveDate = System.DateTime.Now.ToString();

            // 1. Save Dungeon State
            var dungeonManager = DungeonManager.Instance;
            if (dungeonManager != null)
            {
                // Save Dungeon State
                data.CurrentFloor = dungeonManager.CurrentFloor;
                data.CurrentRoomIndex = dungeonManager.CurrentRoomIndex;
                data.DungeonSeed = dungeonManager.Seed;
            }

            // 2. Save Player State
            // We need to access PlayerState. Since it's not a singleton, we might need to find it or pass it in.
            // Assuming we can find the active PlayerState or it's managed by DungeonManager.
            // For now, let's look for the Board and reconstruct from there, OR better, use PlayerState if available.
            // Actually, PlayerState is usually passed around. Let's try to find the active pieces on the board.
            
            var board = FindFirstObjectByType<Board>();
            var gameManager = GameManager.Instance;
            Team playerTeam = gameManager != null ? gameManager.PlayerTeam : Team.White;

            if (board != null)
            {
                data.PlayerData = new PlayerSaveData();
                data.PlayerData.PlayerTeam = playerTeam;
                data.PlayerData.Pieces = new List<PieceSaveData>();

                var playerPieces = board.GetPiecesByTeam(playerTeam);
                foreach (var piece in playerPieces)
                {
                    var pieceData = new PieceSaveData
                    {
                        Type = piece.Type,
                        Position = piece.Position,
                        IsAlive = true, // If it's on the board, it's alive
                        MutationNames = new List<string>()
                    };

                    // Get mutations
                    if (MutationManager.Instance != null)
                    {
                        var mutations = MutationManager.Instance.GetMutations(piece);
                        foreach (var m in mutations)
                        {
                            pieceData.MutationNames.Add(m.MutationName);
                        }
                    }

                    data.PlayerData.Pieces.Add(pieceData);
                }
                
                // Save broken pieces
                var repairSystem = FindFirstObjectByType<Systems.PieceManagement.RepairSystem>();
                if (repairSystem != null && repairSystem.BrokenPieces != null)
                {
                    data.PlayerData.BrokenPieces = new List<PieceSaveData>();
                    foreach (var brokenPiece in repairSystem.BrokenPieces)
                    {
                        if (brokenPiece.Piece != null)
                        {
                            var pieceData = new PieceSaveData
                            {
                                Type = brokenPiece.Piece.Type,
                                Position = brokenPiece.Piece.Position,
                                IsAlive = false,
                                MutationNames = new List<string>()
                            };
                            
                            // Save mutations for broken pieces too
                            if (MutationManager.Instance != null)
                            {
                                var mutations = MutationManager.Instance.GetMutations(brokenPiece.Piece);
                                foreach (var m in mutations)
                                {
                                    pieceData.MutationNames.Add(m.MutationName);
                                }
                            }
                            
                            data.PlayerData.BrokenPieces.Add(pieceData);
                        }
                    }
                }
            }

            // 3. Save Artifacts
            var artifactManager = FindFirstObjectByType<ArtifactManager>();
            if (artifactManager != null)
            {
                data.ActiveArtifactNames = new List<string>();
                foreach(var art in artifactManager.ActiveArtifacts)
                {
                    data.ActiveArtifactNames.Add(art.ArtifactName);
                }
            }

            // Serialize and write to file
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            File.WriteAllText(path, json);
            
            Debug.Log($"Game saved to {path}");
        }

        /// <summary>
        /// Loads the game state.
        /// </summary>
        public GameSaveData LoadGame()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(path))
            {
                Debug.LogWarning("No save file found.");
                return null;
            }

            string json = File.ReadAllText(path);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            
            Debug.Log("Game loaded.");
            return data;
        }

        /// <summary>
        /// Checks if a save file exists.
        /// </summary>
        public bool HasSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            return File.Exists(path);
        }

        /// <summary>
        /// Deletes the save file (e.g. on game over or new game).
        /// </summary>
        public void DeleteSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("Save file deleted.");
            }
        }
        
        // Helper to restore state
        public void RestoreGameState(GameSaveData data, Board board)
        {
            if (data == null || board == null) return;

            // 1. Clear board
            board.Clear();

            // 2. Restore Pieces
            Team playerTeam = data.PlayerData != null ? data.PlayerData.PlayerTeam : Team.White;

            if (data.PlayerData != null && data.PlayerData.Pieces != null)
            {
                foreach (var pieceData in data.PlayerData.Pieces)
                {
                    if (pieceData.IsAlive)
                    {
                        // Create piece
                        // Use Board's SpawnPiece method
                        var piece = board.SpawnPiece(pieceData.Type, playerTeam, pieceData.Position);
                        
                        // Apply mutations
                        if (piece != null && pieceData.MutationNames != null)
                        {
                           foreach (var mutName in pieceData.MutationNames) {
                               var mutation = mutationLibrary.GetMutationByName(mutName);
                               if (mutation != null)
                               {
                                   MutationManager.Instance.ApplyMutation(piece, mutation);
                               }
                           }
                        }
                    }
                }
            }

            // 3. Restore Dungeon State
            if (DungeonManager.Instance != null)
            {
                // DungeonManager handles its own restoration via LoadRun, but if we needed to set state here:
                // DungeonManager.Instance.SetState(data.CurrentFloor, data.CurrentRoomIndex);
            }
        }
    }
}
