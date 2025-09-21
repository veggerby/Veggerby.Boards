using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

/// <summary>
/// Evaluates whether an incoming game event is valid within the supplied engine/state context.
/// </summary>
/// <typeparam name="T">Concrete event type.</typeparam>
public interface IGameEventCondition<in T> where T : IGameEvent
{
    /// <summary>
    /// Evaluates the condition against the provided context and event.
    /// </summary>
    /// <param name="engine">Game engine.</param>
    /// <param name="state">Current state.</param>
    /// <param name="event">Incoming event.</param>
    /// <returns>Condition response (valid, invalid or ignore).</returns>
    ConditionResponse Evaluate(GameEngine engine, GameState state, T @event);
}