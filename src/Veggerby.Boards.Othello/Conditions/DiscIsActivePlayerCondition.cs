using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello.Conditions;

/// <summary>
/// Validates that the disc being placed belongs to the currently active player.
/// </summary>
public sealed class DiscIsActivePlayerCondition : IGameEventCondition<PlaceDiscGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PlaceDiscGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        if (!state.TryGetActivePlayer(out var activePlayer))
        {
            return ConditionResponse.Ignore("No active player");
        }

        if (@event.Disc.Owner == null)
        {
            return ConditionResponse.Invalid("Disc has no owner");
        }

        return @event.Disc.Owner.Equals(activePlayer)
            ? ConditionResponse.Valid
            : ConditionResponse.Ignore("Inactive player turn");
    }
}
