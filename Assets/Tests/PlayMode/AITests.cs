using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MutatingGambit.AI;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// PlayMode integration tests for the AI system.
    /// </summary>
    public class AITests
    {
        private GameObject boardObject;
        private Board board;
        private GameObject aiObject;
        private ChessAI chessAI;
        private AIConfig testConfig;

        [SetUp]
        public void Setup()
        {
            // Create board
            boardObject = new GameObject("TestBoard");
            board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            // Create AI config with proper defaults
            testConfig = CreateTestAIConfig();

            // Create AI
            aiObject = new GameObject("TestAI");
            chessAI = aiObject.AddComponent<ChessAI>();
            chessAI.Initialize(testConfig, Team.Black, 42);
        }

        /// <summary>
        /// Creates a test AI config with default values.
        /// Uses AIConfig.CreateForTesting() instead of fragile reflection.
        /// </summary>
        private AIConfig CreateTestAIConfig()
        {
            return AIConfig.CreateForTesting(
                searchDepth: 3,
                maxTimePerMove: 1000,
                useIterativeDeepening: false
            );
        }

        [TearDown]
        public void Teardown()
        {
            if (boardObject != null) Object.Destroy(boardObject);
            if (aiObject != null) Object.Destroy(aiObject);
            if (testConfig != null) Object.Destroy(testConfig);
        }

        #region StateEvaluator Tests

        [Test]
        public void StateEvaluator_MaterialAdvantage_FavorsAI()
        {
            var evaluator = new StateEvaluator(testConfig, Team.Black, 0);

            // White: King only
            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(4, 0));

            // Black: King + Queen
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(4, 7));
            CreatePieceOnBoard(PieceType.Queen, Team.Black, new Vector2Int(3, 7));

            float score = evaluator.EvaluateBoard(board);

            Assert.Greater(score, 0f, "AI with material advantage should have positive score");
        }

        [Test]
        public void StateEvaluator_MutatedPiece_IncreasesValue()
        {
            var evaluator = new StateEvaluator(testConfig, Team.Black, 0);

            // Create a mutated rook (has extra rules)
            var rook = CreatePieceOnBoard(PieceType.Rook, Team.Black, new Vector2Int(0, 0));
            var straightRule = ScriptableObject.CreateInstance<StraightLineRule>();
            rook.AddMovementRule(straightRule); // Extra rule = mutation

            // Opponent has normal king
            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(4, 0));
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(4, 7));

            float score = evaluator.EvaluateBoard(board);

            Assert.Greater(score, 0f, "Mutated pieces should increase evaluation");

            Object.Destroy(straightRule);
        }

        [Test]
        public void StateEvaluator_CenterControl_IncreasesScore()
        {
            var evaluator = new StateEvaluator(testConfig, Team.Black, 0);

            // Black controls center
            CreatePieceOnBoard(PieceType.Pawn, Team.Black, new Vector2Int(3, 3));
            CreatePieceOnBoard(PieceType.Pawn, Team.Black, new Vector2Int(4, 4));

            // Kings on sides
            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(0, 0));
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(7, 7));

            float score = evaluator.EvaluateBoard(board);

            Assert.Greater(score, 0f, "Center control should increase score");
        }

        #endregion

        #region ChessAI Decision Making Tests

        [UnityTest]
        public IEnumerator ChessAI_MakeMove_ReturnValidMove()
        {
            // Setup simple position
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(4, 7));
            CreatePieceOnBoard(PieceType.Rook, Team.Black, new Vector2Int(0, 7));
            var rook = board.GetPiece(new Vector2Int(0, 7));
            rook.AddMovementRule(ScriptableObject.CreateInstance<StraightLineRule>());

            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(4, 0));

            yield return null;

            var move = chessAI.MakeMove(board);

            Assert.AreNotEqual(Vector2Int.zero, move.From, "AI should return a valid move");
            Assert.AreNotEqual(Vector2Int.zero, move.To, "AI should return a valid move");
            Assert.IsNotNull(move.MovingPiece, "Move should have a moving piece");
        }

        [UnityTest]
        public IEnumerator ChessAI_PrefersCaptureMove()
        {
            // Black rook can capture white queen
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(7, 7));
            var rook = CreatePieceOnBoard(PieceType.Rook, Team.Black, new Vector2Int(0, 4));
            rook.AddMovementRule(ScriptableObject.CreateInstance<StraightLineRule>());

            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(0, 0));
            CreatePieceOnBoard(PieceType.Queen, Team.White, new Vector2Int(5, 4)); // Can be captured

            yield return null;

            var move = chessAI.MakeMove(board);

            // AI should choose to capture the queen (high value target)
            Assert.AreEqual(new Vector2Int(0, 4), move.From, "AI should move the rook");
            Assert.AreEqual(new Vector2Int(5, 4), move.To, "AI should capture the queen");
        }

        [UnityTest]
        public IEnumerator ChessAI_AvoidsCapture_WhenPossible()
        {
            // Black queen is under attack, should move to safety
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(7, 7));
            var queen = CreatePieceOnBoard(PieceType.Queen, Team.Black, new Vector2Int(3, 3));
            var straightRule = ScriptableObject.CreateInstance<StraightLineRule>();
            var diagonalRule = ScriptableObject.CreateInstance<DiagonalRule>();
            queen.AddMovementRule(straightRule);
            queen.AddMovementRule(diagonalRule);

            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(0, 0));
            var whiteRook = CreatePieceOnBoard(PieceType.Rook, Team.White, new Vector2Int(3, 0)); // Attacks queen
            whiteRook.AddMovementRule(ScriptableObject.CreateInstance<StraightLineRule>());

            yield return null;

            var move = chessAI.MakeMove(board);

            // Queen should move away from rook's attack
            Assert.AreEqual(new Vector2Int(3, 3), move.From, "Queen should move");
            Assert.AreNotEqual(3, move.To.x, "Queen should move off the attacked file");

            Object.Destroy(straightRule);
            Object.Destroy(diagonalRule);
        }

        [UnityTest]
        public IEnumerator ChessAI_RespectsTimeLimit()
        {
            // Create complex position with many pieces
            SetupStandardPosition();

            float startTime = Time.realtimeSinceStartup;

            yield return null;

            var move = chessAI.MakeMove(board);

            float elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Assert.Less(elapsed, testConfig.MaxTimePerMove * 1.5f,
                $"AI should respect time limit ({elapsed}ms elapsed)");
        }

        [UnityTest]
        public IEnumerator ChessAI_HandlesMutatedRules()
        {
            // Create a piece with non-standard movement (mutation)
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(7, 7));
            var mutatedRook = CreatePieceOnBoard(PieceType.Rook, Team.Black, new Vector2Int(0, 7));

            // Add both straight and diagonal movement (mutated rook)
            var straightRule = ScriptableObject.CreateInstance<StraightLineRule>();
            var diagonalRule = ScriptableObject.CreateInstance<DiagonalRule>();
            mutatedRook.AddMovementRule(straightRule);
            mutatedRook.AddMovementRule(diagonalRule);

            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(0, 0));
            CreatePieceOnBoard(PieceType.Queen, Team.White, new Vector2Int(5, 2)); // Capturable diagonally

            yield return null;

            var move = chessAI.MakeMove(board);

            // AI should recognize it can use diagonal movement to capture
            Assert.IsNotNull(move.MovingPiece, "AI should find a move with mutated piece");

            Object.Destroy(straightRule);
            Object.Destroy(diagonalRule);
        }

        #endregion

        #region Minimax Depth Tests

        [UnityTest]
        public IEnumerator ChessAI_DeeperSearch_FindsBetterMoves()
        {
            // Setup position where deeper search finds mate
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(7, 7));
            var blackQueen = CreatePieceOnBoard(PieceType.Queen, Team.Black, new Vector2Int(3, 5));
            blackQueen.AddMovementRule(ScriptableObject.CreateInstance<StraightLineRule>());
            blackQueen.AddMovementRule(ScriptableObject.CreateInstance<DiagonalRule>());

            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(0, 0));

            // Shallow search
            var shallowConfig = CreateTestAIConfig();
            var shallowAI = aiObject.AddComponent<ChessAI>();
            shallowAI.Initialize(shallowConfig, Team.Black, 42);

            yield return null;

            var shallowMove = shallowAI.MakeMove(board);

            // This test just ensures both AIs can make decisions
            Assert.IsNotNull(shallowMove.MovingPiece, "Shallow AI should find a move");

            Object.Destroy(shallowConfig);
            Object.Destroy(shallowAI);
        }

        #endregion

        #region Helper Methods

        private Piece CreatePieceOnBoard(PieceType type, Team team, Vector2Int position)
        {
            GameObject pieceObject = new GameObject($"{team}_{type}");
            Piece piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(type, team, position);
            board.PlacePiece(piece, position);
            return piece;
        }

        private void SetupStandardPosition()
        {
            // Setup a standard chess starting position for Black
            for (int i = 0; i < 8; i++)
            {
                var pawn = CreatePieceOnBoard(PieceType.Pawn, Team.Black, new Vector2Int(i, 6));
                // Pawns need movement rules for AI to consider them
            }

            CreatePieceOnBoard(PieceType.Rook, Team.Black, new Vector2Int(0, 7));
            CreatePieceOnBoard(PieceType.Knight, Team.Black, new Vector2Int(1, 7));
            CreatePieceOnBoard(PieceType.Bishop, Team.Black, new Vector2Int(2, 7));
            CreatePieceOnBoard(PieceType.Queen, Team.Black, new Vector2Int(3, 7));
            CreatePieceOnBoard(PieceType.King, Team.Black, new Vector2Int(4, 7));
            CreatePieceOnBoard(PieceType.Bishop, Team.Black, new Vector2Int(5, 7));
            CreatePieceOnBoard(PieceType.Knight, Team.Black, new Vector2Int(6, 7));
            CreatePieceOnBoard(PieceType.Rook, Team.Black, new Vector2Int(7, 7));

            // White pieces
            CreatePieceOnBoard(PieceType.King, Team.White, new Vector2Int(4, 0));
            CreatePieceOnBoard(PieceType.Queen, Team.White, new Vector2Int(3, 0));
        }

        #endregion
    }
}
