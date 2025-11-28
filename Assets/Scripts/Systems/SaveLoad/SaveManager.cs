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
                // TODO: Add CurrentFloor and CurrentRoomIndex properties to DungeonManager
                // data.CurrentFloor = dungeonManager.CurrentFloor;
                // data.CurrentRoomIndex = dungeonManager.CurrentRoomIndex;
                data.CurrentFloor = 0;
                data.CurrentRoomIndex = 0;
                // Seed saving would go here if implemented
            }

            // 2. Save Player State
            // We need to access PlayerState. Since it's not a singleton, we might need to find it or pass it in.
            // Assuming we can find the active PlayerState or it's managed by DungeonManager.
            // For now, let's look for the Board and reconstruct from there, OR better, use PlayerState if available.
            // Actually, PlayerState is usually passed around. Let's try to find the active pieces on the board.
            
            var board = FindFirstObjectByType<Board>();
            if (board != null)
            {
                data.PlayerData = new PlayerSaveData();
                data.PlayerData.Pieces = new List<PieceSaveData>();

                var playerPieces = board.GetPiecesByTeam(Team.White); // Assuming player is White
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
                
                // TODO: Also save dead pieces if they are relevant (e.g. for resurrection)
                // This requires PlayerState to track dead pieces persistently.
            }

            // 3. Save Artifacts
            var artifactManager = FindFirstObjectByType<ArtifactManager>();
            if (artifactManager != null)
            {
                data.ActiveArtifactNames = new List<string>();
                // TODO: Add GetActiveArtifacts method to ArtifactManager
                // var artifacts = artifactManager.GetActiveArtifacts();
                // foreach(var art in artifacts) data.ActiveArtifactNames.Add(art.ArtifactName);
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
            if (data.PlayerData != null && data.PlayerData.Pieces != null)
            {
                foreach (var pieceData in data.PlayerData.Pieces)
                {
                    if (pieceData.IsAlive)
                    {
                        // Create piece
                        // Need a way to spawn piece by type. Board usually has a method or we use a factory.
                        // For MVP, we might need to instantiate prefabs.
                        // Assuming Board has a SpawnPiece method or similar.
                        // board.SpawnPiece(pieceData.Type, Team.White, pieceData.Position); 
                        
                        // Since Board.SpawnPiece might not exist or be public, let's assume we have a way.
                        // If not, we need to implement it.
                        // Let's check Board.cs later.
                        
                        // Apply mutations
                        // var piece = board.GetPiece(pieceData.Position);
                        // foreach (var mutName in pieceData.MutationNames) {
                        //    var mutation = mutationLibrary.GetMutationByName(mutName);
                        //    MutationManager.Instance.ApplyMutation(piece, mutation);
                        // }
                    }
                }
            }

            // 3. Restore Dungeon State
            if (DungeonManager.Instance != null)
            {
                // DungeonManager.Instance.SetState(data.CurrentFloor, data.CurrentRoomIndex);
            }
        }
    }
}
