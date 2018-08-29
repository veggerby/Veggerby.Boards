using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class RandomDiceValueGenerator : NumericDiceValueGenerator
    {
        private readonly Random _random;

        public override int GetValue(IArtifactState currentState)
        {
            return _random.Next(MinValue, MaxValue + 1);
        }

        public RandomDiceValueGenerator(int minValue, int maxValue) : base(minValue, maxValue)
        {
            _random = new Random();
        }
    }
}