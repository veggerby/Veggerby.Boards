using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.Flows.Rules.Conditions;

namespace Veggerby.Boards.Core.Builder.Rules
{
    internal class GameEventConditionDefinition<T> : GameEventConditionDefinitionBase<T> where T : IGameEvent
    {
        public GameEventConditionDefinition(GameBuilder builder, GameEventConditionFactory<T> conditionFactory, IThenStateMutator<T> parent) : base(builder, parent)
        {
            if (conditionFactory == null)
            {
                throw new ArgumentNullException(nameof(conditionFactory));
            }

            ConditionFactory = conditionFactory;
        }

        public GameEventConditionFactory<T> ConditionFactory { get; }

        internal override IGameEventCondition<T> Build(Game game)
        {
            return ConditionFactory(game);
        }
    }
}
