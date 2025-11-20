using System.Collections.Generic;
using System.Linq;

namespace MutatingGambit.Core.Victory
{
    /// <summary>
    /// Logic for combining victory conditions
    /// </summary>
    public enum VictoryLogic
    {
        And,  // All conditions must be met
        Or    // Any condition must be met
    }

    /// <summary>
    /// Combines multiple victory conditions
    /// </summary>
    public class CompositeVictoryCondition : IVictoryCondition
    {
        private readonly VictoryLogic _logic;
        private readonly List<IVictoryCondition> _conditions;

        public CompositeVictoryCondition(VictoryLogic logic, params IVictoryCondition[] conditions)
        {
            _logic = logic;
            _conditions = new List<IVictoryCondition>(conditions);
        }

        public string GetDescription()
        {
            var separator = _logic == VictoryLogic.And ? " AND " : " OR ";
            return string.Join(separator, _conditions.Select(c => c.GetDescription()));
        }

        public bool IsMet(Board board)
        {
            if (_logic == VictoryLogic.And)
            {
                return _conditions.All(c => c.IsMet(board));
            }
            else
            {
                return _conditions.Any(c => c.IsMet(board));
            }
        }

        public void AddCondition(IVictoryCondition condition)
        {
            _conditions.Add(condition);
        }
    }
}
