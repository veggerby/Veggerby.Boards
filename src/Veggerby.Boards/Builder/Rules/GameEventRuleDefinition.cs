using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Rules;

internal class GameEventRuleDefinition<T>(GameBuilder builder, IGameEventRuleDefinitions parent) : DefinitionBase(builder), IGameEventRuleDefinition, IGameEventRuleDefinition<T> where T : IGameEvent
{
    private readonly IGameEventRuleDefinitions _parent = parent;
    private CompositeGameEventConditionDefinition<T>? _conditionDefinition;
    private GameEventRuleStateMutatorDefinition<T>? _mutatorsDefinition;

    IGameEventConditionDefinitionAndOr<T> IGameEventRuleDefinition<T>.If(GameEventConditionFactory<T> factory)
    {
        _conditionDefinition = new CompositeGameEventConditionDefinition<T>(Builder, this).Add(new GameEventConditionDefinition<T>(Builder, factory, this), null);
        return _conditionDefinition;
    }

    IGameEventConditionDefinitionAndOr<T> IGameEventRuleDefinition<T>.If<TCondition>()
    {
        _conditionDefinition = new CompositeGameEventConditionDefinition<T>(Builder, this).Add(new GameEventConditionDefinition<T>(Builder, game => new TCondition(), this), null);
        return _conditionDefinition;
    }

    public IGameEventRule Build(Game game)
    {
        var condition = _conditionDefinition?.Build(game);
        var onBefore = _mutatorsDefinition?.BuildBefore(game) ?? Veggerby.Boards.Flows.Mutators.NullStateMutator<T>.Instance;
        var onAfter = _mutatorsDefinition?.BuildAfter(game) ?? Veggerby.Boards.Flows.Mutators.NullStateMutator<T>.Instance;

        return SimpleGameEventRule<T>.New(condition ?? new SimpleGameEventCondition<T>((engine, state, @event) => ConditionResponse.Valid), onBefore, onAfter);
    }

    IGameEventRuleStateMutatorDefinition<T> IThenStateMutator<T>.Then()
    {
        _mutatorsDefinition = new GameEventRuleStateMutatorDefinition<T>(Builder, _parent);
        return _mutatorsDefinition;
    }
}