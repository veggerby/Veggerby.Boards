using System;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder.Phases;

internal class GamePhaseConditionDefinition : GamePhaseConditionDefinitionBase
{
    public GamePhaseConditionDefinition(GameBuilder builder, GameStateConditionFactory conditionFactory, IThenGameEventRule parent) : base(builder, parent)
    {
        ArgumentNullException.ThrowIfNull(conditionFactory);

        ConditionFactory = conditionFactory;
    }

    public GameStateConditionFactory ConditionFactory { get; }

    internal override IGameStateCondition Build(Game game)
    {
        return ConditionFactory(game);
    }
}