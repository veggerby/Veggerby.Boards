using System;

using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

public class RandomDiceValueGenerator(int minValue, int maxValue) : NumericDiceValueGenerator(minValue, maxValue)
{
    private readonly Random _random = new();

    public override int GetValue(IArtifactState currentState)
    {
        return _random.Next(MinValue, MaxValue + 1);
    }
}