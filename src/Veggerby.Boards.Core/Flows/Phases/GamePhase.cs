using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core.Flows.Phases
{
    public class GamePhase
    {
        public int Number { get; }
        public string Label { get; }
        public CompositeGamePhase Parent { get; }
        public IEnumerable<IGameEventPreProcessor> PreProcessors { get; }
        public IGameStateCondition Condition { get; }
        public IGameEventRule Rule { get; }

        protected GamePhase(int number, string label, IGameStateCondition condition, IGameEventRule rule, CompositeGamePhase parent, IEnumerable<IGameEventPreProcessor> preProcessors)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Round number must be positive and non-zero");
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            Number = number;
            Label = label;
            Condition = condition;
            Rule = rule;
            PreProcessors = preProcessors;

            Parent = parent;
            parent?.Add(this);
        }

        public virtual GamePhase GetActiveGamePhase(GameState gameState)
        {
            return Condition.Evaluate(gameState).Equals(ConditionResponse.Valid) ? this : null;
        }

        public IEnumerable<IGameEvent> PreProcessEvent(GameProgress progress, IGameEvent @event)
        {
            if (!PreProcessors.Any())
            {
                return [@event];
            }

            var preProcessedEvents = PreProcessors
                .SelectMany(x => x.ProcessEvent(progress, @event))
                .Where(x => x != null)
                .ToList();

            return preProcessedEvents;
        }

        public static GamePhase New(int number, string label, IGameStateCondition condition, IGameEventRule rule, CompositeGamePhase parent = null, IEnumerable<IGameEventPreProcessor> preProcessors = null)
        {
            return new GamePhase(number, label, condition, rule, parent, preProcessors);
        }

        public static CompositeGamePhase NewParent(int number, string label = "n/a", IGameStateCondition condition = null, CompositeGamePhase parent = null, IEnumerable<IGameEventPreProcessor> preProcessors = null)
        {
            return new CompositeGamePhase(number, label, condition ?? new NullGameStateCondition(), parent, preProcessors ?? Enumerable.Empty<IGameEventPreProcessor>());
        }
    }
}