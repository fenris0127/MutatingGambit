using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Displays the dungeon map with nodes and connections in a Slay the Spire style.
    /// </summary>
    public class DungeonMapUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject mapPanel;

        [SerializeField]
        private Transform nodeContainer;

        [SerializeField]
        private GameObject nodeButtonPrefab;

        [SerializeField]
        private LineRenderer connectionLinePrefab;

        [Header("Layout Settings")]
        [SerializeField]
        private float layerSpacing = 150f;

        [SerializeField]
        private float nodeSpacing = 100f;

        [SerializeField]
        private Vector2 mapOffset = new Vector2(0f, 0f);

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

        [Header("Events")]
        public UnityEvent<RoomNode> OnNodeSelected;

        private DungeonMap currentMap;
        private Dictionary<RoomNode, DungeonMapNodeUI> nodeUIMap = new Dictionary<RoomNode, DungeonMapNodeUI>();
        private List<LineRenderer> connectionLines = new List<LineRenderer>();

        private void Awake()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Displays the dungeon map.
        /// </summary>
        public void ShowMap(DungeonMap map)
        {
            currentMap = map;
            GenerateMapUI();

            if (mapPanel != null)
            {
                mapPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the dungeon map.
        /// </summary>
        public void HideMap()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Generates the visual representation of the map.
        /// </summary>
        private void GenerateMapUI()
        {
            ClearMapUI();

            if (currentMap == null || nodeContainer == null)
            {
                return;
            }

            // Create node UI elements
            foreach (var node in currentMap.AllNodes)
            {
                CreateNodeUI(node);
            }

            // Create connection lines
            foreach (var node in currentMap.AllNodes)
            {
                foreach (var connectedNode in node.Connections)
                {
                    CreateConnectionLine(node, connectedNode);
                }
            }

            UpdateNodeStates();
        }

        /// <summary>
        /// Creates a UI element for a room node.
        /// </summary>
        private void CreateNodeUI(RoomNode node)
        {
            GameObject nodeObject;

            if (nodeButtonPrefab != null)
            {
                nodeObject = Instantiate(nodeButtonPrefab, nodeContainer);
            }
            else
            {
                nodeObject = new GameObject($"Node_{node.NodeId}");
                nodeObject.transform.SetParent(nodeContainer);
            }

            // Position the node
            Vector2 position = CalculateNodePosition(node);
            RectTransform rectTransform = nodeObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }

            // Setup node UI component
            DungeonMapNodeUI nodeUI = nodeObject.GetComponent<DungeonMapNodeUI>();
            if (nodeUI == null)
            {
                nodeUI = nodeObject.AddComponent<DungeonMapNodeUI>();
            }

            nodeUI.Initialize(node, GetNodeColor(node.Type));
            nodeUI.OnClick += () => HandleNodeClick(node);

            nodeUIMap[node] = nodeUI;
        }

        /// <summary>
        /// Calculates the screen position for a node.
        /// </summary>
        private Vector2 CalculateNodePosition(RoomNode node)
        {
            int layer = node.Layer;
            int posInLayer = node.PositionInLayer;

            // Get all nodes in this layer to center them
            var layerNodes = currentMap.GetNodesInLayer(layer);
            int totalInLayer = layerNodes.Count;

            // Calculate position
            float x = layer * layerSpacing + mapOffset.x;
            float y = (posInLayer - (totalInLayer - 1) / 2f) * nodeSpacing + mapOffset.y;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Creates a visual connection line between two nodes.
        /// </summary>
        private void CreateConnectionLine(RoomNode fromNode, RoomNode toNode)
        {
            if (!nodeUIMap.ContainsKey(fromNode) || !nodeUIMap.ContainsKey(toNode))
            {
                return;
            }

            Vector2 startPos = nodeUIMap[fromNode].GetComponent<RectTransform>().anchoredPosition;
            Vector2 endPos = nodeUIMap[toNode].GetComponent<RectTransform>().anchoredPosition;

            // Create line renderer or use UI line
            if (connectionLinePrefab != null)
            {
                LineRenderer line = Instantiate(connectionLinePrefab, nodeContainer);
                line.SetPosition(0, startPos);
                line.SetPosition(1, endPos);
                connectionLines.Add(line);
            }
        }

        /// <summary>
        /// Updates the visual state of all nodes (cleared, accessible, etc).
        /// </summary>
        public void UpdateNodeStates()
        {
            foreach (var kvp in nodeUIMap)
            {
                RoomNode node = kvp.Key;
                DungeonMapNodeUI nodeUI = kvp.Value;

                if (node.IsCleared)
                {
                    nodeUI.SetState(DungeonMapNodeUI.NodeState.Cleared);
                }
                else if (node.IsAccessible)
                {
                    nodeUI.SetState(DungeonMapNodeUI.NodeState.Accessible);
                }
                else
                {
                    nodeUI.SetState(DungeonMapNodeUI.NodeState.Locked);
                }
            }
        }

        /// <summary>
        /// Gets the color for a room type.
        /// </summary>
        private Color GetNodeColor(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.NormalCombat => normalNodeColor,
                RoomType.EliteCombat => eliteNodeColor,
                RoomType.Boss => bossNodeColor,
                RoomType.Treasure => treasureNodeColor,
                RoomType.Rest => restNodeColor,
                RoomType.Start => Color.white,
                _ => normalNodeColor
            };
        }

        /// <summary>
        /// Handles node click events.
        /// </summary>
        private void HandleNodeClick(RoomNode node)
        {
            if (!node.IsAccessible)
            {
                Debug.Log($"Node {node.NodeId} is not accessible!");
                return;
            }

            OnNodeSelected?.Invoke(node);
            HideMap();
        }

        /// <summary>
        /// Clears all UI elements from the map.
        /// </summary>
        private void ClearMapUI()
        {
            // Clear nodes
            foreach (var nodeUI in nodeUIMap.Values)
            {
                if (nodeUI != null)
                {
                    Destroy(nodeUI.gameObject);
                }
            }
            nodeUIMap.Clear();

            // Clear lines
            foreach (var line in connectionLines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            connectionLines.Clear();
        }

        /// <summary>
        /// Checks if the map is currently visible.
        /// </summary>
        public bool IsVisible => mapPanel != null && mapPanel.activeSelf;
    }

    /// <summary>
    /// Individual node UI component for the dungeon map.
    /// </summary>
    public class DungeonMapNodeUI : MonoBehaviour
    {
        public enum NodeState { Locked, Accessible, Cleared, Current }

        [Header("UI References")]
        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI labelText;

        [SerializeField]
        private Button button;

        private RoomNode roomNode;
        private NodeState currentState = NodeState.Locked;
        private Color baseColor = Color.gray;

        public System.Action OnClick;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        /// <summary>
        /// Initializes the node UI.
        /// </summary>
        public void Initialize(RoomNode node, Color color)
        {
            roomNode = node;
            baseColor = color;

            if (labelText != null)
            {
                labelText.text = GetRoomTypeIcon(node.Type);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Sets the visual state of the node.
        /// </summary>
        public void SetState(NodeState state)
        {
            currentState = state;
            UpdateVisuals();
        }

        /// <summary>
        /// Updates the visual appearance based on state.
        /// </summary>
        private void UpdateVisuals()
        {
            if (backgroundImage == null)
            {
                return;
            }

            Color displayColor = currentState switch
            {
                NodeState.Locked => baseColor * 0.5f,
                NodeState.Accessible => baseColor,
                NodeState.Cleared => Color.gray,
                NodeState.Current => Color.yellow,
                _ => baseColor
            };

            backgroundImage.color = displayColor;

            // Enable/disable interaction
            if (button != null)
            {
                button.interactable = (currentState == NodeState.Accessible);
            }
        }

        /// <summary>
        /// Handles button click.
        /// </summary>
        private void HandleClick()
        {
            OnClick?.Invoke();
        }

        /// <summary>
        /// Gets an icon character for a room type.
        /// </summary>
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
    }
}
