using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.Movement;
using MutatingGambit.Core.Mutations;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 4: Mutation System Tests
    /// </summary>
    [TestFixture]
    public class Phase4_MutationTests
    {
        #region Mutation Framework Tests

        [Test]
        public void Mutation_CanBeAppliedToPiece()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            var mutation = new LeapingRookMutation();

            // Act
            piece.AddMutation(mutation);

            // Assert
            Assert.AreEqual(1, piece.Mutations.Count);
            Assert.Contains(mutation, piece.Mutations);
        }

        [Test]
        public void Mutation_MultipleMutationsCanBeApplied()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            var mutation1 = new LeapingRookMutation();
            var mutation2 = new LeapingRookMutation(); // Same type for testing

            // Act
            piece.AddMutation(mutation1);
            piece.AddMutation(mutation2);

            // Assert
            Assert.AreEqual(2, piece.Mutations.Count);
        }

        [Test]
        public void Mutation_CanBeRemovedFromPiece()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            var mutation = new LeapingRookMutation();
            piece.AddMutation(mutation);

            // Act
            var removed = piece.RemoveMutation(mutation);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, piece.Mutations.Count);
        }

        [Test]
        public void Mutation_AllMutationsCanBeCleared()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            piece.AddMutation(new LeapingRookMutation());
            piece.AddMutation(new LeapingRookMutation());

            // Act
            piece.ClearMutations();

            // Assert
            Assert.AreEqual(0, piece.Mutations.Count);
        }

        [Test]
        public void Mutation_PieceWithMutationsIsVisuallyDistinguished()
        {
            // Arrange
            var normalPiece = new Piece(PieceColor.White, PieceType.Rook);
            var mutatedPiece = new Piece(PieceColor.White, PieceType.Rook);
            mutatedPiece.AddMutation(new LeapingRookMutation());

            // Act
            var normalString = normalPiece.ToString();
            var mutatedString = mutatedPiece.ToString();

            // Assert
            Assert.IsFalse(normalString.Contains("*"));
            Assert.IsTrue(mutatedString.Contains("*"));
        }

        #endregion

        #region Leaping Rook Mutation Tests

        [Test]
        public void LeapingRook_CanJumpOverOneFriendlyPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var friendlyPawn = new Piece(PieceColor.White, PieceType.Pawn);

            rook.AddMutation(new LeapingRookMutation());

            board.PlacePiece(rook, Position.FromNotation("a1"));
            board.PlacePiece(friendlyPawn, Position.FromNotation("a3"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a4")), "Should be able to jump over friendly piece");
            Assert.IsTrue(moves.Contains(Position.FromNotation("a5")), "Should be able to continue beyond jump");
        }

        [Test]
        public void LeapingRook_CannotJumpOverEnemyPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var enemyPawn = new Piece(PieceColor.Black, PieceType.Pawn);

            rook.AddMutation(new LeapingRookMutation());

            board.PlacePiece(rook, Position.FromNotation("a1"));
            board.PlacePiece(enemyPawn, Position.FromNotation("a3"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a3")), "Should be able to capture enemy");
            Assert.IsFalse(moves.Contains(Position.FromNotation("a4")), "Should NOT jump over enemy piece");
        }

        [Test]
        public void LeapingRook_CannotJumpOverTwoPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var friendly1 = new Piece(PieceColor.White, PieceType.Pawn);
            var friendly2 = new Piece(PieceColor.White, PieceType.Pawn);

            rook.AddMutation(new LeapingRookMutation());

            board.PlacePiece(rook, Position.FromNotation("a1"));
            board.PlacePiece(friendly1, Position.FromNotation("a3"));
            board.PlacePiece(friendly2, Position.FromNotation("a5"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a4")), "Should jump over first piece");
            Assert.IsFalse(moves.Contains(Position.FromNotation("a6")), "Should NOT jump over two pieces");
        }

        #endregion

        #region Splitting Knight Mutation Tests

        [Test]
        public void SplittingKnight_SpawnsPawnWhenCapturing()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var enemy = new Piece(PieceColor.Black, PieceType.Pawn);

            knight.AddMutation(new SplittingKnightMutation());

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.PlacePiece(enemy, Position.FromNotation("f6"));

            // Act - Simulate capture move
            var originalPos = Position.FromNotation("e4");
            var targetPos = Position.FromNotation("f6");

            board.PlacePiece(null, originalPos); // Remove knight from original position
            board.PlacePiece(knight, targetPos); // Place knight at target (capturing enemy)

            // Apply mutation effect
            var mutation = knight.Mutations[0] as SplittingKnightMutation;
            mutation.OnCapture(board, originalPos, targetPos, knight);

            // Assert
            var spawnedPiece = board.GetPieceAt(originalPos);
            Assert.IsNotNull(spawnedPiece, "A pawn should be spawned");
            Assert.AreEqual(PieceType.Pawn, spawnedPiece.Type);
            Assert.AreEqual(PieceColor.White, spawnedPiece.Color);
        }

        [Test]
        public void SplittingKnight_SpawnedPawnHasSameColor()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.Black, PieceType.Knight);
            var enemy = new Piece(PieceColor.White, PieceType.Pawn);

            knight.AddMutation(new SplittingKnightMutation());

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.PlacePiece(enemy, Position.FromNotation("d6"));

            // Act
            var originalPos = Position.FromNotation("e4");
            var targetPos = Position.FromNotation("d6");

            board.PlacePiece(null, originalPos);
            board.PlacePiece(knight, targetPos);

            var mutation = knight.Mutations[0] as SplittingKnightMutation;
            mutation.OnCapture(board, originalPos, targetPos, knight);

            // Assert
            var spawnedPiece = board.GetPieceAt(originalPos);
            Assert.AreEqual(PieceColor.Black, spawnedPiece.Color);
        }

        [Test]
        public void SplittingKnight_OnlySpawnsIfOriginalPositionEmpty()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var enemy = new Piece(PieceColor.Black, PieceType.Pawn);
            var blocker = new Piece(PieceColor.White, PieceType.Pawn);

            knight.AddMutation(new SplittingKnightMutation());

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.PlacePiece(enemy, Position.FromNotation("f6"));

            // Act
            var originalPos = Position.FromNotation("e4");
            var targetPos = Position.FromNotation("f6");

            board.PlacePiece(blocker, originalPos); // Block original position
            board.PlacePiece(knight, targetPos);

            var mutation = knight.Mutations[0] as SplittingKnightMutation;
            mutation.OnCapture(board, originalPos, targetPos, knight);

            // Assert
            var pieceAtOriginal = board.GetPieceAt(originalPos);
            Assert.AreEqual(blocker, pieceAtOriginal, "Original piece should remain if position is blocked");
        }

        #endregion

        #region Glass Bishop Mutation Tests

        [Test]
        public void GlassBishop_CanMoveExactlyThreeSquares()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);

            bishop.AddMutation(new GlassBishopMutation());

            board.PlacePiece(bishop, Position.FromNotation("d4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("d4"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a1")), "Should move exactly 3 diagonally");
            Assert.IsTrue(moves.Contains(Position.FromNotation("g7")), "Should move exactly 3 diagonally");
            Assert.IsTrue(moves.Contains(Position.FromNotation("a7")), "Should move exactly 3 diagonally");
            Assert.IsTrue(moves.Contains(Position.FromNotation("g1")), "Should move exactly 3 diagonally");
        }

        [Test]
        public void GlassBishop_CannotMoveOneOrTwoSquares()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);

            bishop.AddMutation(new GlassBishopMutation());

            board.PlacePiece(bishop, Position.FromNotation("d4"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("d4"));

            // Assert
            Assert.IsFalse(moves.Contains(Position.FromNotation("e5")), "Should NOT move 1 square");
            Assert.IsFalse(moves.Contains(Position.FromNotation("c3")), "Should NOT move 1 square");
            Assert.IsFalse(moves.Contains(Position.FromNotation("f6")), "Should NOT move 2 squares");
            Assert.IsFalse(moves.Contains(Position.FromNotation("b2")), "Should NOT move 2 squares");
        }

        [Test]
        public void GlassBishop_CannotMoveFourOrMoreSquares()
        {
            // Arrange
            var board = new Board(8, 8);
            var bishop = new Piece(PieceColor.White, PieceType.Bishop);

            bishop.AddMutation(new GlassBishopMutation());

            board.PlacePiece(bishop, Position.FromNotation("a1"));

            // Act
            var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("d4")), "Should move exactly 3");
            Assert.IsFalse(moves.Contains(Position.FromNotation("e5")), "Should NOT move 4 squares");
            Assert.IsFalse(moves.Contains(Position.FromNotation("h8")), "Should NOT move 7 squares");
        }

        #endregion

        #region Mutation Persistence Tests

        [Test]
        public void Mutation_IsPersistentAcrossGame()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            var mutation = new LeapingRookMutation();
            piece.AddMutation(mutation);

            // Act - Simulate multiple moves
            piece.HasMoved = true;
            piece.CurrentPosition = Position.FromNotation("e4");

            // Assert
            Assert.AreEqual(1, piece.Mutations.Count);
            Assert.Contains(mutation, piece.Mutations);
        }

        [Test]
        public void Mutation_CanBeSavedAndLoaded()
        {
            // Arrange
            var originalPiece = new Piece(PieceColor.White, PieceType.Knight);
            originalPiece.AddMutation(new SplittingKnightMutation());

            // Act - Clone the piece (simulating save/load)
            var clonedPiece = originalPiece.Clone();

            // Assert
            Assert.AreEqual(originalPiece.Mutations.Count, clonedPiece.Mutations.Count);
            Assert.AreEqual(originalPiece.Type, clonedPiece.Type);
            Assert.AreEqual(originalPiece.Color, clonedPiece.Color);
        }

        [Test]
        public void Mutation_VisualIndicatorPersists()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Bishop);
            piece.AddMutation(new GlassBishopMutation());

            // Act
            var toString1 = piece.ToString();
            piece.HasMoved = true;
            var toString2 = piece.ToString();

            // Assert
            Assert.IsTrue(toString1.Contains("*"));
            Assert.IsTrue(toString2.Contains("*"));
        }

        #endregion
    }
}
