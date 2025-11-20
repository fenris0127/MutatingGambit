using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.UI;
using MutatingGambit.Core.Mutations;
using MutatingGambit.Core.Artifacts;
using MutatingGambit.Core.Victory;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 9: UI System Tests
    /// </summary>
    [TestFixture]
    public class Phase9_UITests
    {
        #region Board Display Tests

        [Test]
        public void BoardDisplay_CanRenderBoard()
        {
            // Arrange
            var board = new Board(8, 8);
            board.SetupStandardBoard();
            var display = new BoardDisplay();

            // Act
            var output = display.RenderBoard(board);

            // Assert
            Assert.IsNotEmpty(output);
            Assert.IsTrue(output.Contains("♔") || output.Contains("K"), "Should show king");
            Assert.IsTrue(output.Contains("♟") || output.Contains("P"), "Should show pawn");
        }

        [Test]
        public void BoardDisplay_HighlightsLegalMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            board.PlacePiece(piece, Position.FromNotation("e4"));

            var display = new BoardDisplay();
            display.HighlightMoves = true;
            display.SelectedPosition = Position.FromNotation("e4");

            // Act
            var output = display.RenderBoard(board);

            // Assert
            Assert.IsNotEmpty(output);
            // Should indicate highlighted squares
        }

        [Test]
        public void BoardDisplay_ShowsMutatedPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            rook.AddMutation(new LeapingRookMutation());
            board.PlacePiece(rook, Position.FromNotation("a1"));

            var display = new BoardDisplay();

            // Act
            var output = display.RenderBoard(board);

            // Assert
            Assert.IsTrue(output.Contains("*") || output.Contains("!"),
                "Mutated pieces should be visually distinguished");
        }

        [Test]
        public void BoardDisplay_ShowsArtifactList()
        {
            // Arrange
            var board = new Board(8, 8);
            board.ArtifactManager.AddArtifact(new KingsShadowArtifact());
            board.ArtifactManager.AddArtifact(new CavalryChargeArtifact());

            var display = new BoardDisplay();

            // Act
            var artifactDisplay = display.RenderArtifacts(board);

            // Assert
            Assert.IsNotEmpty(artifactDisplay);
            Assert.IsTrue(artifactDisplay.Contains("King's Shadow"));
            Assert.IsTrue(artifactDisplay.Contains("Cavalry Charge"));
        }

        #endregion

        #region User Input Tests

        [Test]
        public void InputHandler_CanSelectPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var piece = new Piece(PieceColor.White, PieceType.Knight);
            board.PlacePiece(piece, Position.FromNotation("b1"));

            var inputHandler = new InputHandler();

            // Act
            inputHandler.SelectPosition(Position.FromNotation("b1"));
            var selected = inputHandler.GetSelectedPosition();

            // Assert
            Assert.AreEqual(Position.FromNotation("b1"), selected);
        }

        [Test]
        public void InputHandler_ValidatesIllegalMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            board.PlacePiece(pawn, Position.FromNotation("e2"));

            var inputHandler = new InputHandler();
            inputHandler.SelectPosition(Position.FromNotation("e2"));

            // Act
            var isValid = inputHandler.ValidateMove(board, Position.FromNotation("e2"), Position.FromNotation("e5"));

            // Assert
            Assert.IsFalse(isValid, "Pawn cannot move 3 squares");
        }

        [Test]
        public void InputHandler_AllowsLegalMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            board.PlacePiece(pawn, Position.FromNotation("e2"));

            var inputHandler = new InputHandler();

            // Act
            var isValid = inputHandler.ValidateMove(board, Position.FromNotation("e2"), Position.FromNotation("e4"));

            // Assert
            Assert.IsTrue(isValid, "Pawn can move 2 squares on first move");
        }

        [Test]
        public void InputHandler_ProvidesMoveFeedback()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            board.PlacePiece(knight, Position.FromNotation("b1"));

            var inputHandler = new InputHandler();

            // Act
            var feedback = inputHandler.GetMoveFeedback(board, Position.FromNotation("b1"), Position.FromNotation("a3"));

            // Assert
            Assert.IsNotEmpty(feedback);
        }

        #endregion

        #region Game State Display Tests

        [Test]
        public void GameStateDisplay_ShowsCurrentTurn()
        {
            // Arrange
            var gameState = new GameStateDisplay();
            gameState.CurrentPlayer = PieceColor.White;

            // Act
            var output = gameState.RenderCurrentTurn();

            // Assert
            Assert.IsTrue(output.Contains("White") || output.Contains("white"));
        }

        [Test]
        public void GameStateDisplay_ShowsVictoryCondition()
        {
            // Arrange
            var condition = new CheckmateInNMovesCondition(5);
            var gameState = new GameStateDisplay();
            gameState.VictoryCondition = condition;

            // Act
            var output = gameState.RenderVictoryCondition();

            // Assert
            Assert.IsTrue(output.Contains("5"));
            Assert.IsTrue(output.Contains("checkmate") || output.Contains("Checkmate"));
        }

        [Test]
        public void GameStateDisplay_ShowsRemainingMoves()
        {
            // Arrange
            var condition = new CheckmateInNMovesCondition(10);
            var gameState = new GameStateDisplay();
            gameState.VictoryCondition = condition;
            gameState.MoveCount = 3;

            // Act
            var output = gameState.RenderMoveCount();

            // Assert
            Assert.IsTrue(output.Contains("3") || output.Contains("7"),
                "Should show current or remaining moves");
        }

        [Test]
        public void GameStateDisplay_ShowsBrokenPieces()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            rook.TakeDamage(3); // Break the piece

            board.PlacePiece(rook, Position.FromNotation("a1"));

            var gameState = new GameStateDisplay();

            // Act
            var output = gameState.RenderPieceStatus(board);

            // Assert
            Assert.IsTrue(output.Contains("Broken") || output.Contains("broken") || output.Contains("HP: 0"));
        }

        #endregion

        #region Display Formatting Tests

        [Test]
        public void Display_UsesChessNotation()
        {
            // Arrange
            var formatter = new DisplayFormatter();

            // Act
            var notation = formatter.FormatPosition(Position.FromNotation("e4"));

            // Assert
            Assert.AreEqual("e4", notation);
        }

        [Test]
        public void Display_FormatsMoveHistory()
        {
            // Arrange
            var formatter = new DisplayFormatter();
            var moves = new System.Collections.Generic.List<string>
            {
                "e2-e4",
                "e7-e5",
                "Nf3"
            };

            // Act
            var formatted = formatter.FormatMoveHistory(moves);

            // Assert
            Assert.IsNotEmpty(formatted);
            Assert.IsTrue(formatted.Contains("e2-e4"));
        }

        [Test]
        public void Display_FormatsHPBar()
        {
            // Arrange
            var formatter = new DisplayFormatter();
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            piece.TakeDamage(1); // HP: 2/3

            // Act
            var hpBar = formatter.FormatHP(piece);

            // Assert
            Assert.IsNotEmpty(hpBar);
            Assert.IsTrue(hpBar.Contains("2") && hpBar.Contains("3"));
        }

        #endregion
    }
}
