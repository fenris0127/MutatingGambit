namespace MutatingGambit.Core.Mutations
{
    /// <summary>
    /// Base class for piece mutations
    /// </summary>
    public abstract class Mutation
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual int Cost => 50;

        public override string ToString()
        {
            return Name;
        }
    }
}
