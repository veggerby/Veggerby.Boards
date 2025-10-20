using System;
using System.Diagnostics.CodeAnalysis;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Exception raised when a game event fails validation against the current phase's rules/conditions.
/// </summary>
/// <remarks>
/// Wraps the failing <see cref="IGameEvent"/>, the <see cref="ConditionResponse"/> explaining why it was rejected,
/// and contextual game elements for debugging.
/// </remarks>
[ExcludeFromCodeCoverage]
public class InvalidGameEventException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidGameEventException"/> class.
    /// </summary>
    /// <param name="event">The offending event.</param>
    /// <param name="conditionResponse">The condition response (failure details).</param>
    /// <param name="game">The game instance.</param>
    /// <param name="gamePhase">The active phase.</param>
    /// <param name="gameState">The game state snapshot.</param>
    public InvalidGameEventException(IGameEvent @event, ConditionResponse conditionResponse, Game game, GamePhase gamePhase, GameState gameState)
        : base(BuildMessage(@event, conditionResponse))
    {
        GameEvent = @event;
        ConditionResponse = conditionResponse;
        Game = game;
        GamePhase = gamePhase;
        GameState = gameState;
    }

    private static string BuildMessage(IGameEvent @event, ConditionResponse conditionResponse)
    {
        var eventName = @event?.GetType().Name ?? "<null-event>";
        var reason = conditionResponse?.Reason;
        if (string.IsNullOrWhiteSpace(reason))
        {
            return $"Invalid game event {eventName}";
        }
        return $"Invalid game event {eventName}: {reason}";
    }

    /// <summary>
    /// Gets the offending event instance.
    /// </summary>
    public IGameEvent GameEvent
    {
        get;
    }

    /// <summary>
    /// Gets the evaluation response detailing the failure cause.
    /// </summary>
    public ConditionResponse ConditionResponse
    {
        get;
    }

    /// <summary>
    /// Gets the game snapshot at failure time.
    /// </summary>
    public Game Game
    {
        get;
    }

    /// <summary>
    /// Gets the active game phase when the event was rejected.
    /// </summary>
    public GamePhase GamePhase
    {
        get;
    }

    /// <summary>
    /// Gets the game state at the time of evaluation.
    /// </summary>
    public GameState GameState
    {
        get;
    }
}