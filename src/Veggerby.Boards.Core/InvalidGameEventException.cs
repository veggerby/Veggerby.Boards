using System;
using System.Diagnostics.CodeAnalysis;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    [ExcludeFromCodeCoverage]
    public class InvalidGameEventException : Exception
    {
        public InvalidGameEventException(IGameEvent @event, ConditionResponse conditionResponse, Game game, GamePhase gamePhase, GameState gameState)
        {
            GameEvent = @event;
            ConditionResponse = conditionResponse;
            Game = game;
            GamePhase = gamePhase;
            GameState = gameState;
        }

        public IGameEvent GameEvent { get; }
        public ConditionResponse ConditionResponse { get; }
        public Game Game { get; }
        public GamePhase GamePhase { get; }
        public GameState GameState { get; }
    }
}