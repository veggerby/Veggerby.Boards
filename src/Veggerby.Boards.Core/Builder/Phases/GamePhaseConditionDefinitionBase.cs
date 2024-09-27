using System;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder.Phases
{
    internal abstract class GamePhaseConditionDefinitionBase : DefinitionBase
    {
        public GamePhaseConditionDefinitionBase(GameBuilder builder, IThenGameEventRule parent) : base(builder)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        public IThenGameEventRule Parent { get; }

        internal abstract IGameStateCondition Build(Game game);
    }
}
