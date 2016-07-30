using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class GameEngine
    {
        public Game Game { get; }
        public RuleEngine Rules { get; }
        public GameState GameState { get; private set; }
        public IEnumerable<IGameEvent> Events => _events.ToList().AsReadOnly();
        
        private readonly IList<IGameEvent> _events = new List<IGameEvent>();

        public GameEngine(Game game, GameState initialState, RuleEngine rules)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Game = game;
            GameState = initialState ?? GameState.New(game);
            Rules = rules;
        }

        private bool Check(GameState state, IGameEvent @event)
        {
            var newState = state?.OnBeforeEvent(@event);
            return Rules.Check(this, newState, @event);
        }
    
        private GameState PlayEvent(GameState state, IGameEvent @event)
        {
            var newState = state?.OnBeforeEvent(@event);
            newState = Rules.Evaluate(this, newState, @event);
            return newState?.OnAfterEvent(@event);
        }

        public bool AddEvent(IGameEvent @event)
        {
            if (!Check(GameState, @event))
            {
                return false;
            }

            var newState = PlayEvent(GameState, @event);
            _events.Add(@event);
            GameState = newState;
            
            return true;
        }

        public void PlayEvents(IEnumerable<IGameEvent> events)
        {
            var state = GameState;
            foreach (var @event in events)
            {
                if (!Check(state, @event))
                {
                    throw new BoardException("Invalid event");
                }

                state = PlayEvent(state, @event);
            }
        }

        public TurnState EvaluateTurnState(Turn nextTurn)
        {
            return new TurnState(Game.GamePhases.Single(), nextTurn, Game.TurnPhases.Single());
        }
    }
}