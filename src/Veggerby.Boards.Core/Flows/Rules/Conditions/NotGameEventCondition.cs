using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class NotGameEventConditon<T> : IGameEventCondition<T> where T : IGameEvent
    {
        public NotGameEventConditon(IGameEventCondition<T> innerCondition)
        {
            if (innerCondition == null)
            {
                throw new ArgumentNullException(nameof(innerCondition));
            }

            InnerCondition = innerCondition;
        }

        public IGameEventCondition<T> InnerCondition { get; }

        public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
        {
            var innerResult = InnerCondition.Evaluate(engine, state, @event);
            switch (innerResult.Result)
            {
                case ConditionResult.Valid:
                    return ConditionResponse.Fail(innerResult.Reason);
                case ConditionResult.Ignore:
                    return innerResult;
                case ConditionResult.Invalid:
                    return ConditionResponse.Success(innerResult.Reason);
            }

            throw new BoardException("Invalid response");
        }
    }
}