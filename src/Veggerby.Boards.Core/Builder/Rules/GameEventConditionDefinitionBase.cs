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
    internal abstract class GameEventConditionDefinitionBase<T> : DefinitionBase where T : IGameEvent
    {
        public GameEventConditionDefinitionBase(GameEngineBuilder builder, IThenStateMutator<T> parent) : base(builder)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        public IThenStateMutator<T> Parent { get; }

        internal abstract IGameEventCondition<T> Build(Game game);
    }
}
