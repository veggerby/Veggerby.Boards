using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Phases;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;

namespace Veggerby.Boards.Core.Builder.Rules
{
    public class GameEventRuleStateMutatorDefinition<T> : DefinitionBase, IGameEventRuleStateMutatorDefinition<T> where T : IGameEvent
    {
        public GameEventRuleStateMutatorDefinition(GameBuilder builder, IGameEventRuleDefinitions parent) : base(builder)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        private IList<StateMutatorFactory<T>> _onBeforeMutators = new List<StateMutatorFactory<T>>();
        private IList<StateMutatorFactory<T>> _onAfterMutators = new List<StateMutatorFactory<T>>();

        public IGameEventRuleDefinitions Parent { get; }

        IGameEventRuleStateMutatorDefinition<T> IGameEventRuleStateMutatorDefinitionBefore<T>.Before(StateMutatorFactory<T> mutator)
        {
            _onBeforeMutators.Add(mutator);
            return this;
        }

        IGameEventRuleStateMutatorDefinition<T> IGameEventRuleStateMutatorDefinitionBefore<T>.Before<TMutator>()
        {
            _onBeforeMutators.Add(game => new TMutator());
            return this;
        }

        IGameEventRuleStateMutatorDefinitionDo<T> IGameEventRuleStateMutatorDefinitionDo<T>.Do(StateMutatorFactory<T> mutator)
        {
            _onAfterMutators.Add(mutator);
            return this;
        }

        IGameEventRuleStateMutatorDefinitionDo<T> IGameEventRuleStateMutatorDefinitionDo<T>.Do<TMutator>()
        {
            _onAfterMutators.Add(game => new TMutator());
            return this;
        }

        IGameEventRuleDefinition<TEvent> IGameEventRuleDefinitions.ForEvent<TEvent>()
        {
            return Parent.ForEvent<TEvent>();
        }

        private IStateMutator<T> BuildMutator(IEnumerable<StateMutatorFactory<T>> mutatorFactories, Game game)
        {
            if (!(mutatorFactories?.Any() ?? false))
            {
                return null;
            }

            if (mutatorFactories.Count() == 1)
            {
                return mutatorFactories.Single()(game);
            }

            var mutators = mutatorFactories.Select(x => x(game)).ToArray();
            return new CompositeStateMutator<T>(mutators);
        }

        internal IStateMutator<T> BuildBefore(Game game)
        {
            return BuildMutator(_onBeforeMutators, game);;
        }

        internal IStateMutator<T> BuildAfter(Game game)
        {
            return BuildMutator(_onAfterMutators, game);;
        }
    }
}