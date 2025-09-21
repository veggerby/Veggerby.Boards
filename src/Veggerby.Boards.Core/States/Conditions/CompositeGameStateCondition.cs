using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.States.Conditions;

/// <summary>
/// Composes multiple <see cref="IGameStateCondition"/> instances using a logical mode (<see cref="CompositeMode.All"/> or <see cref="CompositeMode.Any"/>).
/// </summary>
/// <remarks>
/// Ignore results propagate only if all children ignore. Nested composites with the same mode are flattened to avoid deep nesting.
/// </remarks>
public class CompositeGameStateCondition : IGameStateCondition
{
    /// <summary>
    /// Gets the child conditions participating in the composition.
    /// </summary>
    public IEnumerable<IGameStateCondition> ChildConditions { get; }

    /// <summary>
    /// Gets the composite mode governing evaluation semantics.
    /// </summary>
    public CompositeMode CompositeMode { get; }

    private CompositeGameStateCondition(IEnumerable<IGameStateCondition> childConditions, CompositeMode compositeMode)
    {
        if (!childConditions.All(x => x is not null))
        {
            throw new ArgumentException("Conditions cannot contain null", nameof(childConditions));
        }

        ChildConditions = [.. childConditions];
        CompositeMode = compositeMode;
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Creates a composite condition flattening nested composites that use the same mode.
    /// </summary>
    /// <param name="mode">Composition mode.</param>
    /// <param name="conditions">Component conditions.</param>
    /// <returns>Composite or simplified condition.</returns>
    internal static IGameStateCondition CreateCompositeCondition(CompositeMode mode, params IGameStateCondition[] conditions)
    {
        return new CompositeGameStateCondition(
            conditions.SelectMany(x => x.IsCompositeCondition(mode) ? ((CompositeGameStateCondition)x).ChildConditions : [x]),
            mode
        );
    }
}