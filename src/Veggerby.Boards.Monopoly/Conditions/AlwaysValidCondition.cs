using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that always returns Valid. Used for events that need no validation.
/// </summary>
/// <typeparam name="T">Event type.</typeparam>
public sealed class AlwaysValidCondition<T> : IGameEventCondition<T> where T : IGameEvent
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        return ConditionResponse.Valid;
    }
}
