using UnityEngine;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Extension methods and utilities for Vector2Int to work with board positions.
    /// </summary>
    public static class BoardPosition
    {
        /// <summary>
        /// Checks if a position is within the bounds of a board.
        /// </summary>
        public static bool IsWithinBounds(this Vector2Int position, int width, int height)
        {
            return position.x >= 0 && position.x < width &&
                   position.y >= 0 && position.y < height;
        }

        /// <summary>
        /// Converts a chess notation (e.g., "a1", "h8") to a Vector2Int position.
        /// </summary>
        /// <param name="notation">Chess notation string (e.g., "e4")</param>
        /// <returns>Vector2Int position where (0,0) is bottom-left</returns>
        public static Vector2Int FromNotation(string notation)
        {
            if (string.IsNullOrEmpty(notation) || notation.Length < 2)
            {
                return new Vector2Int(-1, -1);
            }

            char file = char.ToLower(notation[0]);
            char rank = notation[1];

            int x = file - 'a';
            int y = rank - '1';

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Converts a Vector2Int position to chess notation.
        /// </summary>
        /// <param name="position">Board position</param>
        /// <returns>Chess notation string (e.g., "e4")</returns>
        public static string ToNotation(this Vector2Int position)
        {
            if (position.x < 0 || position.x > 25 || position.y < 0 || position.y > 9)
            {
                return "??";
            }

            char file = (char)('a' + position.x);
            char rank = (char)('1' + position.y);

            return $"{file}{rank}";
        }

        /// <summary>
        /// Calculates the Manhattan distance between two positions.
        /// </summary>
        public static int ManhattanDistance(this Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        /// <summary>
        /// Calculates the Chebyshev distance (king moves) between two positions.
        /// </summary>
        public static int ChebyshevDistance(this Vector2Int from, Vector2Int to)
        {
            return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
        }

        /// <summary>
        /// Gets the direction vector from one position to another (normalized to -1, 0, or 1).
        /// </summary>
        public static Vector2Int GetDirection(this Vector2Int from, Vector2Int to)
        {
            int dx = to.x - from.x;
            int dy = to.y - from.y;

            int dirX = dx == 0 ? 0 : dx / Mathf.Abs(dx);
            int dirY = dy == 0 ? 0 : dy / Mathf.Abs(dy);

            return new Vector2Int(dirX, dirY);
        }

        /// <summary>
        /// Checks if two positions are on the same diagonal.
        /// </summary>
        public static bool IsDiagonal(this Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            return dx == dy && dx > 0;
        }

        /// <summary>
        /// Checks if two positions are on the same straight line (horizontal or vertical).
        /// </summary>
        public static bool IsStraightLine(this Vector2Int from, Vector2Int to)
        {
            return (from.x == to.x && from.y != to.y) || (from.y == to.y && from.x != to.x);
        }
    }
}
