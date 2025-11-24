using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Composes multiple event conditions using logical ALL / ANY semantics.
/// </summary>
/// <typeparam name="T">Event type.</typeparam>
public class CompositeGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
{
    /// <summary>
    /// Gets the child conditions.
    /// </summary>
    public IEnumerable<IGameEventCondition<T>> ChildConditions
    {
        get;
    }

    /// <summary>
    /// Gets the composite mode.
    /// </summary>
    public CompositeMode CompositeMode
    {
        get;
    }

    private CompositeGameEventCondition(IEnumerable<IGameEventCondition<T>> childConditions, CompositeMode compositeMode)
    {
        // Validate: no null conditions
        foreach (var condition in childConditions)
        {
            if (condition is null)
            {
                throw new ArgumentException("Conditions cannot contain null", nameof(childConditions));
            }
        }

        ChildConditions = [.. childConditions];
        CompositeMode = compositeMode;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        // Evaluate all child conditions
        var results = new List<ConditionResponse>();
        foreach (var condition in ChildConditions)
        {
            results.Add(condition.Evaluate(engine, state, @event));
        }

        // Short-circuit: if all ignore, propagate ignore (rule not applicable for this event).
        var allIgnore = true;
        foreach (var result in results)
        {
            if (result.Result != ConditionResult.Ignore)
            {
                allIgnore = false;
                break;
            }
        }

        if (allIgnore)
        {
            return ConditionResponse.NotApplicable;
        }

        if (CompositeMode == CompositeMode.All)
        {
            // In ALL mode we now require every condition to be explicitly Valid.
            // Any Invalid fails. Any Ignore (with at least one Valid present, since all-ignore handled above)
            // yields a NotApplicable to suppress the rule (treat as silently ignored move attempt).
            var hasInvalid = false;
            var hasIgnore = false;
            var failures = new List<ConditionResponse>();

            foreach (var result in results)
            {
                if (result.Result == ConditionResult.Invalid)
                {
                    hasInvalid = true;
                    failures.Add(result);
                }
                else if (result.Result == ConditionResult.Ignore)
                {
                    hasIgnore = true;
                }
            }

            if (hasInvalid)
            {
                return ConditionResponse.Fail(failures);
            }

            if (hasIgnore)
            {
                return ConditionResponse.NotApplicable;
            }

            return ConditionResponse.Valid; // all valid
        }

        // ANY mode (legacy semantics retained): at least one valid => valid, otherwise if all ignore => not applicable, else fail.
        var anyValid = false;
        var failures2 = new List<ConditionResponse>();

        foreach (var result in results)
        {
            if (result.Result == ConditionResult.Valid)
            {
                anyValid = true;
            }
            else if (result.Result == ConditionResult.Invalid)
            {
                failures2.Add(result);
            }
        }

        if (anyValid)
        {
            return ConditionResponse.Valid;
        }

        if (allIgnore)
        {
            return ConditionResponse.NotApplicable;
        }

        return ConditionResponse.Fail(failures2);
    }

    /// <summary>
    /// Creates (and flattens) a composite of supplied conditions.
    /// </summary>
    /// <param name="mode">Composition mode.</param>
    /// <param name="conditions">Conditions to combine.</param>
    /// <returns>Composite condition.</returns>
    internal static IGameEventCondition<T> CreateCompositeCondition(CompositeMode mode, params IGameEventCondition<T>[] conditions)
    {
        // Flatten: expand any composites of the same mode
        var flattened = new List<IGameEventCondition<T>>();
        foreach (var condition in conditions)
        {
            if (condition.IsCompositeCondition(mode))
            {
                var composite = (CompositeGameEventCondition<T>)condition;
                foreach (var child in composite.ChildConditions)
                {
                    flattened.Add(child);
                }
            }
            else
            {
                flattened.Add(condition);
            }
        }

        return new CompositeGameEventCondition<T>(flattened, mode);
    }
}