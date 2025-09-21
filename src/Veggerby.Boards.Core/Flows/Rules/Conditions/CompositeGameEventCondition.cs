using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

/// <summary>
/// Composes multiple event conditions using logical ALL / ANY semantics.
/// </summary>
/// <typeparam name="T">Event type.</typeparam>
public class CompositeGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
{
    /// <summary>
    /// Gets the child conditions.
    /// </summary>
    public IEnumerable<IGameEventCondition<T>> ChildConditions { get; }

    /// <summary>
    /// Gets the composite mode.
    /// </summary>
    public CompositeMode CompositeMode { get; }

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
        var results = ChildConditions.Select(x => x.Evaluate(engine, state, @event));
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