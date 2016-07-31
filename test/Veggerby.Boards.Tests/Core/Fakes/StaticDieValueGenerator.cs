using Veggerby.Boards.Core;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class StaticDieValueGenerator : IDieValueGenerator<int>
    {
        private readonly int _value;

        public int GetValue(DieState<int> currentState)
        {
            return _value;
        }

        public StaticDieValueGenerator(int value)
        {
            _value = value;
        }
    }
}