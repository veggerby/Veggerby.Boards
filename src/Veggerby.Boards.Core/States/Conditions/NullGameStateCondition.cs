namespace Veggerby.Boards.Core.States.Conditions
{
    public class NullGameStateCondition : IGameStateCondition
    {
        private readonly bool _result;

        public NullGameStateCondition(bool result = true)
        {
            _result = result;
        }

        public bool Evaluate(GameState state)
        {
            return _result;
        }
    }
}