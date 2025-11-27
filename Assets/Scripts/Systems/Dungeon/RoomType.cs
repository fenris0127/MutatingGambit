namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Defines the types of rooms available in the dungeon.
    /// </summary>
    public enum RoomType
    {
        /// <summary>
        /// Normal combat encounter with basic enemies.
        /// </summary>
        NormalCombat,

        /// <summary>
        /// Elite combat encounter with mutated enemies and better rewards.
        /// </summary>
        EliteCombat,

        /// <summary>
        /// Boss encounter at the end of a floor.
        /// </summary>
        Boss,

        /// <summary>
        /// Treasure room - grants an artifact immediately.
        /// </summary>
        Treasure,

        /// <summary>
        /// Rest room - repair broken pieces or upgrade pieces.
        /// </summary>
        Rest,

        /// <summary>
        /// Mystery event room with random encounters.
        /// </summary>
        Mystery,

        /// <summary>
        /// Starting room of the dungeon.
        /// </summary>
        Start
    }
}
