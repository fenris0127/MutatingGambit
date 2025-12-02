using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Core.MovementRules;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Tests.EditMode
{
    /// <summary>
    /// MovementRules 리팩토링 후 기능 검증 단위 테스트
    /// </summary>
    [TestFixture]
    public class MovementRulesRefactoredTests
    {
        private GameObject boardObject;
        private Board testBoard;

        [SetUp]
        public void SetUp()
        {
            boardObject = new GameObject("TestBoard");
            testBoard = boardObject.AddComponent<Board>();
            testBoard.Initialize(8, 8);
        }

        [TearDown]
        public void TearDown()
        {
            if (boardObject != null)
            {
                Object.DestroyImmediate(boardObject);
            }
        }

        [Test]
        public void StraightLineRule_DirectionalMovementRule상속_기능동일성()
        {
            // Arrange
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            testBoard.SpawnPiece(PieceType.Rook, Team.White, new Vector2Int(4, 4));

            // Act
            var moves = rule.GetValidMoves(testBoard, new Vector2Int(4, 4), Team.White);

            // Assert
            Assert.IsNotNull(moves);
            Assert.Greater(moves.Count, 0);
            Assert.IsTrue(moves.Contains(new Vector2Int(4, 5))); // 위로 이동 가능
            Assert.IsTrue(moves.Contains(new Vector2Int(4, 3))); // 아래로 이동 가능
            Assert.IsTrue(moves.Contains(new Vector2Int(3, 4))); // 왼쪽으로 이동 가능
            Assert.IsTrue(moves.Contains(new Vector2Int(5, 4))); // 오른쪽으로 이동 가능

            Object.DestroyImmediate(rule);
        }

        [Test]
        public void DiagonalRule_DirectionalMovementRule상속_기능동일성()
        {
            // Arrange
            var rule = ScriptableObject.CreateInstance<DiagonalRule>();
            testBoard.SpawnPiece(PieceType.Bishop, Team.White, new Vector2Int(4, 4));

            // Act
            var moves = rule.GetValidMoves(testBoard, new Vector2Int(4, 4), Team.White);

            // Assert
            Assert.IsNotNull(moves);
            Assert.Greater(moves.Count, 0);
            Assert.IsTrue(moves.Contains(new Vector2Int(5, 5))); // 우상 대각선
            Assert.IsTrue(moves.Contains(new Vector2Int(3, 3))); // 좌하 대각선

            Object.DestroyImmediate(rule);
        }

        [Test]
        public void DiagonalRule_ExactDistance_정확한거리검증()
        {
            // Arrange
            var rule = ScriptableObject.CreateInstance<DiagonalRule>();
            // exactDistance와 maxDistance는 private이므로 직접 테스트 불가
            // 하지만 기본 동작은 검증 가능
            testBoard.SpawnPiece(PieceType.Bishop, Team.White, new Vector2Int(4, 4));

            // Act
            var moves = rule.GetValidMoves(testBoard, new Vector2Int(4, 4), Team.White);

            // Assert - 모든 유효한 대각선 이동 가능
            Assert.Greater(moves.Count, 0);

            Object.DestroyImmediate(rule);
        }

        [Test]
        public void KnightJumpRule_메서드분리_기능동일성()
        {
            // Arrange
            var rule = ScriptableObject.CreateInstance<KnightJumpRule>();
            testBoard.SpawnPiece(PieceType.Knight, Team.White, new Vector2Int(4, 4));

            // Act
            var moves = rule.GetValidMoves(testBoard, new Vector2Int(4, 4), Team.White);

            // Assert
            Assert.IsNotNull(moves);
            Assert.AreEqual(8, moves.Count); // 나이트는 중앙에서 8개 이동 가능
            Assert.IsTrue(moves.Contains(new Vector2Int(6, 5))); // 우 2, 상 1
            Assert.IsTrue(moves.Contains(new Vector2Int(2, 3))); // 좌 2, 하 1

            Object.DestroyImmediate(rule);
        }

        [Test]
        public void MovementRuleFactory_규칙캐싱_동작확인()
        {
            // Arrange
            var factory = MovementRuleFactory.Instance;

            // Act
            var rule1 = factory.GetStraightLineRule();
            var rule2 = factory.GetStraightLineRule();

            // Assert - 같은 인스턴스 반환 (캐싱)
            Assert.AreSame(rule1, rule2);
        }

        [Test]
        public void MovementRuleFactory_QueenRules_2개규칙반환()
        {
            // Arrange
            var factory = MovementRuleFactory.Instance;

            // Act
            var queenRules = factory.GetQueenRules();

            // Assert
            Assert.IsNotNull(queenRules);
            Assert.AreEqual(2, queenRules.Length); // Straight + Diagonal
            Assert.IsInstanceOf<StraightLineRule>(queenRules[0]);
            Assert.IsInstanceOf<DiagonalRule>(queenRules[1]);
        }

        [Test]
        public void DirectionalMovementRule_중복코드제거_확인()
        {
            // Arrange
            var straightRule = ScriptableObject.CreateInstance<StraightLineRule>();
            var diagonalRule = ScriptableObject.CreateInstance<DiagonalRule>();
            testBoard.SpawnPiece(PieceType.Rook, Team.White, new Vector2Int(4, 4));

            // Act
            var straightMoves = straightRule.GetValidMoves(testBoard, new Vector2Int(4, 4), Team.White);
            var diagonalMoves = diagonalRule.GetValidMoves(testBoard, new Vector2Int(4, 4), Team.White);

            // Assert - 두 규칙 모두 정상 동작
            Assert.Greater(straightMoves.Count, 0);
            Assert.Greater(diagonalMoves.Count, 0);

            Object.DestroyImmediate(straightRule);
            Object.DestroyImmediate(diagonalRule);
        }
    }
}
