using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Victory condition: highest score wins when target is reached.
/// </summary>
/// <remarks>
/// Used for games where play continues until at least one player reaches or exceeds a target score,
/// and the winner is determined by which player has the highest score at that time.
/// If no player has reached the target, the condition returns Ignore.
/// </remarks>
public sealed class HighestScoreCondition : IGameStateCondition
{
    private readonly IReadOnlyList<Player> _players;
    private readonly Func<GameState, Player, int> _scoreGetter;
    private readonly int _targetScore;

    /// <summary>
    /// Initializes a new instance of the <see cref="HighestScoreCondition"/> class.
    /// </summary>
    /// <param name="players">The players in the game.</param>
    /// <param name="scoreGetter">Function to get a player's score from state.</param>
    /// <param name="targetScore">The target score that must be reached.</param>
    public HighestScoreCondition(IEnumerable<Player> players, Func<GameState, Player, int> scoreGetter, int targetScore)
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

        if (_players.Count == 0)
        {
            return ConditionResponse.Ignore("No players in game");
        }

        var highestScore = int.MinValue;
        var hasReachedTarget = false;

        foreach (var player in _players)
        {
            var score = _scoreGetter(state, player);

            if (score > highestScore)
            {
                highestScore = score;
            }

            if (score >= _targetScore)
            {
                hasReachedTarget = true;
            }
        }

        if (!hasReachedTarget)
        {
            return ConditionResponse.Ignore("Target score not reached");
        }

        return ConditionResponse.Valid;
    }
}
