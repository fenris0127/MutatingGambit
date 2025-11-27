using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Defines the type of victory condition for a room.
    /// </summary>
    public enum VictoryConditionType
    {
        Checkmate,          // Achieve checkmate against enemy king
        CheckmateInNMoves,  // Achieve checkmate within N moves
        CaptureSpecificPiece, // Capture a specific enemy piece
        MoveKingToPosition,  // Move your king to a specific position
        SurviveNTurns,      // Survive for N turns without losing
        CaptureAllPieces    // Capture all enemy pieces (except king if not checkmate)
    }

    /// <summary>
    /// Abstract base class for victory conditions in puzzle/combat rooms.
    /// </summary>
    public abstract class VictoryCondition : ScriptableObject
    {
        [Header("Victory Condition Info")]
        [SerializeField]
        private string conditionName;

        [SerializeField]
        [TextArea(2, 3)]
        private string description;

        [SerializeField]
        private VictoryConditionType conditionType;

        /// <summary>
        /// Gets the name of this victory condition.
        /// </summary>
        public string ConditionName => conditionName;

        /// <summary>
        /// Gets the description of this victory condition.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the type of this victory condition.
        /// </summary>
        public VictoryConditionType Type => conditionType;

        /// <summary>
        /// Checks if the victory condition has been met.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="currentTurn">The current turn number</param>
        /// <param name="playerTeam">The player's team</param>
        /// <returns>True if victory condition is met</returns>
        public abstract bool IsVictoryAchieved(Board board, int currentTurn, Team playerTeam);

        /// <summary>
        /// Checks if the player has failed the victory condition (e.g., ran out of moves).
        /// </summary>
        public abstract bool IsDefeatConditionMet(Board board, int currentTurn, Team playerTeam);

        /// <summary>
        /// Gets a progress string for UI display (e.g., "2/5 moves used").
        /// </summary>
        public virtual string GetProgressString(Board board, int currentTurn, Team playerTeam)
        {
            return "";
        }

        /// <summary>
        /// Resets the victory condition state (for when entering a new room).
        /// </summary>
        public virtual void Reset()
        {
            // Override in derived classes if needed
        }
    }
}
