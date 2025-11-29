using System;
using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Mutations;

namespace MutatingGambit.Systems.SaveLoad
{
    /// <summary>
    /// Runtime piece state data for board creation and restoration.
    /// </summary>
    [Serializable]
    public class PieceStateData
    {
        public PieceType pieceType;
        public Team team;
        public Vector2Int position;
        public List<Mutation> mutations;

        public PieceStateData(PieceType type, Team team, Vector2Int position, List<Mutation> mutations = null)
        {
            this.pieceType = type;
            this.team = team;
            this.position = position;
            this.mutations = mutations;
        }
    }

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
        public Team PlayerTeam;
        public List<PieceSaveData> Pieces;
        public List<PieceSaveData> BrokenPieces;
        public List<PieceType> BrokenPieceTypes; // 파괴된 기물 타입 목록
    }

    /// <summary>
    /// Save data for a single piece.
    /// </summary>
    [Serializable]
    public class PieceSaveData
    {
        public PieceType Type;
        public Vector2Int Position;
        public int X; // Position.x를 위한 별칭
        public int Y; // Position.y를 위한 별칭
        public bool IsAlive;
        public List<string> MutationNames;
        public List<string> MutationIDs; // MutationNames의 별칭
    }
}
