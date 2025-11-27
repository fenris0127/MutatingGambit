namespace MutatingGambit.Systems.Board
{
    /// <summary>
    /// Defines the types of tiles that can exist on the board.
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// Standard playable tile.
        /// </summary>
        Normal,

        /// <summary>
        /// Impassable obstacle tile (pieces cannot move through or onto this).
        /// </summary>
        Obstacle,

        /// <summary>
        /// Water tile (may have special rules in the future).
        /// </summary>
        Water,

        /// <summary>
        /// Void tile (out of bounds, used for non-rectangular boards).
        /// </summary>
        Void
    }
}
