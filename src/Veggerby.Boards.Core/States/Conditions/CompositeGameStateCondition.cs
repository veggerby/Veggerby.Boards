using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.States.Conditions
{
    public enum CompositeMode
    {
        Any,
        All
    }

    public class CompositeGameStateCondition : IGameStateCondition
    {
        public IEnumerable<IGameStateCondition> ChildConditions { get; }
        public CompositeMode CompositeMode { get; }

        private CompositeGameStateCondition(IEnumerable<IGameStateCondition> childConditions, CompositeMode compositeMode)
        {
            if (childConditions == null)
            {
                throw new System.ArgumentNullException(nameof(childConditions));
            }

            ChildConditions = childConditions.ToList();
            CompositeMode = compositeMode;
        }

        public bool Evaluate(GameState state)
        {
            return CompositeMode == CompositeMode.All
                ? ChildConditions.All(x => x.Evaluate(state))
                : ChildConditions.Any(x => x.Evaluate(state));
        }

        internal static IGameStateCondition CreateCompositeCondition(CompositeMode mode, params IGameStateCondition[] conditions)
        {
            return new CompositeGameStateCondition(
                conditions.SelectMany(x => x.IsCompositeCondition(mode) ? ((CompositeGameStateCondition)x).ChildConditions : new [] { x }),
                mode
            );
        }
    }
}