using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules;

namespace Veggerby.Boards.Core.Builder
{
    public class GameEventRuleDefinition<T> : DefinitionBase, IGameEventRuleDefinition where T : IGameEvent
    {
        public GameEventRuleDefinition(GameEngineBuilder builder, GameEventRuleDefinitions gameEventRuleDefinitions) : base(builder)
        {
            GameEventRuleDefinitions = gameEventRuleDefinitions;
        }

        public GameEventRuleDefinitions GameEventRuleDefinitions { get; }
        public IEnumerable<Func<IGameEventCondition<T>>> ConditionFactories { get; private set; }
        public CompositeMode? ConditionCompositeMode { get; private set; }
        public Func<IStateMutator<T>> OnBeforeMutator { get; private set; }
        public Func<IStateMutator<T>> OnAfterMutator { get; private set; }

        private void AddConditionFactory(params Func<IGameEventCondition<T>>[] factories)
        {
            ConditionFactories = (ConditionFactories ?? Enumerable.Empty<Func<IGameEventCondition<T>>>()).Concat(factories).ToList();
        }

        public GameEventRuleDefinition<T> If(Func<IGameEventCondition<T>> factory)
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
            ConditionFactories = new Func<IGameEventCondition<T>>[] { () => new TCondition() };
            return this;
        }

        public GameEventRuleDefinition<T> And(Func<IGameEventCondition<T>> factory)
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
            AddConditionFactory(() => new TCondition());

            return this;
        }

        public GameEventRuleDefinition<T> Or(Func<IGameEventCondition<T>> factory)
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
            AddConditionFactory(() => new TCondition());

            return this;
        }

        public GameEventRuleDefinition<T> DoBefore(Func<IStateMutator<T>> mutator)
        {
            if (mutator == null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            OnBeforeMutator = mutator;
            return this;
        }

        public GameEventRuleDefinition<T> DoBefore<TMutator>() where TMutator : IStateMutator<T>, new()
        {
            OnBeforeMutator = () => new TMutator();
            return this;
        }

        public GameEventRuleDefinition<T> Do(Func<IStateMutator<T>> mutator)
        {
            if (mutator == null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            OnAfterMutator = mutator;
            return this;
        }

        public GameEventRuleDefinition<T> Do<TMutator>() where TMutator : IStateMutator<T>, new()
        {
            OnAfterMutator = () => new TMutator();
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

        public IGameEventRule Build()
        {
            IGameEventCondition<T> condition = null;
            if (ConditionFactories?.Count() == 1)
            {
                condition = ConditionFactories.Single()();
            }
            else if (ConditionFactories?.Any() ?? false)
            {
                var conditions = ConditionFactories.Select(x => x()).ToArray();
                condition = CompositeGameEventCondition<T>.CreateCompositeCondition(ConditionCompositeMode.Value, conditions);
            }

            var onBefore = OnBeforeMutator != null ? OnBeforeMutator() : null;
            var onAfter = OnAfterMutator != null ? OnAfterMutator() : null;

            return SimpleGameEventRule<T>.New(condition ?? new SimpleGameEventCondition<T>((s, e) => ConditionResponse.Valid), onBefore, onAfter);
        }
    }
}
