using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules;

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
    public IEnumerable<IGameEventRule> Rules
    {
        get;
    }

    /// <summary>
    /// Gets the composition mode.
    /// </summary>
    public CompositeMode CompositeMode
    {
        get;
    }

    private CompositeGameEventRule(IEnumerable<IGameEventRule> rules, CompositeMode compositeMode)
    {
        Rules = [.. rules];
        CompositeMode = compositeMode;
    }

    private IDictionary<IGameEventRule, ConditionResponse> RunCompositeCheck(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        var results = new Dictionary<IGameEventRule, ConditionResponse>();
        foreach (var rule in Rules)
        {
            results[rule] = rule.Check(engine, gameState, @event);
        }

        return results;
    }

    private ConditionResponse GetCompositeRuleCheckState(IDictionary<IGameEventRule, ConditionResponse> results)
    {
        // Check if all results are Ignore
        var ignoreAll = true;
        foreach (var result in results.Values)
        {
            if (result.Result != ConditionResult.Ignore)
            {
                ignoreAll = false;
                break;
            }
        }

        if (ignoreAll)
        {
            return ConditionResponse.NotApplicable;
        }

        // Evaluate composition result
        bool compositionResult;
        if (CompositeMode == CompositeMode.All)
        {
            // All mode: check that no result is Invalid
            compositionResult = true;
            foreach (var result in results.Values)
            {
                if (result.Result == ConditionResult.Invalid)
                {
                    compositionResult = false;
                    break;
                }
            }
        }
        else
        {
            // Any mode: check that at least one result is Valid
            compositionResult = false;
            foreach (var result in results.Values)
            {
                if (result.Result == ConditionResult.Valid)
                {
                    compositionResult = true;
                    break;
                }
            }
        }

        if (compositionResult)
        {
            return ConditionResponse.Valid;
        }

        // Collect failures
        var failures = new List<ConditionResponse>();
        foreach (var result in results.Values)
        {
            if (result.Result == ConditionResult.Invalid)
            {
                failures.Add(result);
            }
        }

        return ConditionResponse.Fail(failures);
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
                // Find first valid rule and handle event
                foreach (var kvp in results)
                {
                    if (kvp.Value.Result == ConditionResult.Valid)
                    {
                        return kvp.Key.HandleEvent(engine, gameState, @event);
                    }
                }
            }

            // All mode: apply each rule in sequence
            var currentState = gameState;
            foreach (var kvp in results)
            {
                currentState = kvp.Key.HandleEvent(engine, currentState, @event);
            }

            return currentState;
        }
        else if (compositeResult.Result == ConditionResult.Ignore)
        {
            return gameState;
        }

        throw new BoardException("Invalid game event");
    }

    internal static IGameEventRule CreateCompositeRule(CompositeMode mode, params IGameEventRule[] rules)
    {
        // Flatten: expand any composites of the same mode
        var flattened = new List<IGameEventRule>();
        foreach (var rule in rules)
        {
            if (rule.IsCompositeRule(mode))
            {
                var composite = (CompositeGameEventRule)rule;
                foreach (var child in composite.Rules)
                {
                    flattened.Add(child);
                }
            }
            else
            {
                flattened.Add(rule);
            }
        }

        return new CompositeGameEventRule(flattened, mode);
    }
}