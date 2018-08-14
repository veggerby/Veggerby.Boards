using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public class GameEngine
    {
        public Game Game { get; }
        public GameState GameState { get; private set; }
        public GamePhase GamePhaseRoot { get; }
        public IEnumerable<IGameEvent> Events => _events.ToList().AsReadOnly();

        private readonly IList<IGameEvent> _events = new List<IGameEvent>();

        private GameEngine(Game game, GamePhase gamePhaseRoot, GameState initialState)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (gamePhaseRoot == null)
            {
                throw new ArgumentNullException(nameof(gamePhaseRoot));
            }

            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            Game = game;
            GamePhaseRoot = gamePhaseRoot;
            GameState = initialState;
        }

        public static GameEngine New(Game game, GamePhase gamePhaseRoot, GameState initialState)
        {
            return new GameEngine(game, gamePhaseRoot, initialState);
        }
    }
}