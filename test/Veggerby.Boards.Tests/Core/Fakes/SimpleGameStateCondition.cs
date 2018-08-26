using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class SimpleGameStateCondition : IGameStateCondition
    {
        private readonly bool _result;

        public SimpleGameStateCondition(bool result = true)
        {
            _result = result;
        }

        public bool Evaluate(GameState state)
        {
            return _result;
        }
    }
}