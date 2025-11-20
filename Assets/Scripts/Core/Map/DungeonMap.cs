using System.Collections.Generic;
using System.Linq;
using MutatingGambit.Core.Rooms;

namespace MutatingGambit.Core.Map
{
    /// <summary>
    /// Manages the dungeon map structure and navigation
    /// </summary>
    public class DungeonMap
    {
        private List<MapNode> _nodes;
        private MapNode _currentNode;
        private HashSet<MapNode> _visitedNodes;
        private int _layerCount;

        public List<MapNode> Nodes => _nodes;

        public DungeonMap()
        {
            _nodes = new List<MapNode>();
            _visitedNodes = new HashSet<MapNode>();
        }

        /// <summary>
        /// Generates a dungeon map with the specified number of layers
        /// </summary>
        public void Generate(int layerCount)
        {
            _layerCount = layerCount;
            _nodes.Clear();

            // Layer 0: Start node
            var startNode = new MapNode(new CombatRoom(RoomDifficulty.Normal), 0);
            _nodes.Add(startNode);

            // Generate middle layers
            for (int layer = 1; layer < layerCount - 1; layer++)
            {
                GenerateLayer(layer);
            }

            // Final layer: Boss
            if (layerCount > 1)
            {
                var bossNode = new MapNode(new CombatRoom(RoomDifficulty.Boss), layerCount - 1);
                bossNode.IsEndNode = true;
                _nodes.Add(bossNode);

                // Connect previous layer to boss
                var previousLayer = GetNodesAtLayer(layerCount - 2);
                foreach (var node in previousLayer)
                {
                    node.AddConnection(bossNode);
                }
            }

            // Connect layers
            ConnectLayers();
        }

        private void GenerateLayer(int layer)
        {
            int nodesInLayer = 3; // Standard: 3 nodes per layer

            for (int i = 0; i < nodesInLayer; i++)
            {
                Room room = GenerateRandomRoom();
                var node = new MapNode(room, layer);
                _nodes.Add(node);
            }
        }

        private Room GenerateRandomRoom()
        {
            // Simple room distribution
            var random = new System.Random();
            int roll = random.Next(100);

            if (roll < 60)
            {
                return new CombatRoom(RoomDifficulty.Normal);
            }
            else if (roll < 80)
            {
                return new CombatRoom(RoomDifficulty.Elite);
            }
            else if (roll < 90)
            {
                return new TreasureRoom();
            }
            else
            {
                return new RestRoom();
            }
        }

        private void ConnectLayers()
        {
            for (int layer = 0; layer < _layerCount - 1; layer++)
            {
                var currentLayerNodes = GetNodesAtLayer(layer);
                var nextLayerNodes = GetNodesAtLayer(layer + 1);

                foreach (var currentNode in currentLayerNodes)
                {
                    // Each node connects to 2-3 nodes in the next layer
                    int connectionCount = System.Math.Min(2, nextLayerNodes.Count);
                    var random = new System.Random();

                    for (int i = 0; i < connectionCount; i++)
                    {
                        int index = random.Next(nextLayerNodes.Count);
                        currentNode.AddConnection(nextLayerNodes[index]);
                    }
                }
            }
        }

        public MapNode GetStartNode()
        {
            return _nodes.FirstOrDefault(n => n.Layer == 0);
        }

        public List<MapNode> GetNodesAtLayer(int layer)
        {
            return _nodes.Where(n => n.Layer == layer).ToList();
        }

        public void SetCurrentNode(MapNode node)
        {
            _currentNode = node;
        }

        public MapNode GetCurrentNode()
        {
            return _currentNode;
        }

        public void VisitNode(MapNode node)
        {
            _visitedNodes.Add(node);
        }

        public bool IsNodeVisited(MapNode node)
        {
            return _visitedNodes.Contains(node);
        }

        public List<MapNode> GetAvailableNextNodes(MapNode fromNode)
        {
            if (fromNode == null)
            {
                return new List<MapNode>();
            }

            return fromNode.Connections;
        }

        public void MoveToNode(MapNode node)
        {
            if (_currentNode != null)
            {
                VisitNode(_currentNode);
            }

            SetCurrentNode(node);
        }

        public int GetCurrentLayer()
        {
            return _currentNode?.Layer ?? 0;
        }

        public int GetLayerCount()
        {
            return _layerCount;
        }
    }
}
