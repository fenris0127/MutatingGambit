using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Tests.EditMode
{
    /// <summary>
    /// BoardDebugger 클래스의 단위 테스트
    /// 리팩토링 후 기능적 동일성을 검증합니다.
    /// </summary>
    [TestFixture]
    public class BoardDebuggerTests
    {
        private GameObject boardObject;
        private Board board;

        [SetUp]
        public void SetUp()
        {
            boardObject = new GameObject("TestBoard");
            board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);
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
        public void BoardDebugger_ShouldGenerateNonEmptyString()
        {
            // Arrange
            var debugger = new BoardDebugger(board);
            var pieces = new Piece[8, 8];
            var obstacles = new bool[8, 8];

            // Act
            string result = debugger.GenerateBoardString(pieces, obstacles);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("보드"));
        }

        [Test]
        public void BoardToString_ShouldCallBoardDebugger()
        {
            // Arrange
            board.SpawnPiece(PieceType.King, Team.White, new Vector2Int(4, 4));

            // Act
            string result = board.ToString();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("K"));  // King symbol
            Assert.IsTrue(result.Contains("8x8")); // Board dimensions
        }

        [Test]
        public void BoardDebugger_ShouldDisplayPiecesCorrectly()
        {
            // Arrange
            var pieces = new Piece[8, 8];
            pieces[0, 0] = CreateMockPiece(PieceType.Rook, Team.White);
            pieces[7, 7] = CreateMockPiece(PieceType.Queen, Team.Black);
            var obstacles = new bool[8, 8];
            var debugger = new BoardDebugger(board);

            // Act
            string result = debugger.GenerateBoardString(pieces, obstacles);

            // Assert
            Assert.IsTrue(result.Contains("R")); // White Rook
            Assert.IsTrue(result.Contains("q")); // Black Queen (lowercase)
        }

        private Piece CreateMockPiece(PieceType type, Team team)
        {
            var go = new GameObject($"{team}_{type}");
            var piece = go.AddComponent<Piece>();
            piece.Initialize(type, team, Vector2Int.zero);
            return piece;
        }
    }

    /// <summary>
    /// DungeonStateLoader 클래스의 단위 테스트
    /// 리팩토링 후 기능적 동일성을 검증합니다.
    /// </summary>
    [TestFixture]
    public class DungeonStateLoaderTests
    {
        [Test]
        public void DungeonStateLoader_ShouldHandleNullData()
        {
            // Arrange
            var mapGenerator = CreateMockMapGenerator();
            var loader = new MutatingGambit.Systems.Dungeon.DungeonStateLoader(mapGenerator);
            var playerState = new MutatingGambit.Systems.PieceManagement.PlayerState();

            // Act & Assert
            Assert.DoesNotThrow(() => loader.LoadPlayerState(null, playerState));
        }

        [Test]
        public void DungeonStateLoader_ShouldGenerateMapWithSeed()
        {
            // Arrange
            var mapGenerator = CreateMockMapGenerator();
            var loader = new MutatingGambit.Systems.Dungeon.DungeonStateLoader(mapGenerator);
            var saveData = CreateMockSaveData();

            // Act
            var map = loader.LoadDungeonMap(saveData, out var currentRoom);

            // Assert
            Assert.IsNotNull(map);
        }

        private MutatingGambit.Systems.Dungeon.DungeonMapGenerator CreateMockMapGenerator()
        {
            var go = new GameObject("MockMapGenerator");
            var generator = go.AddComponent<MutatingGambit.Systems.Dungeon.DungeonMapGenerator>();
            return generator;
        }

        private MutatingGambit.Systems.SaveLoad.GameSaveData CreateMockSaveData()
        {
            return new MutatingGambit.Systems.SaveLoad.GameSaveData
            {
                DungeonSeed = 12345,
                CurrentRoomIndex = 0,
                Gold = 100,
                PlayerData = new MutatingGambit.Systems.PieceManagement.PlayerStateData()
            };
        }
    }
}
