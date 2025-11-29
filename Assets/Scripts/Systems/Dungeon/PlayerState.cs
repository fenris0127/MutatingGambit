using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 던전 실행 중 플레이어의 지속 상태를 저장합니다.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
        [System.Serializable]
        public class PieceState
        {
            public PieceType type;
            public Vector2Int position;
            public List<Mutation> mutations = new List<Mutation>();
        }

        public Team PlayerTeam = Team.White;
        public List<PieceState> PlayerPieces = new List<PieceState>();
        public List<PieceState> BrokenPieces = new List<PieceState>();
        public List<Artifact> CollectedArtifacts = new List<Artifact>();
        public List<Mutation> AppliedMutations = new List<Mutation>();
        public int Currency = 100;
        public int FloorsCleared = 0;
        public int RoomsCleared = 0;
        public int TotalMoves = 0;

        private PlayerStatePersistence persistence;

        public PlayerState()
        {
            persistence = new PlayerStatePersistence(this);
        }

        /// <summary>
        /// 표준 체스 설정으로 새 PlayerState를 생성합니다.
        /// </summary>
        public static PlayerState CreateStandardSetup(Team team)
        {
            var state = new PlayerState();
            state.PlayerTeam = team;

            int backRank = team == Team.White ? 0 : 7;
            int pawnRank = team == Team.White ? 1 : 6;

            // 뒷줄 기물
            state.PlayerPieces.Add(new PieceState { type = PieceType.Rook, position = new Vector2Int(0, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.Knight, position = new Vector2Int(1, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.Bishop, position = new Vector2Int(2, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.Queen, position = new Vector2Int(3, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.King, position = new Vector2Int(4, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.Bishop, position = new Vector2Int(5, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.Knight, position = new Vector2Int(6, backRank) });
            state.PlayerPieces.Add(new PieceState { type = PieceType.Rook, position = new Vector2Int(7, backRank) });

            // 폰
            for (int x = 0; x < 8; x++)
            {
                state.PlayerPieces.Add(new PieceState { type = PieceType.Pawn, position = new Vector2Int(x, pawnRank) });
            }

            return state;
        }

        public void AddArtifact(Artifact artifact)
        {
            if (artifact != null && !CollectedArtifacts.Contains(artifact))
            {
                CollectedArtifacts.Add(artifact);
            }
        }

        public void AddMutation(Mutation mutation)
        {
            if (mutation != null && !AppliedMutations.Contains(mutation))
            {
                AppliedMutations.Add(mutation);
            }
        }

        public void IncrementFloorsCleared() => FloorsCleared++;
        public void IncrementRoomsCleared() => RoomsCleared++;
        public void IncrementMoves() => TotalMoves++;

        // 위임 메서드
        public void SaveBoardState(Board board, RepairSystem repairSystem) =>
            persistence.SaveBoardState(board, repairSystem);

        public void RestoreBoardState(Board board, RepairSystem repairSystem, GameObject piecePrefab) =>
            persistence.RestoreBoardState(board, repairSystem, piecePrefab);

        public void LoadFromSaveData(SaveLoad.PlayerSaveData data, MutationLibrary mutationLib, ArtifactLibrary artifactLib) =>
            persistence.LoadFromSaveData(data, mutationLib, artifactLib);
    }
}
