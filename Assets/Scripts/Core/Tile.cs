namespace MutatingGambit.Core
{
    /// <summary>
    /// Represents a single tile on the board
    /// </summary>
    public class Tile
    {
        public TileType Type { get; set; }
        public Piece Piece { get; set; }

        public bool IsObstacle => Type == TileType.Wall || Type == TileType.Water;
        public bool IsEmpty => Piece == null && Type == TileType.Empty;

        public Tile(TileType type = TileType.Empty)
        {
            Type = type;
            Piece = null;
        }
    }
}
