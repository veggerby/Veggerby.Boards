using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.LegalMoveGeneration;

/// <summary>
/// Enumerates legal events for the current game state.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ILegalMoveGenerator"/> provides a standard, core-level API to discover what events
/// are permissible in the current game state. This enables:
/// <list type="bullet">
/// <item><description>AI development: Agents can explore legal moves without module-specific code</description></item>
/// <item><description>UI/UX: Highlight valid moves, show rejection explanations</description></item>
/// <item><description>Game analysis: Generate move trees, compute branching factors</description></item>
/// <item><description>Testing: Validate rule completeness via exhaustive legal move generation</description></item>
/// </list>
/// </para>
/// <para>
/// Implementations must be deterministic: the same <see cref="GameState"/> always produces the same
/// set of legal moves (modulo ordering, which may or may not be guaranteed depending on implementation).
/// </para>
/// <para>
/// Typical usage:
/// <code>
/// var generator = progress.GetLegalMoveGenerator();
/// var legalMoves = generator.GetLegalMoves(progress.State);
/// foreach (var move in legalMoves)
/// {
///     Console.WriteLine($"Legal: {move}");
/// }
/// </code>
/// </para>
/// </remarks>
public interface ILegalMoveGenerator
{
    /// <summary>
    /// Gets all legal events that can be applied to the current state.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>
    /// An enumerable collection of legal events. The collection may be empty if no legal moves exist
    /// (e.g., game is over or player has no valid actions).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method enumerates all events that would be accepted by the engine's rules and phases.
    /// Events are generated lazily where possible to avoid unnecessary allocations.
    /// </para>
    /// <para>
    /// Ordering guarantees:
    /// <list type="bullet">
    /// <item><description>Default implementation: no specific ordering guaranteed (performance-optimized)</description></item>
    /// <item><description>Module-specific implementations may provide deterministic ordering</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Performance considerations:
    /// <list type="bullet">
    /// <item><description>Chess mid-game: target &lt; 1ms for ~30-40 legal moves</description></item>
    /// <item><description>Go 19x19 empty board: target &lt; 5ms for ~361 legal placements</description></item>
    /// <item><description>Implementations should avoid LINQ in hot paths for allocation efficiency</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IEnumerable<IGameEvent> GetLegalMoves(GameState state);

    /// <summary>
    /// Checks if a specific event is legal and provides diagnostic information.
    /// </summary>
    /// <param name="event">The event to validate.</param>
    /// <param name="state">The current game state.</param>
    /// <returns>
    /// A <see cref="MoveValidation"/> result indicating whether the move is legal and, if not,
    /// providing structured rejection reason and explanation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is useful for:
    /// <list type="bullet">
    /// <item><description>UI feedback: Show why a user's attempted move was rejected</description></item>
    /// <item><description>Debugging: Understand rule evaluation failures</description></item>
    /// <item><description>Testing: Verify specific moves are correctly classified as legal/illegal</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Unlike <see cref="GetLegalMoves"/>, this method evaluates a single event without
    /// enumerating all possibilities, making it more efficient for single-move checks.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var moveToTest = new MovePieceGameEvent(piece, path);
    /// var validation = generator.Validate(moveToTest, state);
    /// if (!validation.IsLegal)
    /// {
    ///     Console.WriteLine($"Illegal: {validation.Explanation} ({validation.Reason})");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    MoveValidation Validate(IGameEvent @event, GameState state);

    /// <summary>
    /// Gets legal moves for a specific artifact (e.g., piece, player, card).
    /// </summary>
    /// <param name="artifact">The artifact to generate moves for.</param>
    /// <param name="state">The current game state.</param>
    /// <returns>
    /// An enumerable collection of legal events involving the specified artifact.
    /// The collection may be empty if the artifact has no legal actions.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides a filtered view of <see cref="GetLegalMoves"/> for a specific artifact,
    /// useful for UI interactions where a player selects a piece and wants to see its valid moves.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// // User clicks on a chess piece
    /// var legalMovesForPiece = generator.GetLegalMovesFor(selectedPiece, state);
    /// // Highlight destination squares in UI
    /// </code>
    /// </para>
    /// <para>
    /// The definition of "involving" is implementation-specific:
    /// <list type="bullet">
    /// <item><description>For pieces: moves where the piece is the actor</description></item>
    /// <item><description>For players: moves available to that player</description></item>
    /// <item><description>For cards: plays of that card</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IEnumerable<IGameEvent> GetLegalMovesFor(Artifact artifact, GameState state);
}
