using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingCubeValueGenerator : NumericDiceValueGenerator
    {
        public DoublingCubeValueGenerator() : base(1, 64)
        {
        }

        public override int GetValue(DiceState<int> currentState)
        {
            if (currentState.CurrentValue < MinValue || (currentState.CurrentValue & (currentState.CurrentValue - 1)) != 0)
            {
                throw new BoardException("Illegal dice value");
            }

            return currentState.CurrentValue < MaxValue
                ? 2 * currentState.CurrentValue
                : MaxValue;
        }
    }
}