using System;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Base class for dice value generators that produce bounded integer results.
/// </summary>
/// <param name="minValue">Inclusive lower bound of the generated value range.</param>
/// <param name="maxValue">Inclusive upper bound of the generated value range.</param>
public abstract class NumericDiceValueGenerator(int minValue, int maxValue) : IDiceValueGenerator<int>
{
    /// <summary>
    /// Gets the inclusive lower bound of the allowed value range.
    /// </summary>
    public int MinValue { get; } = Math.Min(minValue, maxValue);

    /// <summary>
    /// Gets the inclusive upper bound of the allowed value range.
    /// </summary>
    public int MaxValue { get; } = Math.Max(minValue, maxValue);

    /// <inheritdoc />
    public abstract int GetValue(IArtifactState currentState);
}