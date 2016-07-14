using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class DoublingCubeValueGenerator : NumericDieValueGenerator
    {
        public DoublingCubeValueGenerator() : base(1, 64)
        {
        }
        
        public override int GetValue(DieState<int> currentState)
        {
            return 2 * currentState.CurrentValue;
        }
    }
}