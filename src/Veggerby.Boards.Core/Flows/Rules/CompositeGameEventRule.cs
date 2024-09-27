using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class CompositeGameEventRule : IGameEventRule
    {
        public IEnumerable<IGameEventRule> Rules { get; }
        public CompositeMode CompositeMode { get; }

        private CompositeGameEventRule(IEnumerable<IGameEventRule> rules, CompositeMode compositeMode)
        {
            Rules = rules.ToList();
            CompositeMode = compositeMode;
        }

        private IDictionary<IGameEventRule, ConditionResponse> RunCompositeCheck(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            return Rules.ToDictionary(x => x, x => x.Check(engine, gameState, @event));
        }

        private ConditionResponse GetCompositeRuleCheckState(IDictionary<IGameEventRule, ConditionResponse> results)
        {
            var ignoreAll = results.All(x => x.Value.Result == ConditionResult.Ignore);

            if (ignoreAll)
            {
                return ConditionResponse.NotApplicable;
            }

            var compositionResult = CompositeMode == CompositeMode.All
                ? results.All(x => x.Value.Result != ConditionResult.Invalid) // allow ignore, there will be at least one valid, otherwise ignoreAll would be true
                : results.Any(x => x.Value.Result == ConditionResult.Valid);

            return compositionResult
                ? ConditionResponse.Valid
                : ConditionResponse.Fail(results.Select(x => x.Value).Where(x => x.Result == ConditionResult.Invalid));
        }

        public ConditionResponse Check(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            var results = RunCompositeCheck(engine, gameState, @event);
            return GetCompositeRuleCheckState(results);
        }

        public GameState HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            var results = RunCompositeCheck(engine, gameState, @event);
            var compositeResult = GetCompositeRuleCheckState(results);

            if (compositeResult.Result == ConditionResult.Valid)
            {
                // if mode is "any" only mutate state from the FIRST valid rule (top-down)

                if (CompositeMode == CompositeMode.Any)
                {
                    return results
                        .First(x => x.Value.Result == ConditionResult.Valid)
                        .Key
                        .HandleEvent(engine, gameState, @event);
                }

                // mode is "all" and all rules are valid, chain state mutators in order

                return results
                    .Aggregate(gameState, (state, rule) => rule.Key.HandleEvent(engine, state, @event));
            }
            else if (compositeResult.Result == ConditionResult.Ignore)
            {
                return gameState;
            }

            throw new BoardException("Invalid game event");
        }

        internal static IGameEventRule CreateCompositeRule(CompositeMode mode, params IGameEventRule[] rules)
        {
            return new CompositeGameEventRule(
                rules.SelectMany(x => x.IsCompositeRule(mode) ? ((CompositeGameEventRule)x).Rules : [x]),
                mode
            );
        }
    }
}