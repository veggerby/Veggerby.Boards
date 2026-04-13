using System;
using System.Collections.Generic;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Composes multiple <see cref="IGameStateCondition"/> instances using a logical mode (<see cref="CompositeMode.All"/> or <see cref="CompositeMode.Any"/>).
/// </summary>
/// <remarks>
/// Ignore results propagate only if all children ignore. Nested composites with the same mode are flattened to avoid deep nesting.
/// </remarks>
public class CompositeGameStateCondition : IGameStateCondition
{
    private readonly IReadOnlyList<IGameStateCondition> _childConditions;

    /// <summary>
    /// Gets the child conditions participating in the composition.
    /// </summary>
    /// <remarks>
    /// The return type is <see cref="IEnumerable{T}"/> to preserve binary compatibility with prior versions.
    /// The backing store is a fixed-size list; callers requiring count or index access should cast or iterate.
    /// </remarks>
    public IEnumerable<IGameStateCondition> ChildConditions => _childConditions;

    /// <summary>
    /// Gets the composite mode governing evaluation semantics.
    /// </summary>
    public CompositeMode CompositeMode
    {
        get;
    }

    private CompositeGameStateCondition(IEnumerable<IGameStateCondition> childConditions, CompositeMode compositeMode)
    {
        foreach (var condition in childConditions)
        {
            if (condition is null)
            {
                throw new ArgumentException("Conditions cannot contain null", nameof(childConditions));
            }
        }

        _childConditions = [.. childConditions];
        CompositeMode = compositeMode;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        // Use private backing list for count/index access — avoids allocation on every Evaluate call.
        var children = _childConditions;
        var results = new ConditionResponse[children.Count];

        for (var i = 0; i < children.Count; i++)
        {
            results[i] = children[i].Evaluate(state);
        }

        var ignoreAll = true;

        for (var i = 0; i < results.Length; i++)
        {
            if (results[i].Result != ConditionResult.Ignore)
            {
                ignoreAll = false;
                break;
            }
        }

        if (ignoreAll)
        {
            return ConditionResponse.NotApplicable;
        }

        bool compositionResult;

        if (CompositeMode == CompositeMode.All)
        {
            // All results must be non-invalid (allow ignore; ignoreAll was false so at least one is valid).
            compositionResult = true;

            for (var i = 0; i < results.Length; i++)
            {
                if (results[i].Result == ConditionResult.Invalid)
                {
                    compositionResult = false;
                    break;
                }
            }
        }
        else
        {
            // Any result must be valid.
            compositionResult = false;

            for (var i = 0; i < results.Length; i++)
            {
                if (results[i].Result == ConditionResult.Valid)
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

        var failures = new List<ConditionResponse>();

        for (var i = 0; i < results.Length; i++)
        {
            if (results[i].Result == ConditionResult.Invalid)
            {
                failures.Add(results[i]);
            }
        }

        return ConditionResponse.Fail(failures);
    }

    /// <summary>
    /// Creates a composite condition flattening nested composites that use the same mode.
    /// </summary>
    /// <param name="mode">Composition mode.</param>
    /// <param name="conditions">Component conditions.</param>
    /// <returns>Composite or simplified condition.</returns>
    internal static IGameStateCondition CreateCompositeCondition(CompositeMode mode, params IGameStateCondition[] conditions)
    {
        var flattened = new List<IGameStateCondition>();

        foreach (var condition in conditions)
        {
            if (condition.IsCompositeCondition(mode))
            {
                foreach (var child in ((CompositeGameStateCondition)condition)._childConditions)
                {
                    flattened.Add(child);
                }
            }
            else
            {
                flattened.Add(condition);
            }
        }

        return new CompositeGameStateCondition(flattened, mode);
    }
}