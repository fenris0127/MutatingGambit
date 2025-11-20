using NUnit.Framework;
using MutatingGambit.Core;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 1: Core Chess Logic - Basic Piece Definition Tests
    /// </summary>
    [TestFixture]
    public class Phase1_PieceTests
    {
        [Test]
        public void Piece_HasColor()
        {
            // Arrange & Act
            var whitePiece = new Piece(PieceColor.White, PieceType.Pawn);
            var blackPiece = new Piece(PieceColor.Black, PieceType.Rook);

            // Assert
            Assert.AreEqual(PieceColor.White, whitePiece.Color);
            Assert.AreEqual(PieceColor.Black, blackPiece.Color);
        }

        [Test]
        public void Piece_HasType()
        {
            // Arrange & Act
            var king = new Piece(PieceColor.White, PieceType.King);
            var queen = new Piece(PieceColor.Black, PieceType.Queen);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var bishop = new Piece(PieceColor.Black, PieceType.Bishop);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var pawn = new Piece(PieceColor.Black, PieceType.Pawn);

            // Assert
            Assert.AreEqual(PieceType.King, king.Type);
            Assert.AreEqual(PieceType.Queen, queen.Type);
            Assert.AreEqual(PieceType.Rook, rook.Type);
            Assert.AreEqual(PieceType.Bishop, bishop.Type);
            Assert.AreEqual(PieceType.Knight, knight.Type);
            Assert.AreEqual(PieceType.Pawn, pawn.Type);
        }

        [Test]
        public void Piece_HasCurrentPosition()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Pawn);
            var position = new Position(4, 3);

            // Act
            piece.CurrentPosition = position;

            // Assert
            Assert.AreEqual(position, piece.CurrentPosition);
        }

        [Test]
        public void Piece_CanCheckIfMoved()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Pawn);

            // Assert - initially not moved
            Assert.IsFalse(piece.HasMoved);

            // Act - mark as moved
            piece.HasMoved = true;

            // Assert
            Assert.IsTrue(piece.HasMoved);
        }
    }
}
