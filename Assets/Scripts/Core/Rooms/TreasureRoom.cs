namespace MutatingGambit.Core.Rooms
{
    /// <summary>
    /// A room that provides immediate rewards
    /// </summary>
    public class TreasureRoom : Room
    {
        public override RoomType Type => RoomType.Treasure;

        public override RoomReward GetReward()
        {
            return RoomReward.CreateTreasureReward();
        }
    }
}
