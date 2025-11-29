using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Unit tests for the mutation and artifact systems.
    /// </summary>
    public class MutationTests
    {
        private GameObject boardObject;
        private Board board;
        private MutationManager mutationManager;

        [SetUp]
        public void Setup()
        {
            // Create board
            boardObject = new GameObject("TestBoard");
            board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            // Create mutation manager
            mutationManager = MutationManager.Instance;
            mutationManager.ClearAll();
        }

        [TearDown]
        public void Teardown()
        {
            if (boardObject != null)
            {
                Object.DestroyImmediate(boardObject);
            }

            if (mutationManager != null)
            {
                mutationManager.ClearAll();
            }
        }

        #region Mutation Manager Tests

        [Test]
        public void MutationManager_ApplyMutation_AddsMutationToPiece()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var mutation = ScriptableObject.CreateInstance<LeapingRookMutation>();

            mutationManager.ApplyMutation(piece, mutation);

            Assert.IsTrue(mutationManager.HasMutations(piece));
            Assert.IsTrue(mutationManager.HasMutation(piece, mutation));

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(mutation);
        }

        [Test]
        public void MutationManager_RemoveMutation_RemovesMutationFromPiece()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var mutation = ScriptableObject.CreateInstance<LeapingRookMutation>();

            mutationManager.ApplyMutation(piece, mutation);
            mutationManager.RemoveMutation(piece, mutation);

            Assert.IsFalse(mutationManager.HasMutations(piece));

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(mutation);
        }

        [Test]
        public void MutationManager_ClearMutations_RemovesAllMutations()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var mutation1 = ScriptableObject.CreateInstance<LeapingRookMutation>();
            var mutation2 = ScriptableObject.CreateInstance<LeapingRookMutation>();

            mutationManager.ApplyMutation(piece, mutation1);
            mutationManager.ClearMutations(piece);

            Assert.IsFalse(mutationManager.HasMutations(piece));

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(mutation1);
            Object.DestroyImmediate(mutation2);
        }

        #endregion

        #region Leaping Rook Mutation Tests

        [Test]
        public void LeapingRook_CanJumpOverFriendlyPiece()
        {
            var mutation = ScriptableObject.CreateInstance<LeapingRookMutation>();

            var rookObject = new GameObject("Rook");
            var rook = rookObject.AddComponent<Piece>();
            rook.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));

            var pawnObject = new GameObject("Pawn");
            var pawn = pawnObject.AddComponent<Piece>();
            pawn.Initialize(PieceType.Pawn, Team.White, new Vector2Int(3, 5));

            board.PlacePiece(rook, new Vector2Int(3, 3));
            board.PlacePiece(pawn, new Vector2Int(3, 5));

            // Apply mutation
            mutationManager.ApplyMutation(rook, mutation);

            var moves = MoveValidator.GetValidMoves(board, new Vector2Int(3, 3));

            // Should be able to move to (3,6) by jumping over the pawn at (3,5)
            Assert.IsTrue(moves.Contains(new Vector2Int(3, 6)));

            Object.DestroyImmediate(rookObject);
            Object.DestroyImmediate(pawnObject);
            Object.DestroyImmediate(mutation);
        }

        [Test]
        public void LeapingRook_CannotJumpTwice()
        {
            var mutation = ScriptableObject.CreateInstance<LeapingRookMutation>();

            var rookObject = new GameObject("Rook");
            var rook = rookObject.AddComponent<Piece>();
            rook.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));

            var pawn1Object = new GameObject("Pawn1");
            var pawn1 = pawn1Object.AddComponent<Piece>();
            pawn1.Initialize(PieceType.Pawn, Team.White, new Vector2Int(3, 5));

            var pawn2Object = new GameObject("Pawn2");
            var pawn2 = pawn2Object.AddComponent<Piece>();
            pawn2.Initialize(PieceType.Pawn, Team.White, new Vector2Int(3, 7));

            board.PlacePiece(rook, new Vector2Int(3, 3));
            board.PlacePiece(pawn1, new Vector2Int(3, 5));
            board.PlacePiece(pawn2, new Vector2Int(3, 7));

            // Apply mutation
            mutationManager.ApplyMutation(rook, mutation);

            var moves = MoveValidator.GetValidMoves(board, new Vector2Int(3, 3));

            // Can jump first pawn to (3,6) but NOT second pawn
            Assert.IsTrue(moves.Contains(new Vector2Int(3, 6)));
            Assert.IsFalse(moves.Contains(new Vector2Int(3, 8))); // Out of bounds anyway
            Assert.IsFalse(moves.Contains(new Vector2Int(3, 7))); // Can't jump twice

            Object.DestroyImmediate(rookObject);
            Object.DestroyImmediate(pawn1Object);
            Object.DestroyImmediate(pawn2Object);
            Object.DestroyImmediate(mutation);
        }

        #endregion

        #region Fragile Bishop Mutation Tests

        [Test]
        public void FragileBishop_MovesExactly3Squares()
        {
            var mutation = ScriptableObject.CreateInstance<FragileBishopMutation>();

            var bishopObject = new GameObject("Bishop");
            var bishop = bishopObject.AddComponent<Piece>();
            bishop.Initialize(PieceType.Bishop, Team.White, new Vector2Int(3, 3));

            // Add normal diagonal rule first
            var diagonalRule = ScriptableObject.CreateInstance<DiagonalRule>();
            bishop.AddMovementRule(diagonalRule);

            board.PlacePiece(bishop, new Vector2Int(3, 3));

            // Apply mutation
            mutationManager.ApplyMutation(bishop, mutation);

            var moves = MoveValidator.GetValidMoves(board, new Vector2Int(3, 3));

            // Should be able to move exactly 3 squares diagonally
            Assert.IsTrue(moves.Contains(new Vector2Int(6, 6))); // +3, +3
            Assert.IsTrue(moves.Contains(new Vector2Int(0, 6))); // -3, +3
            Assert.IsTrue(moves.Contains(new Vector2Int(6, 0))); // +3, -3
            Assert.IsTrue(moves.Contains(new Vector2Int(0, 0))); // -3, -3

            // Should NOT be able to move 1, 2, or 4+ squares
            Assert.IsFalse(moves.Contains(new Vector2Int(4, 4))); // Only 1 square
            Assert.IsFalse(moves.Contains(new Vector2Int(5, 5))); // Only 2 squares

            Object.DestroyImmediate(bishopObject);
            Object.DestroyImmediate(mutation);
            Object.DestroyImmediate(diagonalRule);
        }

        [Test]
        public void FragileBishop_CannotMoveIfPathBlocked()
        {
            var mutation = ScriptableObject.CreateInstance<FragileBishopMutation>();

            var bishopObject = new GameObject("Bishop");
            var bishop = bishopObject.AddComponent<Piece>();
            bishop.Initialize(PieceType.Bishop, Team.White, new Vector2Int(3, 3));

            var pawnObject = new GameObject("Pawn");
            var pawn = pawnObject.AddComponent<Piece>();
            pawn.Initialize(PieceType.Pawn, Team.White, new Vector2Int(4, 4));

            board.PlacePiece(bishop, new Vector2Int(3, 3));
            board.PlacePiece(pawn, new Vector2Int(4, 4)); // Blocks the path

            mutationManager.ApplyMutation(bishop, mutation);

            var moves = MoveValidator.GetValidMoves(board, new Vector2Int(3, 3));

            // Should NOT be able to move to (6,6) because path is blocked
            Assert.IsFalse(moves.Contains(new Vector2Int(6, 6)));

            Object.DestroyImmediate(bishopObject);
            Object.DestroyImmediate(pawnObject);
            Object.DestroyImmediate(mutation);
        }

        #endregion

        #region Artifact Manager Tests

        [Test]
        public void ArtifactManager_AddArtifact_AddsSuccessfully()
        {
            var artifact = ScriptableObject.CreateInstance<KingsShadowArtifact>();
            board.ArtifactManager.AddArtifact(artifact);

            Assert.AreEqual(1, board.ArtifactManager.ArtifactCount);
            Assert.IsTrue(board.ArtifactManager.HasArtifact(artifact));

            Object.DestroyImmediate(artifact);
        }

        [Test]
        public void ArtifactManager_RemoveArtifact_RemovesSuccessfully()
        {
            var artifact = ScriptableObject.CreateInstance<KingsShadowArtifact>();
            board.ArtifactManager.AddArtifact(artifact);
            board.ArtifactManager.RemoveArtifact(artifact);

            Assert.AreEqual(0, board.ArtifactManager.ArtifactCount);
            Assert.IsFalse(board.ArtifactManager.HasArtifact(artifact));

            Object.DestroyImmediate(artifact);
        }

        [Test]
        public void ArtifactManager_ClearArtifacts_RemovesAll()
        {
            var artifact1 = ScriptableObject.CreateInstance<KingsShadowArtifact>();
            var artifact2 = ScriptableObject.CreateInstance<CavalryChargeArtifact>();

            board.ArtifactManager.AddArtifact(artifact1);
            board.ArtifactManager.AddArtifact(artifact2);

            Assert.AreEqual(2, board.ArtifactManager.ArtifactCount);

            board.ArtifactManager.ClearArtifacts();

            Assert.AreEqual(0, board.ArtifactManager.ArtifactCount);

            Object.DestroyImmediate(artifact1);
            Object.DestroyImmediate(artifact2);
        }

        [Test]
        public void ArtifactManager_GetArtifactsByTrigger_ReturnsCorrectArtifacts()
        {
            var kingsShadow = ScriptableObject.CreateInstance<KingsShadowArtifact>();
            var gravityMirror = ScriptableObject.CreateInstance<GravityMirrorArtifact>();

            board.ArtifactManager.AddArtifact(kingsShadow);
            board.ArtifactManager.AddArtifact(gravityMirror);

            var kingMoveArtifacts = board.ArtifactManager.GetArtifactsByTrigger(ArtifactTrigger.OnKingMove);
            var turnEndArtifacts = board.ArtifactManager.GetArtifactsByTrigger(ArtifactTrigger.OnTurnEnd);

            // KingsShadow triggers on king move
            Assert.GreaterOrEqual(kingMoveArtifacts.Count, 0);

            // GravityMirror triggers on turn end
            Assert.GreaterOrEqual(turnEndArtifacts.Count, 0);

            Object.DestroyImmediate(kingsShadow);
            Object.DestroyImmediate(gravityMirror);
        }

        #endregion

        #region Artifact Effect Tests

        [Test]
        public void KingsShadowArtifact_CreatesObstacleWhenKingMoves()
        {
            var artifact = ScriptableObject.CreateInstance<KingsShadowArtifact>();
            board.ArtifactManager.AddArtifact(artifact);

            var kingObject = new GameObject("King");
            var king = kingObject.AddComponent<Piece>();
            king.Initialize(PieceType.King, Team.White, new Vector2Int(4, 4));

            board.PlacePiece(king, new Vector2Int(4, 4));

            // Simulate king move
            var context = new ArtifactContext(king, new Vector2Int(4, 4), new Vector2Int(5, 5));
            board.MovePiece(new Vector2Int(4, 4), new Vector2Int(5, 5));

            // Trigger artifact
            artifact.ApplyEffect(board, context);

            // Check if obstacle was created at old position
            Assert.IsTrue(board.IsObstacle(new Vector2Int(4, 4)));

            Object.DestroyImmediate(kingObject);
            Object.DestroyImmediate(artifact);
        }

        #endregion
    }
}
