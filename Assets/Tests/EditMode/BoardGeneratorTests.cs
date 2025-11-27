using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Systems.Board;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Unit tests for the BoardGenerator and custom board system.
    /// </summary>
    public class BoardGeneratorTests
    {
        private GameObject generatorObject;
        private BoardGenerator generator;
        private BoardData testBoardData;

        [SetUp]
        public void Setup()
        {
            // Create BoardGenerator
            generatorObject = new GameObject("TestBoardGenerator");
            generator = generatorObject.AddComponent<BoardGenerator>();

            // Create test BoardData
            testBoardData = ScriptableObject.CreateInstance<BoardData>();
        }

        [TearDown]
        public void Teardown()
        {
            if (generatorObject != null)
            {
                Object.DestroyImmediate(generatorObject);
            }

            if (testBoardData != null)
            {
                Object.DestroyImmediate(testBoardData);
            }
        }

        #region BoardData Tests

        [Test]
        public void BoardData_DefaultValues_Are8x8()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            Assert.AreEqual(8, data.Width);
            Assert.AreEqual(8, data.Height);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_GetTileType_ReturnsNormal_ForUninitialized()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            TileType type = data.GetTileType(0, 0);
            Assert.AreEqual(TileType.Normal, type);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_GetTileType_ReturnsVoid_ForOutOfBounds()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            TileType type = data.GetTileType(10, 10);
            Assert.AreEqual(TileType.Void, type);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_SetTileType_UpdatesTile()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            data.InitializeTiles();

            data.SetTileType(3, 3, TileType.Obstacle);
            TileType type = data.GetTileType(3, 3);

            Assert.AreEqual(TileType.Obstacle, type);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_InitializeTiles_CreatesCorrectSize()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            data.InitializeTiles();

            // Check a few positions
            Assert.AreEqual(TileType.Normal, data.GetTileType(0, 0));
            Assert.AreEqual(TileType.Normal, data.GetTileType(7, 7));

            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_Resize_PreservesExistingTiles()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            data.InitializeTiles();

            // Set some obstacles
            data.SetTileType(2, 2, TileType.Obstacle);
            data.SetTileType(3, 3, TileType.Water);

            // Resize to larger
            data.Resize(10, 10);

            // Check preserved tiles
            Assert.AreEqual(TileType.Obstacle, data.GetTileType(2, 2));
            Assert.AreEqual(TileType.Water, data.GetTileType(3, 3));

            // Check new tiles are Normal
            Assert.AreEqual(TileType.Normal, data.GetTileType(9, 9));

            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_Resize_HandlesSmaller()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            data.InitializeTiles();

            data.SetTileType(2, 2, TileType.Obstacle);

            // Resize to smaller
            data.Resize(5, 5);

            Assert.AreEqual(5, data.Width);
            Assert.AreEqual(5, data.Height);
            Assert.AreEqual(TileType.Obstacle, data.GetTileType(2, 2));

            Object.DestroyImmediate(data);
        }

        [Test]
        public void BoardData_Validate_ReturnsTrueForValid()
        {
            var data = ScriptableObject.CreateInstance<BoardData>();
            data.InitializeTiles();

            bool isValid = data.Validate();
            Assert.IsTrue(isValid);

            Object.DestroyImmediate(data);
        }

        #endregion

        #region Tile Tests

        [Test]
        public void Tile_Initialize_SetsProperties()
        {
            var tileObject = new GameObject("TestTile");
            var spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
            var tile = tileObject.AddComponent<Tile>();

            var position = new Vector2Int(3, 4);
            tile.Initialize(position, TileType.Obstacle);

            Assert.AreEqual(position, tile.Position);
            Assert.AreEqual(TileType.Obstacle, tile.Type);

            Object.DestroyImmediate(tileObject);
        }

        [Test]
        public void Tile_IsWalkable_ReturnsTrueForNormal()
        {
            var tileObject = new GameObject("TestTile");
            var spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
            var tile = tileObject.AddComponent<Tile>();

            tile.Initialize(Vector2Int.zero, TileType.Normal);

            Assert.IsTrue(tile.IsWalkable);

            Object.DestroyImmediate(tileObject);
        }

        [Test]
        public void Tile_IsWalkable_ReturnsFalseForObstacle()
        {
            var tileObject = new GameObject("TestTile");
            var spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
            var tile = tileObject.AddComponent<Tile>();

            tile.Initialize(Vector2Int.zero, TileType.Obstacle);

            Assert.IsFalse(tile.IsWalkable);

            Object.DestroyImmediate(tileObject);
        }

        [Test]
        public void Tile_Highlight_ChangesState()
        {
            var tileObject = new GameObject("TestTile");
            var spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
            var tile = tileObject.AddComponent<Tile>();

            tile.Initialize(Vector2Int.zero, TileType.Normal);

            tile.Highlight(true);
            // Visual check would be needed for full validation
            // For now we just ensure it doesn't throw

            tile.Highlight(false);

            Assert.Pass("Highlight executed without errors");

            Object.DestroyImmediate(tileObject);
        }

        #endregion

        #region BoardGenerator Tests

        [Test]
        public void BoardGenerator_GenerateFromData_CreatesBoard()
        {
            testBoardData.InitializeTiles();
            generator.GenerateFromData(testBoardData);

            Assert.IsTrue(generator.IsGenerated);
            Assert.AreEqual(testBoardData, generator.Data);
        }

        [Test]
        public void BoardGenerator_GetTile_ReturnsCorrectTile()
        {
            testBoardData.InitializeTiles();
            generator.GenerateFromData(testBoardData);

            var tile = generator.GetTile(new Vector2Int(3, 3));

            Assert.IsNotNull(tile);
            Assert.AreEqual(new Vector2Int(3, 3), tile.Position);
        }

        [Test]
        public void BoardGenerator_GetTile_ReturnsNullForInvalidPosition()
        {
            testBoardData.InitializeTiles();
            generator.GenerateFromData(testBoardData);

            var tile = generator.GetTile(new Vector2Int(10, 10));

            Assert.IsNull(tile);
        }

        [Test]
        public void BoardGenerator_ClearBoard_RemovesTiles()
        {
            testBoardData.InitializeTiles();
            generator.GenerateFromData(testBoardData);

            Assert.IsTrue(generator.IsGenerated);

            generator.ClearBoard();

            Assert.IsFalse(generator.IsGenerated);
            Assert.IsNull(generator.GetTile(new Vector2Int(0, 0)));
        }

        [Test]
        public void BoardGenerator_HighlightTiles_HighlightsMultipleTiles()
        {
            testBoardData.InitializeTiles();
            generator.GenerateFromData(testBoardData);

            Vector2Int[] positions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1),
                new Vector2Int(2, 2)
            };

            generator.HighlightTiles(positions, true);

            // Just ensure it doesn't throw - visual validation needed
            Assert.Pass("Highlight executed without errors");
        }

        [Test]
        public void BoardGenerator_ClearHighlights_RemovesAllHighlights()
        {
            testBoardData.InitializeTiles();
            generator.GenerateFromData(testBoardData);

            Vector2Int[] positions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 1)
            };

            generator.HighlightTiles(positions, true);
            generator.ClearHighlights();

            // Visual validation needed - just ensure no errors
            Assert.Pass("Clear highlights executed without errors");
        }

        [Test]
        public void BoardGenerator_GeneratesObstacles_Correctly()
        {
            testBoardData.InitializeTiles();
            testBoardData.SetTileType(3, 3, TileType.Obstacle);
            testBoardData.SetTileType(4, 4, TileType.Obstacle);

            generator.GenerateFromData(testBoardData);

            var tile1 = generator.GetTile(new Vector2Int(3, 3));
            var tile2 = generator.GetTile(new Vector2Int(4, 4));

            Assert.IsNotNull(tile1);
            Assert.IsNotNull(tile2);
            Assert.AreEqual(TileType.Obstacle, tile1.Type);
            Assert.AreEqual(TileType.Obstacle, tile2.Type);
        }

        [Test]
        public void BoardGenerator_CustomSize_GeneratesCorrectly()
        {
            // Create a 6x6 board
            var customData = ScriptableObject.CreateInstance<BoardData>();
            customData.Resize(6, 6);
            customData.InitializeTiles();

            generator.GenerateFromData(customData);

            Assert.IsTrue(generator.IsGenerated);
            Assert.IsNotNull(generator.GetTile(new Vector2Int(5, 5)));
            Assert.IsNull(generator.GetTile(new Vector2Int(6, 6)));

            Object.DestroyImmediate(customData);
        }

        #endregion
    }
}
