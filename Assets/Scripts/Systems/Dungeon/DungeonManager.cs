using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.SaveLoad;
using MutatingGambit.UI;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 던전 진행, 방 전환, 플레이어 상태 지속성을 관리합니다.
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        private static DungeonManager instance;

        [Header("Core References")]
        [SerializeField] private DungeonMapGenerator mapGenerator;
        [SerializeField] private RepairSystem repairSystem;

        [Header("Components")]
        [SerializeField] private RoomTransitionHandler transitionHandler;
        [SerializeField] private DungeonRewardManager rewardManager;
        [SerializeField] private RoomEventHandler eventHandler;

        [Header("UI References")]
        [SerializeField] private DungeonMapUI dungeonMapUI;

        [Header("Dungeon State")]
        [SerializeField] private PlayerState playerState;
        [SerializeField] private DungeonMap currentDungeonMap;
        [SerializeField] private RoomNode currentRoomNode;

        [Header("Events")]
        public UnityEvent<RoomNode> OnRoomEntered;
        public UnityEvent<RoomNode> OnRoomCleared;
        public UnityEvent OnDungeonCompleted;
        public UnityEvent OnDungeonFailed;

        public static DungeonManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DungeonManager>();
                }
                return instance;
            }
        }

        public DungeonMap CurrentMap => currentDungeonMap;
        public RoomNode CurrentRoomNode => currentRoomNode;
        public PlayerState PlayerState => playerState;
        public int CurrentFloor => playerState != null ? playerState.FloorsCleared + 1 : 1;
        
        public int CurrentRoomIndex
        {
            get
            {
                if (currentDungeonMap != null && currentRoomNode != null)
                {
                    return currentDungeonMap.AllNodes.IndexOf(currentRoomNode);
                }
                return 0;
            }
        }

        public int Seed
        {
            get
            {
                if (currentDungeonMap != null)
                {
                    return currentDungeonMap.Seed;
                }
                return 0;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            InitializeComponents();
        }

        private void Start()
        {
            if (dungeonMapUI != null)
            {
                dungeonMapUI.OnNodeSelected.AddListener(OnNodeSelected);
            }

            if (transitionHandler != null)
            {
                transitionHandler.Initialize(this);
            }

            if (rewardManager != null)
            {
                rewardManager.Initialize(this, playerState);
            }

            if (eventHandler != null)
            {
                eventHandler.Initialize(this, playerState, rewardManager);
            }
        }

        /// <summary>
        /// 새 던전 실행을 시작합니다.
        /// </summary>
        public void StartNewRun()
        {
            playerState = PlayerState.CreateStandardSetup(Team.White);

            if (mapGenerator != null)
            {
                currentDungeonMap = mapGenerator.GenerateMap();
            }
            else
            {
                Debug.LogError("던전을 시작할 수 없습니다 - MapGenerator가 null입니다!");
                return;
            }

            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.ShowMap(currentDungeonMap);
            }

            Debug.Log("새 던전 실행 시작!");
        }

        /// <summary>
        /// 저장 데이터에서 던전 실행을 로드합니다.
        /// </summary>
        public void LoadRun(GameSaveData data)
        {
            if (data == null) return;

            if (playerState == null) playerState = new PlayerState();
            
            var mutationLib = Resources.Load<Mutations.MutationLibrary>("MutationLibrary");
            var artifactLib = Resources.Load<Artifacts.ArtifactLibrary>("ArtifactLibrary");
            
            playerState.LoadFromSaveData(data.PlayerData, mutationLib, artifactLib);
            playerState.Currency = data.Gold;

            if (data.ActiveArtifactNames != null)
            {
                foreach (var artName in data.ActiveArtifactNames)
                {
                    var artifact = artifactLib.GetArtifactByName(artName);
                    if (artifact != null)
                    {
                        playerState.AddArtifact(artifact);
                    }
                }
            }

            if (mapGenerator != null)
            {
                int seed = data.DungeonSeed != 0 ? data.DungeonSeed : Random.Range(int.MinValue, int.MaxValue);
                currentDungeonMap = mapGenerator.GenerateMap(5, 2, 4, seed);
                
                if (currentDungeonMap != null && currentDungeonMap.AllNodes.Count > data.CurrentRoomIndex)
                {
                    currentRoomNode = currentDungeonMap.AllNodes[data.CurrentRoomIndex];
                }
            }

            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.ShowMap(currentDungeonMap);
            }
            
            Debug.Log("던전 실행 로드 완료!");
        }

        /// <summary>
        /// 방에 진입합니다.
        /// </summary>
        public void EnterRoom(RoomNode roomNode)
        {
            if (roomNode == null || roomNode.Room == null)
            {
                Debug.LogError("방에 진입할 수 없습니다: 노드 또는 방 데이터가 null입니다");
                return;
            }

            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;
            }

            currentRoomNode = roomNode;

            switch (roomNode.Type)
            {
                case RoomType.Rest:
                    eventHandler?.EnterRestRoom(repairSystem);
                    break;

                case RoomType.Treasure:
                    eventHandler?.EnterTreasureRoom(roomNode);
                    break;

                case RoomType.NormalCombat:
                case RoomType.EliteCombat:
                case RoomType.Boss:
                case RoomType.Start:
                case RoomType.Tutorial:
                    transitionHandler?.EnterCombatRoom(roomNode, playerState);
                    break;

                case RoomType.Mystery:
                    eventHandler?.EnterMysteryRoom(roomNode);
                    break;
            }

            OnRoomEntered?.Invoke(roomNode);
        }

        /// <summary>
        /// 방 승리 시 호출됩니다.
        /// </summary>
        public void OnRoomVictory()
        {
            if (currentRoomNode == null) return;

            if (playerState != null)
            {
                playerState.IncrementRoomsCleared();
            }

            currentRoomNode.IsCleared = true;

            foreach (var connectedNode in currentRoomNode.Connections)
            {
                connectedNode.IsAccessible = true;
            }

            OnRoomCleared?.Invoke(currentRoomNode);

            if (currentRoomNode.Type == RoomType.Boss)
            {
                HandleDungeonComplete();
                return;
            }

            if (currentRoomNode.Room != null && rewardManager != null)
            {
                rewardManager.ShowRewardSelection(currentRoomNode.Room);
            }
        }

        /// <summary>
        /// 휴식 후 계속 진행합니다.
        /// </summary>
        public void ContinueAfterRest()
        {
            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;

                foreach (var connectedNode in currentRoomNode.Connections)
                {
                    connectedNode.IsAccessible = true;
                }
            }

            ShowDungeonMap();
        }

        private void InitializeComponents()
        {
            if (mapGenerator == null) mapGenerator = FindFirstObjectByType<DungeonMapGenerator>();
            if (repairSystem == null) repairSystem = FindFirstObjectByType<RepairSystem>();
            if (dungeonMapUI == null) dungeonMapUI = FindFirstObjectByType<DungeonMapUI>();
            if (transitionHandler == null) transitionHandler = GetComponent<RoomTransitionHandler>();
            if (rewardManager == null) rewardManager = GetComponent<DungeonRewardManager>();
            if (eventHandler == null) eventHandler = GetComponent<RoomEventHandler>();
        }

        private void OnNodeSelected(RoomNode node)
        {
            if (node == null || !node.IsAccessible)
            {
                Debug.LogWarning($"노드 {node?.NodeId}에 진입할 수 없습니다: 접근 불가");
                return;
            }

            EnterRoom(node);
        }

        private void HandleDungeonComplete()
        {
            Debug.Log("던전 완료!");
            OnDungeonCompleted?.Invoke();
            
            if (GlobalDataManager.Instance != null)
            {
                GlobalDataManager.Instance.RecordRun(true, playerState);
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSaveFile();
            }
        }

        private void ShowDungeonMap()
        {
            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.UpdateNodeStates();
                dungeonMapUI.ShowMap(currentDungeonMap);
            }
        }
    }
}
