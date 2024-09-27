using System;


using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingDiceValueGenerator : NumericDiceValueGenerator
    {
        public DoublingDiceValueGenerator() : base(1, 64)
        {
        }

        public override int GetValue(IArtifactState currentState)
        {
            var state = currentState as DiceState<int>;
            if (state == null || state.CurrentValue < MinValue || (state.CurrentValue & (state.CurrentValue - 1)) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentState), "Illegal dice value");
            }

            return state.CurrentValue < MaxValue
                ? 2 * state.CurrentValue
                : MaxValue;
        }
    }
}