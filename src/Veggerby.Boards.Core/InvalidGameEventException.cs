using System;
using System.Diagnostics.CodeAnalysis;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core;

[ExcludeFromCodeCoverage]
public class InvalidGameEventException(IGameEvent @event, ConditionResponse conditionResponse, Game game, GamePhase gamePhase, GameState gameState) : Exception
{
    public IGameEvent GameEvent { get; } = @event;
    public ConditionResponse ConditionResponse { get; } = conditionResponse;
    public Game Game { get; } = game;
    public GamePhase GamePhase { get; } = gamePhase;
    public GameState GameState { get; } = gameState;
}