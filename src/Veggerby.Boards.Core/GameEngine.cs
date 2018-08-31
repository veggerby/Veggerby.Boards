using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
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

/*        public GameState HandleEvent(IGameEvent @event)
        {
            var gamePhase = GamePhaseRoot.GetActiveGamePhase(GameState);
            var ruleCheck = gamePhase.Rule.Check(GameState, @event);
            if (ruleCheck.Result == RuleCheckResult.Valid)
            {
                var newState = gamePhase.Rule.HandleEvent(GameState, @event);
                if (newState.Equals(GameState))
                {
                    // NOOP, do not record event
                    return GameState;
                }

                _events.Add(@event);

                GameState = newState;
            }

            return GameState;
        }*/

        private GameEngine(GameState initialState, GamePhase gamePhaseRoot)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            if (!initialState.IsInitialState)
            {
                throw new ArgumentException("GameState is not initial state", nameof(initialState));
            }

            if (gamePhaseRoot == null)
            {
                throw new ArgumentNullException(nameof(gamePhaseRoot));
            }

            GameState = initialState;
            GamePhaseRoot = gamePhaseRoot;
            Game = initialState.Game;
        }

        public static GameEngine New(GameState initialState, GamePhase gamePhaseRoot)
        {
            return new GameEngine(initialState, gamePhaseRoot);
        }
    }
}