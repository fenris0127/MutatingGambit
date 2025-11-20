using UnityEngine;
using MutatingGambit.Core;
using MutatingGambit.Core.Map;
using MutatingGambit.Core.Meta;

namespace MutatingGambit
{
    /// <summary>
    /// Main game flow manager - handles transitions between map/combat/reward screens
    /// </summary>
    public enum GameState
    {
        Map,        // Choosing next room on dungeon map
        Combat,     // Fighting in a room
        Reward,     // Selecting reward after combat
        Rest,       // Repairing pieces
        GameOver,   // Lost the run
        Victory     // Completed the dungeon
    }

    public class GameFlowManager : MonoBehaviour
    {
        [Header("Controllers")]
        public MapController mapController;
        public CombatController combatController;
        public RewardController rewardController;
        public RestController restController;

        [Header("Game Settings")]
        public int dungeonLayers = 5;
        public int startingPieceHP = 3;

        // Game state
        private GameState _currentState;
        private DungeonMap _dungeonMap;
        private MetaProgression _metaProgression;
        private Board _playerBoard;

        void Start()
        {
            InitializeGame();
        }

        void InitializeGame()
        {
            // Load meta progression
            _metaProgression = new MetaProgression();
            // TODO: Load from PlayerPrefs

            // Create player's board
            _playerBoard = new Board(8, 8);
            _playerBoard.SetupStandardBoard();

            // Generate dungeon map
            _dungeonMap = new DungeonMap();
            _dungeonMap.Generate(dungeonLayers);

            // Start at map screen
            TransitionToMap();
        }

        public void TransitionToMap()
        {
            _currentState = GameState.Map;

            // Hide all screens
            HideAllScreens();

            // Show map
            if (mapController != null)
            {
                mapController.gameObject.SetActive(true);
                mapController.Initialize(_dungeonMap);
                mapController.OnNodeSelected += OnRoomSelected;
            }

            Debug.Log("Showing dungeon map");
        }

        public void TransitionToCombat(MapNode node)
        {
            _currentState = GameState.Combat;

            // Hide all screens
            HideAllScreens();

            // Show combat
            if (combatController != null)
            {
                combatController.gameObject.SetActive(true);
                combatController.Initialize(_playerBoard, node);
                combatController.OnCombatWon += OnCombatWon;
                combatController.OnCombatLost += OnCombatLost;
            }

            Debug.Log($"Starting combat in {node.Room.Type} room");
        }

        public void TransitionToReward(MapNode completedNode)
        {
            _currentState = GameState.Reward;

            // Hide all screens
            HideAllScreens();

            // Show reward
            if (rewardController != null)
            {
                var reward = completedNode.GetReward();
                rewardController.gameObject.SetActive(true);
                rewardController.Initialize(reward, _playerBoard);
                rewardController.OnRewardSelected += OnRewardSelected;
            }

            Debug.Log("Showing reward screen");
        }

        public void TransitionToRest(MapNode node)
        {
            _currentState = GameState.Rest;

            // Hide all screens
            HideAllScreens();

            // Show rest
            if (restController != null)
            {
                restController.gameObject.SetActive(true);
                restController.Initialize(_playerBoard);
                restController.OnRestComplete += OnRestComplete;
            }

            Debug.Log("Showing rest screen");
        }

        void OnRoomSelected(MapNode node)
        {
            // Unsubscribe
            if (mapController != null)
                mapController.OnNodeSelected -= OnRoomSelected;

            // Transition based on room type
            if (node.Room.Type == Core.Rooms.RoomType.Rest)
            {
                TransitionToRest(node);
            }
            else if (node.Room.Type == Core.Rooms.RoomType.Treasure)
            {
                // Treasure rooms give instant rewards
                node.CompleteRoom();
                TransitionToReward(node);
            }
            else
            {
                // Combat rooms
                TransitionToCombat(node);
            }
        }

        void OnCombatWon(MapNode node)
        {
            // Unsubscribe
            if (combatController != null)
            {
                combatController.OnCombatWon -= OnCombatWon;
                combatController.OnCombatLost -= OnCombatLost;
            }

            // Mark node as complete
            node.CompleteRoom();

            // Check if dungeon complete
            if (IsDungeonComplete())
            {
                OnVictory();
                return;
            }

            // Show reward
            TransitionToReward(node);
        }

        void OnCombatLost()
        {
            // Unsubscribe
            if (combatController != null)
            {
                combatController.OnCombatWon -= OnCombatWon;
                combatController.OnCombatLost -= OnCombatLost;
            }

            OnGameOver();
        }

        void OnRewardSelected()
        {
            // Unsubscribe
            if (rewardController != null)
                rewardController.OnRewardSelected -= OnRewardSelected;

            // Return to map
            TransitionToMap();
        }

        void OnRestComplete()
        {
            // Unsubscribe
            if (restController != null)
                restController.OnRestComplete -= OnRestComplete;

            // Return to map
            TransitionToMap();
        }

        bool IsDungeonComplete()
        {
            // Check if at final node
            var currentNode = _dungeonMap.GetCurrentNode();
            return currentNode != null && currentNode.Layer == dungeonLayers - 1 && currentNode.IsCompleted;
        }

        void OnVictory()
        {
            _currentState = GameState.Victory;
            HideAllScreens();

            // Award currency
            int currencyEarned = dungeonLayers * 50;
            _metaProgression.AddCurrency(currencyEarned);
            _metaProgression.RecordRunComplete(dungeonLayers, true);

            Debug.Log($"VICTORY! Earned {currencyEarned} Gambit Fragments");
            // TODO: Show victory screen
        }

        void OnGameOver()
        {
            _currentState = GameState.GameOver;
            HideAllScreens();

            // Award currency based on progress
            var currentNode = _dungeonMap.GetCurrentNode();
            int layer = currentNode != null ? currentNode.Layer : 0;
            int currencyEarned = layer * 10;
            _metaProgression.AddCurrency(currencyEarned);
            _metaProgression.RecordRunComplete(layer, false);

            Debug.Log($"GAME OVER! Reached layer {layer}. Earned {currencyEarned} Gambit Fragments");
            // TODO: Show game over screen
        }

        void HideAllScreens()
        {
            if (mapController != null)
                mapController.gameObject.SetActive(false);
            if (combatController != null)
                combatController.gameObject.SetActive(false);
            if (rewardController != null)
                rewardController.gameObject.SetActive(false);
            if (restController != null)
                restController.gameObject.SetActive(false);
        }

        public void RestartGame()
        {
            InitializeGame();
        }
    }
}
