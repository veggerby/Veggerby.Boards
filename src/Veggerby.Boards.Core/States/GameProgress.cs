using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;

namespace Veggerby.Boards.Core.States
{
    public class GameProgress
    {
        public GameProgress(GameEngine engine, GameState state, IEnumerable<IGameEvent> events)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            Engine = engine;
            State = state;
            Events = (events ?? Enumerable.Empty<IGameEvent>()).ToList();
            Phase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
        }

        public GameEngine Engine { get; }
        public GameState State { get; }
        public GamePhase Phase { get; }
        public IEnumerable<IGameEvent> Events { get; }
        public Game Game => Engine.Game;

        private GameProgress HandleSingleEvent(IGameEvent @event)
        {
            var ruleCheck = Phase.Rule.Check(Engine, State, @event);
            if (ruleCheck.Result == ConditionResult.Valid)
            {
                var newState = Phase.Rule.HandleEvent(Engine, State, @event);
                return new GameProgress(Engine, newState, Events.Concat(new [] { @event }));
            }
            else if (ruleCheck.Result == ConditionResult.Invalid)
            {
                throw new InvalidGameEventException(@event, ruleCheck, Game, Phase, State);
            }

            return this;
        }

        public GameProgress NewState(IEnumerable<IArtifactState> newStates)
        {
            return new GameProgress(
                Engine,
                State.Next(newStates),
                Events
            );
        }

        public GameProgress HandleEvent(IGameEvent @event)
        {
            var events = Phase.PreProcessEvent(this, @event);
            return events.Aggregate(this, (seed, e) => seed.HandleSingleEvent(e));
        }
    }
}