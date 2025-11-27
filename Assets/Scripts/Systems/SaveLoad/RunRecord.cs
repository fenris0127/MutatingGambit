using System;
using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.SaveLoad
{
    /// <summary>
    /// Represents a record of a single completed game run.
    /// </summary>
    [Serializable]
    public class RunRecord
    {
        public string RunId;
        public string Date;
        public bool IsVictory;
        public int FloorsCleared;
        public int RoomsCleared;
        public int FinalScore; // Or Gold
        
        // Final Team Composition
        public List<string> FinalPieceTypes;
        public List<string> FinalMutations; // All mutations across all pieces
        public List<string> CollectedArtifacts;

        public RunRecord()
        {
            RunId = Guid.NewGuid().ToString();
            Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            FinalPieceTypes = new List<string>();
            FinalMutations = new List<string>();
            CollectedArtifacts = new List<string>();
        }
    }

    /// <summary>
    /// Persistent global data across all runs.
    /// </summary>
    [Serializable]
    public class GlobalData
    {
        public List<string> DiscoveredMutations = new List<string>();
        public List<string> DiscoveredArtifacts = new List<string>();
        public List<RunRecord> RunHistory = new List<RunRecord>();
    }
}
