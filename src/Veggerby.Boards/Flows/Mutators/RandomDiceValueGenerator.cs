using System;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Generates pseudo-random integer values within a configured range.
/// </summary>
/// <remarks>
/// Uses <see cref="System.Random"/> without additional seeding; therefore deterministic replay is not guaranteed.
/// For deterministic tests replace with a custom implementation of <see cref="IDiceValueGenerator{T}"/> or
/// a test-specific subclass of <see cref="NumericDiceValueGenerator"/>.
/// </remarks>
/// <param name="minValue">Inclusive lower bound.</param>
/// <param name="maxValue">Inclusive upper bound.</param>
public class RandomDiceValueGenerator(int minValue, int maxValue) : NumericDiceValueGenerator(minValue, maxValue)
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public override int GetValue(IArtifactState currentState)
    {
        return _random.Next(MinValue, MaxValue + 1);
    }
}