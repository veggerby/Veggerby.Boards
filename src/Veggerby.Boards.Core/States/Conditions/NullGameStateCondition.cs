namespace Veggerby.Boards.Core.States.Conditions
{
    public class NullGameStateCondition : IGameStateCondition
    {
        private readonly bool _result;

        public NullGameStateCondition() : this(true) // needed for initializer
        {
        }

        public NullGameStateCondition(bool result)
        {
            _result = result;
        }

        public ConditionResponse Evaluate(GameState state)
        {
            return _result ? ConditionResponse.Valid : ConditionResponse.Invalid;
        }
    }
}