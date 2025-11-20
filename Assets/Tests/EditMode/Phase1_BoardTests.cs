using NUnit.Framework;
using MutatingGambit.Core;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 1: Core Chess Logic - Board Representation Tests
    /// </summary>
    [TestFixture]
    public class Phase1_BoardTests
    {
        [Test]
        public void Board_CanCreate8x8Grid()
        {
            // Arrange & Act
            var board = new Board(8, 8);

            // Assert
            Assert.AreEqual(8, board.Width);
            Assert.AreEqual(8, board.Height);
        }

        [Test]
        public void Board_CanPlacePieceAtPosition()
        {
            // Arrange
            var board = new Board(8, 8);
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            var position = PositionCache.Get(0, 0);

            // Act
            board.PlacePiece(piece, position);

            // Assert
            Assert.AreEqual(piece, board.GetPieceAt(position));
            Assert.AreEqual(position, piece.CurrentPosition);
        }

        [Test]
        public void Board_CanGetPieceAtPosition()
        {
            // Arrange
            var board = new Board(8, 8);
            var piece = new Piece(PieceColor.White, PieceType.Knight);
            var position = PositionCache.Get(3, 4);
            board.PlacePiece(piece, position);

            // Act
            var retrievedPiece = board.GetPieceAt(position);

            // Assert
            Assert.AreEqual(piece, retrievedPiece);
            Assert.AreEqual(PieceColor.White, retrievedPiece.Color);
            Assert.AreEqual(PieceType.Knight, retrievedPiece.Type);
        }

        [Test]
        public void Board_ReturnsNullForEmptyPosition()
        {
            // Arrange
            var board = new Board(8, 8);
            var position = PositionCache.Get(5, 5);

            // Act
            var piece = board.GetPieceAt(position);

            // Assert
            Assert.IsNull(piece);
        }

        [Test]
        public void Board_ThrowsExceptionForOutOfBoundsAccess()
        {
            // Arrange
            var board = new Board(8, 8);
            var outOfBoundsPosition = new Position(10, 10);

            // Act & Assert
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                board.GetPieceAt(outOfBoundsPosition);
            });
        }
    }
}
