namespace MutatingGambit.Core.Victory
{
    /// <summary>
    /// Interface for victory conditions
    /// </summary>
    public interface IVictoryCondition
    {
        string GetDescription();
        bool IsMet(Board board);
    }
}
