using System;
using System.Diagnostics.CodeAnalysis;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core;

/// <summary>
/// Exception raised when a game event fails validation against the current phase's rules/conditions.
/// </summary>
/// <remarks>
/// Wraps the failing <see cref="IGameEvent"/>, the <see cref="ConditionResponse"/> explaining why it was rejected,
/// and contextual game elements for debugging.
/// </remarks>
[ExcludeFromCodeCoverage]
public class InvalidGameEventException(IGameEvent @event, ConditionResponse conditionResponse, Game game, GamePhase gamePhase, GameState gameState) : Exception
{
    /// <summary>
    /// Gets the offending event instance.
    /// </summary>
    public IGameEvent GameEvent { get; } = @event;

    /// <summary>
    /// Gets the evaluation response detailing the failure cause.
    /// </summary>
    public ConditionResponse ConditionResponse { get; } = conditionResponse;

    /// <summary>
    /// Gets the game snapshot at failure time.
    /// </summary>
    public Game Game { get; } = game;

    /// <summary>
    /// Gets the active game phase when the event was rejected.
    /// </summary>
    public GamePhase GamePhase { get; } = gamePhase;

    /// <summary>
    /// Gets the game state at the time of evaluation.
    /// </summary>
    public GameState GameState { get; } = gameState;
}