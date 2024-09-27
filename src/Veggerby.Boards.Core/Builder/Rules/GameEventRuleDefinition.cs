using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.Flows.Rules.Conditions;

namespace Veggerby.Boards.Core.Builder.Rules
{
    internal class GameEventRuleDefinition<T> : DefinitionBase, IGameEventRuleDefinition, IGameEventRuleDefinition<T> where T : IGameEvent
    {
        public GameEventRuleDefinition(GameBuilder builder, IGameEventRuleDefinitions parent) : base(builder)
        {
            _parent = parent;
        }

        private readonly IGameEventRuleDefinitions _parent;
        private CompositeGameEventConditionDefinition<T> _conditionDefinition;
        private GameEventRuleStateMutatorDefinition<T> _mutatorsDefinition;

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
            var onBefore = _mutatorsDefinition.BuildBefore(game);
            var onAfter = _mutatorsDefinition.BuildAfter(game);

            return SimpleGameEventRule<T>.New(condition ?? new SimpleGameEventCondition<T>((engine, state, @event) => ConditionResponse.Valid), onBefore, onAfter);
        }

        IGameEventRuleStateMutatorDefinition<T> IThenStateMutator<T>.Then()
        {
            _mutatorsDefinition = new GameEventRuleStateMutatorDefinition<T>(Builder, _parent);
            return _mutatorsDefinition;
        }
    }
}
