namespace Veggerby.Boards.Core.States
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