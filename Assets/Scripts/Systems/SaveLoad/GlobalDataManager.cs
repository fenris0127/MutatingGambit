using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.Systems.SaveLoad
{
    /// <summary>
    /// Manages persistent global data (Codex, History) separate from run saves.
    /// </summary>
    public class GlobalDataManager : MonoBehaviour
    {
        public static GlobalDataManager Instance { get; private set; }

        private const string GLOBAL_DATA_FILE = "global_data.json";
        private GlobalData globalData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadGlobalData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadGlobalData()
        {
            string path = Path.Combine(Application.persistentDataPath, GLOBAL_DATA_FILE);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                globalData = JsonUtility.FromJson<GlobalData>(json);
            }
            else
            {
                globalData = new GlobalData();
                SaveGlobalData();
            }
        }

        private void SaveGlobalData()
        {
            string json = JsonUtility.ToJson(globalData, true);
            string path = Path.Combine(Application.persistentDataPath, GLOBAL_DATA_FILE);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Records a completed run.
        /// </summary>
        public void RecordRun(bool isVictory, PlayerState playerState)
        {
            if (playerState == null) return;

            RunRecord record = new RunRecord
            {
                IsVictory = isVictory,
                FloorsCleared = playerState.FloorsCleared,
                RoomsCleared = playerState.RoomsCleared,
                FinalScore = playerState.Currency
            };

            // Record pieces
            foreach (var piece in playerState.Pieces)
            {
                record.FinalPieceTypes.Add(piece.pieceType.ToString());
                foreach (var mut in piece.mutations)
                {
                    if (!record.FinalMutations.Contains(mut.MutationName))
                    {
                        record.FinalMutations.Add(mut.MutationName);
                    }
                    UnlockMutation(mut.MutationName); // Ensure it's unlocked in codex
                }
            }

            // Record artifacts
            foreach (var art in playerState.CollectedArtifacts)
            {
                record.CollectedArtifacts.Add(art.ArtifactName);
                UnlockArtifact(art.ArtifactName); // Ensure it's unlocked in codex
            }

            globalData.RunHistory.Insert(0, record); // Add to front
            
            // Limit history size (e.g., keep last 50 runs)
            if (globalData.RunHistory.Count > 50)
            {
                globalData.RunHistory.RemoveAt(globalData.RunHistory.Count - 1);
            }

            SaveGlobalData();
            Debug.Log("Run recorded to history.");
        }

        /// <summary>
        /// Unlocks a mutation in the codex.
        /// </summary>
        public void UnlockMutation(string mutationName)
        {
            if (!globalData.DiscoveredMutations.Contains(mutationName))
            {
                globalData.DiscoveredMutations.Add(mutationName);
                SaveGlobalData();
                Debug.Log($"Codex unlocked: Mutation '{mutationName}'");
            }
        }

        /// <summary>
        /// Unlocks an artifact in the codex.
        /// </summary>
        public void UnlockArtifact(string artifactName)
        {
            if (!globalData.DiscoveredArtifacts.Contains(artifactName))
            {
                globalData.DiscoveredArtifacts.Add(artifactName);
                SaveGlobalData();
                Debug.Log($"Codex unlocked: Artifact '{artifactName}'");
            }
        }

        /// <summary>
        /// Checks if a mutation is discovered.
        /// </summary>
        public bool IsMutationDiscovered(string mutationName)
        {
            return globalData.DiscoveredMutations.Contains(mutationName);
        }

        /// <summary>
        /// Checks if an artifact is discovered.
        /// </summary>
        public bool IsArtifactDiscovered(string artifactName)
        {
            return globalData.DiscoveredArtifacts.Contains(artifactName);
        }

        /// <summary>
        /// Gets the run history.
        /// </summary>
        public List<RunRecord> GetRunHistory()
        {
            return globalData.RunHistory;
        }
    }
}
