using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.BoardSystem
{
    /// <summary>
    /// Generates and manages the visual board from BoardData.
    /// Creates tile GameObjects and integrates with the chess engine Board.
    /// </summary>
    public class BoardGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private BoardData boardData;

        [SerializeField]
        private GameObject tilePrefab;

        [SerializeField]
        private Transform boardContainer;

        [SerializeField]
        private Core.ChessEngine.Board chessBoard;

        [Header("Generated Tiles")]
        private Tile[,] tiles;
        private bool isGenerated = false;

        /// <summary>
        /// Gets the BoardData used to generate this board.
        /// </summary>
        public BoardData Data => boardData;

        /// <summary>
        /// Gets whether the board has been generated.
        /// </summary>
        public bool IsGenerated => isGenerated;

        /// <summary>
        /// Gets the tile at the specified position.
        /// </summary>
        public Tile GetTile(Vector2Int position)
        {
            if (!isGenerated || tiles == null)
            {
                return null;
            }

            if (position.x < 0 || position.x >= boardData.Width ||
                position.y < 0 || position.y >= boardData.Height)
            {
                return null;
            }

            return tiles[position.x, position.y];
        }

        /// <summary>
        /// Generates the board from the BoardData.
        /// </summary>
        public void GenerateFromData(BoardData data)
        {
            if (data == null)
            {
                Debug.LogError("BoardData is null. Cannot generate board.");
                return;
            }

            boardData = data;

            // Validate board data
            if (!boardData.Validate())
            {
                Debug.LogError("BoardData validation failed.");
                return;
            }

            // Clear existing board
            ClearBoard();

            // Initialize chess board
            if (chessBoard != null)
            {
                chessBoard.Initialize(boardData.Width, boardData.Height);
            }

            // Create board container if it doesn't exist
            if (boardContainer == null)
            {
                boardContainer = new GameObject("BoardContainer").transform;
                boardContainer.SetParent(transform);
                boardContainer.localPosition = Vector3.zero;
            }

            // Create tile array
            tiles = new Tile[boardData.Width, boardData.Height];

            // Generate tiles
            for (int y = 0; y < boardData.Height; y++)
            {
                for (int x = 0; x < boardData.Width; x++)
                {
                    CreateTile(x, y);
                }
            }

            // Center the board
            CenterBoard();

            isGenerated = true;
        }

        private void CreateTile(int x, int y)
        {
            Vector2Int position = new Vector2Int(x, y);
            TileType tileType = boardData.GetTileType(x, y);

            // Create tile GameObject
            GameObject tileObject;
            if (tilePrefab != null)
            {
                tileObject = Instantiate(tilePrefab, boardContainer);
            }
            else
            {
                // Create basic tile if no prefab is provided
                tileObject = new GameObject($"Tile_{x}_{y}");
                tileObject.transform.SetParent(boardContainer);

                // Add SpriteRenderer for visual representation
                var spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateSquareSprite();
                spriteRenderer.sortingOrder = -1; // Behind pieces

                // Add BoxCollider2D for mouse interaction
                var collider = tileObject.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one * boardData.TileSize;
            }

            // Position the tile
            float totalTileSize = boardData.TileSize + boardData.TileSpacing;
            Vector3 worldPosition = new Vector3(x * totalTileSize, y * totalTileSize, 0);
            tileObject.transform.localPosition = worldPosition;

            // Scale the tile
            tileObject.transform.localScale = Vector3.one * boardData.TileSize;

            // Get or add Tile component
            Tile tile = tileObject.GetComponent<Tile>();
            if (tile == null)
            {
                tile = tileObject.AddComponent<Tile>();
            }

            // Initialize tile
            tile.Initialize(position, tileType);

            // Store tile reference
            tiles[x, y] = tile;

            // Update chess board obstacles
            if (chessBoard != null && tileType == TileType.Obstacle)
            {
                chessBoard.SetObstacle(position, true);
            }
        }

        private Sprite CreateSquareSprite()
        {
            // Create a simple white square texture
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private void CenterBoard()
        {
            if (boardContainer == null)
            {
                return;
            }

            float totalTileSize = boardData.TileSize + boardData.TileSpacing;
            float boardWidth = boardData.Width * totalTileSize - boardData.TileSpacing;
            float boardHeight = boardData.Height * totalTileSize - boardData.TileSpacing;

            Vector3 offset = new Vector3(-boardWidth / 2f, -boardHeight / 2f, 0);
            boardContainer.localPosition = offset;
        }

        /// <summary>
        /// Clears all generated tiles from the board.
        /// </summary>
        public void ClearBoard()
        {
            if (boardContainer != null)
            {
                // Destroy all child objects
                for (int i = boardContainer.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(boardContainer.GetChild(i).gameObject);
                    }
                    else
                    {
                        DestroyImmediate(boardContainer.GetChild(i).gameObject);
                    }
                }
            }

            tiles = null;
            isGenerated = false;
        }

        /// <summary>
        /// Highlights tiles at the specified positions.
        /// </summary>
        public void HighlightTiles(Vector2Int[] positions, bool highlight)
        {
            if (!isGenerated || positions == null)
            {
                return;
            }

            foreach (var position in positions)
            {
                Tile tile = GetTile(position);
                if (tile != null)
                {
                    tile.Highlight(highlight);
                }
            }
        }

        /// <summary>
        /// Clears all tile highlights.
        /// </summary>
        public void ClearHighlights()
        {
            if (!isGenerated || tiles == null)
            {
                return;
            }

            for (int y = 0; y < boardData.Height; y++)
            {
                for (int x = 0; x < boardData.Width; x++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].Highlight(false);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the world position of a tile at the specified board position.
        /// </summary>
        public Vector3 GetTileWorldPosition(Vector2Int position)
        {
            Tile tile = GetTile(position);
            if (tile != null)
            {
                return tile.transform.position;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the board position from a world position.
        /// </summary>
        public Vector2Int GetBoardPosition(Vector3 worldPosition)
        {
            if (!isGenerated || boardContainer == null)
            {
                return new Vector2Int(-1, -1);
            }

            Vector3 localPosition = boardContainer.InverseTransformPoint(worldPosition);
            float totalTileSize = boardData.TileSize + boardData.TileSpacing;

            int x = Mathf.RoundToInt(localPosition.x / totalTileSize);
            int y = Mathf.RoundToInt(localPosition.y / totalTileSize);

            if (x >= 0 && x < boardData.Width && y >= 0 && y < boardData.Height)
            {
                return new Vector2Int(x, y);
            }

            return new Vector2Int(-1, -1);
        }

        private void Start()
        {
            // Auto-generate board if data is assigned
            if (boardData != null && !isGenerated)
            {
                GenerateFromData(boardData);
            }
        }

        /// <summary>
        /// Returns a string representation of the board for debugging.
        /// </summary>
        public override string ToString()
        {
            if (boardData == null)
            {
                return "BoardGenerator (no data)";
            }

            return $"BoardGenerator - {boardData} - Generated: {isGenerated}";
        }
    }
}
