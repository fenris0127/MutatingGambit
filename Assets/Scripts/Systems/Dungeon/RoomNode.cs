using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Represents a single room node in the dungeon map.
    /// Uses a node-based graph structure similar to Slay the Spire.
    /// </summary>
    [System.Serializable]
    public class RoomNode
    {
        [SerializeField]
        private string nodeId;

        [SerializeField]
        private RoomType roomType;

        [SerializeField]
        private Vector2Int position; // Position in the map grid (x = layer, y = position in layer)

        [SerializeField]
        private RoomData roomData;

        [SerializeField]
        private List<RoomNode> connections = new List<RoomNode>();

        [SerializeField]
        private bool isCleared = false;

        [SerializeField]
        private bool isAccessible = false;

        /// <summary>
        /// Gets or sets the unique ID of this node.
        /// </summary>
        public string NodeId
        {
            get => nodeId;
            set => nodeId = value;
        }

        /// <summary>
        /// Gets or sets the type of room.
        /// </summary>
        public RoomType Type
        {
            get => roomType;
            set => roomType = value;
        }

        /// <summary>
        /// Gets or sets the position in the map grid.
        /// </summary>
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Gets or sets the room data (board layout, enemies, etc.).
        /// </summary>
        public RoomData Room
        {
            get => roomData;
            set => roomData = value;
        }

        /// <summary>
        /// Gets the list of connected nodes (rooms player can move to from here).
        /// </summary>
        public List<RoomNode> Connections => connections;

        /// <summary>
        /// Gets or sets whether this room has been cleared.
        /// </summary>
        public bool IsCleared
        {
            get => isCleared;
            set => isCleared = value;
        }

        /// <summary>
        /// Gets or sets whether this room is currently accessible to the player.
        /// </summary>
        public bool IsAccessible
        {
            get => isAccessible;
            set => isAccessible = value;
        }

        /// <summary>
        /// Creates a new room node.
        /// </summary>
        public RoomNode(string id, RoomType type, Vector2Int pos)
        {
            nodeId = id;
            roomType = type;
            position = pos;
            connections = new List<RoomNode>();
            isCleared = false;
            isAccessible = false;
        }

        /// <summary>
        /// Adds a connection to another room node.
        /// </summary>
        public void AddConnection(RoomNode node)
        {
            if (node != null && !connections.Contains(node))
            {
                connections.Add(node);
            }
        }

        /// <summary>
        /// Removes a connection to another room node.
        /// </summary>
        public void RemoveConnection(RoomNode node)
        {
            connections.Remove(node);
        }

        /// <summary>
        /// Checks if this node is connected to another node.
        /// </summary>
        public bool IsConnectedTo(RoomNode node)
        {
            return connections.Contains(node);
        }

        /// <summary>
        /// Gets the layer (x position) of this node.
        /// </summary>
        public int Layer => position.x;

        /// <summary>
        /// Gets the position within the layer (y position).
        /// </summary>
        public int PositionInLayer => position.y;

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"RoomNode [{nodeId}] - {roomType} at Layer {Layer}, Pos {PositionInLayer} - Cleared: {isCleared}, Accessible: {isAccessible}";
        }
    }
}
