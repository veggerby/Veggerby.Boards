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
        public GameEventRuleStateMutatorDefinition(GameEngineBuilder builder, IForGameEventRule parent) : base(builder)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Parent = parent;
        }

        private IList<StateMutatorFactory<T>> _onBeforeMutators = new List<StateMutatorFactory<T>>();
        private IList<StateMutatorFactory<T>> _onAfterMutators = new List<StateMutatorFactory<T>>();

        public IForGameEventRule Parent { get; }

        IGameEventRuleStateMutatorDefinition<T> IGameEventRuleStateMutatorDefinition<T>.Do(StateMutatorFactory<T> mutator)
        {
            _onAfterMutators.Add(mutator);
            return this;
        }

        IGameEventRuleStateMutatorDefinition<T> IGameEventRuleStateMutatorDefinition<T>.Do<TMutator>()
        {
            _onAfterMutators.Add(game => new TMutator());
            return this;
        }

        IGameEventRuleDefinition<TEvent> IForGameEventRule.ForEvent<TEvent>()
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