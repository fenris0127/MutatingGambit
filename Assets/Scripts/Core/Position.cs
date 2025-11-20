using System;
using System.Collections.Generic;

namespace MutatingGambit.Core
{
    /// <summary>
    /// Represents a position on the chess board
    /// </summary>
    public class Position : IEquatable<Position>
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Creates a position from chess notation (e.g., "e4")
        /// </summary>
        public static Position FromNotation(string notation)
        {
            if (string.IsNullOrEmpty(notation) || notation.Length < 2)
            {
                throw new ArgumentException("Invalid notation format", nameof(notation));
            }

            char file = char.ToLower(notation[0]);
            if (file < 'a' || file > 'h')
            {
                throw new ArgumentException("File must be between 'a' and 'h'", nameof(notation));
            }

            if (!int.TryParse(notation.Substring(1), out int rank) || rank < 1 || rank > 8)
            {
                throw new ArgumentException("Rank must be between 1 and 8", nameof(notation));
            }

            // Convert: a=0, b=1, etc. and rank 1=0, rank 2=1, etc.
            int x = file - 'a';
            int y = rank - 1;

            return new Position(x, y);
        }

        /// <summary>
        /// Converts position to chess notation (e.g., "e4")
        /// </summary>
        public string ToNotation()
        {
            if (X < 0 || X > 7 || Y < 0 || Y > 7)
            {
                throw new InvalidOperationException("Position is out of standard board bounds");
            }

            char file = (char)('a' + X);
            int rank = Y + 1;
            return $"{file}{rank}";
        }

        public bool Equals(Position other)
        {
            if (other == null) return false;
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(Position left, Position right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }
    }
}
