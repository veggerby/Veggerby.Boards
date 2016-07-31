using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Events
{
    public class RollDieGameEvent<T> : IGameEvent
    {
        public Die<T> Die { get; }
        public T NewDieValue { get; }

        public RollDieGameEvent(Die<T> die, T newDieValue)
        {
            if (die == null)
            {
                throw new ArgumentNullException(nameof(die));
            }

            if (newDieValue == null)
            {
                throw new ArgumentNullException(nameof(newDieValue));
            }

            Die = die;
            NewDieValue = newDieValue;
        }
    }
}