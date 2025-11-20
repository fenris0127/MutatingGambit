namespace MutatingGambit.Core
{
    /// <summary>
    /// Represents a chess board with configurable dimensions
    /// </summary>
    public class Board
    {
        private readonly Tile[,] _grid;
        private readonly int _width;
        private readonly int _height;
        private readonly Artifacts.ArtifactManager _artifactManager;

        public int Width => _width;
        public int Height => _height;
        public Artifacts.ArtifactManager ArtifactManager => _artifactManager;

        public Board(int width = 8, int height = 8)
        {
            if (width <= 0 || height <= 0)
            {
                throw new System.ArgumentException("Board dimensions must be positive");
            }

            _width = width;
            _height = height;
            _grid = new Tile[width, height];
            _artifactManager = new Artifacts.ArtifactManager();

            // Initialize all tiles as empty
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = new Tile(TileType.Empty);
                }
            }
        }

        /// <summary>
        /// Places a piece at the specified position
        /// </summary>
        public void PlacePiece(Piece piece, Position position)
        {
            ValidatePosition(position);
            var tile = _grid[position.X, position.Y];

            if (tile.IsObstacle)
            {
                throw new System.InvalidOperationException($"Cannot place piece on obstacle at {position}");
            }

            tile.Piece = piece;
            if (piece != null)
            {
                piece.CurrentPosition = position;
            }
        }

        /// <summary>
        /// Gets the piece at the specified position
        /// </summary>
        public Piece GetPieceAt(Position position)
        {
            ValidatePosition(position);
            return _grid[position.X, position.Y].Piece;
        }

        /// <summary>
        /// Gets the tile at the specified position
        /// </summary>
        public Tile GetTileAt(Position position)
        {
            ValidatePosition(position);
            return _grid[position.X, position.Y];
        }

        /// <summary>
        /// Sets the tile type at the specified position
        /// </summary>
        public void SetTileType(Position position, TileType tileType)
        {
            ValidatePosition(position);
            var tile = _grid[position.X, position.Y];

            if (tile.Piece != null && tileType != TileType.Empty)
            {
                throw new System.InvalidOperationException($"Cannot set tile to obstacle when a piece is present at {position}");
            }

            tile.Type = tileType;
        }

        /// <summary>
        /// Validates if a position is within board bounds
        /// </summary>
        public bool IsValidPosition(Position position)
        {
            return position.X >= 0 && position.X < _width &&
                   position.Y >= 0 && position.Y < _height;
        }

        /// <summary>
        /// Sets up the standard chess board with all pieces in starting positions
        /// </summary>
        public void SetupStandardBoard()
        {
            if (_width != 8 || _height != 8)
            {
                throw new System.InvalidOperationException("Standard board setup requires an 8x8 board");
            }

            // Clear the board first
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y].Piece = null;
                    _grid[x, y].Type = TileType.Empty;
                }
            }

            // Setup white pieces (rank 1 and 2)
            SetupBackRank(PieceColor.White, 0);
            SetupPawnRank(PieceColor.White, 1);

            // Setup black pieces (rank 8 and 7)
            SetupBackRank(PieceColor.Black, 7);
            SetupPawnRank(PieceColor.Black, 6);
        }

        private void SetupBackRank(PieceColor color, int rank)
        {
            // Rooks
            PlacePiece(new Piece(color, PieceType.Rook), PositionCache.Get(0, rank));
            PlacePiece(new Piece(color, PieceType.Rook), PositionCache.Get(7, rank));

            // Knights
            PlacePiece(new Piece(color, PieceType.Knight), PositionCache.Get(1, rank));
            PlacePiece(new Piece(color, PieceType.Knight), PositionCache.Get(6, rank));

            // Bishops
            PlacePiece(new Piece(color, PieceType.Bishop), PositionCache.Get(2, rank));
            PlacePiece(new Piece(color, PieceType.Bishop), PositionCache.Get(5, rank));

            // Queen and King
            PlacePiece(new Piece(color, PieceType.Queen), PositionCache.Get(3, rank));
            PlacePiece(new Piece(color, PieceType.King), PositionCache.Get(4, rank));
        }

        private void SetupPawnRank(PieceColor color, int rank)
        {
            for (int file = 0; file < 8; file++)
            {
                PlacePiece(new Piece(color, PieceType.Pawn), PositionCache.Get(file, rank));
            }
        }

        private void ValidatePosition(Position position)
        {
            if (!IsValidPosition(position))
            {
                throw new System.ArgumentOutOfRangeException(nameof(position),
                    $"Position ({position.X}, {position.Y}) is out of bounds for board size {_width}x{_height}");
            }
        }

        /// <summary>
        /// Creates a deep copy of the board
        /// </summary>
        public Board Clone()
        {
            var clonedBoard = new Board(_width, _height);

            // Copy all tiles and pieces
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var originalTile = _grid[x, y];
                    var clonedTile = clonedBoard._grid[x, y];

                    // Copy tile type
                    clonedTile.Type = originalTile.Type;

                    // Clone piece if present
                    if (originalTile.Piece != null)
                    {
                        var clonedPiece = originalTile.Piece.Clone();
                        clonedTile.Piece = clonedPiece;
                        clonedPiece.CurrentPosition = PositionCache.Get(x, y);
                    }
                }
            }

            // Note: We don't clone artifacts as they are typically global effects
            // and don't need to be cloned for move simulation

            return clonedBoard;
        }
    }
}
