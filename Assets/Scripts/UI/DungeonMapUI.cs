using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 던전 맵을 Slay the Spire 스타일로 표시합니다.
    /// 리팩토링: SRP 준수, Region 그룹화, 함수 크기 제약(10줄), 한국어 문서화
    /// </summary>
    public class DungeonMapUI : MonoBehaviour
    {
        #region UI 참조
        [Header("UI References")]
        [SerializeField]
        private GameObject mapPanel;

        [SerializeField]
        private Transform nodeContainer;

        [SerializeField]
        private GameObject nodeButtonPrefab;

        [SerializeField]
        private LineRenderer connectionLinePrefab;
        #endregion

        #region 레이아웃 설정
        [Header("Layout Settings")]
        [SerializeField]
        private float layerSpacing = 150f;

        [SerializeField]
        private float nodeSpacing = 100f;

        [SerializeField]
        private Vector2 mapOffset = new Vector2(0f, 0f);
        #endregion

        #region 색상 설정
        [Header("Visual Settings")]
        [SerializeField]
        private Color normalNodeColor = Color.gray;

        [SerializeField]
        private Color eliteNodeColor = Color.red;

        [SerializeField]
        private Color bossNodeColor = new Color(0.8f, 0f, 0.8f);

        [SerializeField]
        private Color treasureNodeColor = Color.yellow;

        [SerializeField]
        private Color restNodeColor = Color.green;

        [SerializeField]
        private Color clearedNodeColor = new Color(0.5f, 0.5f, 0.5f);

        [SerializeField]
        private Color accessibleNodeColor = Color.white;
        #endregion

        #region 이벤트
        [Header("Events")]
        public UnityEvent<RoomNode> OnNodeSelected;
        #endregion

        #region 상태 변수
        private DungeonMap currentMap;
        private Dictionary<RoomNode, DungeonMapNodeUI> nodeUIMap = new Dictionary<RoomNode, DungeonMapNodeUI>();
        
        // 헬퍼 클래스들
        private DungeonMapLayoutCalculator layoutCalculator;
        private DungeonMapColorProvider colorProvider;
        private DungeonMapNodeFactory nodeFactory;
        private DungeonMapConnectionRenderer connectionRenderer;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 초기화 시 맵 패널을 숨깁니다.
        /// </summary>
        private void Awake()
        {
            InitializeHelpers();
            HideMapPanel();
        }
        #endregion

        #region 공개 메서드 - 맵 표시/숨김
        /// <summary>
        /// 던전 맵을 표시합니다.
        /// </summary>
        /// <param name="map">표시할 던전 맵</param>
        public void ShowMap(DungeonMap map)
        {
            if (map == null)
            {
                Debug.LogWarning("표시할 맵이 null입니다.");
                return;
            }

            currentMap = map;
            RegenerateMapUI();
            ShowMapPanel();
        }

        /// <summary>
        /// 던전 맵을 숨깁니다.
        /// </summary>
        public void HideMap()
        {
            HideMapPanel();
        }

        /// <summary>
        /// 모든 노드의 상태를 업데이트합니다 (접근 가능 여부, 클리어 여부 등).
        /// </summary>
        public void UpdateNodeStates()
        {
            foreach (var kvp in nodeUIMap)
            {
                UpdateSingleNodeState(kvp.Key, kvp.Value);
            }
        }
        #endregion

        #region 공개 속성
        /// <summary>
        /// 맵이 현재 표시 중인지 여부를 확인합니다.
        /// </summary>
        public bool IsVisible => mapPanel != null && mapPanel.activeSelf;
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 헬퍼 클래스들을 초기화합니다.
        /// </summary>
        private void InitializeHelpers()
        {
            layoutCalculator = new DungeonMapLayoutCalculator(layerSpacing, nodeSpacing, mapOffset);
            colorProvider = new DungeonMapColorProvider(
                normalNodeColor, eliteNodeColor, bossNodeColor, treasureNodeColor, restNodeColor);
            nodeFactory = new DungeonMapNodeFactory(nodeButtonPrefab, nodeContainer);
            connectionRenderer = new DungeonMapConnectionRenderer(connectionLinePrefab, nodeContainer);
        }
        #endregion

        #region 비공개 메서드 - UI 생성
        /// <summary>
        /// 맵 UI를 재생성합니다.
        /// </summary>
        private void RegenerateMapUI()
        {
            ClearAllUI();
            
            if (!ValidateMapData())
            {
                return;
            }

            CreateAllNodes();
            CreateAllConnections();
            UpdateNodeStates();
        }

        /// <summary>
        /// 맵 데이터의 유효성을 검증합니다.
        /// </summary>
        private bool ValidateMapData()
        {
            if (currentMap == null || nodeContainer == null)
            {
                Debug.LogWarning("맵 또는 노드 컨테이너가 null입니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 모든 노드 UI를 생성합니다.
        /// </summary>
        private void CreateAllNodes()
        {
            foreach (var node in currentMap.AllNodes)
            {
                CreateSingleNode(node);
            }
        }

        /// <summary>
        /// 단일 노드 UI를 생성합니다.
        /// </summary>
        private void CreateSingleNode(RoomNode node)
        {
            GameObject nodeObject = nodeFactory.CreateNodeObject(node.NodeId);
            Vector2 position = layoutCalculator.CalculateNodePosition(node, currentMap);
            nodeFactory.SetNodePosition(nodeObject, position);

            Color color = colorProvider.GetNodeColor(node.Type);
            DungeonMapNodeUI nodeUI = nodeFactory.SetupNodeComponent(nodeObject, node, color);
            
            RegisterNodeEvents(nodeUI, node);
            nodeUIMap[node] = nodeUI;
        }

        /// <summary>
        /// 노드의 이벤트를 등록합니다.
        /// </summary>
        private void RegisterNodeEvents(DungeonMapNodeUI nodeUI, RoomNode node)
        {
            nodeUI.OnClick += () => HandleNodeClick(node);
        }

        /// <summary>
        /// 모든 연결선을 생성합니다.
        /// </summary>
        private void CreateAllConnections()
        {
            foreach (var node in currentMap.AllNodes)
            {
                CreateConnectionsForNode(node);
            }
        }

        /// <summary>
        /// 단일 노드의 모든 연결선을 생성합니다.
        /// </summary>
        private void CreateConnectionsForNode(RoomNode node)
        {
            foreach (var connectedNode in node.Connections)
            {
                CreateConnectionBetweenNodes(node, connectedNode);
            }
        }

        /// <summary>
        /// 두 노드 사이의 연결선을 생성합니다.
        /// </summary>
        private void CreateConnectionBetweenNodes(RoomNode fromNode, RoomNode toNode)
        {
            if (!nodeUIMap.ContainsKey(fromNode) || !nodeUIMap.ContainsKey(toNode))
            {
                return;
            }

            connectionRenderer.CreateConnection(nodeUIMap[fromNode], nodeUIMap[toNode]);
        }
        #endregion

        #region 비공개 메서드 - 상태 업데이트
        /// <summary>
        /// 단일 노드의 상태를 업데이트합니다.
        /// </summary>
        private void UpdateSingleNodeState(RoomNode node, DungeonMapNodeUI nodeUI)
        {
            var newState = DetermineNodeState(node);
            nodeUI.SetState(newState);
        }

        /// <summary>
        /// 노드의 현재 상태를 결정합니다.
        /// </summary>
        private DungeonMapNodeUI.NodeState DetermineNodeState(RoomNode node)
        {
            if (node.IsCleared)
            {
                return DungeonMapNodeUI.NodeState.Cleared;
            }
            
            if (node.IsAccessible)
            {
                return DungeonMapNodeUI.NodeState.Accessible;
            }
            
            return DungeonMapNodeUI.NodeState.Locked;
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 노드 클릭 이벤트를 처리합니다.
        /// </summary>
        private void HandleNodeClick(RoomNode node)
        {
            if (!ValidateNodeAccess(node))
            {
                LogInaccessibleNode(node);
                return;
            }

            NotifyNodeSelection(node);
            HideMap();
        }

        /// <summary>
        /// 노드 접근 가능 여부를 검증합니다.
        /// </summary>
        private bool ValidateNodeAccess(RoomNode node)
        {
            return node.IsAccessible;
        }

        /// <summary>
        /// 접근 불가능 노드 로그를 출력합니다.
        /// </summary>
        private void LogInaccessibleNode(RoomNode node)
        {
            Debug.Log($"노드 {node.NodeId}에 접근할 수 없습니다!");
        }

        /// <summary>
        /// 노드 선택을 외부에 알립니다.
        /// </summary>
        private void NotifyNodeSelection(RoomNode node)
        {
            OnNodeSelected?.Invoke(node);
        }
        #endregion

        #region 비공개 메서드 - UI 정리
        /// <summary>
        /// 모든 UI 요소를 제거합니다.
        /// </summary>
        private void ClearAllUI()
        {
            ClearAllNodes();
            connectionRenderer.ClearAllConnections();
        }

        /// <summary>
        /// 모든 노드를 제거합니다.
        /// </summary>
        private void ClearAllNodes()
        {
            foreach (var nodeUI in nodeUIMap.Values)
            {
                if (nodeUI != null)
                {
                    Destroy(nodeUI.gameObject);
                }
            }
            nodeUIMap.Clear();
        }
        #endregion

        #region 비공개 메서드 - 패널 제어
        /// <summary>
        /// 맵 패널을 표시합니다.
        /// </summary>
        private void ShowMapPanel()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 맵 패널을 숨깁니다.
        /// </summary>
        private void HideMapPanel()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
            }
        }
        #endregion
    }

    /// <summary>
    /// 던전 맵의 개별 노드 UI 컴포넌트입니다.
    /// </summary>
    public class DungeonMapNodeUI : MonoBehaviour
    {
        #region 열거형
        /// <summary>
        /// 노드의 상태를 나타냅니다.
        /// </summary>
        public enum NodeState { Locked, Accessible, Cleared, Current }
        #endregion

        #region UI 참조
        [Header("UI References")]
        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI labelText;

        [SerializeField]
        private Button button;
        #endregion

        #region 상태 변수
        private RoomNode roomNode;
        private NodeState currentState = NodeState.Locked;
        private Color baseColor = Color.gray;

        /// <summary>
        /// 클릭 이벤트 콜백
        /// </summary>
        public System.Action OnClick;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 버튼 컴포넌트를 초기화하고 클릭 리스너를 등록합니다.
        /// </summary>
        private void Awake()
        {
            EnsureButtonComponent();
            RegisterClickListener();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 노드 UI를 초기화합니다.
        /// </summary>
        /// <param name="node">연결할 방 노드</param>
        /// <param name="color">기본 색상</param>
        public void Initialize(RoomNode node, Color color)
        {
            roomNode = node;
            baseColor = color;

            UpdateLabel();
            UpdateVisuals();
        }

        /// <summary>
        /// 노드의 시각적 상태를 설정합니다.
        /// </summary>
        /// <param name="state">새 상태</param>
        public void SetState(NodeState state)
        {
            currentState = state;
            UpdateVisuals();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 버튼 컴포넌트를 확보합니다.
        /// </summary>
        private void EnsureButtonComponent()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }

        /// <summary>
        /// 클릭 리스너를 등록합니다.
        /// </summary>
        private void RegisterClickListener()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }
        #endregion

        #region 비공개 메서드 - UI 업데이트
        /// <summary>
        /// 라벨 텍스트를 업데이트합니다.
        /// </summary>
        private void UpdateLabel()
        {
            if (labelText != null && roomNode != null)
            {
                labelText.text = GetRoomTypeIcon(roomNode.Type);
            }
        }

        /// <summary>
        /// 상태에 따라 시각적 표현을 업데이트합니다.
        /// </summary>
        private void UpdateVisuals()
        {
            if (backgroundImage == null)
            {
                return;
            }

            UpdateBackgroundColor();
            UpdateInteractability();
        }

        /// <summary>
        /// 배경 색상을 업데이트합니다.
        /// </summary>
        private void UpdateBackgroundColor()
        {
            Color displayColor = GetColorForState();
            backgroundImage.color = displayColor;
        }

        /// <summary>
        /// 현재 상태에 맞는 색상을 가져옵니다.
        /// </summary>
        private Color GetColorForState()
        {
            return currentState switch
            {
                NodeState.Locked => baseColor * 0.5f,
                NodeState.Accessible => baseColor,
                NodeState.Cleared => Color.gray,
                NodeState.Current => Color.yellow,
                _ => baseColor
            };
        }

        /// <summary>
        /// 버튼 상호작용 가능 여부를 업데이트합니다.
        /// </summary>
        private void UpdateInteractability()
        {
            if (button != null)
            {
                button.interactable = (currentState == NodeState.Accessible);
            }
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 버튼 클릭을 처리합니다.
        /// </summary>
        private void HandleClick()
        {
            OnClick?.Invoke();
        }
        #endregion

        #region 비공개 메서드 - 유틸리티
        /// <summary>
        /// 방 타입에 해당하는 아이콘 문자를 가져옵니다.
        /// </summary>
        /// <param name="roomType">방 타입</param>
        /// <returns>아이콘 문자</returns>
        private string GetRoomTypeIcon(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.NormalCombat => "⚔",
                RoomType.EliteCombat => "⚔⚔",
                RoomType.Boss => "👑",
                RoomType.Treasure => "💎",
                RoomType.Rest => "🏕",
                RoomType.Mystery => "?",
                RoomType.Start => "►",
                _ => "◯"
            };
        }
        #endregion
    }
}
