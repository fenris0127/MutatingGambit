using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.MovementRules;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Represents the chess board and manages piece positions.
    /// Implements IBoard interface for movement rule queries.
    /// </summary>
    public class Board : MonoBehaviour, IBoard
    {
        [SerializeField]
        private int width = 8;

        [SerializeField]
        private int height = 8;

        [SerializeField]
        private bool[,] obstacles;

        [Header("Artifact System")]
        [SerializeField]
        private ArtifactManager artifactManager;

        private Piece[,] pieces;
        private List<Piece> allPieces = new List<Piece>();

        /// <summary>
        /// Event fired when a piece moves.
        /// Args: Moving Piece, From Position, To Position, Captured Piece (can be null)
        /// </summary>
        public event System.Action<Piece, Vector2Int, Vector2Int, Piece> OnPieceMoved;

        /// <summary>
        /// Gets the artifact manager for this board.
        /// </summary>
        public ArtifactManager ArtifactManager
        {
            get
            {
                if (artifactManager == null)
                {
                    artifactManager = gameObject.AddComponent<ArtifactManager>();
                    artifactManager.SetBoard(this);
                }
                return artifactManager;
            }
        }

        /// <summary>
        /// Gets the width of the board.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// Gets the height of the board.
        /// </summary>
        public int Height => height;

        /// <summary>
        /// Initializes the board with the specified dimensions.
        /// </summary>
        public void Initialize(int boardWidth, int boardHeight)
        {
            width = boardWidth;
            height = boardHeight;
            pieces = new Piece[width, height];
            obstacles = new bool[width, height];
            allPieces.Clear();
        }

        private void Awake()
        {
            if (pieces == null)
            {
                Initialize(width, height);
            }
        }

        /// <summary>
        /// Gets the piece at the specified position.
        /// </summary>
        public IPiece GetPieceAt(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return null;
            }

            return pieces[position.x, position.y];
        }

        /// <summary>
        /// Gets the piece at the specified position (returns Piece type).
        /// </summary>
        public Piece GetPiece(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return null;
            }

            return pieces[position.x, position.y];
        }

        /// <summary>
        /// Checks if a position is within board bounds.
        /// </summary>
        public bool IsPositionValid(Vector2Int position)
        {
            return position.IsWithinBounds(width, height);
        }

        /// <summary>
        /// Checks if a position contains an obstacle.
        /// </summary>
        public bool IsObstacle(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return false;
            }

            return obstacles[position.x, position.y];
        }

        /// <summary>
        /// Sets or removes an obstacle at the specified position.
        /// </summary>
        public void SetObstacle(Vector2Int position, bool isObstacle)
        {
            if (IsPositionValid(position))
            {
                obstacles[position.x, position.y] = isObstacle;
            }
        }

        /// <summary>
        /// Places a piece on the board at the specified position.
        /// </summary>
        public void PlacePiece(Piece piece, Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                Debug.LogError($"Cannot place piece at invalid position {position}");
                return;
            }

            // Remove piece from old position if it exists
            if (piece.Position.IsWithinBounds(width, height))
            {
                pieces[piece.Position.x, piece.Position.y] = null;
            }

            // Place piece at new position
            pieces[position.x, position.y] = piece;
            piece.Position = position;

            // Add to piece list if not already present
            if (!allPieces.Contains(piece))
            {
                allPieces.Add(piece);
            }
        }

        /// <summary>
        /// Moves a piece from one position to another.
        /// </summary>
        public bool MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!IsPositionValid(from) || !IsPositionValid(to))
            {
                return false;
            }

            Piece piece = pieces[from.x, from.y];
            if (piece == null)
            {
                return false;
            }

            // Capture piece at destination if present
            Piece capturedPiece = pieces[to.x, to.y];
            if (capturedPiece != null)
            {
                RemovePiece(to);
            }

            // Move the piece
            pieces[from.x, from.y] = null;
            pieces[to.x, to.y] = piece;
            piece.Position = to;

            OnPieceMoved?.Invoke(piece, from, to, capturedPiece);

            return true;
        }

        /// <summary>
        /// Removes a piece from the board.
        /// </summary>
        public void RemovePiece(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return;
            }

            Piece piece = pieces[position.x, position.y];
            if (piece != null)
            {
                pieces[position.x, position.y] = null;
                allPieces.Remove(piece);

                if (piece != null)
                {
                    Destroy(piece.gameObject);
                }
            }
        }

        /// <summary>
        /// Gets all pieces of a specific team.
        /// </summary>
        public List<Piece> GetPiecesByTeam(Team team)
        {
            var result = new List<Piece>();
            foreach (var piece in allPieces)
            {
                if (piece != null && piece.Team == team)
                {
                    result.Add(piece);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all pieces on the board.
        /// </summary>
        public List<Piece> GetAllPieces()
        {
            return new List<Piece>(allPieces);
        }

        /// <summary>
        /// Clears all pieces from the board.
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        Destroy(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                    obstacles[x, y] = false;
                }
            }
            allPieces.Clear();
        }

        /// <summary>
        /// Creates a deep copy of the board for AI simulation.
        /// </summary>
        public Board Clone()
        {
            GameObject clonedObject = new GameObject("ClonedBoard");
            Board clonedBoard = clonedObject.AddComponent<Board>();
            clonedBoard.Initialize(width, height);

            // Copy obstacles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    clonedBoard.obstacles[x, y] = obstacles[x, y];
                }
            }

            // Copy pieces
            foreach (var piece in allPieces)
            {
                if (piece != null)
                {
                    GameObject pieceObject = new GameObject($"Clone_{piece.Type}_{piece.Team}");
                    Piece clonedPiece = pieceObject.AddComponent<Piece>();
                    clonedPiece.Initialize(piece.Type, piece.Team, piece.Position);

                    // Copy movement rules
                    foreach (var rule in piece.MovementRules)
                    {
                        clonedPiece.AddMovementRule(rule);
                    }

                    clonedBoard.PlacePiece(clonedPiece, piece.Position);
                }
            }

            return clonedBoard;
        }

        /// <summary>
        /// Returns a string representation of the board for debugging.
        /// </summary>
        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine($"Board ({width}x{height}):");

            for (int y = height - 1; y >= 0; y--)
            {
                result.Append($"{y + 1} ");
                for (int x = 0; x < width; x++)
                {
                    var piece = pieces[x, y];
                    if (piece != null)
                    {
                        result.Append(GetPieceSymbol(piece));
                    }
                    else if (obstacles[x, y])
                    {
                        result.Append("# ");
                    }
                    else
                    {
                        result.Append(". ");
                    }
                }
                result.AppendLine();
            }

            result.Append("  ");
            for (int x = 0; x < width; x++)
            {
                result.Append((char)('a' + x) + " ");
            }

            return result.ToString();
        }

        private string GetPieceSymbol(Piece piece)
        {
            string symbol = piece.Type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                PieceType.Pawn => "P",
                _ => "?"
            };

            return piece.Team == Team.White ? symbol + " " : symbol.ToLower() + " ";
        }
    }
}
