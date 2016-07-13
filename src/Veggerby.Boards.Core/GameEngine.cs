using System.Collections.Generic;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class GameEngine
    {
        public Game Game { get; }
        private readonly IList<IGameEvent> _events = new List<IGameEvent>();
        private GameState _gameState;

        public GameEngine(Game game)
        {
            Game = game;
            _gameState = new GameState(game, null);
        }

        public bool AddEvent(IGameEvent @event)
        {
            var newState = Game.Rules.GetState(_gameState, @event);

            if (newState == null)
            {
                return false;
            }

            _events.Add(@event);
            _gameState = newState;
            return true;
        }
    }
}