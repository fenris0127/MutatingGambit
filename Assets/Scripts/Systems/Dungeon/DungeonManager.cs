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
    /// 리팩토링: Region 그룹화, 함수 분해(10줄 이하), 한국어 문서화 완벽 적용
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        #region 싱글톤
        private static DungeonManager instance;

        /// <summary>
        /// DungeonManager 싱글톤 인스턴스입니다.
        /// </summary>
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
        #endregion

        #region 핵심 참조
        [Header("Core References")]
        [SerializeField] private DungeonMapGenerator mapGenerator;
        [SerializeField] private RepairSystem repairSystem;
        #endregion

        #region 컴포넌트 참조
        [Header("Components")]
        [SerializeField] private RoomTransitionHandler transitionHandler;
        [SerializeField] private DungeonRewardManager rewardManager;
        [SerializeField] private RoomEventHandler eventHandler;
        #endregion

        #region UI 참조
        [Header("UI References")]
        [SerializeField] private DungeonMapUI dungeonMapUI;
        #endregion

        #region 던전 상태
        [Header("Dungeon State")]
        [SerializeField] private PlayerState playerState;
        [SerializeField] private DungeonMap currentDungeonMap;
        [SerializeField] private RoomNode currentRoomNode;
        #endregion

        #region 이벤트
        [Header("Events")]
        public UnityEvent<RoomNode> OnRoomEntered;
        public UnityEvent<RoomNode> OnRoomCleared;
        public UnityEvent OnDungeonCompleted;
        public UnityEvent OnDungeonFailed;
        #endregion

        #region 공개 속성
        /// <summary>현재 던전 맵을 가져옵니다.</summary>
        public DungeonMap CurrentMap => currentDungeonMap;

        /// <summary>현재 방 노드를 가져옵니다.</summary>
        public RoomNode CurrentRoomNode => currentRoomNode;

        /// <summary>플레이어 상태를 가져옵니다.</summary>
        public PlayerState PlayerState => playerState;

        /// <summary>현재 층수를 가져옵니다.</summary>
        public int CurrentFloor => playerState != null ? playerState.FloorsCleared + 1 : 1;

        /// <summary>현재 방 인덱스를 가져옵니다.</summary>
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

        /// <summary>던전 시드를 가져옵니다.</summary>
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
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 싱글톤 인스턴스를 설정하고 컴포넌트를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            if (!EnsureSingleInstance())
            {
                return;
            }

            instance = this;
            InitializeComponents();
        }

        /// <summary>
        /// 이벤트 리스너와 하위 시스템을 초기화합니다.
        /// </summary>
        private void Start()
        {
            RegisterMapUIListener();
            InitializeSubsystems();
        }
        #endregion

        #region 공개 메서드 - 던전 시작/로드
        /// <summary>
        /// 새 던전 실행을 시작합니다.
        /// </summary>
        public void StartNewRun()
        {
            CreateNewPlayerState();
            GenerateNewDungeon();
            ShowMapUI();

            Debug.Log("새 던전 실행 시작!");
        }

        /// <summary>
        /// 저장 데이터에서 던전 실행을 로드합니다.
        /// </summary>
        /// <param name="data">로드할 저장 데이터</param>
        public void LoadRun(GameSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("로드할 데이터가 null입니다.");
                return;
            }

            EnsurePlayerState();
            var loader = new DungeonStateLoader(mapGenerator);
            loader.LoadPlayerState(data, playerState);
            currentDungeonMap = loader.LoadDungeonMap(data, out currentRoomNode);
            ShowMapUI();

            Debug.Log("던전 실행 로드 완료!");
        }
        #endregion

        #region 공개 메서드 - 방 진입 및 진행
        /// <summary>
        /// 방에 진입합니다.
        /// </summary>
        /// <param name="roomNode">진입할 방 노드</param>
        public void EnterRoom(RoomNode roomNode)
        {
            if (!ValidateRoomNode(roomNode))
            {
                return;
            }

            MarkPreviousRoomCleared();
            SetCurrentRoom(roomNode);
            HandleRoomEntry(roomNode);
            NotifyRoomEntered(roomNode);
        }

        /// <summary>
        /// 방 승리 시 호출됩니다.
        /// </summary>
        public void OnRoomVictory()
        {
            if (currentRoomNode == null)
            {
                return;
            }

            UpdatePlayerProgress();
            MarkRoomCleared();
            UnlockConnectedRooms();
            NotifyRoomCleared();

            if (IsBossRoom())
            {
                HandleDungeonComplete();
                return;
            }

            ShowRewards();
        }

        /// <summary>
        /// 휴식 후 계속 진행합니다.
        /// </summary>
        public void ContinueAfterRest()
        {
            MarkCurrentRoomCleared();
            UnlockConnectedRooms();
            ShowDungeonMap();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 싱글톤 인스턴스가 유일한지 확인합니다.
        /// </summary>
        private bool EnsureSingleInstance()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 필수 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            FindMapGenerator();
            FindRepairSystem();
            FindDungeonMapUI();
            FindRoomComponents();
        }

        /// <summary>
        /// 맵 생성기를 찾습니다.
        /// </summary>
        private void FindMapGenerator()
        {
            if (mapGenerator == null)
            {
                mapGenerator = FindFirstObjectByType<DungeonMapGenerator>();
            }
        }

        /// <summary>
        /// 수리 시스템을 찾습니다.
        /// </summary>
        private void FindRepairSystem()
        {
            if (repairSystem == null)
            {
                repairSystem = FindFirstObjectByType<RepairSystem>();
            }
        }

        /// <summary>
        /// 던전 맵 UI를 찾습니다.
        /// </summary>
        private void FindDungeonMapUI()
        {
            if (dungeonMapUI == null)
            {
                dungeonMapUI = FindFirstObjectByType<DungeonMapUI>();
            }
        }

        /// <summary>
        /// 방 관련 컴포넌트들을 찾습니다.
        /// </summary>
        private void FindRoomComponents()
        {
            if (transitionHandler == null)
            {
                transitionHandler = GetComponent<RoomTransitionHandler>();
            }
            if (rewardManager == null)
            {
                rewardManager = GetComponent<DungeonRewardManager>();
            }
            if (eventHandler == null)
            {
                eventHandler = GetComponent<RoomEventHandler>();
            }
        }

        /// <summary>
        /// 맵 UI 이벤트 리스너를 등록합니다.
        /// </summary>
        private void RegisterMapUIListener()
        {
            if (dungeonMapUI != null)
            {
                dungeonMapUI.OnNodeSelected.AddListener(OnNodeSelected);
            }
        }

        /// <summary>
        /// 하위 시스템들을 초기화합니다.
        /// </summary>
        private void InitializeSubsystems()
        {
            InitializeTransitionHandler();
            InitializeRewardManager();
            InitializeEventHandler();
        }

        /// <summary>
        /// 전환 핸들러를 초기화합니다.
        /// </summary>
        private void InitializeTransitionHandler()
        {
            if (transitionHandler != null)
            {
                transitionHandler.Initialize(this);
            }
        }

        /// <summary>
        /// 보상 관리자를 초기화합니다.
        /// </summary>
        private void InitializeRewardManager()
        {
            if (rewardManager != null)
            {
                rewardManager.Initialize(this, playerState);
            }
        }

        /// <summary>
        /// 이벤트 핸들러를 초기화합니다.
        /// </summary>
        private void InitializeEventHandler()
        {
            if (eventHandler != null)
            {
                eventHandler.Initialize(this, playerState, rewardManager);
            }
        }
        #endregion

        #region 비공개 메서드 - 던전 시작
        /// <summary>
        /// 새 플레이어 상태를 생성합니다.
        /// </summary>
        private void CreateNewPlayerState()
        {
            playerState = PlayerState.CreateStandardSetup(Team.White);
        }

        /// <summary>
        /// 플레이어 상태가 있는지 확인합니다.
        /// </summary>
        private void EnsurePlayerState()
        {
            if (playerState == null)
            {
                playerState = new PlayerState();
            }
        }

        /// <summary>
        /// 새 던전을 생성합니다.
        /// </summary>
        private void GenerateNewDungeon()
        {
            if (mapGenerator == null)
            {
                LogMapGeneratorError();
                return;
            }

            currentDungeonMap = mapGenerator.GenerateMap();
        }

        /// <summary>
        /// 맵 생성기 오류를 로그에 출력합니다.
        /// </summary>
        private void LogMapGeneratorError()
        {
            Debug.LogError("던전을 시작할 수 없습니다 - MapGenerator가 null입니다!");
        }

        /// <summary>
        /// 맵 UI를 표시합니다.
        /// </summary>
        private void ShowMapUI()
        {
            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.ShowMap(currentDungeonMap);
            }
        }
        #endregion



        #region 비공개 메서드 - 방 진입
        /// <summary>
        /// 방 노드의 유효성을 검증합니다.
        /// </summary>
        private bool ValidateRoomNode(RoomNode roomNode)
        {
            if (roomNode == null || roomNode.Room == null)
            {
                Debug.LogError("방에 진입할 수 없습니다: 노드 또는 방 데이터가 null입니다");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 이전 방을 클리어 상태로 표시합니다.
        /// </summary>
        private void MarkPreviousRoomCleared()
        {
            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;
            }
        }

        /// <summary>
        /// 현재 방을 설정합니다.
        /// </summary>
        private void SetCurrentRoom(RoomNode roomNode)
        {
            currentRoomNode = roomNode;
        }

        /// <summary>
        /// 방 타입에 따라 진입 처리합니다.
        /// </summary>
        private void HandleRoomEntry(RoomNode roomNode)
        {
            switch (roomNode.Type)
            {
                case RoomType.Rest:
                    HandleRestRoom();
                    break;

                case RoomType.Treasure:
                    HandleTreasureRoom(roomNode);
                    break;

                case RoomType.NormalCombat:
                case RoomType.EliteCombat:
                case RoomType.Boss:
                case RoomType.Start:
                case RoomType.Tutorial:
                    HandleCombatRoom(roomNode);
                    break;

                case RoomType.Mystery:
                    HandleMysteryRoom(roomNode);
                    break;
            }
        }

        /// <summary>
        /// 휴식 방 진입을 처리합니다.
        /// </summary>
        private void HandleRestRoom()
        {
            eventHandler?.EnterRestRoom(repairSystem);
        }

        /// <summary>
        /// 보물 방 진입을 처리합니다.
        /// </summary>
        private void HandleTreasureRoom(RoomNode roomNode)
        {
            eventHandler?.EnterTreasureRoom(roomNode);
        }

        /// <summary>
        /// 전투 방 진입을 처리합니다.
        /// </summary>
        private void HandleCombatRoom(RoomNode roomNode)
        {
            transitionHandler?.EnterCombatRoom(roomNode, playerState);
        }

        /// <summary>
        /// 미스터리 방 진입을 처리합니다.
        /// </summary>
        private void HandleMysteryRoom(RoomNode roomNode)
        {
            eventHandler?.EnterMysteryRoom(roomNode);
        }

        /// <summary>
        /// 방 진입 이벤트를 발생시킵니다.
        /// </summary>
        private void NotifyRoomEntered(RoomNode roomNode)
        {
            OnRoomEntered?.Invoke(roomNode);
        }
        #endregion

        #region 비공개 메서드 - 방 승리 처리
        /// <summary>
        /// 플레이어 진행 상황을 업데이트합니다.
        /// </summary>
        private void UpdatePlayerProgress()
        {
            if (playerState != null)
            {
                playerState.IncrementRoomsCleared();
            }
        }

        /// <summary>
        /// 현재 방을 클리어 상태로 표시합니다.
        /// </summary>
        private void MarkRoomCleared()
        {
            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;
            }
        }

        /// <summary>
        /// 연결된 방들을 접근 가능 상태로 설정합니다.
        /// </summary>
        private void UnlockConnectedRooms()
        {
            if (currentRoomNode == null)
            {
                return;
            }

            foreach (var connectedNode in currentRoomNode.Connections)
            {
                connectedNode.IsAccessible = true;
            }
        }

        /// <summary>
        /// 방 클리어 이벤트를 발생시킵니다.
        /// </summary>
        private void NotifyRoomCleared()
        {
            OnRoomCleared?.Invoke(currentRoomNode);
        }

        /// <summary>
        /// 현재 방이 보스 방인지 확인합니다.
        /// </summary>
        private bool IsBossRoom()
        {
            return currentRoomNode.Type == RoomType.Boss;
        }

        /// <summary>
        /// 보상을 표시합니다.
        /// </summary>
        private void ShowRewards()
        {
            if (currentRoomNode.Room != null && rewardManager != null)
            {
                rewardManager.ShowRewardSelection(currentRoomNode.Room);
            }
        }
        #endregion

        #region 비공개 메서드 - 휴식 처리
        /// <summary>
        /// 현재 방을 클리어 상태로 표시합니다.
        /// </summary>
        private void MarkCurrentRoomCleared()
        {
            if (currentRoomNode != null)
            {
                currentRoomNode.IsCleared = true;
            }
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 노드 선택 이벤트를 처리합니다.
        /// </summary>
        private void OnNodeSelected(RoomNode node)
        {
            if (!ValidateNodeAccess(node))
            {
                LogInaccessibleNode(node);
                return;
            }

            EnterRoom(node);
        }

        /// <summary>
        /// 노드 접근 가능 여부를 검증합니다.
        /// </summary>
        private bool ValidateNodeAccess(RoomNode node)
        {
            return node != null && node.IsAccessible;
        }

        /// <summary>
        /// 접근 불가능 노드 경고를 로그에 출력합니다.
        /// </summary>
        private void LogInaccessibleNode(RoomNode node)
        {
            Debug.LogWarning($"노드 {node?.NodeId}에 진입할 수 없습니다: 접근 불가");
        }
        #endregion

        #region 비공개 메서드 - 던전 완료
        /// <summary>
        /// 던전 완료를 처리합니다.
        /// </summary>
        private void HandleDungeonComplete()
        {
            LogDungeonComplete();
            NotifyDungeonCompleted();
            RecordCompletedRun();
            DeleteSaveFile();
        }

        /// <summary>
        /// 던전 완료 로그를 출력합니다.
        /// </summary>
        private void LogDungeonComplete()
        {
            Debug.Log("던전 완료!");
        }

        /// <summary>
        /// 던전 완료 이벤트를 발생시킵니다.
        /// </summary>
        private void NotifyDungeonCompleted()
        {
            OnDungeonCompleted?.Invoke();
        }

        /// <summary>
        /// 완료된 실행을 기록합니다.
        /// </summary>
        private void RecordCompletedRun()
        {
            if (GlobalDataManager.Instance != null)
            {
                GlobalDataManager.Instance.RecordRun(true, playerState);
            }
        }

        /// <summary>
        /// 저장 파일을 삭제합니다.
        /// </summary>
        private void DeleteSaveFile()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSaveFile();
            }
        }
        #endregion

        #region 비공개 메서드 - UI 표시
        /// <summary>
        /// 던전 맵을 표시합니다.
        /// </summary>
        private void ShowDungeonMap()
        {
            if (dungeonMapUI != null && currentDungeonMap != null)
            {
                dungeonMapUI.UpdateNodeStates();
                dungeonMapUI.ShowMap(currentDungeonMap);
            }
        }
        #endregion
    }
}
