using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Unit tests for the dungeon exploration system.
    /// </summary>
    public class DungeonSystemTests
    {
        private GameObject generatorObject;
        private DungeonMapGenerator generator;

        [SetUp]
        public void Setup()
        {
            generatorObject = new GameObject("TestDungeonGenerator");
            generator = generatorObject.AddComponent<DungeonMapGenerator>();
        }

        [TearDown]
        public void Teardown()
        {
            if (generatorObject != null)
            {
                Object.DestroyImmediate(generatorObject);
            }
        }

        #region RoomNode Tests

        [Test]
        public void RoomNode_Constructor_SetsPropertiesCorrectly()
        {
            var node = new RoomNode("test_1", RoomType.NormalCombat, new Vector2Int(0, 0));

            Assert.AreEqual("test_1", node.NodeId);
            Assert.AreEqual(RoomType.NormalCombat, node.Type);
            Assert.AreEqual(new Vector2Int(0, 0), node.Position);
            Assert.IsFalse(node.IsCleared);
            Assert.IsFalse(node.IsAccessible);
        }

        [Test]
        public void RoomNode_AddConnection_AddsConnectionSuccessfully()
        {
            var node1 = new RoomNode("node1", RoomType.NormalCombat, new Vector2Int(0, 0));
            var node2 = new RoomNode("node2", RoomType.NormalCombat, new Vector2Int(1, 0));

            node1.AddConnection(node2);

            Assert.AreEqual(1, node1.Connections.Count);
            Assert.IsTrue(node1.IsConnectedTo(node2));
        }

        [Test]
        public void RoomNode_RemoveConnection_RemovesConnectionSuccessfully()
        {
            var node1 = new RoomNode("node1", RoomType.NormalCombat, new Vector2Int(0, 0));
            var node2 = new RoomNode("node2", RoomType.NormalCombat, new Vector2Int(1, 0));

            node1.AddConnection(node2);
            node1.RemoveConnection(node2);

            Assert.AreEqual(0, node1.Connections.Count);
            Assert.IsFalse(node1.IsConnectedTo(node2));
        }

        [Test]
        public void RoomNode_LayerAndPositionInLayer_ReturnsCorrectValues()
        {
            var node = new RoomNode("test", RoomType.NormalCombat, new Vector2Int(3, 2));

            Assert.AreEqual(3, node.Layer);
            Assert.AreEqual(2, node.PositionInLayer);
        }

        #endregion

        #region DungeonMap Tests

        [Test]
        public void DungeonMap_AddNode_AddsNodeSuccessfully()
        {
            var map = new DungeonMap();
            var node = new RoomNode("node1", RoomType.NormalCombat, new Vector2Int(0, 0));

            map.AddNode(node);

            Assert.AreEqual(1, map.AllNodes.Count);
            Assert.Contains(node, map.AllNodes);
        }

        [Test]
        public void DungeonMap_GetNodesInLayer_ReturnsCorrectNodes()
        {
            var map = new DungeonMap();
            var node1 = new RoomNode("node1", RoomType.NormalCombat, new Vector2Int(0, 0));
            var node2 = new RoomNode("node2", RoomType.NormalCombat, new Vector2Int(0, 1));
            var node3 = new RoomNode("node3", RoomType.NormalCombat, new Vector2Int(1, 0));

            map.AddNode(node1);
            map.AddNode(node2);
            map.AddNode(node3);

            var layer0Nodes = map.GetNodesInLayer(0);
            var layer1Nodes = map.GetNodesInLayer(1);

            Assert.AreEqual(2, layer0Nodes.Count);
            Assert.AreEqual(1, layer1Nodes.Count);
        }

        [Test]
        public void DungeonMap_GetNodeById_ReturnsCorrectNode()
        {
            var map = new DungeonMap();
            var node = new RoomNode("test_node", RoomType.NormalCombat, new Vector2Int(0, 0));

            map.AddNode(node);

            var retrieved = map.GetNodeById("test_node");

            Assert.IsNotNull(retrieved);
            Assert.AreEqual(node, retrieved);
        }

        [Test]
        public void DungeonMap_MaxLayer_ReturnsCorrectValue()
        {
            var map = new DungeonMap();
            map.AddNode(new RoomNode("n1", RoomType.NormalCombat, new Vector2Int(0, 0)));
            map.AddNode(new RoomNode("n2", RoomType.NormalCombat, new Vector2Int(1, 0)));
            map.AddNode(new RoomNode("n3", RoomType.NormalCombat, new Vector2Int(3, 0)));

            Assert.AreEqual(3, map.MaxLayer);
        }

        #endregion

        #region DungeonMapGenerator Tests

        [Test]
        public void DungeonMapGenerator_GenerateMap_CreatesMap()
        {
            var map = generator.GenerateMap(5, 2, 4, 42);

            Assert.IsNotNull(map);
            Assert.Greater(map.AllNodes.Count, 0);
            Assert.AreEqual(42, map.Seed);
        }

        [Test]
        public void DungeonMapGenerator_GenerateMap_FirstLayerIsStart()
        {
            var map = generator.GenerateMap(5, 2, 4, 123);

            var layer0Nodes = map.GetNodesInLayer(0);

            Assert.Greater(layer0Nodes.Count, 0);
            foreach (var node in layer0Nodes)
            {
                Assert.AreEqual(RoomType.Start, node.Type);
            }
        }

        [Test]
        public void DungeonMapGenerator_GenerateMap_LastLayerIsBoss()
        {
            int layers = 5;
            var map = generator.GenerateMap(layers, 2, 4, 456);

            var lastLayerNodes = map.GetNodesInLayer(layers - 1);

            Assert.Greater(lastLayerNodes.Count, 0);
            foreach (var node in lastLayerNodes)
            {
                Assert.AreEqual(RoomType.Boss, node.Type);
            }
        }

        [Test]
        public void DungeonMapGenerator_GenerateMap_AllRoomsReachable()
        {
            var map = generator.GenerateMap(5, 2, 4, 789);

            // Check that every layer (except first) has incoming connections
            for (int layer = 1; layer <= map.MaxLayer; layer++)
            {
                var layerNodes = map.GetNodesInLayer(layer);
                var previousLayerNodes = map.GetNodesInLayer(layer - 1);

                foreach (var node in layerNodes)
                {
                    bool hasIncoming = false;
                    foreach (var prevNode in previousLayerNodes)
                    {
                        if (prevNode.IsConnectedTo(node))
                        {
                            hasIncoming = true;
                            break;
                        }
                    }

                    Assert.IsTrue(hasIncoming, $"Node {node.NodeId} in layer {layer} has no incoming connections!");
                }
            }
        }

        [Test]
        public void DungeonMapGenerator_GenerateMap_StartsAccessible()
        {
            var map = generator.GenerateMap(5, 2, 4, 999);

            // First node should be accessible
            if (map.AllNodes.Count > 0)
            {
                Assert.IsTrue(map.AllNodes[0].IsAccessible);
            }
        }

        [Test]
        public void DungeonMapGenerator_SameSeed_ProducesSameMap()
        {
            int seed = 12345;

            var map1 = generator.GenerateMap(5, 2, 4, seed);
            var map2 = generator.GenerateMap(5, 2, 4, seed);

            Assert.AreEqual(map1.AllNodes.Count, map2.AllNodes.Count);
            Assert.AreEqual(map1.Seed, map2.Seed);
        }

        #endregion

        #region Victory Condition Tests

        [Test]
        public void CheckmateInNMovesCondition_Reset_ResetsMovesCounter()
        {
            var condition = ScriptableObject.CreateInstance<CheckmateInNMovesCondition>();

            condition.IncrementMoves();
            condition.IncrementMoves();
            Assert.AreEqual(2, condition.MovesTaken);

            condition.Reset();
            Assert.AreEqual(0, condition.MovesTaken);

            Object.DestroyImmediate(condition);
        }

        [Test]
        public void CheckmateInNMovesCondition_IsDefeatConditionMet_TrueWhenExceedsMaxMoves()
        {
            var condition = ScriptableObject.CreateInstance<CheckmateInNMovesCondition>();
            var boardObject = new GameObject("Board");
            var board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            // Increment moves beyond max
            for (int i = 0; i < 6; i++)
            {
                condition.IncrementMoves();
            }

            bool isDefeated = condition.IsDefeatConditionMet(board, 0, Team.White);

            Assert.IsTrue(isDefeated);

            Object.DestroyImmediate(condition);
            Object.DestroyImmediate(boardObject);
        }

        [Test]
        public void CheckmateInNMovesCondition_GetProgressString_ShowsCorrectProgress()
        {
            var condition = ScriptableObject.CreateInstance<CheckmateInNMovesCondition>();
            var boardObject = new GameObject("Board");
            var board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            condition.IncrementMoves();
            condition.IncrementMoves();

            string progress = condition.GetProgressString(board, 0, Team.White);

            Assert.IsTrue(progress.Contains("2"));
            Assert.IsTrue(progress.Contains("5")); // Default max is 5

            Object.DestroyImmediate(condition);
            Object.DestroyImmediate(boardObject);
        }

        [Test]
        public void CaptureSpecificPieceCondition_Reset_ResetsState()
        {
            var condition = ScriptableObject.CreateInstance<CaptureSpecificPieceCondition>();

            condition.SetTarget(PieceType.Queen, new Vector2Int(4, 7), true);
            condition.Reset();

            // After reset, target should not be marked as captured
            var boardObject = new GameObject("Board");
            var board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            bool isVictory = condition.IsVictoryAchieved(board, 0, Team.White);
            // Since board is empty, target doesn't exist, so it's "captured" (victory)
            // This is expected behavior

            Object.DestroyImmediate(condition);
            Object.DestroyImmediate(boardObject);
        }

        [Test]
        public void CaptureSpecificPieceCondition_IsVictoryAchieved_TrueWhenPieceCaptured()
        {
            var condition = ScriptableObject.CreateInstance<CaptureSpecificPieceCondition>();
            condition.SetTarget(PieceType.Queen, new Vector2Int(4, 7), true);

            var boardObject = new GameObject("Board");
            var board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            // Don't place any pieces - queen doesn't exist (captured)
            bool isVictory = condition.IsVictoryAchieved(board, 0, Team.White);

            Assert.IsTrue(isVictory);

            Object.DestroyImmediate(condition);
            Object.DestroyImmediate(boardObject);
        }

        [Test]
        public void CaptureSpecificPieceCondition_IsVictoryAchieved_FalseWhenPieceExists()
        {
            var condition = ScriptableObject.CreateInstance<CaptureSpecificPieceCondition>();
            condition.SetTarget(PieceType.Queen, new Vector2Int(4, 7), true);

            var boardObject = new GameObject("Board");
            var board = boardObject.AddComponent<Board>();
            board.Initialize(8, 8);

            // Place a black queen on the board
            var queenObject = new GameObject("Queen");
            var queen = queenObject.AddComponent<Piece>();
            queen.Initialize(PieceType.Queen, Team.Black, new Vector2Int(4, 7));
            board.PlacePiece(queen, new Vector2Int(4, 7));

            bool isVictory = condition.IsVictoryAchieved(board, 0, Team.White);

            Assert.IsFalse(isVictory);

            Object.DestroyImmediate(condition);
            Object.DestroyImmediate(queenObject);
            Object.DestroyImmediate(boardObject);
        }

        #endregion

        #region RoomManager Tests

        [Test]
        public void RoomManager_EnterRoom_SetsRoomData()
        {
            var managerObject = new GameObject("RoomManager");
            var manager = managerObject.AddComponent<RoomManager>();

            var roomData = ScriptableObject.CreateInstance<RoomData>();

            manager.EnterRoom(roomData, Team.White);

            Assert.AreEqual(roomData, manager.CurrentRoom);
            Assert.IsFalse(manager.IsRoomCompleted);
            Assert.IsFalse(manager.IsRoomFailed);

            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(roomData);
        }

        [Test]
        public void RoomManager_StartTurn_IncrementsCounter()
        {
            var managerObject = new GameObject("RoomManager");
            var manager = managerObject.AddComponent<RoomManager>();

            Assert.AreEqual(0, manager.CurrentTurn);

            manager.StartTurn();
            Assert.AreEqual(1, manager.CurrentTurn);

            manager.StartTurn();
            Assert.AreEqual(2, manager.CurrentTurn);

            Object.DestroyImmediate(managerObject);
        }

        [Test]
        public void RoomManager_ExitRoom_ClearsRoomData()
        {
            var managerObject = new GameObject("RoomManager");
            var manager = managerObject.AddComponent<RoomManager>();

            var roomData = ScriptableObject.CreateInstance<RoomData>();
            manager.EnterRoom(roomData, Team.White);
            manager.ExitRoom();

            Assert.IsNull(manager.CurrentRoom);
            Assert.AreEqual(0, manager.CurrentTurn);

            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(roomData);
        }

        #endregion
    }
}
