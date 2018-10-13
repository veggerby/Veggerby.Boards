using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;

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
        }

        public GameEngine Engine { get; }
        public GameState State { get; }
        public IEnumerable<IGameEvent> Events { get; }
        public Game Game => State.Game;

        public GameProgress HandleEvent(IGameEvent @event)
        {
            var gamePhase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
            var ruleCheck = gamePhase.Rule.Check(State, @event);
            if (ruleCheck.Result == ConditionResult.Valid)
            {
                var newState = gamePhase.Rule.HandleEvent(State, @event);
                return new GameProgress(Engine, newState, Events.Concat(new [] { @event }));
            }
            else if (ruleCheck.Result == ConditionResult.Invalid)
            {
                throw new InvalidGameEventException(@event, ruleCheck, Engine.Game, gamePhase, State);
            }

            return this;
        }

    }
}