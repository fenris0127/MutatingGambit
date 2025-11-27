using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Generates procedural dungeon maps in a node-based structure (Slay the Spire style).
    /// </summary>
    public class DungeonMapGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField]
        [Tooltip("Number of layers (floors) in the dungeon.")]
        private int layerCount = 5;

        [SerializeField]
        [Tooltip("Minimum number of rooms per layer.")]
        private int minRoomsPerLayer = 2;

        [SerializeField]
        [Tooltip("Maximum number of rooms per layer.")]
        private int maxRoomsPerLayer = 4;

        [SerializeField]
        [Tooltip("Random seed for generation (0 = random).")]
        private int seed = 0;

        [Header("Room Type Distribution")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Probability of generating an elite combat room.")]
        private float eliteCombatChance = 0.2f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Probability of generating a treasure room.")]
        private float treasureChance = 0.15f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Probability of generating a rest room.")]
        private float restChance = 0.15f;

        private DungeonMap currentMap;
        private System.Random rng;

        /// <summary>
        /// Gets the currently generated dungeon map.
        /// </summary>
        public DungeonMap CurrentMap => currentMap;

        /// <summary>
        /// Generates a new dungeon map.
        /// </summary>
        public DungeonMap GenerateMap()
        {
            return GenerateMap(layerCount, minRoomsPerLayer, maxRoomsPerLayer, seed);
        }

        /// <summary>
        /// Generates a dungeon map with specified parameters.
        /// </summary>
        public DungeonMap GenerateMap(int layers, int minRooms, int maxRooms, int generationSeed = 0)
        {
            // Initialize RNG
            if (generationSeed == 0)
            {
                generationSeed = Random.Range(1, 1000000);
            }
            rng = new System.Random(generationSeed);

            currentMap = new DungeonMap();
            currentMap.Seed = generationSeed;

            // Generate layers
            for (int layer = 0; layer < layers; layer++)
            {
                GenerateLayer(layer, layers, minRooms, maxRooms);
            }

            // Create connections between layers
            ConnectLayers();

            // Set accessibility (start room is accessible)
            if (currentMap.AllNodes.Count > 0)
            {
                currentMap.AllNodes[0].IsAccessible = true;
            }

            Debug.Log($"Generated dungeon map with {currentMap.AllNodes.Count} rooms across {layers} layers. Seed: {generationSeed}");

            return currentMap;
        }

        private void GenerateLayer(int layer, int totalLayers, int minRooms, int maxRooms)
        {
            int roomCount = rng.Next(minRooms, maxRooms + 1);

            for (int i = 0; i < roomCount; i++)
            {
                RoomType roomType = DetermineRoomType(layer, totalLayers);
                string nodeId = $"L{layer}_R{i}";
                Vector2Int position = new Vector2Int(layer, i);

                RoomNode node = new RoomNode(nodeId, roomType, position);
                currentMap.AddNode(node);
            }
        }

        private RoomType DetermineRoomType(int layer, int totalLayers)
        {
            // First layer - always start
            if (layer == 0)
            {
                return RoomType.Start;
            }

            // Last layer - always boss
            if (layer == totalLayers - 1)
            {
                return RoomType.Boss;
            }

            // Random type based on probabilities
            float roll = (float)rng.NextDouble();

            if (roll < eliteCombatChance)
            {
                return RoomType.EliteCombat;
            }
            else if (roll < eliteCombatChance + treasureChance)
            {
                return RoomType.Treasure;
            }
            else if (roll < eliteCombatChance + treasureChance + restChance)
            {
                return RoomType.Rest;
            }
            else if (roll < eliteCombatChance + treasureChance + restChance + 0.1f)
            {
                return RoomType.Mystery;
            }
            else
            {
                return RoomType.NormalCombat;
            }
        }

        private void ConnectLayers()
        {
            for (int layer = 0; layer < layerCount - 1; layer++)
            {
                var currentLayerNodes = currentMap.GetNodesInLayer(layer);
                var nextLayerNodes = currentMap.GetNodesInLayer(layer + 1);

                if (currentLayerNodes.Count == 0 || nextLayerNodes.Count == 0)
                {
                    continue;
                }

                // Each room in current layer connects to 1-2 rooms in next layer
                foreach (var node in currentLayerNodes)
                {
                    int connectionsToMake = rng.Next(1, Mathf.Min(3, nextLayerNodes.Count + 1));

                    // Shuffle next layer nodes
                    var shuffled = new List<RoomNode>(nextLayerNodes);
                    ShuffleList(shuffled);

                    for (int i = 0; i < connectionsToMake && i < shuffled.Count; i++)
                    {
                        node.AddConnection(shuffled[i]);
                    }
                }

                // Ensure all rooms in next layer are reachable
                foreach (var nextNode in nextLayerNodes)
                {
                    bool hasIncomingConnection = false;
                    foreach (var currentNode in currentLayerNodes)
                    {
                        if (currentNode.IsConnectedTo(nextNode))
                        {
                            hasIncomingConnection = true;
                            break;
                        }
                    }

                    // If no incoming connection, connect from random room in previous layer
                    if (!hasIncomingConnection && currentLayerNodes.Count > 0)
                    {
                        int randomIndex = rng.Next(currentLayerNodes.Count);
                        currentLayerNodes[randomIndex].AddConnection(nextNode);
                    }
                }
            }
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }

    /// <summary>
    /// Represents the complete dungeon map structure.
    /// </summary>
    [System.Serializable]
    public class DungeonMap
    {
        private List<RoomNode> allNodes = new List<RoomNode>();
        private int seed;

        /// <summary>
        /// Gets all nodes in the map.
        /// </summary>
        public List<RoomNode> AllNodes => allNodes;

        /// <summary>
        /// Gets or sets the generation seed.
        /// </summary>
        public int Seed
        {
            get => seed;
            set => seed = value;
        }

        /// <summary>
        /// Adds a node to the map.
        /// </summary>
        public void AddNode(RoomNode node)
        {
            if (!allNodes.Contains(node))
            {
                allNodes.Add(node);
            }
        }

        /// <summary>
        /// Gets all nodes in a specific layer.
        /// </summary>
        public List<RoomNode> GetNodesInLayer(int layer)
        {
            var result = new List<RoomNode>();
            foreach (var node in allNodes)
            {
                if (node.Layer == layer)
                {
                    result.Add(node);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets a node by its ID.
        /// </summary>
        public RoomNode GetNodeById(string nodeId)
        {
            foreach (var node in allNodes)
            {
                if (node.NodeId == nodeId)
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the maximum layer number in the map.
        /// </summary>
        public int MaxLayer
        {
            get
            {
                int max = 0;
                foreach (var node in allNodes)
                {
                    if (node.Layer > max)
                    {
                        max = node.Layer;
                    }
                }
                return max;
            }
        }
    }
}
