using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player is currently in jail.
/// </summary>
/// <remarks>
/// This condition is used to validate that a <see cref="StayInJailGameEvent"/>
/// can be processed (the player must actually be in jail).
/// </remarks>
public class PlayerInJailCondition : IGameEventCondition<StayInJailGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, StayInJailGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        if (!playerState.InJail)
        {
            return ConditionResponse.NotApplicable;
        }

        return ConditionResponse.Valid;
    }
}
