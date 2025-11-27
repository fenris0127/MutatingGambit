using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.PieceManagement;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Stores the persistent state of the player across rooms in the dungeon.
    /// Includes pieces, broken pieces, artifacts, and run progress.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
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
        /// Creates a new player state with a standard chess setup.
        /// </summary>
        public static PlayerState CreateStandardSetup(Team team)
        {
            PlayerState state = new PlayerState();
            state.playerTeam = team;
            state.currency = 0;
            state.roomsCleared = 0;
            state.floorsCleared = 0;

            // Create standard chess pieces
            state.pieces = CreateStandardChessPieces(team);

            return state;
        }

        /// <summary>
        /// Creates standard chess piece data for a team.
        /// </summary>
        private static List<PieceStateData> CreateStandardChessPieces(Team team)
        {
            var pieces = new List<PieceStateData>();
            int backRow = team == Team.White ? 0 : 7;
            int pawnRow = team == Team.White ? 1 : 6;

            // Back row
            pieces.Add(new PieceStateData(PieceType.Rook, team, new Vector2Int(0, backRow)));
            pieces.Add(new PieceStateData(PieceType.Knight, team, new Vector2Int(1, backRow)));
            pieces.Add(new PieceStateData(PieceType.Bishop, team, new Vector2Int(2, backRow)));
            pieces.Add(new PieceStateData(PieceType.Queen, team, new Vector2Int(3, backRow)));
            pieces.Add(new PieceStateData(PieceType.King, team, new Vector2Int(4, backRow)));
            pieces.Add(new PieceStateData(PieceType.Bishop, team, new Vector2Int(5, backRow)));
            pieces.Add(new PieceStateData(PieceType.Knight, team, new Vector2Int(6, backRow)));
            pieces.Add(new PieceStateData(PieceType.Rook, team, new Vector2Int(7, backRow)));

            // Pawns
            for (int x = 0; x < 8; x++)
            {
                pieces.Add(new PieceStateData(PieceType.Pawn, team, new Vector2Int(x, pawnRow)));
            }

            return pieces;
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
                var brokenPiecesList = repairSystem.GetBrokenPieces(playerTeam);
                foreach (var pieceHealth in brokenPiecesList)
                {
                    if (pieceHealth != null && pieceHealth.Piece != null)
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
                Piece piece = CreatePieceFromData(pieceData, piecePrefab);
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
                    Piece piece = CreatePieceFromData(pieceData, piecePrefab);
                    if (piece != null)
                    {
                        var pieceHealth = piece.gameObject.AddComponent<PieceHealth>();
                        pieceHealth.Initialize(piece);
                        repairSystem.BreakPiece(pieceHealth);
                    }
                }
            }

            Debug.Log($"Restored player state: {pieces.Count} active pieces, {brokenPieces.Count} broken pieces");
        }

        /// <summary>
        /// Creates a piece GameObject from piece state data.
        /// </summary>
        private Piece CreatePieceFromData(PieceStateData data, GameObject piecePrefab)
        {
            GameObject pieceObject;

            if (piecePrefab != null)
            {
                pieceObject = GameObject.Instantiate(piecePrefab);
                pieceObject.name = $"{data.team}_{data.pieceType}";
            }
            else
            {
                pieceObject = new GameObject($"{data.team}_{data.pieceType}");
            }

            Piece piece = pieceObject.GetComponent<Piece>();
            if (piece == null)
            {
                piece = pieceObject.AddComponent<Piece>();
            }

            piece.Initialize(data.pieceType, data.team, data.position);

            // Restore mutations
            if (data.mutations != null)
            {
                foreach (var mutation in data.mutations)
                {
                    if (mutation != null && MutationManager.Instance != null)
                    {
                        MutationManager.Instance.ApplyMutation(piece, mutation);
                    }
                }
            }

            return piece;
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
        public void RemoveArtifact(Artifact artifact)
        {
            collectedArtifacts.Remove(artifact);
        }

        /// <summary>
        /// Checks if the player has a specific artifact.
        /// </summary>
        public bool HasArtifact(Artifact artifact)
        {
            return collectedArtifacts.Contains(artifact);
        }

        /// <summary>
        /// Increments the room clear counter.
        /// </summary>
        public void IncrementRoomsCleared()
        {
            roomsCleared++;
        }

        /// <summary>
        /// Increments the floor clear counter.
        /// </summary>
        public void IncrementFloorsCleared()
        {
            floorsCleared++;
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
