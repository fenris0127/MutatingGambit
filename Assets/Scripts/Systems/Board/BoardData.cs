using UnityEngine;

namespace MutatingGambit.Systems.BoardSystem
{
    /// <summary>
    /// ScriptableObject that stores board configuration data.
    /// Defines the size, layout, and tile types for a chess board.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardData", menuName = "Mutating Gambit/Board Data")]
    public class BoardData : ScriptableObject
    {
        [Header("Board Dimensions")]
        [SerializeField]
        [Tooltip("Width of the board (number of files).")]
        private int width = 8;

        [SerializeField]
        [Tooltip("Height of the board (number of ranks).")]
        private int height = 8;

        [Header("Tile Configuration")]
        [SerializeField]
        [Tooltip("2D array of tile types. If null or empty, all tiles will be Normal.")]
        private TileTypeData[] tiles;

        [Header("Visual Settings")]
        [SerializeField]
        [Tooltip("Size of each tile in world units.")]
        private float tileSize = 1.0f;

        [SerializeField]
        [Tooltip("Spacing between tiles.")]
        private float tileSpacing = 0.1f;

        /// <summary>
        /// Gets the width of the board.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the board.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Gets the size of each tile in world units.
        /// </summary>
        public float TileSize => tileSize;

        /// <summary>
        /// Gets the spacing between tiles.
        /// </summary>
        public float TileSpacing => tileSpacing;

        /// <summary>
        /// Gets the tile type at the specified position.
        /// Returns Normal if position is out of bounds or no data exists.
        /// </summary>
        public TileType GetTileType(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return TileType.Void;
            }

            if (tiles == null || tiles.Length == 0)
            {
                return TileType.Normal;
            }

            int index = y * width + x;
            if (index >= 0 && index < tiles.Length)
            {
                return tiles[index].type;
            }

            return TileType.Normal;
        }

        /// <summary>
        /// Gets the tile type at the specified position.
        /// </summary>
        public TileType GetTileType(Vector2Int position)
        {
            return GetTileType(position.x, position.y);
        }

        /// <summary>
        /// Sets the tile type at the specified position.
        /// Used for editor tools and runtime board modification.
        /// </summary>
        public void SetTileType(int x, int y, TileType type)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            // Initialize tiles array if needed
            if (tiles == null || tiles.Length != width * height)
            {
                InitializeTiles();
            }

            int index = y * width + x;
            if (index >= 0 && index < tiles.Length)
            {
                tiles[index].type = type;
            }
        }

        /// <summary>
        /// Sets the tile type at the specified position.
        /// </summary>
        public void SetTileType(Vector2Int position, TileType type)
        {
            SetTileType(position.x, position.y, type);
        }

        /// <summary>
        /// Initializes the tiles array with all Normal tiles.
        /// </summary>
        public void InitializeTiles()
        {
            tiles = new TileTypeData[width * height];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new TileTypeData { type = TileType.Normal };
            }
        }

        /// <summary>
        /// Resizes the board to the specified dimensions.
        /// Preserves existing tile data where possible.
        /// </summary>
        public void Resize(int newWidth, int newHeight)
        {
            if (newWidth <= 0 || newHeight <= 0)
            {
                Debug.LogError("Board dimensions must be positive.");
                return;
            }

            var oldTiles = tiles;
            int oldWidth = width;
            int oldHeight = height;

            width = newWidth;
            height = newHeight;

            InitializeTiles();

            // Copy old tile data
            if (oldTiles != null)
            {
                for (int y = 0; y < Mathf.Min(oldHeight, newHeight); y++)
                {
                    for (int x = 0; x < Mathf.Min(oldWidth, newWidth); x++)
                    {
                        int oldIndex = y * oldWidth + x;
                        int newIndex = y * newWidth + x;
                        if (oldIndex < oldTiles.Length && newIndex < tiles.Length)
                        {
                            tiles[newIndex] = oldTiles[oldIndex];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the board data for consistency.
        /// </summary>
        public bool Validate()
        {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError($"Invalid board dimensions: {width}x{height}");
                return false;
            }

            if (tiles != null && tiles.Length != width * height)
            {
                Debug.LogWarning($"Tile array size ({tiles.Length}) doesn't match board dimensions ({width * height}). Reinitializing...");
                InitializeTiles();
            }

            return true;
        }

        /// <summary>
        /// Helper method called when the asset is loaded in the editor.
        /// </summary>
        private void OnValidate()
        {
            // Ensure width and height are at least 1
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            // Validate tile array size
            if (tiles != null && tiles.Length != width * height)
            {
                Debug.LogWarning($"Board '{name}': Tile array size mismatch. Expected {width * height}, got {tiles.Length}.");
            }
        }

        /// <summary>
        /// Returns a string representation of the board for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"BoardData '{name}' ({width}x{height})";
        }
    }

    /// <summary>
    /// Serializable data structure for storing tile type.
    /// </summary>
    [System.Serializable]
    public struct TileTypeData
    {
        public TileType type;
    }
}
