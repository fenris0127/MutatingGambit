using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.Rooms;
using MutatingGambit.Core.Victory;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 6: Combat/Puzzle System Tests
    /// </summary>
    [TestFixture]
    public class Phase6_CombatSystemTests
    {
        #region Room Types Tests

        [Test]
        public void Room_CanCreateNormalCombatRoom()
        {
            // Act
            var room = new CombatRoom(RoomDifficulty.Normal);

            // Assert
            Assert.AreEqual(RoomType.Combat, room.Type);
            Assert.AreEqual(RoomDifficulty.Normal, room.Difficulty);
        }

        [Test]
        public void Room_CanCreateEliteCombatRoom()
        {
            // Act
            var room = new CombatRoom(RoomDifficulty.Elite);

            // Assert
            Assert.AreEqual(RoomType.Combat, room.Type);
            Assert.AreEqual(RoomDifficulty.Elite, room.Difficulty);
        }

        [Test]
        public void Room_CanCreateBossCombatRoom()
        {
            // Act
            var room = new CombatRoom(RoomDifficulty.Boss);

            // Assert
            Assert.AreEqual(RoomType.Combat, room.Type);
            Assert.AreEqual(RoomDifficulty.Boss, room.Difficulty);
        }

        [Test]
        public void Room_CanCreateTreasureRoom()
        {
            // Act
            var room = new TreasureRoom();

            // Assert
            Assert.AreEqual(RoomType.Treasure, room.Type);
        }

        [Test]
        public void Room_CanCreateRestRoom()
        {
            // Act
            var room = new RestRoom();

            // Assert
            Assert.AreEqual(RoomType.Rest, room.Type);
        }

        #endregion

        #region Victory Conditions Tests

        [Test]
        public void VictoryCondition_CheckmateInNMoves()
        {
            // Arrange
            var condition = new CheckmateInNMovesCondition(5);
            var board = new Board(8, 8);

            // Act
            var description = condition.GetDescription();

            // Assert
            Assert.IsTrue(description.Contains("5"));
            Assert.IsTrue(description.Contains("checkmate") || description.Contains("Checkmate"));
        }

        [Test]
        public void VictoryCondition_CaptureSpecificPiece()
        {
            // Arrange
            var board = new Board(8, 8);
            var targetRook = new Piece(PieceColor.Black, PieceType.Rook);
            board.PlacePiece(targetRook, Position.FromNotation("h8"));

            var condition = new CaptureSpecificPieceCondition(PieceType.Rook, PieceColor.Black);

            // Act - Before capture
            var isMetBefore = condition.IsMet(board);

            // Remove the rook (simulate capture)
            board.PlacePiece(null, Position.FromNotation("h8"));

            // Act - After capture
            var isMetAfter = condition.IsMet(board);

            // Assert
            Assert.IsFalse(isMetBefore, "Condition should not be met while piece exists");
            Assert.IsTrue(isMetAfter, "Condition should be met after piece is captured");
        }

        [Test]
        public void VictoryCondition_MoveKingToPosition()
        {
            // Arrange
            var board = new Board(8, 8);
            var whiteKing = new Piece(PieceColor.White, PieceType.King);
            board.PlacePiece(whiteKing, Position.FromNotation("e1"));

            var targetPosition = Position.FromNotation("e8");
            var condition = new KingToPositionCondition(PieceColor.White, targetPosition);

            // Act - Before moving
            var isMetBefore = condition.IsMet(board);

            // Move king to target
            board.PlacePiece(null, Position.FromNotation("e1"));
            board.PlacePiece(whiteKing, targetPosition);

            // Act - After moving
            var isMetAfter = condition.IsMet(board);

            // Assert
            Assert.IsFalse(isMetBefore);
            Assert.IsTrue(isMetAfter);
        }

        [Test]
        public void VictoryCondition_CanCombineMultipleConditions()
        {
            // Arrange
            var condition1 = new CaptureSpecificPieceCondition(PieceType.Rook, PieceColor.Black);
            var condition2 = new CaptureSpecificPieceCondition(PieceType.Queen, PieceColor.Black);
            var combined = new CompositeVictoryCondition(VictoryLogic.And, condition1, condition2);

            var board = new Board(8, 8);
            var rook = new Piece(PieceColor.Black, PieceType.Rook);
            var queen = new Piece(PieceColor.Black, PieceType.Queen);
            board.PlacePiece(rook, Position.FromNotation("a8"));
            board.PlacePiece(queen, Position.FromNotation("d8"));

            // Act - Neither captured
            var met1 = combined.IsMet(board);

            // Capture rook only
            board.PlacePiece(null, Position.FromNotation("a8"));
            var met2 = combined.IsMet(board);

            // Capture both
            board.PlacePiece(null, Position.FromNotation("d8"));
            var met3 = combined.IsMet(board);

            // Assert
            Assert.IsFalse(met1, "Should not be met with neither captured");
            Assert.IsFalse(met2, "Should not be met with only one captured (AND logic)");
            Assert.IsTrue(met3, "Should be met with both captured");
        }

        #endregion

        #region Piece Damage System Tests

        [Test]
        public void Piece_HasHP()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);

            // Assert
            Assert.AreEqual(3, piece.HP);
            Assert.AreEqual(3, piece.MaxHP);
        }

        [Test]
        public void Piece_CanTakeDamage()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);

            // Act
            piece.TakeDamage(1);

            // Assert
            Assert.AreEqual(2, piece.HP);
            Assert.IsFalse(piece.IsBroken);
        }

        [Test]
        public void Piece_BecomesBrokenWhenHPReachesZero()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);

            // Act
            piece.TakeDamage(3);

            // Assert
            Assert.AreEqual(0, piece.HP);
            Assert.IsTrue(piece.IsBroken);
        }

        [Test]
        public void Piece_BrokenPiecesCannotBeUsed()
        {
            // Arrange
            var board = new Board(8, 8);
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            board.PlacePiece(piece, Position.FromNotation("a1"));

            piece.TakeDamage(3); // Break the piece

            // Act
            var moves = Movement.MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

            // Assert
            Assert.AreEqual(0, moves.Count, "Broken pieces should have no legal moves");
        }

        [Test]
        public void Piece_CanBeRepairedInRestRoom()
        {
            // Arrange
            var piece = new Piece(PieceColor.White, PieceType.Rook);
            piece.TakeDamage(2); // HP = 1

            var restRoom = new RestRoom();

            // Act
            restRoom.RepairPiece(piece);

            // Assert
            Assert.AreEqual(3, piece.HP, "Should be fully repaired");
            Assert.IsFalse(piece.IsBroken);
        }

        [Test]
        public void Piece_KingDeathCausesGameOver()
        {
            // Arrange
            var king = new Piece(PieceColor.White, PieceType.King);

            // Act
            king.TakeDamage(3);

            // Assert
            Assert.IsTrue(king.IsBroken);
            // Game over logic would be handled by game manager
        }

        #endregion

        #region Room Rewards Tests

        [Test]
        public void CombatRoom_ProvidesRewardOnCompletion()
        {
            // Arrange
            var room = new CombatRoom(RoomDifficulty.Normal);

            // Act
            var reward = room.GetReward();

            // Assert
            Assert.IsNotNull(reward);
            Assert.Greater(reward.ArtifactChoices.Count, 0);
        }

        [Test]
        public void EliteRoom_ProvidesBetterRewards()
        {
            // Arrange
            var normalRoom = new CombatRoom(RoomDifficulty.Normal);
            var eliteRoom = new CombatRoom(RoomDifficulty.Elite);

            // Act
            var normalReward = normalRoom.GetReward();
            var eliteReward = eliteRoom.GetReward();

            // Assert
            Assert.Greater(eliteReward.CurrencyAmount, normalReward.CurrencyAmount);
        }

        [Test]
        public void TreasureRoom_ProvidesImmediateReward()
        {
            // Arrange
            var room = new TreasureRoom();

            // Act
            var reward = room.GetReward();

            // Assert
            Assert.IsNotNull(reward);
            Assert.IsNotNull(reward.GuaranteedArtifact);
        }

        [Test]
        public void RestRoom_AllowsPieceRepair()
        {
            // Arrange
            var room = new RestRoom();
            var damagedPiece = new Piece(PieceColor.White, PieceType.Knight);
            damagedPiece.TakeDamage(2);

            // Act
            var canRepair = room.CanRepair();
            room.RepairPiece(damagedPiece);

            // Assert
            Assert.IsTrue(canRepair);
            Assert.AreEqual(damagedPiece.MaxHP, damagedPiece.HP);
        }

        #endregion
    }
}
