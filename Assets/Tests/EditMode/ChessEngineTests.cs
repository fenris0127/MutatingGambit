using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;
using System.Collections.Generic;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Unit tests for the chess engine core functionality.
    /// </summary>
    public class ChessEngineTests
    {
        private GameObject boardObject;
        private Board board;

        [SetUp]
        public void Setup()
        {
            // Create a board for testing
            boardObject = new GameObject("TestBoard");
            board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);
        }

        [TearDown]
        public void Teardown()
        {
            if (boardObject != null)
            {
                Object.DestroyImmediate(boardObject);
            }
        }

        #region Board Position Extension Tests

        [Test]
        public void BoardPosition_IsWithinBounds_ReturnsTrue_ForValidPosition()
        {
            var position = new Vector2Int(3, 4);
            Assert.IsTrue(position.IsWithinBounds(8, 8));
        }

        [Test]
        public void BoardPosition_IsWithinBounds_ReturnsFalse_ForInvalidPosition()
        {
            var position = new Vector2Int(8, 8);
            Assert.IsFalse(position.IsWithinBounds(8, 8));
        }

        [Test]
        public void BoardPosition_FromNotation_ConvertsCorrectly()
        {
            var position = BoardPosition.FromNotation("e4");
            Assert.AreEqual(4, position.x); // 'e' is 5th file (index 4)
            Assert.AreEqual(3, position.y); // '4' is 4th rank (index 3)
        }

        [Test]
        public void BoardPosition_ToNotation_ConvertsCorrectly()
        {
            var position = new Vector2Int(4, 3);
            string notation = position.ToNotation();
            Assert.AreEqual("e4", notation);
        }

        [Test]
        public void BoardPosition_ManhattanDistance_CalculatesCorrectly()
        {
            var from = new Vector2Int(0, 0);
            var to = new Vector2Int(3, 4);
            int distance = from.ManhattanDistance(to);
            Assert.AreEqual(7, distance);
        }

        [Test]
        public void BoardPosition_IsDiagonal_ReturnsTrue_ForDiagonalPositions()
        {
            var from = new Vector2Int(2, 2);
            var to = new Vector2Int(5, 5);
            Assert.IsTrue(from.IsDiagonal(to));
        }

        [Test]
        public void BoardPosition_IsStraightLine_ReturnsTrue_ForStraightLine()
        {
            var from = new Vector2Int(2, 2);
            var to = new Vector2Int(2, 6);
            Assert.IsTrue(from.IsStraightLine(to));
        }

        #endregion

        #region Board Tests

        [Test]
        public void Board_Initialize_CreatesCorrectSize()
        {
            Assert.AreEqual(8, board.Width);
            Assert.AreEqual(8, board.Height);
        }

        [Test]
        public void Board_PlacePiece_PieceIsPlacedCorrectly()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var position = new Vector2Int(3, 3);
            board.PlacePiece(piece, position);

            var retrievedPiece = board.GetPiece(position);
            Assert.IsNotNull(retrievedPiece);
            Assert.AreEqual(piece, retrievedPiece);
            Assert.AreEqual(position, piece.Position);

            Object.DestroyImmediate(pieceObject);
        }

        [Test]
        public void Board_RemovePiece_PieceIsRemoved()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var position = new Vector2Int(3, 3);
            board.PlacePiece(piece, position);
            board.RemovePiece(position);

            var retrievedPiece = board.GetPiece(position);
            Assert.IsNull(retrievedPiece);
        }

        [Test]
        public void Board_MovePiece_PieceMovesCorrectly()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var from = new Vector2Int(3, 3);
            var to = new Vector2Int(5, 5);

            board.PlacePiece(piece, from);
            board.MovePiece(from, to);

            Assert.IsNull(board.GetPiece(from));
            Assert.AreEqual(piece, board.GetPiece(to));
            Assert.AreEqual(to, piece.Position);

            Object.DestroyImmediate(pieceObject);
        }

        [Test]
        public void Board_SetObstacle_CreatesObstacle()
        {
            var position = new Vector2Int(4, 4);
            board.SetObstacle(position, true);

            Assert.IsTrue(board.IsObstacle(position));
        }

        #endregion

        #region Piece Tests

        [Test]
        public void Piece_Initialize_SetsPropertiesCorrectly()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();

            var position = new Vector2Int(2, 3);
            piece.Initialize(PieceType.Knight, Team.Black, position);

            Assert.AreEqual(PieceType.Knight, piece.Type);
            Assert.AreEqual(Team.Black, piece.Team);
            Assert.AreEqual(position, piece.Position);

            Object.DestroyImmediate(pieceObject);
        }

        [Test]
        public void Piece_AddMovementRule_AddsRuleToList()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            piece.AddMovementRule(rule);

            Assert.Contains(rule, piece.MovementRules);
            Assert.AreEqual(1, piece.MovementRules.Count);

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void Piece_RemoveMovementRule_RemovesRuleFromList()
        {
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            piece.AddMovementRule(rule);
            piece.RemoveMovementRule(rule);

            Assert.IsFalse(piece.MovementRules.Contains(rule));
            Assert.AreEqual(0, piece.MovementRules.Count);

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        #endregion

        #region Movement Rule Tests

        [Test]
        public void StraightLineRule_GeneratesValidMoves_ForRook()
        {
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            var moves = rule.GetValidMoves(board, new Vector2Int(3, 3), Team.White);

            // Rook at (3,3) on empty board should have 14 moves (7 horizontal + 7 vertical)
            Assert.AreEqual(14, moves.Count);

            // Check some specific moves
            Assert.Contains(new Vector2Int(3, 0), moves); // Down to edge
            Assert.Contains(new Vector2Int(3, 7), moves); // Up to edge
            Assert.Contains(new Vector2Int(0, 3), moves); // Left to edge
            Assert.Contains(new Vector2Int(7, 3), moves); // Right to edge

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void StraightLineRule_BlockedByFriendlyPiece()
        {
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();

            var rookObject = new GameObject("Rook");
            var rook = rookObject.AddComponent<Piece>();
            rook.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));
            rook.AddMovementRule(rule);

            var pawnObject = new GameObject("Pawn");
            var pawn = pawnObject.AddComponent<Piece>();
            pawn.Initialize(PieceType.Pawn, Team.White, new Vector2Int(3, 5));

            board.PlacePiece(rook, new Vector2Int(3, 3));
            board.PlacePiece(pawn, new Vector2Int(3, 5));

            var moves = rule.GetValidMoves(board, new Vector2Int(3, 3), Team.White);

            // Rook should NOT be able to move through or to friendly pawn position
            Assert.IsFalse(moves.Contains(new Vector2Int(3, 5)));
            Assert.IsFalse(moves.Contains(new Vector2Int(3, 6)));
            Assert.IsFalse(moves.Contains(new Vector2Int(3, 7)));

            // But should be able to move to (3,4)
            Assert.IsTrue(moves.Contains(new Vector2Int(3, 4)));

            Object.DestroyImmediate(rookObject);
            Object.DestroyImmediate(pawnObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void DiagonalRule_GeneratesValidMoves_ForBishop()
        {
            var rule = ScriptableObject.CreateInstance<DiagonalRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Bishop, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            var moves = rule.GetValidMoves(board, new Vector2Int(3, 3), Team.White);

            // Bishop at (3,3) should have 13 diagonal moves on empty board
            Assert.GreaterOrEqual(moves.Count, 10);

            // Check diagonal moves
            Assert.Contains(new Vector2Int(4, 4), moves);
            Assert.Contains(new Vector2Int(5, 5), moves);
            Assert.Contains(new Vector2Int(2, 2), moves);
            Assert.Contains(new Vector2Int(1, 1), moves);

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void KnightJumpRule_GeneratesValidMoves()
        {
            var rule = ScriptableObject.CreateInstance<KnightJumpRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Knight, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            var moves = rule.GetValidMoves(board, new Vector2Int(3, 3), Team.White);

            // Knight at (3,3) should have 8 L-shaped moves
            Assert.AreEqual(8, moves.Count);

            // Check some specific L-shaped moves
            Assert.Contains(new Vector2Int(5, 4), moves); // Right 2, Up 1
            Assert.Contains(new Vector2Int(5, 2), moves); // Right 2, Down 1
            Assert.Contains(new Vector2Int(1, 4), moves); // Left 2, Up 1
            Assert.Contains(new Vector2Int(4, 5), moves); // Right 1, Up 2

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void KingStepRule_GeneratesValidMoves()
        {
            var rule = ScriptableObject.CreateInstance<KingStepRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.King, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            var moves = rule.GetValidMoves(board, new Vector2Int(3, 3), Team.White);

            // King at (3,3) should have 8 one-square moves
            Assert.AreEqual(8, moves.Count);

            // Check all 8 directions
            Assert.Contains(new Vector2Int(3, 4), moves); // Up
            Assert.Contains(new Vector2Int(3, 2), moves); // Down
            Assert.Contains(new Vector2Int(2, 3), moves); // Left
            Assert.Contains(new Vector2Int(4, 3), moves); // Right
            Assert.Contains(new Vector2Int(4, 4), moves); // Up-Right
            Assert.Contains(new Vector2Int(4, 2), moves); // Down-Right
            Assert.Contains(new Vector2Int(2, 4), moves); // Up-Left
            Assert.Contains(new Vector2Int(2, 2), moves); // Down-Left

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        #endregion

        #region MoveValidator Tests

        [Test]
        public void MoveValidator_GetValidMoves_ReturnsCorrectMoves()
        {
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            var moves = MoveValidator.GetValidMoves(board, new Vector2Int(3, 3));

            Assert.IsNotEmpty(moves);
            Assert.AreEqual(14, moves.Count);

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void MoveValidator_IsValidMove_ReturnsTrue_ForValidMove()
        {
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            bool isValid = MoveValidator.IsValidMove(board, new Vector2Int(3, 3), new Vector2Int(3, 6));

            Assert.IsTrue(isValid);

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void MoveValidator_IsValidMove_ReturnsFalse_ForInvalidMove()
        {
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();
            var pieceObject = new GameObject("TestPiece");
            var piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));
            piece.AddMovementRule(rule);

            board.PlacePiece(piece, new Vector2Int(3, 3));

            // Rook can't move diagonally
            bool isValid = MoveValidator.IsValidMove(board, new Vector2Int(3, 3), new Vector2Int(5, 5));

            Assert.IsFalse(isValid);

            Object.DestroyImmediate(pieceObject);
            Object.DestroyImmediate(rule);
        }

        [Test]
        public void MoveValidator_CanCapture_ReturnsTrue_ForEnemyPiece()
        {
            var rule = ScriptableObject.CreateInstance<StraightLineRule>();

            var rookObject = new GameObject("Rook");
            var rook = rookObject.AddComponent<Piece>();
            rook.Initialize(PieceType.Rook, Team.White, new Vector2Int(3, 3));
            rook.AddMovementRule(rule);

            var enemyObject = new GameObject("Enemy");
            var enemy = enemyObject.AddComponent<Piece>();
            enemy.Initialize(PieceType.Pawn, Team.Black, new Vector2Int(3, 6));

            board.PlacePiece(rook, new Vector2Int(3, 3));
            board.PlacePiece(enemy, new Vector2Int(3, 6));

            bool canCapture = MoveValidator.CanCapture(board, new Vector2Int(3, 3), new Vector2Int(3, 6));

            Assert.IsTrue(canCapture);

            Object.DestroyImmediate(rookObject);
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(rule);
        }

        #endregion
    }
}
