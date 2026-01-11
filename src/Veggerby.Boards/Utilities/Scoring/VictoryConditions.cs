using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Utilities.Scoring;

/// <summary>
/// Reusable victory condition factory helpers.
/// </summary>
/// <remarks>
/// Provides static factory methods for creating common victory condition patterns.
/// Each method returns a factory function that produces an <see cref="IGameStateCondition"/> when given a compiled <see cref="Game"/>.
/// These factories are designed to be used with the GameBuilder fluent API.
/// </remarks>
public static class VictoryConditions
{
    /// <summary>
    /// Creates a factory for a victory condition where the highest score wins when any player reaches the target.
    /// </summary>
    /// <param name="scoreGetter">Function to extract a player's score from game state.</param>
    /// <param name="targetScore">The target score that triggers victory evaluation.</param>
    /// <returns>A factory that produces a condition when given a compiled game.</returns>
    /// <remarks>
    /// The condition returns Ignore until at least one player reaches the target score.
    /// Once the target is reached, the player with the highest score is considered the winner.
    /// </remarks>
    public static GameStateConditionFactory HighestScore(
        Func<GameState, Player, int> scoreGetter,
        int targetScore)
    {
        return game => new HighestScoreCondition(
            game.Artifacts.OfType<Player>(),
            scoreGetter,
            targetScore);
    }

    /// <summary>
    /// Creates a factory for a victory condition where the first player to reach the target score wins.
    /// </summary>
    /// <param name="scoreGetter">Function to extract a player's score from game state.</param>
    /// <param name="targetScore">The target score that triggers victory.</param>
    /// <returns>A factory that produces a condition when given a compiled game.</returns>
    /// <remarks>
    /// The condition returns Valid immediately when the first player reaches or exceeds the target score.
    /// </remarks>
    public static GameStateConditionFactory FirstToScore(
        Func<GameState, Player, int> scoreGetter,
        int targetScore)
    {
        return game => new FirstToScoreCondition(
            game.Artifacts.OfType<Player>(),
            scoreGetter,
            targetScore);
    }

    /// <summary>
    /// Creates a factory for a victory condition where the last remaining player wins (elimination-based).
    /// </summary>
    /// <returns>A factory that produces a condition when given a compiled game.</returns>
    /// <remarks>
    /// Players are marked as eliminated using <see cref="PlayerEliminatedState"/>.
    /// The condition returns Valid when only one non-eliminated player remains.
    /// </remarks>
    public static GameStateConditionFactory LastPlayerStanding()
    {
        return game => new LastPlayerStandingCondition(game.Artifacts.OfType<Player>());
    }
}
