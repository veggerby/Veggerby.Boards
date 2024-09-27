using System;

using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

public abstract class NumericDiceValueGenerator(int minValue, int maxValue) : IDiceValueGenerator<int>
{
    public int MinValue { get; } = Math.Min(minValue, maxValue);
    public int MaxValue { get; } = Math.Max(minValue, maxValue);

    public abstract int GetValue(IArtifactState currentState);
}