using System.Collections.Generic;
using MutatingGambit.Core.Rooms;
using MutatingGambit.Core.Artifacts;

namespace MutatingGambit.Core.Map
{
    /// <summary>
    /// Represents a single node in the dungeon map
    /// </summary>
    public class MapNode
    {
        public Room Room { get; }
        public int Layer { get; }
        public List<MapNode> Connections { get; }
        public bool IsCompleted { get; private set; }
        public bool IsEndNode { get; set; }

        private RoomReward _reward;
        private Artifact _chosenReward;

        public MapNode(Room room, int layer)
        {
            Room = room;
            Layer = layer;
            Connections = new List<MapNode>();
            IsCompleted = false;
        }

        public void AddConnection(MapNode node)
        {
            if (node != null && !Connections.Contains(node))
            {
                Connections.Add(node);
            }
        }

        public void CompleteRoom()
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                Room.IsCompleted = true;
                _reward = Room.GetReward();
            }
        }

        public RoomReward GetReward()
        {
            return _reward;
        }

        public void SetChosenReward(Artifact artifact)
        {
            _chosenReward = artifact;
        }

        public Artifact GetChosenReward()
        {
            return _chosenReward;
        }
    }
}
