using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    internal class RuleResult
    {
        public RuleCheckState Check { get; }
        public GameState State { get; }

        public RuleResult(RuleCheckState check, GameState state)
        {
            Check = check;
            State = state;
        }
    }
}