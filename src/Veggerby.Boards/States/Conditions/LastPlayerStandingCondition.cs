using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Victory condition: last player standing (elimination-based).
/// </summary>
/// <remarks>
/// Used for games where players are eliminated and the last remaining player wins.
/// Checks for <see cref="PlayerEliminatedState"/> markers to determine active players.
/// Returns Valid when only one player remains active.
/// </remarks>
public sealed class LastPlayerStandingCondition : IGameStateCondition
{
    private readonly IReadOnlyList<Player> _players;

    /// <summary>
    /// Initializes a new instance of the <see cref="LastPlayerStandingCondition"/> class.
    /// </summary>
    /// <param name="players">The players in the game.</param>
    public LastPlayerStandingCondition(IEnumerable<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        _players = players.ToList();
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var eliminatedPlayers = state.GetStates<PlayerEliminatedState>()
            .Select(s => s.Player)
            .ToHashSet();

        var remainingPlayers = _players
            .Where(p => !eliminatedPlayers.Contains(p))
            .ToList();

        if (remainingPlayers.Count == 1)
        {
            return ConditionResponse.Valid;
        }

        if (remainingPlayers.Count == 0)
        {
            return ConditionResponse.Ignore("No players remaining");
        }

        return ConditionResponse.Ignore("Multiple players remaining");
    }
}
