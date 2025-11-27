using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// Abstract base class for all movement rules.
    /// Movement rules define how pieces can move on the board.
    /// This uses the Strategy Pattern to allow dynamic rule modification.
    /// </summary>
    public abstract class MovementRule : ScriptableObject
    {
        [SerializeField]
        private string ruleName;

        [SerializeField]
        [TextArea(2, 4)]
        private string ruleDescription;

        /// <summary>
        /// Gets the name of this movement rule.
        /// </summary>
        public string RuleName => ruleName;

        /// <summary>
        /// Gets the description of this movement rule.
        /// </summary>
        public string RuleDescription => ruleDescription;

        /// <summary>
        /// Calculates all valid moves for a piece at the given position.
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="fromPosition">The position of the piece</param>
        /// <param name="pieceTeam">The team of the piece</param>
        /// <returns>List of valid destination positions</returns>
        public abstract List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            ChessEngine.Team pieceTeam
        );

        /// <summary>
        /// Helper method to check if a position contains an enemy piece.
        /// </summary>
        protected bool IsEnemyPiece(IBoard board, Vector2Int position, ChessEngine.Team pieceTeam)
        {
            var piece = board.GetPieceAt(position);
            return piece != null && piece.Team != pieceTeam;
        }

        /// <summary>
        /// Helper method to check if a position contains a friendly piece.
        /// </summary>
        protected bool IsFriendlyPiece(IBoard board, Vector2Int position, ChessEngine.Team pieceTeam)
        {
            var piece = board.GetPieceAt(position);
            return piece != null && piece.Team == pieceTeam;
        }

        /// <summary>
        /// Helper method to check if a position is empty.
        /// </summary>
        protected bool IsEmptyPosition(IBoard board, Vector2Int position)
        {
            return board.GetPieceAt(position) == null;
        }
    }

    /// <summary>
    /// Interface for board state access.
    /// This allows MovementRules to query the board without tight coupling.
    /// </summary>
    public interface IBoard
    {
        int Width { get; }
        int Height { get; }
        IPiece GetPieceAt(Vector2Int position);
        bool IsPositionValid(Vector2Int position);
        bool IsObstacle(Vector2Int position);
    }

    /// <summary>
    /// Interface for piece information.
    /// </summary>
    public interface IPiece
    {
        ChessEngine.Team Team { get; }
        ChessEngine.PieceType Type { get; }
        Vector2Int Position { get; }
    }
}
