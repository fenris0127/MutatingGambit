using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Board
{
    /// <summary>
    /// Represents a single tile on the chess board.
    /// Contains position, type, and visual representation.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int position;

        [SerializeField]
        private TileType tileType = TileType.Normal;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Color normalColor = new Color(0.9f, 0.9f, 0.9f);

        [SerializeField]
        private Color alternateColor = new Color(0.6f, 0.6f, 0.6f);

        [SerializeField]
        private Color obstacleColor = new Color(0.3f, 0.2f, 0.2f);

        [SerializeField]
        private Color waterColor = new Color(0.4f, 0.6f, 0.9f);

        [SerializeField]
        private Color highlightColor = new Color(0.9f, 0.9f, 0.3f, 0.5f);

        private bool isHighlighted = false;
        private Piece occupyingPiece;

        /// <summary>
        /// Gets the position of this tile on the board.
        /// </summary>
        public Vector2Int Position => position;

        /// <summary>
        /// Gets or sets the type of this tile.
        /// </summary>
        public TileType Type
        {
            get => tileType;
            set
            {
                tileType = value;
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Gets or sets the piece currently occupying this tile.
        /// </summary>
        public Piece OccupyingPiece
        {
            get => occupyingPiece;
            set => occupyingPiece = value;
        }

        /// <summary>
        /// Checks if this tile is walkable (not an obstacle or void).
        /// </summary>
        public bool IsWalkable => tileType == TileType.Normal || tileType == TileType.Water;

        /// <summary>
        /// Checks if this tile is occupied by a piece.
        /// </summary>
        public bool IsOccupied => occupyingPiece != null;

        /// <summary>
        /// Initializes the tile with position and type.
        /// </summary>
        public void Initialize(Vector2Int tilePosition, TileType type)
        {
            position = tilePosition;
            tileType = type;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            UpdateVisuals();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Updates the visual appearance of the tile based on its type.
        /// </summary>
        public void UpdateVisuals()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            Color baseColor = GetBaseColor();
            spriteRenderer.color = isHighlighted ? Color.Lerp(baseColor, highlightColor, 0.5f) : baseColor;
        }

        private Color GetBaseColor()
        {
            switch (tileType)
            {
                case TileType.Obstacle:
                    return obstacleColor;
                case TileType.Water:
                    return waterColor;
                case TileType.Void:
                    return Color.clear;
                case TileType.Normal:
                default:
                    // Checkerboard pattern
                    bool isAlternate = (position.x + position.y) % 2 == 0;
                    return isAlternate ? alternateColor : normalColor;
            }
        }

        /// <summary>
        /// Highlights this tile (e.g., for showing valid moves).
        /// </summary>
        public void Highlight(bool highlight)
        {
            isHighlighted = highlight;
            UpdateVisuals();
        }

        /// <summary>
        /// Called when the mouse enters this tile.
        /// </summary>
        private void OnMouseEnter()
        {
            // Can be used for hover effects
        }

        /// <summary>
        /// Called when the mouse exits this tile.
        /// </summary>
        private void OnMouseExit()
        {
            // Can be used for hover effects
        }

        /// <summary>
        /// Called when the mouse clicks this tile.
        /// </summary>
        private void OnMouseDown()
        {
            // Will be handled by BoardManager or GameManager
        }

        /// <summary>
        /// Returns a string representation of this tile for debugging.
        /// </summary>
        public override string ToString()
        {
            string occupiedStatus = IsOccupied ? $" (occupied by {occupyingPiece})" : "";
            return $"Tile at {position.ToNotation()} - {tileType}{occupiedStatus}";
        }
    }
}
