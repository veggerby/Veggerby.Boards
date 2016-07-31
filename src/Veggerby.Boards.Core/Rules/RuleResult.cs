using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    internal class RuleResult
    {
        public bool Check { get; }
        public GameState State { get; }

        public RuleResult(bool check, GameState state)
        {
            Check = check;
            State = state;
        }
    }
}