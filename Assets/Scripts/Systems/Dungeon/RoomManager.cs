using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Manages the current room state, including victory/defeat conditions.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        [Header("Current Room")]
        [SerializeField]
        private RoomData currentRoom;

        [SerializeField]
        private Board board;

        private int currentTurn = 0;
        private Team playerTeam = Team.White;
        private bool roomCompleted = false;
        private bool roomFailed = false;

        /// <summary>
        /// Gets the current room data.
        /// </summary>
        public RoomData CurrentRoom => currentRoom;

        /// <summary>
        /// Gets whether the room has been completed successfully.
        /// </summary>
        public bool IsRoomCompleted => roomCompleted;

        /// <summary>
        /// Gets whether the room has been failed.
        /// </summary>
        public bool IsRoomFailed => roomFailed;

        /// <summary>
        /// Gets the current turn number.
        /// </summary>
        public int CurrentTurn => currentTurn;

        /// <summary>
        /// Sets the board reference.
        /// </summary>
        public void SetBoard(Board gameBoard)
        {
            board = gameBoard;
        }

        /// <summary>
        /// Enters a new room with the specified room data.
        /// </summary>
        public void EnterRoom(RoomData roomData, Team player)
        {
            currentRoom = roomData;
            playerTeam = player;
            currentTurn = 0;
            roomCompleted = false;
            roomFailed = false;

            if (currentRoom != null && currentRoom.VictoryCondition != null)
            {
                currentRoom.VictoryCondition.Reset();
            }

            Debug.Log($"Entered room: {roomData.RoomName} ({roomData.Type})");
        }

        /// <summary>
        /// Called at the start of each turn.
        /// </summary>
        public void StartTurn()
        {
            currentTurn++;
        }

        /// <summary>
        /// Called when the player makes a move.
        /// Increments move counters for victory conditions.
        /// </summary>
        public void OnPlayerMove()
        {
            if (currentRoom == null || currentRoom.VictoryCondition == null)
            {
                return;
            }

            // If victory condition has move tracking, increment it
            if (currentRoom.VictoryCondition is CheckmateInNMovesCondition checkmateCondition)
            {
                checkmateCondition.IncrementMoves();
            }
        }

        /// <summary>
        /// Checks if victory or defeat conditions are met.
        /// Call this after each move.
        /// </summary>
        public void CheckConditions()
        {
            if (board == null || currentRoom == null || currentRoom.VictoryCondition == null)
            {
                return;
            }

            if (roomCompleted || roomFailed)
            {
                return; // Already determined
            }

            // Check victory
            bool victoryAchieved = currentRoom.VictoryCondition.IsVictoryAchieved(board, currentTurn, playerTeam);
            if (victoryAchieved)
            {
                roomCompleted = true;
                Debug.Log($"Victory! Room '{currentRoom.RoomName}' completed.");
                return;
            }

            // Check defeat
            bool defeatMet = currentRoom.VictoryCondition.IsDefeatConditionMet(board, currentTurn, playerTeam);
            if (defeatMet)
            {
                roomFailed = true;
                Debug.Log($"Defeat! Room '{currentRoom.RoomName}' failed.");
                return;
            }
        }

        /// <summary>
        /// Gets the progress string for the current victory condition.
        /// </summary>
        public string GetProgressString()
        {
            if (currentRoom == null || currentRoom.VictoryCondition == null)
            {
                return "";
            }

            return currentRoom.VictoryCondition.GetProgressString(board, currentTurn, playerTeam);
        }

        /// <summary>
        /// Gets the victory condition description.
        /// </summary>
        public string GetVictoryDescription()
        {
            if (currentRoom == null || currentRoom.VictoryCondition == null)
            {
                return "No victory condition";
            }

            return currentRoom.VictoryCondition.Description;
        }

        /// <summary>
        /// Exits the current room.
        /// </summary>
        public void ExitRoom()
        {
            currentRoom = null;
            currentTurn = 0;
            roomCompleted = false;
            roomFailed = false;
        }
    }
}
