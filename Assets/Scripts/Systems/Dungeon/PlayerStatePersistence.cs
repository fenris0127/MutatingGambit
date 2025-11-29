using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// PlayerState의 저장 및 복원을 처리합니다.
    /// </summary>
    public class PlayerStatePersistence
    {
        private PlayerState state;

        public PlayerStatePersistence(PlayerState playerState)
        {
            state = playerState;
        }

        /// <summary>
        /// 현재 보드 상태를 저장합니다.
        /// </summary>
        public void SaveBoardState(Board board, RepairSystem repairSystem)
        {
            if (board == null)
            {
                Debug.LogWarning("보드가 null이므로 상태를 저장할 수 없습니다");
                return;
            }

            state.PlayerPieces.Clear();
            state.BrokenPieces.Clear();

            var playerPieces = board.GetPiecesByTeam(state.PlayerTeam);
            foreach (var piece in playerPieces)
            {
                var pieceState = CreatePieceState(piece);
                state.PlayerPieces.Add(pieceState);
            }

            if (repairSystem != null)
            {
                var brokenPieces = repairSystem.GetBrokenPiecesByTeam(state.PlayerTeam);
                foreach (var brokenPiece in brokenPieces)
                {
                    var piece = brokenPiece.GetComponent<Piece>();
                    if (piece != null)
                    {
                        var pieceState = CreatePieceState(piece);
                        state.BrokenPieces.Add(pieceState);
                    }
                }
            }

            Debug.Log($"보드 상태 저장: {state.PlayerPieces.Count}개 기물, {state.BrokenPieces.Count}개 파괴됨");
        }

        /// <summary>
        /// 저장된 상태를 보드로 복원합니다.
        /// </summary>
        public void RestoreBoardState(Board board, RepairSystem repairSystem, GameObject piecePrefab)
        {
            if (board == null)
            {
                Debug.LogWarning("보드가 null이므로 상태를 복원할 수 없습니다");
                return;
            }

            foreach (var pieceState in state.PlayerPieces)
            {
                var piece = CreatePieceFromState(pieceState, piecePrefab);
                if (piece != null)
                {
                    board.PlacePiece(piece, pieceState.position);
                }
            }

            if (repairSystem != null)
            {
                foreach (var brokenState in state.BrokenPieces)
                {
                    var piece = CreatePieceFromState(brokenState, piecePrefab);
                    if (piece != null)
                    {
                        var health = piece.GetComponent<PieceHealth>();
                        if (health == null)
                        {
                            health = piece.gameObject.AddComponent<PieceHealth>();
                        }
                        repairSystem.BreakPiece(health);
                    }
                }
            }

            Debug.Log($"상태 복원: {state.PlayerPieces.Count}개 기물, {state.BrokenPieces.Count}개 파괴됨");
        }

        /// <summary>
        /// 저장 데이터에서 로드합니다.
        /// </summary>
        public void LoadFromSaveData(SaveLoad.PlayerSaveData data, MutationLibrary mutationLib, ArtifactLibrary artifactLib)
        {
            if (data == null) return;

            state.PlayerPieces.Clear();
            state.BrokenPieces.Clear();
            state.AppliedMutations.Clear();

            foreach (var pieceData in data.Pieces)
            {
                var pieceState = new PlayerState.PieceState
                {
                    type = pieceData.Type,
                    position = new Vector2Int(pieceData.X, pieceData.Y)
                };

                if (pieceData.MutationIDs != null && mutationLib != null)
                {
                    foreach (var mutId in pieceData.MutationIDs)
                    {
                        var mutation = mutationLib.GetMutationById(mutId);
                        if (mutation != null)
                        {
                            pieceState.mutations.Add(mutation);
                        }
                    }
                }

                state.PlayerPieces.Add(pieceState);
            }

            if (data.BrokenPieceTypes != null)
            {
                foreach (var brokenType in data.BrokenPieceTypes)
                {
                    state.BrokenPieces.Add(new PlayerState.PieceState { type = brokenType });
                }
            }

            Debug.Log($"저장 데이터 로드: {state.PlayerPieces.Count}개 기물");
        }

        private PlayerState.PieceState CreatePieceState(Piece piece)
        {
            var pieceState = new PlayerState.PieceState
            {
                type = piece.Type,
                position = piece.Position
            };

            if (MutationManager.Instance != null)
            {
                var mutations = MutationManager.Instance.GetMutationsForPiece(piece);
                pieceState.mutations.AddRange(mutations);
            }

            return pieceState;
        }

        private Piece CreatePieceFromState(PlayerState.PieceState pieceState, GameObject piecePrefab)
        {
            GameObject pieceObj = piecePrefab != null
                ? Object.Instantiate(piecePrefab)
                : new GameObject($"{state.PlayerTeam}_{pieceState.type}");

            pieceObj.name = $"{state.PlayerTeam}_{pieceState.type}";

            var piece = pieceObj.GetComponent<Piece>();
            if (piece == null)
            {
                piece = pieceObj.AddComponent<Piece>();
            }

            piece.Initialize(pieceState.type, state.PlayerTeam, pieceState.position);

            foreach (var mutation in pieceState.mutations)
            {
                if (mutation != null)
                {
                    mutation.ApplyToPiece(piece);
                }
            }

            return piece;
        }
    }
}
