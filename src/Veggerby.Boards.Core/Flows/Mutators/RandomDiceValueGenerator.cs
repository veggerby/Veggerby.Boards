using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class RandomDiceValueGenerator : NumericDiceValueGenerator
    {
        public override int GetValue(DiceState<int> currentState)
        {
            return new Random().Next(MinValue, MaxValue);
        }

        public RandomDiceValueGenerator(int minValue, int maxValue) : base(minValue, maxValue)
        {
        }
    }
}