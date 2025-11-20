namespace MutatingGambit.Core
{
    /// <summary>
    /// Cache for Position objects to reduce memory allocations
    /// </summary>
    public static class PositionCache
    {
        private static readonly Position[,] _cache = new Position[16, 16];

        static PositionCache()
        {
            // Pre-create positions for common board sizes (up to 16x16)
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    _cache[x, y] = new Position(x, y);
                }
            }
        }

        /// <summary>
        /// Gets a cached position or creates a new one if out of cache range
        /// </summary>
        public static Position Get(int x, int y)
        {
            if (x >= 0 && x < 16 && y >= 0 && y < 16)
            {
                return _cache[x, y];
            }
            return new Position(x, y);
        }
    }
}
