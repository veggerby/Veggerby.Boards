using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Utilities.Scoring;

/// <summary>
/// Reusable scoring accumulator for point-based games.
/// </summary>
/// <remarks>
/// Provides fluent API for accumulating player scores and generating ranked results.
/// Immutable operation pattern - each method returns a new instance.
/// </remarks>
public sealed class ScoreAccumulator
{
    private readonly Dictionary<Player, int> _scores;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScoreAccumulator"/> class.
    /// </summary>
    public ScoreAccumulator()
    {
        _scores = new Dictionary<Player, int>();
    }

    private ScoreAccumulator(Dictionary<Player, int> scores)
    {
        _scores = new Dictionary<Player, int>(scores);
    }

    /// <summary>
    /// Adds points to a player's score.
    /// </summary>
    /// <param name="player">The player to add points to.</param>
    /// <param name="points">The points to add (can be negative).</param>
    /// <returns>A new accumulator with the updated score.</returns>
    public ScoreAccumulator Add(Player player, int points)
    {
        var newScores = new Dictionary<Player, int>(_scores);

        newScores.TryGetValue(player, out var current);
        newScores[player] = current + points;

        return new ScoreAccumulator(newScores);
    }

    /// <summary>
    /// Sets a player's score to a specific value.
    /// </summary>
    /// <param name="player">The player to set the score for.</param>
    /// <param name="points">The score value to set.</param>
    /// <returns>A new accumulator with the updated score.</returns>
    public ScoreAccumulator Set(Player player, int points)
    {
        var newScores = new Dictionary<Player, int>(_scores);

        newScores[player] = points;

        return new ScoreAccumulator(newScores);
    }

    /// <summary>
    /// Gets the current score for a player.
    /// </summary>
    /// <param name="player">The player to get the score for.</param>
    /// <returns>The player's current score, or 0 if not found.</returns>
    public int GetScore(Player player)
    {
        return _scores.GetValueOrDefault(player, 0);
    }

    /// <summary>
    /// Gets all scores ranked from highest to lowest.
    /// </summary>
    /// <remarks>
    /// Players with the same score will have the same rank.
    /// The next rank after a tie skips numbers (e.g., two players tied at rank 1 means next rank is 3).
    /// </remarks>
    /// <returns>A read-only list of ranked player scores.</returns>
    public IReadOnlyList<PlayerScore> GetRankedScores()
    {
        var sorted = _scores
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key.Id)
            .ToList();

        var results = new List<PlayerScore>();
        var currentRank = 1;

        for (var i = 0; i < sorted.Count; i++)
        {
            var kv = sorted[i];

            if (i > 0 && sorted[i - 1].Value != kv.Value)
            {
                currentRank = i + 1;
            }

            results.Add(new PlayerScore
            {
                Player = kv.Key,
                Score = kv.Value,
                Rank = currentRank
            });
        }

        return results;
    }
}
