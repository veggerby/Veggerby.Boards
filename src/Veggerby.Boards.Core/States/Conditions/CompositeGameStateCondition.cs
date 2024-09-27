using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.States.Conditions
{
    public class CompositeGameStateCondition : IGameStateCondition
    {
        public IEnumerable<IGameStateCondition> ChildConditions { get; }
        public CompositeMode CompositeMode { get; }

        private CompositeGameStateCondition(IEnumerable<IGameStateCondition> childConditions, CompositeMode compositeMode)
        {
            if (!childConditions.All(x => x != null))
            {
                throw new ArgumentException("Conditions cannot contain null", nameof(childConditions));
            }

            ChildConditions = childConditions.ToList();
            CompositeMode = compositeMode;
        }

        public ConditionResponse Evaluate(GameState state)
        {
            var results = ChildConditions.Select(x => x.Evaluate(state));
            var ignoreAll = results.All(x => x.Result == ConditionResult.Ignore);

            if (ignoreAll)
            {
                return ConditionResponse.NotApplicable;
            }

            var compositionResult = CompositeMode == CompositeMode.All
                ? results.All(x => x.Result != ConditionResult.Invalid) // allow ignore, there will be at least one valid, otherwise ignoreAll would be true
                : results.Any(x => x.Result == ConditionResult.Valid);

            return compositionResult
                ? ConditionResponse.Valid
                : ConditionResponse.Fail(results.Where(x => x.Result == ConditionResult.Invalid));
        }

        internal static IGameStateCondition CreateCompositeCondition(CompositeMode mode, params IGameStateCondition[] conditions)
        {
            return new CompositeGameStateCondition(
                conditions.SelectMany(x => x.IsCompositeCondition(mode) ? ((CompositeGameStateCondition)x).ChildConditions : [x]),
                mode
            );
        }
    }
}