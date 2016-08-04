using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public interface IDieValueGenerator<T>
    {
        T GetValue(DieState<T> currentState);
    }
}