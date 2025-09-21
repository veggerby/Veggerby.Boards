using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules;

/// <summary>
/// Represents a composite rule aggregating multiple child <see cref="IGameEventRule"/> instances using a <see cref="CompositeMode"/> policy.
/// </summary>
/// <remarks>
/// Composition flattens nested composites of the same mode to reduce depth and evaluation overhead.
/// In ANY mode only the first valid rule mutates state; in ALL mode each valid rule mutates state in sequence.
/// </remarks>
public class CompositeGameEventRule : IGameEventRule
{
    /// <summary>
    /// Gets the child rules participating in this composite.
    /// </summary>
    public IEnumerable<IGameEventRule> Rules { get; }

    /// <summary>
    /// Gets the composition mode.
    /// </summary>
    public CompositeMode CompositeMode { get; }

    private CompositeGameEventRule(IEnumerable<IGameEventRule> rules, CompositeMode compositeMode)
    {
        Rules = [.. rules];
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
            ? results.All(x => x.Value.Result != ConditionResult.Invalid)
            : results.Any(x => x.Value.Result == ConditionResult.Valid);

        return compositionResult
            ? ConditionResponse.Valid
            : ConditionResponse.Fail(results.Select(x => x.Value).Where(x => x.Result == ConditionResult.Invalid));
    }

    /// <inheritdoc />
    public ConditionResponse Check(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        var results = RunCompositeCheck(engine, gameState, @event);
        return GetCompositeRuleCheckState(results);
    }

    /// <inheritdoc />
    public GameState HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        var results = RunCompositeCheck(engine, gameState, @event);
        var compositeResult = GetCompositeRuleCheckState(results);

        if (compositeResult.Result == ConditionResult.Valid)
        {
            if (CompositeMode == CompositeMode.Any)
            {
                return results
                    .First(x => x.Value.Result == ConditionResult.Valid)
                    .Key
                    .HandleEvent(engine, gameState, @event);
            }

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