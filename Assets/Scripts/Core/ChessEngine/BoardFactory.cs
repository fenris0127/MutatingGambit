using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.Systems.Mutations;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Factory class for creating board setups and pieces.
    /// </summary>
    public static class BoardFactory
    {
        /// <summary>
        /// Creates standard chess piece data for a team.
        /// </summary>
        public static List<PieceStateData> CreateStandardChessPieces(Team team)
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
        /// Creates a piece GameObject from piece state data.
        /// </summary>
        public static Piece CreatePieceFromData(PieceStateData data, GameObject piecePrefab)
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
    }
}
