namespace Veggerby.Boards.Core
{
    public abstract class NumericDieValueGenerator : IDieValueGenerator<int>
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public NumericDieValueGenerator(int minValue, int maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
        
        public abstract int GetValue();
    }
}