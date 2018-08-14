using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Flows.Events
{
    public class RollDiceGameEvent<T> : IGameEvent
    {
        public Dice<T> Dice { get; }
        public T NewDiceValue { get; }

        public RollDiceGameEvent(Dice<T> dice, T newDiceValue)
        {
            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice));
            }

            if (newDiceValue == null)
            {
                throw new ArgumentNullException(nameof(newDiceValue));
            }

            Dice = dice;
            NewDiceValue = newDiceValue;
        }
    }
}