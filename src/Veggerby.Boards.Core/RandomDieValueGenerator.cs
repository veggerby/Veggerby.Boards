using System;

namespace Veggerby.Boards.Core
{
    public class RandomDieValueGenerator : NumericDieValueGenerator
    {
        public override int GetValue()
        {
            return new Random().Next(MinValue, MaxValue);
        }

        public RandomDieValueGenerator(int minValue, int maxValue) : base(minValue, maxValue)
        {
        }
    }
}