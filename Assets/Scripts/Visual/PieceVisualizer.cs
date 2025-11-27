using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Visual
{
    /// <summary>
    /// Creates simple visual representation for chess pieces.
    /// For prototyping - generates basic shapes for each piece type.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PieceVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Piece piece;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [Header("Visual Settings")]
        [SerializeField]
        private float pieceSize = 0.8f;

        [SerializeField]
        private Color whiteColor = new Color(0.9f, 0.9f, 0.9f);

        [SerializeField]
        private Color blackColor = new Color(0.2f, 0.2f, 0.2f);

        [Header("Sprite References (Optional)")]
        [SerializeField]
        private Sprite kingSprite;

        [SerializeField]
        private Sprite queenSprite;

        [SerializeField]
        private Sprite rookSprite;

        [SerializeField]
        private Sprite bishopSprite;

        [SerializeField]
        private Sprite knightSprite;

        [SerializeField]
        private Sprite pawnSprite;

        private void Awake()
        {
            if (piece == null)
            {
                piece = GetComponent<Piece>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            UpdateVisual();
        }

        /// <summary>
        /// Updates the visual representation based on piece type and team.
        /// </summary>
        public void UpdateVisual()
        {
            if (piece == null || spriteRenderer == null)
            {
                return;
            }

            // Set sprite
            Sprite selectedSprite = GetSpriteForPieceType(piece.Type);

            if (selectedSprite != null)
            {
                // Use custom sprite
                spriteRenderer.sprite = selectedSprite;
            }
            else
            {
                // Generate simple sprite
                spriteRenderer.sprite = GenerateSimpleSprite(piece.Type);
            }

            // Set color based on team
            spriteRenderer.color = piece.Team == Team.White ? whiteColor : blackColor;

            // Set scale
            transform.localScale = Vector3.one * pieceSize;

            // Set sorting order (to appear above board)
            spriteRenderer.sortingOrder = 10;
        }

        /// <summary>
        /// Gets the appropriate sprite for a piece type.
        /// </summary>
        private Sprite GetSpriteForPieceType(PieceType type)
        {
            return type switch
            {
                PieceType.King => kingSprite,
                PieceType.Queen => queenSprite,
                PieceType.Rook => rookSprite,
                PieceType.Bishop => bishopSprite,
                PieceType.Knight => knightSprite,
                PieceType.Pawn => pawnSprite,
                _ => null
            };
        }

        /// <summary>
        /// Generates a simple procedural sprite for prototyping.
        /// Creates basic geometric shapes to represent each piece.
        /// </summary>
        private Sprite GenerateSimpleSprite(PieceType type)
        {
            // Create texture
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            // Fill with transparent
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            Color drawColor = Color.white;

            // Draw shape based on piece type
            switch (type)
            {
                case PieceType.King:
                    DrawCross(pixels, size, drawColor);
                    break;

                case PieceType.Queen:
                    DrawDiamond(pixels, size, drawColor);
                    break;

                case PieceType.Rook:
                    DrawSquare(pixels, size, drawColor);
                    break;

                case PieceType.Bishop:
                    DrawTriangle(pixels, size, drawColor);
                    break;

                case PieceType.Knight:
                    DrawL(pixels, size, drawColor);
                    break;

                case PieceType.Pawn:
                    DrawCircle(pixels, size, drawColor);
                    break;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            // Create sprite
            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        #region Shape Drawing Methods

        private void DrawCircle(Color[] pixels, int size, Color color)
        {
            int center = size / 2;
            int radius = size / 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - center;
                    int dy = y - center;
                    if (dx * dx + dy * dy < radius * radius)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private void DrawSquare(Color[] pixels, int size, Color color)
        {
            int margin = size / 4;

            for (int y = margin; y < size - margin; y++)
            {
                for (int x = margin; x < size - margin; x++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        private void DrawDiamond(Color[] pixels, int size, Color color)
        {
            int center = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - center);
                    int dy = Mathf.Abs(y - center);
                    if (dx + dy < center)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private void DrawTriangle(Color[] pixels, int size, Color color)
        {
            int margin = size / 4;

            for (int y = margin; y < size - margin; y++)
            {
                int width = (y - margin) * 2 / 3;
                int center = size / 2;

                for (int x = center - width; x < center + width; x++)
                {
                    if (x >= 0 && x < size)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private void DrawCross(Color[] pixels, int size, Color color)
        {
            int center = size / 2;
            int thickness = size / 8;
            int margin = size / 4;

            // Vertical line
            for (int y = margin; y < size - margin; y++)
            {
                for (int x = center - thickness; x < center + thickness; x++)
                {
                    pixels[y * size + x] = color;
                }
            }

            // Horizontal line
            for (int x = margin; x < size - margin; x++)
            {
                for (int y = center - thickness; y < center + thickness; y++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        private void DrawL(Color[] pixels, int size, Color color)
        {
            int margin = size / 4;
            int thickness = size / 6;

            // Vertical part of L
            for (int y = margin; y < size - margin; y++)
            {
                for (int x = margin; x < margin + thickness; x++)
                {
                    pixels[y * size + x] = color;
                }
            }

            // Horizontal part of L
            for (int x = margin; x < size - margin; x++)
            {
                for (int y = size - margin - thickness; y < size - margin; y++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        #endregion

        /// <summary>
        /// Adds a text label above the piece (for debugging).
        /// </summary>
        public void AddLabel()
        {
            // Create label GameObject
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = new Vector3(0, 0.6f, 0);

            // Add TextMesh
            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = GetPieceSymbol(piece.Type);
            textMesh.fontSize = 36;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = piece.Team == Team.White ? Color.black : Color.white;
        }

        /// <summary>
        /// Gets the chess symbol for a piece type.
        /// </summary>
        private string GetPieceSymbol(PieceType type)
        {
            return type switch
            {
                PieceType.King => "♔",
                PieceType.Queen => "♕",
                PieceType.Rook => "♖",
                PieceType.Bishop => "♗",
                PieceType.Knight => "♘",
                PieceType.Pawn => "♙",
                _ => "?"
            };
        }
    }
}
