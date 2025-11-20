using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.Movement;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 3: Piece System - Movement Rules Tests
    /// </summary>
    [TestFixture]
    public class Phase3_PieceMovementTests
    {
        #region Pawn Movement Tests

        [Test]
        public void Pawn_CanMoveForwardOneSquare()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            board.PlacePiece(pawn, Position.FromNotation("e2"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e2"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("e3")));
        }

        [Test]
        public void Pawn_CanMoveTwoSquaresOnFirstMove()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            board.PlacePiece(pawn, Position.FromNotation("e2"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e2"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("e3")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("e4")));
        }

        [Test]
        public void Pawn_CanCaptureDiagonally()
        {
            // Arrange
            var board = new Board(8, 8);
            var whitePawn = new Piece(PieceColor.White, PieceType.Pawn);
            var blackPawn = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(whitePawn, Position.FromNotation("e4"));
            board.PlacePiece(blackPawn, Position.FromNotation("d5"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("d5"))); // Diagonal capture
        }

        [Test]
        public void Pawn_CannotMoveBackward()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            board.PlacePiece(pawn, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("e3")));
            Assert.IsFalse(moves.Contains(Position.FromNotation("e2")));
        }

        [Test]
        public void Pawn_CannotMoveForwardIfBlocked()
        {
            // Arrange
            var board = new Board(8, 8);
            var whitePawn = new Piece(PieceColor.White, PieceType.Pawn);
            var blackPawn = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(whitePawn, Position.FromNotation("e2"));
            board.PlacePiece(blackPawn, Position.FromNotation("e3"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e2"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("e3")));
            Assert.IsFalse(moves.Contains(Position.FromNotation("e4")));
        }

        [Test]
        public void Pawn_PromotesOnRank8ForWhite()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            board.PlacePiece(pawn, Position.FromNotation("e7"));

            // Act
            var targetPos = Position.FromNotation("e8");
            var canPromote = MoveValidator.WillPromote(board, Position.FromNotation("e7"), targetPos);

            // Assert
            Assert.IsTrue(canPromote);
        }

        #endregion

        #region Rook Movement Tests

        [Test]
        public void Rook_CanMoveVertically()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            board.PlacePiece(rook, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("e1")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("e5")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("e8")));
        }

        [Test]
        public void Rook_CanMoveHorizontally()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            board.PlacePiece(rook, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a4")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("h4")));
        }

        [Test]
        public void Rook_CannotMoveDiagonally()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            board.PlacePiece(rook, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("f5")));
            Assert.IsFalse(moves.Contains(Position.FromNotation("d3")));
        }

        [Test]
        public void Rook_CannotJumpOverPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var blockingPiece = new Piece(PieceColor.White, PieceType.Pawn);

            board.PlacePiece(rook, Position.FromNotation("e1"));
            board.PlacePiece(blockingPiece, Position.FromNotation("e3"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("e2")));
            Assert.IsFalse(moves.Contains(Position.FromNotation("e4"))); // Can't jump
        }

        [Test]
        public void Rook_CanCaptureEnemyPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var enemy = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(rook, Position.FromNotation("e1"));
            board.PlacePiece(enemy, Position.FromNotation("e5"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("e5"))); // Can capture
            Assert.IsFalse(moves.Contains(Position.FromNotation("e6"))); // Can't move beyond
        }

        [Test]
        public void Rook_CannotCaptureOwnPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var friendly = new Piece(PieceColor.White, PieceType.Pawn);

            board.PlacePiece(rook, Position.FromNotation("e1"));
            board.PlacePiece(friendly, Position.FromNotation("e5"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e1"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("e5")));
        }

        #endregion

        #region Knight Movement Tests

        [Test]
        public void Knight_MovesInLShape()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            board.PlacePiece(knight, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert - All 8 possible L-shaped moves
            Assert.IsTrue(moves.Contains(Position.FromNotation("d6")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("f6")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("d2")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("f2")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("c5")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("c3")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("g5")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("g3")));
        }

        [Test]
        public void Knight_CanJumpOverPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var blocking1 = new Piece(PieceColor.White, PieceType.Pawn);
            var blocking2 = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.PlacePiece(blocking1, Position.FromNotation("e5"));
            board.PlacePiece(blocking2, Position.FromNotation("d4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert - Knight can still move to all valid squares
            Assert.IsTrue(moves.Contains(Position.FromNotation("d6")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("f6")));
        }

        [Test]
        public void Knight_RespectsBoardBoundaries()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            board.PlacePiece(knight, Position.FromNotation("a1"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

            // Assert - Only 2 valid moves from corner
            Assert.AreEqual(2, moves.Count);
            Assert.IsTrue(moves.Contains(Position.FromNotation("b3")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("c2")));
        }

        #endregion

        #region Bishop Movement Tests

        [Test]
        public void Bishop_CanMoveDiagonally()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);
            board.PlacePiece(bishop, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a8"))); // Up-left
            Assert.IsTrue(moves.Contains(Position.FromNotation("h7"))); // Up-right
            Assert.IsTrue(moves.Contains(Position.FromNotation("b1"))); // Down-left
            Assert.IsTrue(moves.Contains(Position.FromNotation("h1"))); // Down-right
        }

        [Test]
        public void Bishop_CannotMoveHorizontallyOrVertically()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);
            board.PlacePiece(bishop, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("e5"))); // Vertical
            Assert.IsFalse(moves.Contains(Position.FromNotation("f4"))); // Horizontal
        }

        [Test]
        public void Bishop_CannotJumpOverPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);
            var blocking = new Piece(PieceColor.White, PieceType.Pawn);

            board.PlacePiece(bishop, Position.FromNotation("e4"));
            board.PlacePiece(blocking, Position.FromNotation("f5"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("g6"))); // Can't jump
            Assert.IsFalse(moves.Contains(Position.FromNotation("h7")));
        }

        [Test]
        public void Bishop_CanCaptureEnemyPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);
            var enemy = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(bishop, Position.FromNotation("e4"));
            board.PlacePiece(enemy, Position.FromNotation("g6"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("g6"))); // Can capture
            Assert.IsFalse(moves.Contains(Position.FromNotation("h7"))); // Can't move beyond
        }

        #endregion

        #region Queen Movement Tests

        [Test]
        public void Queen_CanMoveInAllDirections()
        {
            // Arrange
            var board = new Board(8, 8);
            var queen = new Piece(PieceColor.White, PieceType.Queen);
            board.PlacePiece(queen, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert - Horizontal
            Assert.IsTrue(moves.Contains(Position.FromNotation("a4")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("h4")));

            // Assert - Vertical
            Assert.IsTrue(moves.Contains(Position.FromNotation("e1")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("e8")));

            // Assert - Diagonal
            Assert.IsTrue(moves.Contains(Position.FromNotation("a8")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("h7")));
        }

        [Test]
        public void Queen_CannotJumpOverPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var queen = new Piece(PieceColor.White, PieceType.Queen);
            var blocking = new Piece(PieceColor.White, PieceType.Pawn);

            board.PlacePiece(queen, Position.FromNotation("e4"));
            board.PlacePiece(blocking, Position.FromNotation("e6"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("e5")));
            Assert.IsFalse(moves.Contains(Position.FromNotation("e7"))); // Can't jump
        }

        [Test]
        public void Queen_CombinesRookAndBishopMovement()
        {
            // Arrange
            var board = new Board(8, 8);
            var queen = new Piece(PieceColor.White, PieceType.Queen);
            board.PlacePiece(queen, Position.FromNotation("d4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("d4"));

            // Assert - Should have both rook-like and bishop-like moves
            Assert.IsTrue(moves.Count > 20); // Queen has many moves from center

            // Rook-like moves
            Assert.IsTrue(moves.Contains(Position.FromNotation("d1")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("a4")));

            // Bishop-like moves
            Assert.IsTrue(moves.Contains(Position.FromNotation("a1")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("g7")));
        }

        #endregion

        #region King Movement Tests

        [Test]
        public void King_CanMoveOneSquareInAnyDirection()
        {
            // Arrange
            var board = new Board(8, 8);
            var king = new Piece(PieceColor.White, PieceType.King);
            board.PlacePiece(king, Position.FromNotation("e4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("e4"));

            // Assert - All 8 adjacent squares
            Assert.IsTrue(moves.Contains(Position.FromNotation("e5")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("e3")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("d4")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("f4")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("d5")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("f5")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("d3")));
            Assert.IsTrue(moves.Contains(Position.FromNotation("f3")));
        }

        [Test]
        public void King_CanCastle_Kingside()
        {
            // Arrange
            var board = new Board(8, 8);
            var king = new Piece(PieceColor.White, PieceType.King);
            var rook = new Piece(PieceColor.White, PieceType.Rook);

            board.PlacePiece(king, Position.FromNotation("e1"));
            board.PlacePiece(rook, Position.FromNotation("h1"));

            // Act
            var canCastle = MoveValidator.CanCastle(board, Position.FromNotation("e1"), Position.FromNotation("g1"));

            // Assert
            Assert.IsTrue(canCastle);
        }

        #endregion
    }
}
