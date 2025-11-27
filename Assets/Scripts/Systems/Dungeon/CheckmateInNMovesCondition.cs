using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Victory condition: Achieve checkmate within N moves.
    /// </summary>
    [CreateAssetMenu(fileName = "CheckmateInNMoves", menuName = "Victory Conditions/Checkmate in N Moves")]
    public class CheckmateInNMovesCondition : VictoryCondition
    {
        [Header("Move Limit")]
        [SerializeField]
        [Tooltip("Maximum number of moves allowed to achieve checkmate.")]
        private int maxMoves = 5;

        private int movesTaken = 0;

        /// <summary>
        /// Gets the maximum number of moves allowed.
        /// </summary>
        public int MaxMoves => maxMoves;

        /// <summary>
        /// Gets the number of moves taken so far.
        /// </summary>
        public int MovesTaken => movesTaken;

        public override bool IsVictoryAchieved(Board board, int currentTurn, Team playerTeam)
        {
            // Check if enemy king is in checkmate
            Team enemyTeam = playerTeam == Team.White ? Team.Black : Team.White;

            // Find enemy king
            var enemyPieces = board.GetPiecesByTeam(enemyTeam);
            Piece enemyKing = null;

            foreach (var piece in enemyPieces)
            {
                if (piece.Type == PieceType.King)
                {
                    enemyKing = piece;
                    break;
                }
            }

            if (enemyKing == null)
            {
                // King was captured - victory!
                return true;
            }

            // Check if enemy king is in check
            bool isInCheck = MoveValidator.IsPositionUnderAttack(board, enemyKing.Position, playerTeam);

            if (!isInCheck)
            {
                return false; // Not even in check
            }

            // Check if enemy has any valid moves (if not, it's checkmate)
            bool hasValidMoves = MoveValidator.HasValidMoves(board, enemyTeam);

            return !hasValidMoves; // Checkmate if no valid moves while in check
        }

        public override bool IsDefeatConditionMet(Board board, int currentTurn, Team playerTeam)
        {
            // Failed if exceeded move limit without achieving checkmate
            return movesTaken >= maxMoves && !IsVictoryAchieved(board, currentTurn, playerTeam);
        }

        public override string GetProgressString(Board board, int currentTurn, Team playerTeam)
        {
            return $"Moves: {movesTaken}/{maxMoves}";
        }

        /// <summary>
        /// Call this when the player makes a move.
        /// </summary>
        public void IncrementMoves()
        {
            movesTaken++;
        }

        public override void Reset()
        {
            movesTaken = 0;
        }
    }
}
