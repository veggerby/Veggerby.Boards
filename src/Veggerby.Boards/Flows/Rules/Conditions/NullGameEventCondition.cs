using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// No-op game event condition that always returns <see cref="ConditionResponse.NotApplicable"/>.
/// Used as a safe neutral placeholder in rule composition instead of returning null.
/// </summary>
/// <typeparam name="T">Event type.</typeparam>
internal sealed class NullGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        return ConditionResponse.NotApplicable;
    }
}
