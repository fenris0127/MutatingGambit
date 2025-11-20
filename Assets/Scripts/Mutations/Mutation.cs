using MutatingGambit.Core.Movement;

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

        /// <summary>
        /// Applies this mutation to a move rule
        /// </summary>
        public virtual IMoveRule ApplyToMoveRule(IMoveRule baseRule, Piece piece)
        {
            return baseRule; // Default: no modification
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
