namespace MutatingGambit.Core.Rooms
{
    /// <summary>
    /// Types of rooms in the dungeon
    /// </summary>
    public enum RoomType
    {
        Combat,
        Treasure,
        Rest,
        Event,
        Shop,
        Boss
    }

    /// <summary>
    /// Difficulty level of combat rooms
    /// </summary>
    public enum RoomDifficulty
    {
        Normal,
        Elite,
        Boss
    }
}
