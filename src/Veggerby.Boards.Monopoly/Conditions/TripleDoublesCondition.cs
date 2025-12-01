using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player has rolled three consecutive doubles.
/// </summary>
/// <remarks>
/// In Monopoly, rolling three consecutive doubles results in the player going to jail.
/// This condition evaluates to <see cref="ConditionResponse.Valid"/> when:
/// <list type="bullet">
/// <item>The player has already rolled 2 consecutive doubles (tracked in state)</item>
/// <item>The current dice roll is also doubles</item>
/// </list>
/// </remarks>
public class TripleDoublesCondition : IGameEventCondition<MovePlayerGameEvent>
{
    /// <summary>
    /// The number of consecutive doubles required to go to jail.
    /// </summary>
    public const int TripleDoublesThreshold = 3;

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePlayerGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check if current roll is doubles
        if (!@event.IsDoubles)
        {
            return ConditionResponse.Ignore("Current roll is not doubles");
        }

        // Get player's current consecutive doubles count
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        // Check if this would be the third consecutive doubles
        // Note: ConsecutiveDoubles reflects doubles count BEFORE this roll
        if (playerState.ConsecutiveDoubles >= TripleDoublesThreshold - 1)
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore($"Only {playerState.ConsecutiveDoubles + 1} consecutive doubles");
    }
}
