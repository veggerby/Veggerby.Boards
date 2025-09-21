using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Fakes;

public class StaticDiceValueGenerator(int value) : IDiceValueGenerator<int>
{
    private readonly int _value = value;

    public int GetValue(IArtifactState currentState)
    {
        return _value;
    }
}