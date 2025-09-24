using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Internal.Metrics;

// Internal no-op rule used only for metrics path resolution tests; it never blocks events and returns original state.
internal sealed class NoOpRule : Veggerby.Boards.Flows.Rules.IGameEventRule
{
    public ConditionResponse Check(GameEngine engine, GameState gameState, Veggerby.Boards.Flows.Events.IGameEvent @event)
    {
        return ConditionResponse.Valid; // Always allow; we are not exercising rule evaluation here.
    }

    public GameState HandleEvent(GameEngine engine, GameState gameState, Veggerby.Boards.Flows.Events.IGameEvent @event)
    {
        return gameState; // No mutation.
    }
}