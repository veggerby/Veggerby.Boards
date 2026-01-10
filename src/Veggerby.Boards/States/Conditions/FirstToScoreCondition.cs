using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Victory condition: first player to reach target score wins.
/// </summary>
/// <remarks>
/// Used for games where the first player to reach or exceed a target score wins immediately.
/// Returns Valid as soon as any player meets the target.
/// </remarks>
public sealed class FirstToScoreCondition : IGameStateCondition
{
    private readonly IReadOnlyList<Player> _players;
    private readonly Func<GameState, Player, int> _scoreGetter;
    private readonly int _targetScore;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstToScoreCondition"/> class.
    /// </summary>
    /// <param name="players">The players in the game.</param>
    /// <param name="scoreGetter">Function to get a player's score from state.</param>
    /// <param name="targetScore">The target score that must be reached.</param>
    public FirstToScoreCondition(IEnumerable<Player> players, Func<GameState, Player, int> scoreGetter, int targetScore)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(scoreGetter);

        _players = players.ToList();
        _scoreGetter = scoreGetter;
        _targetScore = targetScore;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (var player in _players)
        {
            var score = _scoreGetter(state, player);

            if (score >= _targetScore)
            {
                return ConditionResponse.Valid;
            }
        }

        return ConditionResponse.Ignore("No player reached target");
    }
}
