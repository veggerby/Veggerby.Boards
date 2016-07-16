using Veggerby.Boards.Core;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingCubeValueGenerator : NumericDieValueGenerator
    {
        public DoublingCubeValueGenerator() : base(1, 64)
        {
        }
        
        public override int GetValue(DieState<int> currentState)
        {
            if (currentState.CurrentValue < MinValue || (currentState.CurrentValue & (currentState.CurrentValue - 1)) != 0)
            {
                throw new BoardException("Illegal die value");
            }

            return currentState.CurrentValue < MaxValue
                ? 2 * currentState.CurrentValue
                : MaxValue;
        }
    }
}