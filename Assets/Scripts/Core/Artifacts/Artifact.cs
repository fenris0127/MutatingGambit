namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// Base class for global game artifacts
    /// </summary>
    public abstract class Artifact
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual int Cost => 100;

        public override string ToString()
        {
            return Name;
        }
    }
}
