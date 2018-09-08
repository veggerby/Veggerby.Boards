using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core.Builder.Phases
{
    internal class GamePhaseConditionDefinition : GamePhaseConditionDefinitionBase
    {
        public GamePhaseConditionDefinition(GameEngineBuilder builder, GameStateConditionFactory conditionFactory, IForGameEventRule parent) : base(builder, parent)
        {
            if (conditionFactory == null)
            {
                throw new ArgumentNullException(nameof(conditionFactory));
            }

            ConditionFactory = conditionFactory;
        }

        public GameStateConditionFactory ConditionFactory { get; }

        internal override IGameStateCondition Build(Game game)
        {
            return ConditionFactory(game);
        }
    }
}
