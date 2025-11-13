using System;
using System.Collections.Generic;
using System.Linq;

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
        if (!childConditions.All(x => x is not null))
        {
            throw new ArgumentException("Conditions cannot contain null", nameof(childConditions));
        }

        ChildConditions = [.. childConditions];
        CompositeMode = compositeMode;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        var results = ChildConditions.Select(x => x.Evaluate(engine, state, @event)).ToList();

        // Short-circuit: if all ignore, propagate ignore (rule not applicable for this event).
        if (results.All(x => x.Result == ConditionResult.Ignore))
        {
            return ConditionResponse.NotApplicable;
        }

        if (CompositeMode == CompositeMode.All)
        {
            // In ALL mode we now require every condition to be explicitly Valid.
            // Any Invalid fails. Any Ignore (with at least one Valid present, since all-ignore handled above)
            // yields a NotApplicable to suppress the rule (treat as silently ignored move attempt).
            if (results.Any(x => x.Result == ConditionResult.Invalid))
            {
                return ConditionResponse.Fail(results.Where(x => x.Result == ConditionResult.Invalid));
            }

            if (results.Any(x => x.Result == ConditionResult.Ignore))
            {
                return ConditionResponse.NotApplicable;
            }

            return ConditionResponse.Valid; // all valid
        }

        // ANY mode (legacy semantics retained): at least one valid => valid, otherwise if all ignore => not applicable, else fail.
        var anyValid = results.Any(x => x.Result == ConditionResult.Valid);
        if (anyValid)
        {
            return ConditionResponse.Valid;
        }
        if (results.All(x => x.Result == ConditionResult.Ignore))
        {
            return ConditionResponse.NotApplicable;
        }
        return ConditionResponse.Fail(results.Where(x => x.Result == ConditionResult.Invalid));
    }

    /// <summary>
    /// Creates (and flattens) a composite of supplied conditions.
    /// </summary>
    /// <param name="mode">Composition mode.</param>
    /// <param name="conditions">Conditions to combine.</param>
    /// <returns>Composite condition.</returns>
    internal static IGameEventCondition<T> CreateCompositeCondition(CompositeMode mode, params IGameEventCondition<T>[] conditions)
    {
        return new CompositeGameEventCondition<T>(
            conditions.SelectMany(x => x.IsCompositeCondition(mode) ? ((CompositeGameEventCondition<T>)x).ChildConditions : [x]),
            mode
        );
    }
}