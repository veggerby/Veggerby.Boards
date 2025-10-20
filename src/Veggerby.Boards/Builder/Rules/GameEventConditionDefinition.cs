using System;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Rules;

internal class GameEventConditionDefinition<T> : GameEventConditionDefinitionBase<T> where T : IGameEvent
{
    public GameEventConditionDefinition(GameBuilder builder, GameEventConditionFactory<T> conditionFactory, IThenStateMutator<T> parent) : base(builder, parent)
    {
        ArgumentNullException.ThrowIfNull(conditionFactory);

        ConditionFactory = conditionFactory;
    }

    public GameEventConditionFactory<T> ConditionFactory
    {
        get;
    }

    internal override IGameEventCondition<T> Build(Game game)
    {
        return ConditionFactory(game);
    }
}