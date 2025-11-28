using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Lightweight board state for AI simulation without GameObject overhead.
    /// This is a pure data structure that doesn't inherit from MonoBehaviour.
    /// </summary>
    public class BoardState : IBoard
    {
        private int width;
        private int height;
        private PieceData[,] pieces;
        private bool[,] obstacles;

        /// <summary>
        /// Lightweight piece data for simulation.
        /// </summary>
        public class PieceData : IPiece
        {
            public PieceType Type { get; set; }
            public Team Team { get; set; }
            public Vector2Int Position { get; set; }
            public List<MovementRule> MovementRules { get; set; }

            // IPiece implementation
            Team IPiece.Team => Team;
            PieceType IPiece.Type => Type;
            Vector2Int IPiece.Position => Position;

            public PieceData(PieceType type, Team team, Vector2Int position)
            {
                Type = type;
                Team = team;
                Position = position;
                MovementRules = new List<MovementRule>();
            }

            public PieceData(Piece piece)
            {
                Type = piece.Type;
                Team = piece.Team;
                Position = piece.Position;
                MovementRules = new List<MovementRule>(piece.MovementRules);
            }

            public PieceData Clone()
            {
                var clone = new PieceData(Type, Team, Position);
                clone.MovementRules = new List<MovementRule>(MovementRules);
                return clone;
            }
        }

        public int Width => width;
        public int Height => height;

        public BoardState(int width, int height)
        {
            this.width = width;
            this.height = height;
            pieces = new PieceData[width, height];
            obstacles = new bool[width, height];
        }

        /// <summary>
        /// Creates a BoardState from an existing Board.
        /// </summary>
        public static BoardState FromBoard(Board board)
        {
            var state = new BoardState(board.Width, board.Height);

            // Copy obstacles
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    state.obstacles[x, y] = board.HasObstacle(new Vector2Int(x, y));
                }
            }

            // Copy pieces
            var allPieces = board.GetAllPieces();
            foreach (var piece in allPieces)
            {
                if (piece != null)
                {
                    var pieceData = new PieceData(piece);
                    state.pieces[piece.Position.x, piece.Position.y] = pieceData;
                }
            }

            return state;
        }

        /// <summary>
        /// Creates a deep copy of this board state.
        /// </summary>
        public BoardState Clone()
        {
            var clone = new BoardState(width, height);

            // Copy obstacles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    clone.obstacles[x, y] = obstacles[x, y];
                    if (pieces[x, y] != null)
                    {
                        clone.pieces[x, y] = pieces[x, y].Clone();
                    }
                }
            }

            return clone;
        }

        public bool IsPositionValid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }

        public bool HasObstacle(Vector2Int pos)
        {
            if (!IsPositionValid(pos)) return true;
            return obstacles[pos.x, pos.y];
        }

        public PieceData GetPieceData(Vector2Int pos)
        {
            if (!IsPositionValid(pos)) return null;
            return pieces[pos.x, pos.y];
        }

        public bool IsOccupied(Vector2Int pos)
        {
            return GetPieceData(pos) != null;
        }

        public bool IsOccupiedByTeam(Vector2Int pos, Team team)
        {
            var piece = GetPieceData(pos);
            return piece != null && piece.Team == team;
        }

        /// <summary>
        /// Simulates a move on this board state.
        /// </summary>
        public bool SimulateMove(Vector2Int from, Vector2Int to)
        {
            if (!IsPositionValid(from) || !IsPositionValid(to))
                return false;

            var piece = GetPieceData(from);
            if (piece == null)
                return false;

            // Remove from old position
            pieces[from.x, from.y] = null;

            // Place at new position (removing any piece there)
            pieces[to.x, to.y] = piece;
            piece.Position = to;

            return true;
        }

        /// <summary>
        /// Gets all pieces on the board.
        /// </summary>
        public List<PieceData> GetAllPieceData()
        {
            var result = new List<PieceData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        result.Add(pieces[x, y]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all pieces belonging to a specific team.
        /// </summary>
        public List<PieceData> GetPiecesByTeam(Team team)
        {
            var result = new List<PieceData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null && pieces[x, y].Team == team)
                    {
                        result.Add(pieces[x, y]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all valid moves for a piece using its movement rules.
        /// </summary>
        public List<Vector2Int> GetValidMoves(PieceData piece)
        {
            var allMoves = new List<Vector2Int>();

            foreach (var rule in piece.MovementRules)
            {
                var moves = rule.GetValidMoves(this, piece.Position, piece.Team);
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

        // IBoard implementation (for movement rule compatibility)
        IPiece IBoard.GetPieceAt(Vector2Int position)
        {
            var pieceData = GetPieceData(position);
            if (pieceData == null) return null;

            // Return PieceData as IPiece (PieceData implements IPiece below)
            return pieceData;
        }

        bool IBoard.IsObstacle(Vector2Int position)
        {
            return HasObstacle(position);
        }
    }
}
