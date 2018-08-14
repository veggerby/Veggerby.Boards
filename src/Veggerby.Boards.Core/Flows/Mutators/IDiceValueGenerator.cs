using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public interface IDiceValueGenerator<T>
    {
        T GetValue(DiceState<T> currentState);
    }
}