using System;
using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.SaveLoad
{
    /// <summary>
    /// Root object for all save data.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public string SaveDate;
        public int DungeonSeed;
        public int CurrentFloor;
        public int CurrentRoomIndex;
        public PlayerSaveData PlayerData;
        public List<string> ActiveArtifactNames;
        public int Gold;
    }

    /// <summary>
    /// Save data for the player's state (pieces, mutations).
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public List<PieceSaveData> Pieces;
    }

    /// <summary>
    /// Save data for a single piece.
    /// </summary>
    [Serializable]
    public class PieceSaveData
    {
        public PieceType Type;
        public Vector2Int Position;
        public bool IsAlive;
        public List<string> MutationNames;
    }
}
