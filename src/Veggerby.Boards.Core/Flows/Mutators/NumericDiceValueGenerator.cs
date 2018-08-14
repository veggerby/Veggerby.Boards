using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public abstract class NumericDiceValueGenerator : IDiceValueGenerator<int>
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public NumericDiceValueGenerator(int minValue, int maxValue)
        {
            MinValue = Math.Min(minValue, maxValue);
            MaxValue = Math.Max(minValue, maxValue);
        }

        public abstract int GetValue(DiceState<int> currentState);
    }
}