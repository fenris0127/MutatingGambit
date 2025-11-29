using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.SaveLoad;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Stores the persistent state of the player across rooms in the dungeon.
    /// Includes pieces, broken pieces, artifacts, and run progress.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
        #region 변수
        [Header("Player Info")]
        [SerializeField]
        private Team playerTeam = Team.White;

        [SerializeField]
        private int currency = 0;

        [Header("Pieces")]
        [SerializeField]
        private List<PieceStateData> pieces = new List<PieceStateData>();

        [SerializeField]
        private List<PieceStateData> brokenPieces = new List<PieceStateData>();

        [Header("Artifacts")]
        [SerializeField]
        private List<Artifact> collectedArtifacts = new List<Artifact>();

        [Header("Run Progress")]
        [SerializeField]
        private int roomsCleared = 0;

        [SerializeField]
        private int floorsCleared = 0;

        [SerializeField]
        private int totalMoves = 0;
        #endregion

        #region 속성
        /// <summary>
        /// Gets the player's team.
        /// </summary>
        public Team PlayerTeam => playerTeam;

        /// <summary>
        /// Gets or sets the player's currency.
        /// </summary>
        public int Currency
        {
            get => currency;
            set => currency = value;
        }

        /// <summary>
        /// Gets the list of active pieces.
        /// </summary>
        public List<PieceStateData> Pieces => pieces;

        /// <summary>
        /// Gets the list of broken pieces.
        /// </summary>
        public List<PieceStateData> BrokenPieces => brokenPieces;

        /// <summary>
        /// Gets the list of collected artifacts.
        /// </summary>
        public List<Artifact> CollectedArtifacts => collectedArtifacts;

        /// <summary>
        /// Gets or sets the number of rooms cleared.
        /// </summary>
        public int RoomsCleared
        {
            get => roomsCleared;
            set => roomsCleared = value;
        }

        /// <summary>
        /// Gets or sets the number of floors cleared.
        /// </summary>
        public int FloorsCleared
        {
            get => floorsCleared;
            set => floorsCleared = value;
        }

        /// <summary>
        /// Gets or sets the total moves made.
        /// </summary>
        public int TotalMoves
        {
            get => totalMoves;
            set => totalMoves = value;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// Creates a new player state with a standard chess setup.
        /// </summary>
        public static PlayerState CreateStandardSetup(Team team)
        {
            PlayerState state = new PlayerState();
            state.playerTeam = team;
            state.currency = 0;
            state.roomsCleared = 0;
            state.floorsCleared = 0;
            state.totalMoves = 0;

            // Create standard chess pieces
            state.pieces = BoardFactory.CreateStandardChessPieces(team);

            return state;
        }

        /// <summary>
        /// Saves the current board state to this player state.
        /// </summary>
        public void SaveBoardState(Board board, RepairSystem repairSystem)
        {
            if (board == null)
            {
                Debug.LogError("Cannot save board state: board is null");
                return;
            }

            pieces.Clear();
            brokenPieces.Clear();

            // Save active pieces
            var activePieces = board.GetPiecesByTeam(playerTeam);
            foreach (var piece in activePieces)
            {
                if (piece != null)
                {
                    pieces.Add(new PieceStateData(piece));
                }
            }

            // Save broken pieces
            if (repairSystem != null)
            {
                var brokenPiecesList = repairSystem.BrokenPieces;
                foreach (var pieceHealth in brokenPiecesList)
                {
                    if (pieceHealth != null && pieceHealth.Piece != null && pieceHealth.Piece.Team == playerTeam)
                    {
                        brokenPieces.Add(new PieceStateData(pieceHealth.Piece));
                    }
                }
            }

            Debug.Log($"Saved player state: {pieces.Count} active pieces, {brokenPieces.Count} broken pieces");
        }

        /// <summary>
        /// Restores the saved state to the board.
        /// </summary>
        public void RestoreBoardState(Board board, RepairSystem repairSystem, GameObject piecePrefab)
        {
            if (board == null)
            {
                Debug.LogError("Cannot restore board state: board is null");
                return;
            }

            // Restore active pieces
            foreach (var pieceData in pieces)
            {
                Piece piece = BoardFactory.CreatePieceFromData(pieceData, piecePrefab);
                if (piece != null)
                {
                    board.PlacePiece(piece, pieceData.position);
                }
            }

            // Restore broken pieces to repair system
            if (repairSystem != null)
            {
                foreach (var pieceData in brokenPieces)
                {
                    Piece piece = BoardFactory.CreatePieceFromData(pieceData, piecePrefab);
                    if (piece != null)
                    {
                        var pieceHealth = piece.gameObject.GetComponent<PieceHealth>();
                        if (pieceHealth == null)
                        {
                            pieceHealth = piece.gameObject.AddComponent<PieceHealth>();
                        }
                        repairSystem.BreakPiece(pieceHealth);
                    }
                }
            }

            Debug.Log($"Restored player state: {pieces.Count} active pieces, {brokenPieces.Count} broken pieces");
        }

        /// <summary>
        /// Adds an artifact to the player's collection.
        /// </summary>
        public void AddArtifact(Artifact artifact)
        {
            if (artifact != null && !collectedArtifacts.Contains(artifact))
            {
                collectedArtifacts.Add(artifact);
                Debug.Log($"Artifact collected: {artifact.name}");
            }
        }

        /// <summary>
        /// Removes an artifact from the player's collection.
        /// </summary>
        public void RemoveArtifact(Artifact artifact) => collectedArtifacts.Remove(artifact);

        /// <summary>
        /// Checks if the player has a specific artifact.
        /// </summary>
        public bool HasArtifact(Artifact artifact) => collectedArtifacts.Contains(artifact);

        /// <summary>
        /// Increments the room clear counter.
        /// </summary>
        public void IncrementRoomsCleared() => roomsCleared++;

        /// <summary>
        /// Increments the floor clear counter.
        /// </summary>
        public void IncrementFloorsCleared() => floorsCleared++;

        /// <summary>
        /// Loads player state from save data.
        /// </summary>
        public void LoadFromSaveData(Systems.SaveLoad.PlayerSaveData data, MutationLibrary mutationLib, ArtifactLibrary artifactLib)
        {
            if (data == null) return;

            pieces.Clear();
            collectedArtifacts.Clear();
            brokenPieces.Clear();
            
            // Restore broken pieces
            if (data.BrokenPieces != null)
            {
                foreach (var pieceData in data.BrokenPieces)
                {
                    var stateData = new PieceStateData(pieceData.Type, Team.White, pieceData.Position);
                    
                    // Restore mutations for broken pieces
                    if (pieceData.MutationNames != null)
                    {
                        foreach (var mutationName in pieceData.MutationNames)
                        {
                            var mutation = mutationLib.GetMutationByName(mutationName);
                            if (mutation != null)
                            {
                                stateData.mutations.Add(mutation);
                            }
                        }
                    }
                    brokenPieces.Add(stateData);
                }
            }

            // Restore pieces
            foreach (var pieceData in data.Pieces)
            {
                var stateData = new PieceStateData(pieceData.Type, Team.White, pieceData.Position); // Assuming player is White
                
                // Restore mutations
                if (pieceData.MutationNames != null)
                {
                    foreach (var mutationName in pieceData.MutationNames)
                    {
                        var mutation = mutationLib.GetMutationByName(mutationName);
                        if (mutation != null)
                        {
                            stateData.mutations.Add(mutation);
                        }
                    }
                }
                pieces.Add(stateData);
            }

            // Restore artifacts (passed in separately or handled by SaveManager)
            // If we add artifact names to PlayerSaveData, we would load them here.
        }

        /// <summary>
        /// Resets the player state (for new run).
        /// </summary>
        public void Reset()
        {
            pieces.Clear();
            brokenPieces.Clear();
            collectedArtifacts.Clear();
            currency = 0;
            roomsCleared = 0;
            floorsCleared = 0;
        }
        #endregion
    }

    /// <summary>
    /// Serializable data structure for a piece's state.
    /// </summary>
    [System.Serializable]
    public class PieceStateData
    {
        public PieceType pieceType;
        public Team team;
        public Vector2Int position;
        public List<Mutation> mutations;

        public PieceStateData(PieceType type, Team pieceTeam, Vector2Int pos)
        {
            pieceType = type;
            team = pieceTeam;
            position = pos;
            mutations = new List<Mutation>();
        }

        public PieceStateData(Piece piece)
        {
            pieceType = piece.Type;
            team = piece.Team;
            position = piece.Position;
            mutations = new List<Mutation>();

            // Save mutations
            if (MutationManager.Instance != null)
            {
                mutations = new List<Mutation>(MutationManager.Instance.GetMutations(piece));
            }
        }
    }
}
