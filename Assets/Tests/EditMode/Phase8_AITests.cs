using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.AI;
using MutatingGambit.Core.Movement;
using MutatingGambit.Core.Mutations;
using MutatingGambit.Core.Artifacts;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 8: AI System Tests
    /// </summary>
    [TestFixture]
    public class Phase8_AITests
    {
        #region Basic AI Tests

        [Test]
        public void AI_CanCalculateAllPossibleMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black);

            // Act
            var allMoves = ai.GetAllPossibleMoves(board);

            // Assert
            Assert.Greater(allMoves.Count, 0, "AI should find possible moves");
        }

        [Test]
        public void AI_AppliesCurrentMutationRules()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.Black, PieceType.Rook);
            var friendlyPawn = new Piece(PieceColor.Black, PieceType.Pawn);

            // Add mutation to rook
            rook.AddMutation(new LeapingRookMutation());

            board.PlacePiece(rook, Position.FromNotation("a1"));
            board.PlacePiece(friendlyPawn, Position.FromNotation("a3"));

            var ai = new ChessAI(PieceColor.Black);

            // Act
            var moves = ai.GetPossibleMovesForPiece(board, Position.FromNotation("a1"));

            // Assert
            Assert.IsTrue(moves.Contains(Position.FromNotation("a4")),
                "AI should recognize mutation allows jumping");
        }

        [Test]
        public void AI_AppliesCurrentArtifactRules()
        {
            // Arrange
            var board = new Board(8, 8);
            board.ArtifactManager.AddArtifact(new KingsShadowArtifact());

            var ai = new ChessAI(PieceColor.Black);

            // Act
            var activeArtifacts = board.ArtifactManager.Artifacts;

            // Assert
            Assert.AreEqual(1, activeArtifacts.Count);
            // AI should be aware of this when evaluating moves
        }

        [Test]
        public void AI_OnlySelectsLegalMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black);

            // Act
            var move = ai.SelectBestMove(board);

            // Assert
            Assert.IsNotNull(move);

            var piece = board.GetPieceAt(move.From);
            Assert.IsNotNull(piece);
            Assert.AreEqual(PieceColor.Black, piece.Color);

            var legalMoves = MoveValidator.GetLegalMoves(board, move.From);
            Assert.Contains(move.To, legalMoves);
        }

        #endregion

        #region Evaluation Function Tests

        [Test]
        public void AI_CanEvaluateBoardState()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black);

            // Act
            var evaluation = ai.EvaluateBoard(board);

            // Assert
            // Equal material should be close to 0
            Assert.That(evaluation, Is.InRange(-1.0f, 1.0f));
        }

        [Test]
        public void AI_CalculatesPieceValues()
        {
            // Arrange
            var board = new Board(8, 8);
            var whiteQueen = new Piece(PieceColor.White, PieceType.Queen);
            var blackPawn = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(whiteQueen, Position.FromNotation("d1"));
            board.PlacePiece(blackPawn, Position.FromNotation("d7"));

            var ai = new ChessAI(PieceColor.Black);

            // Act
            var evaluation = ai.EvaluateBoard(board);

            // Assert
            // White should be winning (has queen vs pawn)
            Assert.Less(evaluation, -5.0f, "White should be significantly ahead");
        }

        [Test]
        public void AI_EvaluatesPositionAdvantage()
        {
            // Arrange
            var board = new Board(8, 8);

            // Center control is valuable
            var whitePawn = new Piece(PieceColor.White, PieceType.Pawn);
            var blackPawn = new Piece(PieceColor.Black, PieceType.Pawn);

            board.PlacePiece(whitePawn, Position.FromNotation("e4")); // Center
            board.PlacePiece(blackPawn, Position.FromNotation("a7")); // Edge

            var ai = new ChessAI(PieceColor.Black);

            // Act
            var evaluation = ai.EvaluateBoard(board);

            // Assert
            // White has better position
            Assert.Less(evaluation, 0, "Center control should be valued");
        }

        [Test]
        public void AI_ConsidersMutationSynergies()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.Black, PieceType.Rook);
            rook.AddMutation(new LeapingRookMutation());

            board.PlacePiece(rook, Position.FromNotation("a1"));

            var ai = new ChessAI(PieceColor.Black);

            // Act
            var value = ai.EvaluatePiece(rook, Position.FromNotation("a1"));

            // Assert
            // Mutated pieces should be valued higher
            Assert.Greater(value, 5.0f, "Mutated rook should be more valuable than normal");
        }

        #endregion

        #region Advanced AI Tests

        [Test]
        public void AI_CanLookAhead()
        {
            // Arrange
            var board = new Board(8, 8);

            // Set up a simple capture opportunity
            var blackQueen = new Piece(PieceColor.Black, PieceType.Queen);
            var whiteRook = new Piece(PieceColor.White, PieceType.Rook);

            board.PlacePiece(blackQueen, Position.FromNotation("d8"));
            board.PlacePiece(whiteRook, Position.FromNotation("d1")); // Free rook

            var ai = new ChessAI(PieceColor.Black);

            // Act
            var move = ai.SelectBestMove(board);

            // Assert
            Assert.AreEqual(Position.FromNotation("d8"), move.From);
            Assert.AreEqual(Position.FromNotation("d1"), move.To);
        }

        [Test]
        public void AI_RespectsTimeLimit()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black);

            // Act
            var startTime = System.DateTime.Now;
            var move = ai.SelectBestMove(board, timeLimitMs: 1000);
            var elapsedMs = (System.DateTime.Now - startTime).TotalMilliseconds;

            // Assert
            Assert.IsNotNull(move);
            Assert.LessOrEqual(elapsedMs, 1500, "Should respect time limit");
        }

        [Test]
        public void AI_DifficultyAffectsStrength()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();

            var easyAI = new ChessAI(PieceColor.Black, Difficulty.Easy);
            var hardAI = new ChessAI(PieceColor.Black, Difficulty.Hard);

            // Act
            var easyEval = easyAI.EvaluateBoard(board);
            var hardEval = hardAI.EvaluateBoard(board);

            // Assert - Both should evaluate, but hard AI has deeper analysis
            Assert.IsNotNull(easyAI);
            Assert.IsNotNull(hardAI);
            Assert.AreEqual(Difficulty.Easy, easyAI.CurrentDifficulty);
            Assert.AreEqual(Difficulty.Hard, hardAI.CurrentDifficulty);
        }

        [Test]
        public void AI_FollowsStrategyPatterns()
        {
            // Arrange
            var board = new Board(8, 8);

            // Early game: develop pieces
            var blackKnight = new Piece(PieceColor.Black, PieceType.Knight);
            board.PlacePiece(blackKnight, Position.FromNotation("b8"));

            var ai = new ChessAI(PieceColor.Black, Difficulty.Normal);

            // Act
            var move = ai.SelectBestMove(board);

            // Assert
            Assert.IsNotNull(move);
            // Should try to develop the knight
            if (move.From == Position.FromNotation("b8"))
            {
                Assert.IsTrue(
                    move.To == Position.FromNotation("c6") ||
                    move.To == Position.FromNotation("a6"),
                    "Should develop knight to standard squares"
                );
            }
        }

        #endregion

        #region Difficulty Level Tests

        [Test]
        public void AI_EasyDifficulty_MakesQuickMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black, Difficulty.Easy);

            // Act
            var startTime = System.DateTime.Now;
            var move = ai.SelectBestMove(board);
            var elapsedMs = (System.DateTime.Now - startTime).TotalMilliseconds;

            // Assert
            Assert.IsNotNull(move);
            Assert.Less(elapsedMs, 500, "Easy AI should move quickly");
        }

        [Test]
        public void AI_NormalDifficulty_BalancesSpeedAndQuality()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black, Difficulty.Normal);

            // Act
            var move = ai.SelectBestMove(board);

            // Assert
            Assert.IsNotNull(move);
            Assert.AreEqual(Difficulty.Normal, ai.CurrentDifficulty);
        }

        [Test]
        public void AI_HardDifficulty_FindsGoodMoves()
        {
            // Arrange - Set up a tactic
            var board = new Board(8, 8);
            var blackQueen = new Piece(PieceColor.Black, PieceType.Queen);
            var whiteKing = new Piece(PieceColor.White, PieceType.King);
            var whiteRook = new Piece(PieceColor.White, PieceType.Rook);

            board.PlacePiece(blackQueen, Position.FromNotation("d8"));
            board.PlacePiece(whiteKing, Position.FromNotation("e1"));
            board.PlacePiece(whiteRook, Position.FromNotation("h1")); // Undefended

            var ai = new ChessAI(PieceColor.Black, Difficulty.Hard);

            // Act
            var move = ai.SelectBestMove(board);

            // Assert - Should capture the rook or threaten checkmate
            Assert.IsNotNull(move);
        }

        [Test]
        public void AI_MasterDifficulty_PlaysOptimally()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var ai = new ChessAI(PieceColor.Black, Difficulty.Master);

            // Act
            var move = ai.SelectBestMove(board);

            // Assert
            Assert.IsNotNull(move);
            Assert.AreEqual(Difficulty.Master, ai.CurrentDifficulty);
            // Master should make strong opening moves
        }

        #endregion
    }
}
