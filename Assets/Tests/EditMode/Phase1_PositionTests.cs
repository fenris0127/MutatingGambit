using NUnit.Framework;
using MutatingGambit.Core;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 1: Core Chess Logic - Position System Tests
    /// </summary>
    [TestFixture]
    public class Phase1_PositionTests
    {
        [Test]
        public void Position_CanCreateFromFileAndRank()
        {
            // Act
            var position = new Position(4, 3); // e4 in chess notation

            // Assert
            Assert.AreEqual(4, position.X);
            Assert.AreEqual(3, position.Y);
        }

        [Test]
        public void Position_CanCreateFromCoordinates()
        {
            // Act
            var position = new Position(2, 5);

            // Assert
            Assert.AreEqual(2, position.X);
            Assert.AreEqual(5, position.Y);
        }

        [Test]
        public void Position_CanCreateFromNotation()
        {
            // Act
            var position = Position.FromNotation("e4");

            // Assert
            Assert.AreEqual(4, position.X); // 'e' = 4
            Assert.AreEqual(3, position.Y); // '4' = 3 (0-indexed)
        }

        [Test]
        public void Position_ThrowsExceptionForInvalidNotation()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => Position.FromNotation("z9"));
            Assert.Throws<System.ArgumentException>(() => Position.FromNotation("a0"));
            Assert.Throws<System.ArgumentException>(() => Position.FromNotation(""));
        }

        [Test]
        public void Position_CanCompareEquality()
        {
            // Arrange
            var pos1 = new Position(3, 4);
            var pos2 = new Position(3, 4);
            var pos3 = new Position(2, 4);

            // Act & Assert
            Assert.AreEqual(pos1, pos2);
            Assert.AreNotEqual(pos1, pos3);
            Assert.IsTrue(pos1 == pos2);
            Assert.IsTrue(pos1 != pos3);
        }

        [Test]
        public void Position_ToNotation_ConvertsCorrectly()
        {
            // Arrange
            var position = new Position(0, 0); // a1

            // Act
            string notation = position.ToNotation();

            // Assert
            Assert.AreEqual("a1", notation);
        }

        [Test]
        public void Position_ToNotation_HandlesAllSquares()
        {
            // Test a few key positions
            Assert.AreEqual("a1", new Position(0, 0).ToNotation());
            Assert.AreEqual("h8", new Position(7, 7).ToNotation());
            Assert.AreEqual("e4", new Position(4, 3).ToNotation());
            Assert.AreEqual("d2", new Position(3, 1).ToNotation());
        }
    }
}
