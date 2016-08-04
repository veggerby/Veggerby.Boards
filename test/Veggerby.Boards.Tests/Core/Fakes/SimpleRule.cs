using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class SimpleRule : IRule
    {
        private readonly RuleCheckState _check;
        private readonly Func<GameState, GameState> _stateFunc;

        public int CheckCallCount { get; private set; }
        public int EvaluateCallCount { get; private set; }

        public SimpleRule(RuleCheckState check, Func<GameState, GameState> stateFunc = null)
        {
            _check = check;
            _stateFunc = stateFunc;

            CheckCallCount = 0;
            EvaluateCallCount = 0;
        }

        public RuleCheckState Check(Game game, GameState currentState, IGameEvent @event)
        {
            CheckCallCount++;
            return _check;
        }

        public GameState Evaluate(Game game, GameState currentState, IGameEvent @event)
        {
            EvaluateCallCount++;
            return _stateFunc != null ? _stateFunc(currentState) : currentState;
        }
    }
}