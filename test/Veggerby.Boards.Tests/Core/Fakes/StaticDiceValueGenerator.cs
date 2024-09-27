using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class StaticDiceValueGenerator : IDiceValueGenerator<int>
    {
        private readonly int _value;

        public int GetValue(IArtifactState currentState)
        {
            return _value;
        }

        public StaticDiceValueGenerator(int value)
        {
            _value = value;
        }
    }
}