using UnityEngine;
using MutatingGambit.Systems.SaveLoad;
using MutatingGambit.Systems.PieceManagement;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 던전 상태의 저장 및 로드를 전담합니다.
    /// 단일 책임: 게임 저장 데이터로부터 플레이어 상태와 던전 맵 복원만 담당
    /// </summary>
    public class DungeonStateLoader
    {
        #region 의존성
        private readonly DungeonMapGenerator mapGenerator;
        #endregion

        #region 생성자
        /// <summary>
        /// DungeonStateLoader를 초기화합니다.
        /// </summary>
        /// <param name="generator">던전 맵 생성기</param>
        public DungeonStateLoader(DungeonMapGenerator generator)
        {
            this.mapGenerator = generator;
        }
        #endregion

        #region 공개 메서드 - 플레이어 상태 로드
        /// <summary>
        /// 저장 데이터에서 플레이어 상태를 로드합니다.
        /// </summary>
        /// <param name="data">저장 데이터</param>
        /// <param name="playerState">로드할 플레이어 상태</param>
        public void LoadPlayerState(GameSaveData data, PlayerState playerState)
        {
            if (data == null || playerState == null)
            {
                Debug.LogWarning("로드할 데이터 또는 플레이어 상태가 null입니다.");
                return;
            }

            LoadLibraries(out var mutationLib, out var artifactLib);
            ApplyPlayerData(data, playerState, mutationLib, artifactLib);
            LoadArtifacts(data, playerState, artifactLib);
        }
        #endregion

        #region 공개 메서드 - 던전 맵 로드
        /// <summary>
        /// 저장 데이터에서 던전 맵을 생성하고 현재 방을 복원합니다.
        /// </summary>
        /// <param name="data">저장 데이터</param>
        /// <param name="currentRoomNode">복원할 현재 방 노드(out)</param>
        /// <returns>생성된 던전 맵</returns>
        public DungeonMap LoadDungeonMap(GameSaveData data, out RoomNode currentRoomNode)
        {
            currentRoomNode = null;

            if (mapGenerator == null)
            {
                Debug.LogError("MapGenerator가 null입니다.");
                return null;
            }

            int seed = DetermineSeed(data);
            DungeonMap map = GenerateMapWithSeed(seed);
            currentRoomNode = RestoreCurrentRoom(data, map);

            return map;
        }
        #endregion

        #region 비공개 메서드 - 플레이어 로드
        /// <summary>
        /// 라이브러리들을 로드합니다.
        /// </summary>
        private void LoadLibraries(
            out Mutations.MutationLibrary mutationLib,
            out Artifacts.ArtifactLibrary artifactLib)
        {
            mutationLib = Resources.Load<Mutations.MutationLibrary>("MutationLibrary");
            artifactLib = Resources.Load<Artifacts.ArtifactLibrary>("ArtifactLibrary");
        }

        /// <summary>
        /// 플레이어 데이터를 적용합니다.
        /// </summary>
        private void ApplyPlayerData(
            GameSaveData data,
            PlayerState playerState,
            Mutations.MutationLibrary mutationLib,
            Artifacts.ArtifactLibrary artifactLib)
        {
            playerState.LoadFromSaveData(data.PlayerData, mutationLib, artifactLib);
            playerState.Currency = data.Gold;
        }

        /// <summary>
        /// 아티팩트들을 로드합니다.
        /// </summary>
        private void LoadArtifacts(GameSaveData data, PlayerState playerState, Artifacts.ArtifactLibrary artifactLib)
        {
            if (data.ActiveArtifactNames == null)
            {
                return;
            }

            foreach (var artName in data.ActiveArtifactNames)
            {
                LoadSingleArtifact(artName, playerState, artifactLib);
            }
        }

        /// <summary>
        /// 단일 아티팩트를 로드합니다.
        /// </summary>
        private void LoadSingleArtifact(string name, PlayerState playerState, Artifacts.ArtifactLibrary library)
        {
            var artifact = library.GetArtifactByName(name);
            if (artifact != null)
            {
                playerState.AddArtifact(artifact);
            }
        }
        #endregion

        #region 비공개 메서드 - 던전 맵 로드
        /// <summary>
        /// 시드를 결정합니다.
        /// </summary>
        private int DetermineSeed(GameSaveData data)
        {
            return data.DungeonSeed != 0
                ? data.DungeonSeed
                : Random.Range(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// 시드를 사용하여 맵을 생성합니다.
        /// </summary>
        private DungeonMap GenerateMapWithSeed(int seed)
        {
            return mapGenerator.GenerateMap(5, 2, 4, seed);
        }

        /// <summary>
        /// 현재 방을 복원합니다.
        /// </summary>
        private RoomNode RestoreCurrentRoom(GameSaveData data, DungeonMap map)
        {
            if (map != null &&
                map.AllNodes.Count > data.CurrentRoomIndex)
            {
                return map.AllNodes[data.CurrentRoomIndex];
            }
            return null;
        }
        #endregion
    }
}
