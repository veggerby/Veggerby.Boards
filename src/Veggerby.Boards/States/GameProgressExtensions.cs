using System;
using System.Linq;

namespace Veggerby.Boards.States;

/// <summary>
/// Extension methods for <see cref="GameProgress"/> providing unified game termination and outcome tracking.
/// </summary>
public static class GameProgressExtensions
{
    /// <summary>
    /// Checks if the game has reached a terminal state.
    /// </summary>
    /// <param name="progress">The game progress to check.</param>
    /// <returns>True if game is over; false if still in progress.</returns>
    /// <remarks>
    /// This method checks for the standardized <see cref="GameEndedState"/> marker.
    /// Game modules should add this state when the game reaches a terminal condition.
    /// </remarks>
    public static bool IsGameOver(this GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return progress.State.GetStates<GameEndedState>().Any();
    }

    /// <summary>
    /// Gets the game outcome snapshot if available.
    /// </summary>
    /// <param name="progress">The game progress to extract outcome from.</param>
    /// <returns>Outcome details or null if game not terminated or outcome not computed.</returns>
    /// <remarks>
    /// <para>
    /// This method attempts to extract outcome information from the game state by looking for
    /// module-specific outcome states that implement <see cref="IGameOutcome"/>.
    /// </para>
    /// <para>
    /// Supported game modules:
    /// <list type="bullet">
    /// <item><description>Chess: Returns outcome from ChessOutcomeState (checkmate, stalemate, etc.)</description></item>
    /// <item><description>Go: Returns outcome from GoOutcomeState (territory scoring)</description></item>
    /// <item><description>DeckBuilding: Returns outcome from ScoreState (victory points)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Returns null if:
    /// <list type="bullet">
    /// <item><description>Game is not over (no GameEndedState present)</description></item>
    /// <item><description>Game module has not implemented outcome tracking</description></item>
    /// <item><description>Outcome state has not been added to the game state</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IGameOutcome? GetOutcome(this GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        if (!progress.IsGameOver())
        {
            return null;
        }

        // Look for module-specific outcome states that implement IGameOutcome
        var outcomeStates = progress.State.ChildStates.OfType<IGameOutcome>();

        // Return the first outcome found (there should only be one per game)
        return outcomeStates.FirstOrDefault();
    }
}
