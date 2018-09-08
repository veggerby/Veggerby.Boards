using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class CompositeGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
    {
        public IEnumerable<IGameEventCondition<T>> ChildConditions { get; }
        public CompositeMode CompositeMode { get; }

        private CompositeGameEventCondition(IEnumerable<IGameEventCondition<T>> childConditions, CompositeMode compositeMode)
        {
            if (!childConditions.All(x => x != null))
            {
                throw new ArgumentException("Conditions cannot contain null", nameof(childConditions));
            }

            ChildConditions = childConditions.ToList();
            CompositeMode = compositeMode;
        }

        public ConditionResponse Evaluate(GameState state, T @event)
        {
            var results = ChildConditions.Select(x => x.Evaluate(state, @event));
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

        internal static IGameEventCondition<T> CreateCompositeCondition(CompositeMode mode, params IGameEventCondition<T>[] conditions)
        {
            return new CompositeGameEventCondition<T>(
                conditions.SelectMany(x => x.IsCompositeCondition(mode) ? ((CompositeGameEventCondition<T>)x).ChildConditions : new [] { x }),
                mode
            );
        }
    }
}