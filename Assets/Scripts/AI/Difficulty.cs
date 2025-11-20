namespace MutatingGambit.Core.AI
{
    /// <summary>
    /// AI difficulty levels
    /// </summary>
    public enum Difficulty
    {
        Easy,      // Depth 1, quick moves, occasional mistakes
        Normal,    // Depth 2, balanced play
        Hard,      // Depth 3, strong tactical play
        Master     // Depth 4+, optimal play
    }
}
