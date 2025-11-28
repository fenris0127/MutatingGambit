using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Represents a chess piece with dynamic movement rules.
    /// Implements the Strategy Pattern to allow runtime rule modification.
    /// </summary>
    public class Piece : MonoBehaviour, IPiece
    {
        [SerializeField]
        private PieceType pieceType;

        [SerializeField]
        private Team team;

        [SerializeField]
        private Vector2Int position;

        [SerializeField]
        private List<MovementRule> movementRules = new List<MovementRule>();

        /// <summary>
        /// Gets the type of this piece.
        /// </summary>
        public PieceType Type => pieceType;

        /// <summary>
        /// Gets the team this piece belongs to.
        /// </summary>
        public Team Team => team;

        /// <summary>
        /// Gets or sets the current position of this piece on the board.
        /// </summary>
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Gets the list of movement rules applied to this piece.
        /// </summary>
        public List<MovementRule> MovementRules => movementRules;

        /// <summary>
        /// Initializes the piece with type and team.
        /// </summary>
        public void Initialize(PieceType type, Team pieceTeam, Vector2Int startPosition)
        {
            pieceType = type;
            team = pieceTeam;
            position = startPosition;
            movementRules.Clear();
        }

        /// <summary>
        /// Adds a movement rule to this piece.
        /// </summary>
        public void AddMovementRule(MovementRule rule)
        {
            if (rule != null && !movementRules.Contains(rule))
            {
                movementRules.Add(rule);
            }
        }

        /// <summary>
        /// Removes a movement rule from this piece.
        /// </summary>
        public void RemoveMovementRule(MovementRule rule)
        {
            movementRules.Remove(rule);
        }

        /// <summary>
        /// Clears all movement rules from this piece.
        /// </summary>
        public void ClearMovementRules()
        {
            movementRules.Clear();
        }

        /// <summary>
        /// Promotes this piece to a Queen.
        /// Uses MovementRuleFactory to prevent memory leaks.
        /// </summary>
        public void PromoteToQueen()
        {
            pieceType = PieceType.Queen;
            movementRules.Clear();
            
            // Add Queen rules using factory to prevent memory leaks
            var factory = MovementRules.MovementRuleFactory.Instance;
            var queenRules = factory.GetQueenRules();
            foreach (var rule in queenRules)
            {
                AddMovementRule(rule);
            }
            
            Debug.Log($"{team} Piece at {position} promoted to Queen!");
        }

        /// <summary>
        /// Checks if this piece has any mutations (more than standard rules).
        /// </summary>
        public bool HasMutations()
        {
            // A piece is considered mutated if it has unusual rule combinations
            // This is a simple heuristic for now
            return movementRules.Count > GetStandardRuleCount(pieceType);
        }

        /// <summary>
        /// Gets the standard number of movement rules for a piece type.
        /// </summary>
        private int GetStandardRuleCount(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn:
                    return 1; // Special pawn rule
                case PieceType.Knight:
                    return 1; // Knight jump
                case PieceType.Bishop:
                    return 1; // Diagonal
                case PieceType.Rook:
                    return 1; // Straight line
                case PieceType.Queen:
                    return 2; // Diagonal + Straight line
                case PieceType.King:
                    return 1; // King step
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Gets all valid moves for this piece on the given board.
        /// </summary>
        public List<Vector2Int> GetValidMoves(IBoard board)
        {
            var allMoves = new List<Vector2Int>();

            foreach (var rule in movementRules)
            {
                var moves = rule.GetValidMoves(board, position, team);
                foreach (var move in moves)
                {
                    if (!allMoves.Contains(move))
                    {
                        allMoves.Add(move);
                    }
                }
            }

            return allMoves;
        }

        /// <summary>
        /// Returns a string representation of this piece for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{team} {pieceType} at {position.ToNotation()}";
        }
    }
}
