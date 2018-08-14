using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class DiceState<T> : ArtifactState
    {
        public T CurrentValue { get; }

        public DiceState(Dice<T> dice, T currentValue) : base(dice)
        {
            if (currentValue == null)
            {
                throw new ArgumentNullException(nameof(currentValue));
            }

            CurrentValue = currentValue;
        }
    }
}