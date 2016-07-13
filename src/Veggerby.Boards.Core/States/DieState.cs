using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class DieState<T> : State<Die<T>>
    {
        public T CurrentValue { get; }

        public DieState(Die<T> die, T currentValue) : base(die)
        {
            CurrentValue = currentValue;
        }
    }
}