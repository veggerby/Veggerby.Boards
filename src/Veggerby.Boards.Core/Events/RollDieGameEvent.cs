using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Events
{
    public class RollDieGameEvent<T> : IGameEvent
    {
        public Die<T> Die { get; }
        public T NewDieValue { get; }

        public RollDieGameEvent(Die<T> die, T newDieValue)
        {
            Die = die;
            NewDieValue = newDieValue;
        }
    }
}