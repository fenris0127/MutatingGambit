using NUnit.Framework;
using UnityEngine;
using MutatingGambit.AI;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Tests.EditMode
{
    /// <summary>
    /// AI 리팩토링 후 기능 검증 단위 테스트
    /// </summary>
    [TestFixture]
    public class AIRefactoredTests
    {
        private GameObject aiObject;
        private ChessAI chessAI;
        private AIConfig testConfig;

        [SetUp]
        public void SetUp()
        {
            // AI Config 생성
            testConfig = ScriptableObject.CreateInstance<AIConfig>();
            testConfig.SearchDepth = 3;
            testConfig.MaterialWeight = 1.0f;
            testConfig.PositionalWeight = 0.3f;
            testConfig.KingSafetyWeight = 0.5f;
            testConfig.MobilityWeight = 0.2f;
            testConfig.RandomnessFactor = 0f;

            // ChessAI 생성
            aiObject = new GameObject("TestAI");
            chessAI = aiObject.AddComponent<ChessAI>();
        }

        [TearDown]
        public void TearDown()
        {
            if (aiObject != null)
            {
                Object.DestroyImmediate(aiObject);
            }
            if (testConfig != null)
            {
                Object.DestroyImmediate(testConfig);
            }
        }

        [Test]
        public void ChessAI_Initialize_성공()
        {
            // Arrange & Act
            chessAI.Initialize(testConfig, Team.White);

            // Assert
            Assert.AreEqual(Team.White, chessAI.AITeam);
            Assert.AreEqual(testConfig, chessAI.Config);
        }

        [Test]
        public void ChessAI_UpdateConfig_중복제거_확인()
        {
            // Arrange
            chessAI.Initialize(testConfig, Team.White);
            var newConfig = ScriptableObject.CreateInstance<AIConfig>();
            newConfig.SearchDepth = 5;

            // Act
            chessAI.UpdateConfig(newConfig);

            // Assert
            Assert.AreEqual(newConfig, chessAI.Config);
            Assert.AreEqual(5, chessAI.Config.SearchDepth);

            Object.DestroyImmediate(newConfig);
        }

        [Test]
        public void StateEvaluator_중복코드제거_확인()
        {
            // Arrange
            var evaluator = new StateEvaluator(testConfig, Team.White);
            var boardGo = new GameObject("Board");
            var board = boardGo.AddComponent<Board>();
            board.Initialize(8, 8);

            // Act & Assert
            Assert.DoesNotThrow(() => evaluator.EvaluateBoard(board));
            
            var state = board.CloneAsState();
            Assert.DoesNotThrow(() => evaluator.EvaluateBoardState(state));

            Object.DestroyImmediate(boardGo);
        }

        [Test]
        public void StateEvaluator_무작위성추가_일관성()
        {
            // Arrange
            var fixedSeed = new System.Random(42);
            var evaluator = new StateEvaluator(testConfig, Team.White, fixedSeed);
            var boardGo = new GameObject("Board");
            var board = boardGo.AddComponent<Board>();
            board.Initialize(8, 8);

            // Act
            float score1 = evaluator.EvaluateBoard(board);
            float score2 = evaluator.EvaluateBoard(board);

            // Assert - 무작위성이 0이므로 같은 점수여야 함
            Assert.AreEqual(score1, score2, 0.001f);

            Object.DestroyImmediate(boardGo);
        }

        [Test]
        public void MinimaxSearch_메서드분리_기능동일성()
        {
            // Arrange
            var evaluator = new StateEvaluator(testConfig, Team.White);
            var random = new System.Random(42);
            var minimax = new MutatingGambit.AI.Search.MinimaxSearch(testConfig, Team.White, evaluator, random);
            
            var boardGo = new GameObject("Board");
            var board = boardGo.AddComponent<Board>();
            board.Initialize(8, 8);
            
            // 기본 기물 배치
            board.SpawnPiece(PieceType.King, Team.White, new Vector2Int(4, 0));
            board.SpawnPiece(PieceType.Pawn, Team.White, new Vector2Int(4, 1));
            board.SpawnPiece(PieceType.King, Team.Black, new Vector2Int(4, 7));

            // Act
            var move = minimax.DepthLimitedSearch(board, 2);

            // Assert
            Assert.IsNotNull(move);
            Assert.Greater(minimax.NodesEvaluated, 0);

            Object.DestroyImmediate(boardGo);
        }

        [Test]
        public void MinimaxSearch_ResetCounters_동작확인()
        {
            // Arrange
            var evaluator = new StateEvaluator(testConfig, Team.White);
            var random = new System.Random();
            var minimax = new MutatingGambit.AI.Search.MinimaxSearch(testConfig, Team.White, evaluator, random);

            // Act
            minimax.ResetCounters();

            // Assert
            Assert.AreEqual(0, minimax.NodesEvaluated);
        }
    }
}
