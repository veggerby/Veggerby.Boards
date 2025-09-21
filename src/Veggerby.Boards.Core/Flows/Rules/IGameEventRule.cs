using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules;

/// <summary>
/// Defines the contract for validating and applying a game event against a <see cref="GameState"/>.
/// </summary>
/// <remarks>
/// Rules are evaluated in two stages: <see cref="Check"/> returns a <see cref="ConditionResponse"/> describing validity or
/// applicability; if valid the engine may invoke <see cref="HandleEvent"/> to produce a successor state. Implementations must be
/// pure and side-effect free aside from producing new immutable state instances.
/// </remarks>
public interface IGameEventRule
{
    /// <summary>
    /// Evaluates whether the event can be applied given the current engine and state context.
    /// </summary>
    /// <param name="engine">The engine context.</param>
    /// <param name="gameState">The current game state.</param>
    /// <param name="event">The event instance.</param>
    /// <returns>A <see cref="ConditionResponse"/> describing the outcome (valid, invalid, ignore/not applicable).</returns>
    ConditionResponse Check(GameEngine engine, GameState gameState, IGameEvent @event);

    /// <summary>
    /// Applies the event to produce a new <see cref="GameState"/> when valid.
    /// </summary>
    /// <param name="engine">The engine context.</param>
    /// <param name="gameState">The current state.</param>
    /// <param name="event">The event to apply.</param>
    /// <returns>The resulting state (original if ignored, new if applied).</returns>
    GameState HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event);
}