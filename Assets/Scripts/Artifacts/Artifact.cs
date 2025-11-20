namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// When the artifact effect should trigger
    /// </summary>
    public enum ArtifactTrigger
    {
        OnTurnStart,
        OnTurnEnd,
        OnMove,
        OnCapture,
        OnPieceDestroyed
    }

    /// <summary>
    /// Base class for global game artifacts
    /// </summary>
    public abstract class Artifact
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual int Cost => 100;
        public virtual ArtifactTrigger Trigger => ArtifactTrigger.OnMove;
        public virtual int Priority => 5; // Higher priority executes first
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Applies the artifact's effect
        /// </summary>
        public virtual void ApplyEffect(Board board, ArtifactContext context)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Checks if this artifact allows an extra move
        /// </summary>
        public virtual bool AllowsExtraMove(Board board, ArtifactContext context)
        {
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

