using NUnit.Framework;
using MutatingGambit.Core;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 2: Board System Tests
    /// </summary>
    [TestFixture]
    public class Phase2_BoardSystemTests
    {
        #region Standard Board Setup Tests

        [Test]
        public void Board_CanSetupStandardChessBoard()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert
            Assert.IsNotNull(board.GetPieceAt(Position.FromNotation("e1"))); // White King
            Assert.IsNotNull(board.GetPieceAt(Position.FromNotation("e8"))); // Black King
        }

        [Test]
        public void Board_WhitePawnsOnRank2()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert - Check all white pawns on rank 2
            for (int file = 0; file < 8; file++)
            {
                var piece = board.GetPieceAt(PositionCache.Get(file, 1));
                Assert.IsNotNull(piece);
                Assert.AreEqual(PieceColor.White, piece.Color);
                Assert.AreEqual(PieceType.Pawn, piece.Type);
            }
        }

        [Test]
        public void Board_BlackPawnsOnRank7()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert - Check all black pawns on rank 7
            for (int file = 0; file < 8; file++)
            {
                var piece = board.GetPieceAt(PositionCache.Get(file, 6));
                Assert.IsNotNull(piece);
                Assert.AreEqual(PieceColor.Black, piece.Color);
                Assert.AreEqual(PieceType.Pawn, piece.Type);
            }
        }

        [Test]
        public void Board_RooksInCorners()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert
            var whiteRookA1 = board.GetPieceAt(Position.FromNotation("a1"));
            var whiteRookH1 = board.GetPieceAt(Position.FromNotation("h1"));
            var blackRookA8 = board.GetPieceAt(Position.FromNotation("a8"));
            var blackRookH8 = board.GetPieceAt(Position.FromNotation("h8"));

            Assert.AreEqual(PieceType.Rook, whiteRookA1.Type);
            Assert.AreEqual(PieceColor.White, whiteRookA1.Color);
            Assert.AreEqual(PieceType.Rook, whiteRookH1.Type);
            Assert.AreEqual(PieceColor.White, whiteRookH1.Color);
            Assert.AreEqual(PieceType.Rook, blackRookA8.Type);
            Assert.AreEqual(PieceColor.Black, blackRookA8.Color);
            Assert.AreEqual(PieceType.Rook, blackRookH8.Type);
            Assert.AreEqual(PieceColor.Black, blackRookH8.Color);
        }

        [Test]
        public void Board_KnightsInCorrectPositions()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert
            var whiteKnightB1 = board.GetPieceAt(Position.FromNotation("b1"));
            var whiteKnightG1 = board.GetPieceAt(Position.FromNotation("g1"));
            var blackKnightB8 = board.GetPieceAt(Position.FromNotation("b8"));
            var blackKnightG8 = board.GetPieceAt(Position.FromNotation("g8"));

            Assert.AreEqual(PieceType.Knight, whiteKnightB1.Type);
            Assert.AreEqual(PieceType.Knight, whiteKnightG1.Type);
            Assert.AreEqual(PieceType.Knight, blackKnightB8.Type);
            Assert.AreEqual(PieceType.Knight, blackKnightG8.Type);
        }

        [Test]
        public void Board_BishopsInCorrectPositions()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert
            var whiteBishopC1 = board.GetPieceAt(Position.FromNotation("c1"));
            var whiteBishopF1 = board.GetPieceAt(Position.FromNotation("f1"));
            var blackBishopC8 = board.GetPieceAt(Position.FromNotation("c8"));
            var blackBishopF8 = board.GetPieceAt(Position.FromNotation("f8"));

            Assert.AreEqual(PieceType.Bishop, whiteBishopC1.Type);
            Assert.AreEqual(PieceType.Bishop, whiteBishopF1.Type);
            Assert.AreEqual(PieceType.Bishop, blackBishopC8.Type);
            Assert.AreEqual(PieceType.Bishop, blackBishopF8.Type);
        }

        [Test]
        public void Board_QueensInCorrectPositions()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert
            var whiteQueen = board.GetPieceAt(Position.FromNotation("d1"));
            var blackQueen = board.GetPieceAt(Position.FromNotation("d8"));

            Assert.AreEqual(PieceType.Queen, whiteQueen.Type);
            Assert.AreEqual(PieceColor.White, whiteQueen.Color);
            Assert.AreEqual(PieceType.Queen, blackQueen.Type);
            Assert.AreEqual(PieceColor.Black, blackQueen.Color);
        }

        [Test]
        public void Board_KingsInCorrectPositions()
        {
            // Arrange
            var board = new Board(8, 8);

            // Act
            board.SetupStandardBoard();

            // Assert
            var whiteKing = board.GetPieceAt(Position.FromNotation("e1"));
            var blackKing = board.GetPieceAt(Position.FromNotation("e8"));

            Assert.AreEqual(PieceType.King, whiteKing.Type);
            Assert.AreEqual(PieceColor.White, whiteKing.Color);
            Assert.AreEqual(PieceType.King, blackKing.Type);
            Assert.AreEqual(PieceColor.Black, blackKing.Color);
        }

        #endregion

        #region Custom Board Sizes Tests

        [Test]
        public void Board_CanCreate5x5Board()
        {
            // Act
            var board = new Board(5, 5);

            // Assert
            Assert.AreEqual(5, board.Width);
            Assert.AreEqual(5, board.Height);
        }

        [Test]
        public void Board_CanCreate6x8Board()
        {
            // Act
            var board = new Board(6, 8);

            // Assert
            Assert.AreEqual(6, board.Width);
            Assert.AreEqual(8, board.Height);
        }

        [Test]
        public void Board_CustomSizeValidatesPositions()
        {
            // Arrange
            var board = new Board(5, 5);

            // Assert - Valid positions
            Assert.IsTrue(board.IsValidPosition(PositionCache.Get(0, 0)));
            Assert.IsTrue(board.IsValidPosition(PositionCache.Get(4, 4)));

            // Assert - Invalid positions
            Assert.IsFalse(board.IsValidPosition(PositionCache.Get(5, 5)));
            Assert.IsFalse(board.IsValidPosition(PositionCache.Get(-1, 0)));
        }

        [Test]
        public void Board_StandardSetupRequires8x8Board()
        {
            // Arrange
            var board = new Board(6, 6);

            // Act & Assert
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                board.SetupStandardBoard();
            });
        }

        #endregion

        #region Board Obstacles Tests

        [Test]
        public void Board_CanPlaceWallTile()
        {
            // Arrange
            var board = new Board(8, 8);
            var position = PositionCache.Get(3, 3);

            // Act
            board.SetTileType(position, TileType.Wall);

            // Assert
            var tile = board.GetTileAt(position);
            Assert.AreEqual(TileType.Wall, tile.Type);
            Assert.IsTrue(tile.IsObstacle);
        }

        [Test]
        public void Board_CanPlaceWaterTile()
        {
            // Arrange
            var board = new Board(8, 8);
            var position = PositionCache.Get(2, 5);

            // Act
            board.SetTileType(position, TileType.Water);

            // Assert
            var tile = board.GetTileAt(position);
            Assert.AreEqual(TileType.Water, tile.Type);
            Assert.IsTrue(tile.IsObstacle);
        }

        [Test]
        public void Board_ObstacleTileCannotHoldPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var position = PositionCache.Get(4, 4);
            board.SetTileType(position, TileType.Wall);

            var piece = new Piece(PieceColor.White, PieceType.Rook);

            // Act & Assert
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                board.PlacePiece(piece, position);
            });
        }

        [Test]
        public void Board_CannotSetObstacleOnOccupiedTile()
        {
            // Arrange
            var board = new Board(8, 8);
            var position = PositionCache.Get(4, 4);
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            board.PlacePiece(piece, position);

            // Act & Assert
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                board.SetTileType(position, TileType.Wall);
            });
        }

        #endregion
    }
}
