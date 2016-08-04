using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class RandomDieValueGenerator : NumericDieValueGenerator
    {
        public override int GetValue(DieState<int> currentState)
        {
            return new Random().Next(MinValue, MaxValue);
        }

        public RandomDieValueGenerator(int minValue, int maxValue) : base(minValue, maxValue)
        {
        }
    }
}