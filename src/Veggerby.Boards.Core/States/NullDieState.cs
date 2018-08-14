using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class NullDiceState<T> : ArtifactState
    {
        public NullDiceState(Dice<T> dice) : base(dice)
        {
        }
    }
}