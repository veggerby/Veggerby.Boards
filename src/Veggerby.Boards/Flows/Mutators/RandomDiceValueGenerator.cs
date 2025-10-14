using Veggerby.Boards.States;

using SystemRandom = System.Random;

namespace Veggerby.Boards.Flows.Mutators;

#pragma warning disable RS0030 // Do not use banned APIs
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
#pragma warning restore RS0030 // Do not use banned APIs
public class RandomDiceValueGenerator(int minValue, int maxValue) : NumericDiceValueGenerator(minValue, maxValue)
{
#pragma warning disable RS0030 // Do not use banned APIs
    private readonly SystemRandom _random = new();
#pragma warning restore RS0030 // Do not use banned APIs

    /// <inheritdoc />
    public override int GetValue(IArtifactState currentState)
    {
#pragma warning disable RS0030 // Do not use banned APIs
        return _random.Next(MinValue, MaxValue + 1);
#pragma warning restore RS0030 // Do not use banned APIs
    }
}