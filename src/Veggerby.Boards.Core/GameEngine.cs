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
        public IEnumerable<Player> Players { get; }
        public GameState GameState { get; private set; }
        public IEnumerable<IGameEvent> Events => _events.ToList().AsReadOnly();
        
        private readonly IList<IGameEvent> _events = new List<IGameEvent>();

        public GameEngine(Game game, GameState initialState, RuleEngine rules, IEnumerable<Player> players)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            if (!players.Any())
            {
                throw new ArgumentException(nameof(players));
            }

            Game = game;
            GameState = initialState ?? new GameState(game, null, null);
            Rules = rules;
            Players = players.ToList();
        }

        private GameState PlayEvent(GameState state, IGameEvent @event)
        {
            var newState = state?.OnBeforeEvent(@event);
            newState = Rules.GetState(this, newState, @event);
            newState = newState?.OnAfterEvent(@event);

            if (newState == state) 
            {
                return null;
            }

            return newState;
        }

        public bool AddEvent(IGameEvent @event)
        {
            var newState = PlayEvent(GameState, @event);

            if (newState == null)
            {
                return false;
            }

            _events.Add(@event);

            GameState = newState;
            
            return true;
        }

        public void PlayEvents(IEnumerable<IGameEvent> events)
        {
            var state = GameState;
            foreach (var @event in events)
            {
                state = PlayEvent(state, @event);

                if (state == null)
                {
                    throw new BoardException("Event does not generate valid state");
                }
            }
        }

        public TurnState EvaluateTurnState(Turn nextTurn)
        {
            return new TurnState(Game.GamePhases.Single(), nextTurn, Game.TurnPhases.Single());
        }
    }
}