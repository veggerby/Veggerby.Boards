using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;

namespace Veggerby.Boards.Core.States
{
    public class GameProgress
    {
        public GameProgress(GameEngine gameEngine, GameState gameState, IEnumerable<IGameEvent> gameEvents)
        {
            if (gameEngine == null)
            {
                throw new ArgumentNullException(nameof(gameEngine));
            }

            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            GameEngine = gameEngine;
            GameState = gameState;
            GameEvents = (gameEvents ?? Enumerable.Empty<IGameEvent>()).ToList();
        }

        public GameEngine GameEngine { get; }
        public GameState GameState { get; }
        public IEnumerable<IGameEvent> GameEvents { get; }
        public Game Game => GameState.Game;

        public GameProgress HandleEvent(IGameEvent @event)
        {
            var gamePhase = GameEngine.GamePhaseRoot.GetActiveGamePhase(GameState);
            var ruleCheck = gamePhase.Rule.Check(GameState, @event);
            if (ruleCheck.Result == ConditionResult.Valid)
            {
                var newState = gamePhase.Rule.HandleEvent(GameState, @event);
                return new GameProgress(GameEngine, newState, GameEvents.Concat(new [] { @event }));
            }
            else if (ruleCheck.Result == ConditionResult.Invalid)
            {
                throw new InvalidGameEventException(@event, ruleCheck, GameEngine.Game, gamePhase, GameState);
            }

            return this;
        }

    }
}