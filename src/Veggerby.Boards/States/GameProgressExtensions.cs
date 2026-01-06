using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.LegalMoveGeneration;

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

    /// <summary>
    /// Creates a player-specific view of the current game state.
    /// </summary>
    /// <param name="progress">The game progress to project.</param>
    /// <param name="player">The player whose perspective to project.</param>
    /// <param name="policy">
    /// Optional visibility policy to apply. If null, uses <see cref="FullVisibilityPolicy"/>
    /// (perfect information, backward compatible with existing games).
    /// </param>
    /// <returns>A filtered view showing only states visible to the specified player.</returns>
    /// <remarks>
    /// <para>
    /// Player views enable imperfect-information games by filtering hidden state based on the
    /// configured <see cref="IVisibilityPolicy"/>. For perfect-information games (Chess, Go),
    /// the default <see cref="FullVisibilityPolicy"/> shows all state.
    /// </para>
    /// <para>
    /// Example usage for a card game:
    /// <code>
    /// var view = progress.GetViewFor(player, new PlayerOwnedVisibilityPolicy());
    /// var visibleCards = view.GetStates&lt;CardState&gt;();
    /// </code>
    /// </para>
    /// </remarks>
    public static GameStateView GetViewFor(this GameProgress progress, Player player, IVisibilityPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(player);

        var projection = new DefaultGameStateProjection(progress.State, policy);
        return projection.ProjectFor(player);
    }

    /// <summary>
    /// Creates an observer view of the current game state.
    /// </summary>
    /// <param name="progress">The game progress to project.</param>
    /// <param name="role">The observer role determining visibility permissions.</param>
    /// <param name="policy">
    /// Optional visibility policy to apply. If null, uses <see cref="FullVisibilityPolicy"/>
    /// for <see cref="ObserverRole.Full"/>, and public-only filtering for <see cref="ObserverRole.Limited"/>.
    /// </param>
    /// <returns>A filtered view based on the observer's access level.</returns>
    /// <remarks>
    /// <para>
    /// Observer views support spectator modes, tournament displays, and post-game analysis:
    /// <list type="bullet">
    /// <item><description><see cref="ObserverRole.Full"/>: Complete visibility (admin, arbiter, post-game replay)</description></item>
    /// <item><description><see cref="ObserverRole.Limited"/>: Public state only (live tournament spectator)</description></item>
    /// <item><description><see cref="ObserverRole.PlayerPerspective"/>: Same visibility as a player (coaching, training)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example usage for tournament spectator:
    /// <code>
    /// var spectatorView = progress.GetObserverView(ObserverRole.Limited);
    /// var publicPieces = spectatorView.GetStates&lt;PieceState&gt;();
    /// </code>
    /// </para>
    /// </remarks>
    public static GameStateView GetObserverView(this GameProgress progress, ObserverRole role, IVisibilityPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var projection = new DefaultGameStateProjection(progress.State, policy);
        return projection.ProjectForObserver(role);
    }

    /// <summary>
    /// Gets the legal move generator for this game.
    /// </summary>
    /// <param name="progress">The game progress to generate moves for.</param>
    /// <returns>
    /// An <see cref="ILegalMoveGenerator"/> implementation that can enumerate legal events
    /// and validate specific moves for the current game state.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The legal move generator leverages the game's compiled <see cref="Flows.DecisionPlan.DecisionPlan"/>
    /// to efficiently evaluate move legality without repeated rule traversal.
    /// </para>
    /// <para>
    /// This base implementation provides move validation for all games via DecisionPlan integration.
    /// For optimized move enumeration in specific game modules (e.g., Chess), use the module-specific
    /// extension methods such as <c>GetChessLegalMoveGenerator()</c>.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var generator = progress.GetLegalMoveGenerator();
    /// var validation = generator.Validate(move, progress.State);
    /// if (!validation.IsLegal)
    /// {
    ///     Console.WriteLine($"{validation.Reason}: {validation.Explanation}");
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Performance characteristics:
    /// <list type="bullet">
    /// <item><description>First call: O(1) - creates lightweight wrapper around existing plan</description></item>
    /// <item><description>Subsequent calls: reuses same generator instance (stateless)</description></item>
    /// <item><description>Move validation: O(1) via precompiled DecisionPlan (&lt; 1ms typical)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static ILegalMoveGenerator GetLegalMoveGenerator(this GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return new DecisionPlanMoveGenerator(progress.Engine, progress.State);
    }
}
