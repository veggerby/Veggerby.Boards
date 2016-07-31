using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public abstract class NumericDieValueGenerator : IDieValueGenerator<int>
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public NumericDieValueGenerator(int minValue, int maxValue)
        {
            MinValue = Math.Min(minValue, maxValue);
            MaxValue = Math.Max(minValue, maxValue);
        }
        
        public abstract int GetValue(DieState<int> currentState);
    }
}