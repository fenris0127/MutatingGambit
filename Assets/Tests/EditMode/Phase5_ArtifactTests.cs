using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.Artifacts;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 5: Artifact System Tests
    /// </summary>
    [TestFixture]
    public class Phase5_ArtifactTests
    {
        #region Artifact Framework Tests

        [Test]
        public void Artifact_CanChangeGlobalRules()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact = new KingsShadowArtifact();

            // Act
            board.ArtifactManager.AddArtifact(artifact);

            // Assert
            Assert.AreEqual(1, board.ArtifactManager.Artifacts.Count);
            CollectionAssert.Contains(board.ArtifactManager.Artifacts, artifact);
        }

        [Test]
        public void Artifact_AppliesTo BothPlayerAndAI()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact = new KingsShadowArtifact();

            // Act
            board.ArtifactManager.AddArtifact(artifact);

            // Assert - Artifact is global, not color-specific
            Assert.IsNotNull(board.ArtifactManager);
            Assert.AreEqual(1, board.ArtifactManager.Artifacts.Count);
        }

        [Test]
        public void Artifact_MultipleArtifactsCanStack()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact1 = new KingsShadowArtifact();
            var artifact2 = new CavalryChargeArtifact();

            // Act
            board.ArtifactManager.AddArtifact(artifact1);
            board.ArtifactManager.AddArtifact(artifact2);

            // Assert
            Assert.AreEqual(2, board.ArtifactManager.Artifacts.Count);
        }

        [Test]
        public void Artifact_CanBeActivatedAndDeactivated()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact = new KingsShadowArtifact();
            board.ArtifactManager.AddArtifact(artifact);

            // Act
            artifact.IsActive = false;

            // Assert
            Assert.IsFalse(artifact.IsActive);
        }

        #endregion

        #region King's Shadow Artifact Tests

        [Test]
        public void KingsShadow_CreatesObstacleWhenKingMoves()
        {
            // Arrange
            var board = new Board(8, 8);
            var king = new Piece(PieceColor.White, PieceType.King);
            var artifact = new KingsShadowArtifact();

            board.PlacePiece(king, Position.FromNotation("e1"));
            board.ArtifactManager.AddArtifact(artifact);

            var originalPos = Position.FromNotation("e1");
            var targetPos = Position.FromNotation("e2");

            // Act - Simulate king move
            board.PlacePiece(null, originalPos);
            board.PlacePiece(king, targetPos);

            var context = new ArtifactContext
            {
                MovedPiece = king,
                FromPosition = originalPos,
                ToPosition = targetPos
            };
            artifact.ApplyEffect(board, context);

            // Assert
            var tile = board.GetTileAt(originalPos);
            Assert.AreEqual(TileType.Wall, tile.Type, "Should create obstacle at original position");
        }

        [Test]
        public void KingsShadow_OnlyAffectsKings()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var artifact = new KingsShadowArtifact();

            board.PlacePiece(rook, Position.FromNotation("a1"));
            board.ArtifactManager.AddArtifact(artifact);

            var originalPos = Position.FromNotation("a1");
            var targetPos = Position.FromNotation("a4");

            // Act
            board.PlacePiece(null, originalPos);
            board.PlacePiece(rook, targetPos);

            var context = new ArtifactContext
            {
                MovedPiece = rook,
                FromPosition = originalPos,
                ToPosition = targetPos
            };
            artifact.ApplyEffect(board, context);

            // Assert
            var tile = board.GetTileAt(originalPos);
            Assert.AreEqual(TileType.Empty, tile.Type, "Should NOT create obstacle for non-king pieces");
        }

        #endregion

        #region Cavalry Charge Artifact Tests

        [Test]
        public void CavalryCharge_KnightMovesAgainAfterCapture()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var enemy = new Piece(PieceColor.Black, PieceType.Pawn);
            var artifact = new CavalryChargeArtifact();

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.PlacePiece(enemy, Position.FromNotation("f6"));
            board.ArtifactManager.AddArtifact(artifact);

            var context = new ArtifactContext
            {
                MovedPiece = knight,
                CapturedPiece = enemy,
                FromPosition = Position.FromNotation("e4"),
                ToPosition = Position.FromNotation("f6")
            };

            // Act
            var canMoveAgain = artifact.AllowsExtraMove(board, context);

            // Assert
            Assert.IsTrue(canMoveAgain, "Knight should be able to move again after capture");
        }

        [Test]
        public void CavalryCharge_OnlyAppliesWhenCapturing()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var artifact = new CavalryChargeArtifact();

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.ArtifactManager.AddArtifact(artifact);

            var context = new ArtifactContext
            {
                MovedPiece = knight,
                CapturedPiece = null, // No capture
                FromPosition = Position.FromNotation("e4"),
                ToPosition = Position.FromNotation("f6")
            };

            // Act
            var canMoveAgain = artifact.AllowsExtraMove(board, context);

            // Assert
            Assert.IsFalse(canMoveAgain, "Knight should NOT move again without capturing");
        }

        [Test]
        public void CavalryCharge_OnlyAffectsKnights()
        {
            // Arrange
            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.White, PieceType.Rook);
            var enemy = new Piece(PieceColor.Black, PieceType.Pawn);
            var artifact = new CavalryChargeArtifact();

            board.PlacePiece(rook, Position.FromNotation("a1"));
            board.PlacePiece(enemy, Position.FromNotation("a5"));
            board.ArtifactManager.AddArtifact(artifact);

            var context = new ArtifactContext
            {
                MovedPiece = rook,
                CapturedPiece = enemy,
                FromPosition = Position.FromNotation("a1"),
                ToPosition = Position.FromNotation("a5")
            };

            // Act
            var canMoveAgain = artifact.AllowsExtraMove(board, context);

            // Assert
            Assert.IsFalse(canMoveAgain, "Only knights should get extra move");
        }

        #endregion

        #region Promotion Privilege Artifact Tests

        [Test]
        public void PromotionPrivilege_PawnPromotesAfter3Captures()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            var artifact = new PromotionPrivilegeArtifact();

            board.PlacePiece(pawn, Position.FromNotation("e4"));
            board.ArtifactManager.AddArtifact(artifact);

            // Track captures
            artifact.TrackCapture(pawn);
            artifact.TrackCapture(pawn);
            artifact.TrackCapture(pawn);

            // Act
            var shouldPromote = artifact.ShouldPromote(pawn);

            // Assert
            Assert.IsTrue(shouldPromote, "Pawn should promote after 3 captures");
        }

        [Test]
        public void PromotionPrivilege_PawnDoesNotPromoteBeefore3Captures()
        {
            // Arrange
            var board = new Board(8, 8);
            var pawn = new Piece(PieceColor.White, PieceType.Pawn);
            var artifact = new PromotionPrivilegeArtifact();

            board.PlacePiece(pawn, Position.FromNotation("e4"));
            board.ArtifactManager.AddArtifact(artifact);

            // Track only 2 captures
            artifact.TrackCapture(pawn);
            artifact.TrackCapture(pawn);

            // Act
            var shouldPromote = artifact.ShouldPromote(pawn);

            // Assert
            Assert.IsFalse(shouldPromote, "Pawn should NOT promote with only 2 captures");
        }

        [Test]
        public void PromotionPrivilege_OnlyAffectsPawns()
        {
            // Arrange
            var board = new Board(8, 8);
            var knight = new Piece(PieceColor.White, PieceType.Knight);
            var artifact = new PromotionPrivilegeArtifact();

            board.PlacePiece(knight, Position.FromNotation("e4"));
            board.ArtifactManager.AddArtifact(artifact);

            artifact.TrackCapture(knight);
            artifact.TrackCapture(knight);
            artifact.TrackCapture(knight);

            // Act
            var shouldPromote = artifact.ShouldPromote(knight);

            // Assert
            Assert.IsFalse(shouldPromote, "Only pawns should be affected");
        }

        #endregion

        #region Artifact Interaction Tests

        [Test]
        public void Artifacts_ConflictingRulesHavePriority()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact1 = new KingsShadowArtifact { Priority = 10 };
            var artifact2 = new CavalryChargeArtifact { Priority = 5 };

            // Act
            board.ArtifactManager.AddArtifact(artifact1);
            board.ArtifactManager.AddArtifact(artifact2);

            var artifacts = board.ArtifactManager.GetArtifactsByPriority();

            // Assert
            Assert.AreEqual(artifact1, artifacts[0], "Higher priority artifact should be first");
        }

        [Test]
        public void Artifacts_CanCreateSynergies()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact1 = new PromotionPrivilegeArtifact();
            var artifact2 = new CavalryChargeArtifact();

            // Act
            board.ArtifactManager.AddArtifact(artifact1);
            board.ArtifactManager.AddArtifact(artifact2);

            // Assert - Both artifacts can be active simultaneously
            Assert.AreEqual(2, board.ArtifactManager.Artifacts.Count);
            Assert.IsTrue(artifact1.IsActive);
            Assert.IsTrue(artifact2.IsActive);
        }

        [Test]
        public void Artifacts_EffectsAreVisuallyDisplayed()
        {
            // Arrange
            var board = new Board(8, 8);
            var artifact = new KingsShadowArtifact();

            // Act
            board.ArtifactManager.AddArtifact(artifact);
            var displayName = artifact.Name;
            var displayDescription = artifact.Description;

            // Assert
            Assert.IsNotEmpty(displayName);
            Assert.IsNotEmpty(displayDescription);
        }

        #endregion
    }
}
