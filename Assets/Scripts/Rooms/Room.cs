namespace MutatingGambit.Core.Rooms
{
    /// <summary>
    /// Base class for dungeon rooms
    /// </summary>
    public abstract class Room
    {
        public abstract RoomType Type { get; }
        public bool IsCompleted { get; set; }

        public abstract RoomReward GetReward();
    }
}
