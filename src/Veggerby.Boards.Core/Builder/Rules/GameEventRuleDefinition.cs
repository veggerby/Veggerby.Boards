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
    public class GameEventRuleDefinition<T> : DefinitionBase, IGameEventRuleDefinition where T : IGameEvent
    {
        public GameEventRuleDefinition(GameEngineBuilder builder, GameEventRuleDefinitions gameEventRuleDefinitions) : base(builder)
        {
            GameEventRuleDefinitions = gameEventRuleDefinitions;
        }

        public GameEventRuleDefinitions GameEventRuleDefinitions { get; }
        public IEnumerable<GameEventConditionFactory<T>> ConditionFactories { get; private set; }
        public CompositeMode? ConditionCompositeMode { get; private set; }
        public IEnumerable<StateMutatorFactory<T>> OnBeforeMutators { get; private set; }
        public IEnumerable<StateMutatorFactory<T>> OnAfterMutators { get; private set; }

        private void AddConditionFactory(params GameEventConditionFactory<T>[] factories)
        {
            ConditionFactories = (ConditionFactories ?? Enumerable.Empty<GameEventConditionFactory<T>>()).Concat(factories).ToList();
        }

        public GameEventRuleDefinition<T> If(GameEventConditionFactory<T> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            ConditionCompositeMode = null;
            ConditionFactories = new [] { factory };
            return this;
        }

        public GameEventRuleDefinition<T> If<TCondition>() where TCondition : IGameEventCondition<T>, new()
        {
            ConditionCompositeMode = null;
            ConditionFactories = new GameEventConditionFactory<T>[] { game => new TCondition() };
            return this;
        }

        public GameEventRuleDefinition<T> And(GameEventConditionFactory<T> factory)
        {
            if ((ConditionCompositeMode ?? CompositeMode.All) != CompositeMode.All)
            {
                throw new ArgumentException("Incorrect composition mode", nameof(factory));
            }

            ConditionCompositeMode = CompositeMode.All;
            AddConditionFactory(factory);

            return this;
        }

        public GameEventRuleDefinition<T> And<TCondition>() where TCondition : IGameEventCondition<T>, new()
        {
            if ((ConditionCompositeMode ?? CompositeMode.All) != CompositeMode.All)
            {
                throw new ArgumentException("Incorrect composition mode");
            }

            ConditionCompositeMode = CompositeMode.All;
            AddConditionFactory(game => new TCondition());

            return this;
        }

        public GameEventRuleDefinition<T> Or(GameEventConditionFactory<T> factory)
        {
            if ((ConditionCompositeMode ?? CompositeMode.Any) != CompositeMode.Any)
            {
                throw new ArgumentException("Incorrect composition mode", nameof(factory));
            }

            ConditionCompositeMode = CompositeMode.Any;
            AddConditionFactory(factory);

            return this;
        }

        public GameEventRuleDefinition<T> Or<TCondition>() where TCondition : IGameEventCondition<T>, new()
        {
            if ((ConditionCompositeMode ?? CompositeMode.Any) != CompositeMode.Any)
            {
                throw new ArgumentException("Incorrect composition mode");
            }

            ConditionCompositeMode = CompositeMode.Any;
            AddConditionFactory(game => new TCondition());

            return this;
        }

        private void AddOnBeforeMutator(StateMutatorFactory<T> mutator)
        {
            OnBeforeMutators = (OnBeforeMutators ?? Enumerable.Empty<StateMutatorFactory<T>>()).Concat(new [] { mutator });
        }

        public GameEventRuleDefinition<T> DoBefore(StateMutatorFactory<T> mutator)
        {
            if (mutator == null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            OnBeforeMutators = new[] { mutator };
            return this;
        }

        public GameEventRuleDefinition<T> ThenBefore<TMutator>() where TMutator : IStateMutator<T>, new()
        {
            AddOnBeforeMutator(game => new TMutator());
            return this;
        }

        public GameEventRuleDefinition<T> ThenBefore(StateMutatorFactory<T> mutator)
        {
            if (mutator == null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            AddOnBeforeMutator(mutator);
            return this;
        }

        public GameEventRuleDefinition<T> DoBefore<TMutator>() where TMutator : IStateMutator<T>, new()
        {
            OnBeforeMutators = new StateMutatorFactory<T>[] { game => new TMutator() };
            return this;
        }

        private void AddOnAfterMutator(StateMutatorFactory<T> mutator)
        {
            OnAfterMutators = (OnAfterMutators ?? Enumerable.Empty<StateMutatorFactory<T>>()).Concat(new [] { mutator });
        }

        public GameEventRuleDefinition<T> Do(StateMutatorFactory<T> mutator)
        {
            if (mutator == null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            OnAfterMutators = new[] { mutator };
            return this;
        }

        public GameEventRuleDefinition<T> Do<TMutator>() where TMutator : IStateMutator<T>, new()
        {
            OnAfterMutators = new StateMutatorFactory<T>[] { game => new TMutator() };
            return this;
        }

        public GameEventRuleDefinition<T> Then(StateMutatorFactory<T> mutator)
        {
            if (mutator == null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            AddOnAfterMutator(mutator);
            return this;
        }
        public GameEventRuleDefinition<T> Then<TMutator>() where TMutator : IStateMutator<T>, new()
        {
            AddOnAfterMutator(game => new TMutator());
            return this;
        }

        public GameEventRuleDefinition<TNewEvent> AndEvent<TNewEvent>() where TNewEvent : IGameEvent
        {
            return GameEventRuleDefinitions.AndEvent<TNewEvent>();
        }

        public GameEventRuleDefinition<TNewEvent> OrEvent<TNewEvent>() where TNewEvent : IGameEvent
        {
            return GameEventRuleDefinitions.OrEvent<TNewEvent>();
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

        public IGameEventRule Build(Game game)
        {
            IGameEventCondition<T> condition = null;
            if (ConditionFactories?.Count() == 1)
            {
                condition = ConditionFactories.Single()(game);
            }
            else if (ConditionFactories?.Any() ?? false)
            {
                var conditions = ConditionFactories.Select(x => x(game)).ToArray();
                condition = CompositeGameEventCondition<T>.CreateCompositeCondition(ConditionCompositeMode.Value, conditions);
            }

            var onBefore = BuildMutator(OnBeforeMutators, game);
            var onAfter = BuildMutator(OnAfterMutators, game);

            return SimpleGameEventRule<T>.New(condition ?? new SimpleGameEventCondition<T>((s, e) => ConditionResponse.Valid), onBefore, onAfter);
        }
    }
}
