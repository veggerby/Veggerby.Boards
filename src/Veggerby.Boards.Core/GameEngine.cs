using System.Collections.Generic;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class GameEngine
    {
        public Game Game { get; }
        public RuleEngine Rules { get; }
        
        private readonly IList<IGameEvent> _events = new List<IGameEvent>();
        private GameState _gameState;

        public GameEngine(Game game, GameState initialState, RuleEngine rules)
        {
            Game = game;
            _gameState = initialState ?? new GameState(game, null);
        }

        private GameState PlayEvent(GameState state, IGameEvent @event)
        {
            var newState = state.OnBeforeEvent(@event);
            newState = Rules.GetState(newState, @event);
            newState = newState.OnAfterEvent(@event);

            if (newState == state) 
            {
                return null;
            }

            return newState;
        }

        public bool AddEvent(IGameEvent @event)
        {
            var newState = PlayEvent(_gameState, @event);

            if (newState == null)
            {
                return false;
            }

            _events.Add(@event);

            _gameState = newState;
            
            return true;
        }

        public void PlayEvents(IEnumerable<IGameEvent> events)
        {
            var state = _gameState;
            foreach (var @event in events)
            {
                state = PlayEvent(state, @event);

                if (state == null)
                {
                    throw new BoardException("Event does not generate valid state");
                }
            }
        }
    }
}