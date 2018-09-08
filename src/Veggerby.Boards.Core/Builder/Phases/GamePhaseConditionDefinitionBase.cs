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
    internal abstract class GamePhaseConditionDefinitionBase : DefinitionBase
    {
        public GamePhaseConditionDefinitionBase(GameEngineBuilder builder, IForGameEventRule parent) : base(builder)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        public IForGameEventRule Parent { get; }

        internal abstract IGameStateCondition Build(Game game);
    }
}
