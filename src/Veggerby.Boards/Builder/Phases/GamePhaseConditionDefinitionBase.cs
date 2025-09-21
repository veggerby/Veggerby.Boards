using System;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Builder.Phases;

internal abstract class GamePhaseConditionDefinitionBase : DefinitionBase
{
    public GamePhaseConditionDefinitionBase(GameBuilder builder, IThenGameEventRule parent) : base(builder)
    {
        ArgumentNullException.ThrowIfNull(parent);

        Parent = parent;
    }

    public IThenGameEventRule Parent { get; }

    internal abstract IGameStateCondition Build(Game game);
}