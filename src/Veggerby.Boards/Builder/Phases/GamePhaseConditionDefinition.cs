using System;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Builder.Phases;

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