using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.Map;
using MutatingGambit.Core.Rooms;
using System.Linq;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 7: Dungeon Map System Tests
    /// </summary>
    [TestFixture]
    public class Phase7_DungeonMapTests
    {
        #region Map Generation Tests

        [Test]
        public void DungeonMap_CanBeGenerated()
        {
            // Arrange & Act
            var map = new DungeonMap();

            // Assert
            Assert.IsNotNull(map);
            Assert.IsNotNull(map.Nodes);
        }

        [Test]
        public void DungeonMap_HasStartNode()
        {
            // Arrange
            var map = new DungeonMap();

            // Act
            var startNode = map.GetStartNode();

            // Assert
            Assert.IsNotNull(startNode);
            Assert.AreEqual(0, startNode.Layer);
        }

        [Test]
        public void DungeonMap_HasEndNode()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3); // 3 layers

            // Act
            var endNodes = map.GetNodesAtLayer(2);

            // Assert
            Assert.Greater(endNodes.Count, 0);
            Assert.IsTrue(endNodes.Any(n => n.Room.Type == RoomType.Boss || n.IsEndNode));
        }

        [Test]
        public void DungeonMap_NodeConnectionsAreValid()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);

            // Act
            var startNode = map.GetStartNode();
            var connections = startNode.Connections;

            // Assert
            Assert.Greater(connections.Count, 0, "Start node should have connections");
            foreach (var connection in connections)
            {
                Assert.IsNotNull(connection);
                Assert.Greater(connection.Layer, startNode.Layer, "Connected nodes should be in next layer");
            }
        }

        [Test]
        public void DungeonMap_PlayerCanOnlyMoveToAdjacentNodes()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);
            var startNode = map.GetStartNode();

            // Act
            var availableNodes = map.GetAvailableNextNodes(startNode);

            // Assert
            Assert.Greater(availableNodes.Count, 0);
            foreach (var node in availableNodes)
            {
                Assert.IsTrue(startNode.Connections.Contains(node));
            }
        }

        #endregion

        #region Map Navigation Tests

        [Test]
        public void DungeonMap_TracksCurrentPosition()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);
            var startNode = map.GetStartNode();

            // Act
            map.SetCurrentNode(startNode);
            var currentNode = map.GetCurrentNode();

            // Assert
            Assert.AreEqual(startNode, currentNode);
        }

        [Test]
        public void DungeonMap_TracksVisitedNodes()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);
            var startNode = map.GetStartNode();

            // Act
            map.VisitNode(startNode);

            // Assert
            Assert.IsTrue(map.IsNodeVisited(startNode));
        }

        [Test]
        public void DungeonMap_ShowsAvailableNextNodes()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);
            var startNode = map.GetStartNode();
            map.SetCurrentNode(startNode);

            // Act
            var availableNodes = map.GetAvailableNextNodes(startNode);

            // Assert
            Assert.Greater(availableNodes.Count, 0);
            Assert.IsTrue(availableNodes.All(n => n.Layer == startNode.Layer + 1));
        }

        [Test]
        public void DungeonMap_CompletesLayerWhenMovingToNext()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);
            var startNode = map.GetStartNode();
            map.SetCurrentNode(startNode);

            // Act
            var nextNode = map.GetAvailableNextNodes(startNode)[0];
            map.MoveToNode(nextNode);

            // Assert
            Assert.AreEqual(nextNode, map.GetCurrentNode());
            Assert.IsTrue(map.IsNodeVisited(startNode));
            Assert.AreEqual(1, map.GetCurrentLayer());
        }

        #endregion

        #region Room Rewards Tests

        [Test]
        public void MapNode_ProvidesArtifactChoices()
        {
            // Arrange
            var combatRoom = new CombatRoom(RoomDifficulty.Normal);
            var node = new MapNode(combatRoom, 0);

            // Act
            node.CompleteRoom();
            var reward = node.GetReward();

            // Assert
            Assert.IsNotNull(reward);
            Assert.AreEqual(3, reward.ArtifactChoices.Count);
        }

        [Test]
        public void MapNode_EliteRoomsProvidesBetterRewards()
        {
            // Arrange
            var normalRoom = new CombatRoom(RoomDifficulty.Normal);
            var eliteRoom = new CombatRoom(RoomDifficulty.Elite);
            var normalNode = new MapNode(normalRoom, 1);
            var eliteNode = new MapNode(eliteRoom, 1);

            // Act
            normalNode.CompleteRoom();
            eliteNode.CompleteRoom();
            var normalReward = normalNode.GetReward();
            var eliteReward = eliteNode.GetReward();

            // Assert
            Assert.Greater(eliteReward.CurrencyAmount, normalReward.CurrencyAmount);
        }

        [Test]
        public void MapNode_RewardChoiceIsSaved()
        {
            // Arrange
            var combatRoom = new CombatRoom(RoomDifficulty.Normal);
            var node = new MapNode(combatRoom, 0);
            node.CompleteRoom();
            var reward = node.GetReward();
            var chosenArtifact = reward.ArtifactChoices[0];

            // Act
            node.SetChosenReward(chosenArtifact);

            // Assert
            Assert.AreEqual(chosenArtifact, node.GetChosenReward());
        }

        [Test]
        public void MapNode_CanOnlyBeCompletedOnce()
        {
            // Arrange
            var combatRoom = new CombatRoom(RoomDifficulty.Normal);
            var node = new MapNode(combatRoom, 0);

            // Act
            node.CompleteRoom();
            var firstReward = node.GetReward();

            // Try to complete again
            node.CompleteRoom();
            var secondReward = node.GetReward();

            // Assert
            Assert.IsTrue(node.IsCompleted);
            Assert.AreEqual(firstReward, secondReward, "Should return same reward");
        }

        #endregion

        #region Map Structure Tests

        [Test]
        public void DungeonMap_HasMultipleLayers()
        {
            // Arrange
            var map = new DungeonMap();

            // Act
            map.Generate(5);

            // Assert
            Assert.AreEqual(5, map.GetLayerCount());
            for (int i = 0; i < 5; i++)
            {
                var nodesAtLayer = map.GetNodesAtLayer(i);
                Assert.Greater(nodesAtLayer.Count, 0, $"Layer {i} should have nodes");
            }
        }

        [Test]
        public void DungeonMap_NodesHaveRoomTypes()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(3);

            // Act
            var allNodes = map.Nodes;

            // Assert
            Assert.IsTrue(allNodes.All(n => n.Room != null));
            Assert.IsTrue(allNodes.Any(n => n.Room.Type == RoomType.Combat));
        }

        [Test]
        public void DungeonMap_PathsAreBranching()
        {
            // Arrange
            var map = new DungeonMap();
            map.Generate(4);

            // Act
            var startNode = map.GetStartNode();
            var firstLayerConnections = startNode.Connections;

            // Assert
            Assert.Greater(firstLayerConnections.Count, 1, "Should have branching paths");
        }

        #endregion
    }
}
