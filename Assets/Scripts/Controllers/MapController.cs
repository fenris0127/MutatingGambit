using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using MutatingGambit.Core.Map;
using MutatingGambit.Core.Rooms;

namespace MutatingGambit
{
    /// <summary>
    /// Displays dungeon map and handles room selection (Slay the Spire style)
    /// </summary>
    public class MapController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI mapText;
        public Transform nodeButtonContainer;
        public GameObject nodeButtonPrefab;

        [Header("Simple UI (if no prefab)")]
        public TextMeshProUGUI instructionText;

        private DungeonMap _dungeonMap;
        private List<Button> _nodeButtons = new List<Button>();

        public event Action<MapNode> OnNodeSelected;

        public void Initialize(DungeonMap dungeonMap)
        {
            _dungeonMap = dungeonMap;
            DisplayMap();
        }

        void DisplayMap()
        {
            // Clear old buttons
            foreach (var btn in _nodeButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            _nodeButtons.Clear();

            var currentNode = _dungeonMap.GetCurrentNode();
            var availableNodes = currentNode != null
                ? _dungeonMap.GetAvailableNextNodes(currentNode)
                : new List<MapNode> { _dungeonMap.GetStartNode() };

            // Display map as text
            if (mapText != null)
            {
                string mapDisplay = GenerateMapText(currentNode, availableNodes);
                mapText.text = mapDisplay;
            }

            // Show instruction
            if (instructionText != null)
            {
                if (currentNode == null)
                {
                    instructionText.text = "Choose your starting path...";
                }
                else
                {
                    instructionText.text = $"Current Layer: {currentNode.Layer + 1}\nChoose next room:";
                }
            }

            // Create buttons for available nodes (simple version - stacked vertically)
            CreateSimpleButtons(availableNodes);
        }

        string GenerateMapText(MapNode current, List<MapNode> available)
        {
            string text = "=== DUNGEON MAP ===\n\n";

            if (current != null)
            {
                text += $"Current Position: Layer {current.Layer + 1}\n";
                text += $"Room Type: {current.Room.Type}\n";
                text += $"Status: {(current.IsCompleted ? "COMPLETED" : "IN PROGRESS")}\n\n";
            }
            else
            {
                text += "At dungeon entrance...\n\n";
            }

            text += "Available Paths:\n";
            for (int i = 0; i < available.Count; i++)
            {
                var node = available[i];
                string icon = GetRoomIcon(node.Room.Type);
                text += $"{i + 1}. {icon} {node.Room.Type} - Layer {node.Layer + 1}\n";
            }

            return text;
        }

        string GetRoomIcon(RoomType type)
        {
            switch (type)
            {
                case RoomType.Combat: return "⚔️";
                case RoomType.Treasure: return "💎";
                case RoomType.Rest: return "❤️";
                default: return "?";
            }
        }

        void CreateSimpleButtons(List<MapNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var index = i;

                // Create button (simple Unity UI button)
                GameObject btnObj;

                if (nodeButtonPrefab != null && nodeButtonContainer != null)
                {
                    btnObj = Instantiate(nodeButtonPrefab, nodeButtonContainer);
                }
                else
                {
                    // Create simple button programmatically
                    btnObj = new GameObject($"NodeButton_{i}");
                    btnObj.transform.SetParent(transform);

                    var rectTransform = btnObj.AddComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(400, 60);
                    rectTransform.anchoredPosition = new Vector2(0, -i * 70 - 100);

                    var image = btnObj.AddComponent<Image>();
                    image.color = new Color(0.2f, 0.2f, 0.3f);

                    var button = btnObj.AddComponent<Button>();

                    var textObj = new GameObject("Text");
                    textObj.transform.SetParent(btnObj.transform);
                    var textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;

                    var tmp = textObj.AddComponent<TextMeshProUGUI>();
                    tmp.text = $"{GetRoomIcon(node.Room.Type)} {node.Room.Type} Room (Layer {node.Layer + 1})";
                    tmp.fontSize = 20;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;

                    button.onClick.AddListener(() => OnNodeButtonClicked(node));
                    _nodeButtons.Add(button);
                }

                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnNodeButtonClicked(node));
                    _nodeButtons.Add(btn);

                    var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = $"{GetRoomIcon(node.Room.Type)} {node.Room.Type} Room (Layer {node.Layer + 1})";
                    }
                }
            }
        }

        void OnNodeButtonClicked(MapNode node)
        {
            // Move to node
            _dungeonMap.MoveToNode(node);

            // Notify
            OnNodeSelected?.Invoke(node);
        }
    }
}
