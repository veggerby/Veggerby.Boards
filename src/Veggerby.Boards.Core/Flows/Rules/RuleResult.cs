using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    internal class RuleResult
    {
        public RuleCheckState Check { get; }
        public GameState State { get; }

        public RuleResult(RuleCheckState check, GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            Check = check;
            State = state;
        }
    }
}