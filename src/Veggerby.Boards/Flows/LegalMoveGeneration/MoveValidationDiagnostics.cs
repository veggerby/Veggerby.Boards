using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Flows.LegalMoveGeneration;

/// <summary>
/// Provides enhanced, localization-friendly diagnostic messages for move validation failures.
/// </summary>
/// <remarks>
/// <para>
/// This utility class generates human-readable explanations for <see cref="RejectionReason"/> values,
/// supporting parameterized templates for context-specific details (e.g., piece names, tile locations).
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var explanation = MoveValidationDiagnostics.GetExplanation(
///     RejectionReason.PathObstructed,
///     ("piece", piece.Id),
///     ("blocker", blockerPiece.Id),
///     ("tile", tile.Id)
/// );
/// // Returns: "Cannot move piece-id: path is obstructed by blocker-id at tile-id"
/// </code>
/// </para>
/// <para>
/// Localization: Template strings are designed to be easily replaceable with localized versions.
/// Applications can override <see cref="Templates"/> dictionary with culture-specific strings.
/// </para>
/// </remarks>
public static class MoveValidationDiagnostics
{
    /// <summary>
    /// Default explanation templates for each rejection reason.
    /// Applications can replace these with localized versions.
    /// </summary>
    /// <remarks>
    /// Templates use named placeholders in {braces} for parameter substitution.
    /// Common parameters: {piece}, {player}, {tile}, {from}, {to}, {blocker}, {resource}.
    /// </remarks>
    public static Dictionary<RejectionReason, string> Templates { get; set; } = new()
    {
        [RejectionReason.None] = "Move is legal",
        [RejectionReason.PieceNotOwned] = "Cannot move {piece}: piece belongs to {owner}, not active player {activePlayer}",
        [RejectionReason.PathObstructed] = "Cannot move {piece}: path is obstructed by {blocker} at {tile}",
        [RejectionReason.DestinationOccupied] = "Cannot move {piece} to {to}: destination is occupied by {occupant}",
        [RejectionReason.InvalidPattern] = "Cannot move {piece} from {from} to {to}: move does not match any valid movement pattern",
        [RejectionReason.WrongPhase] = "Cannot perform action: not allowed in current game phase ({phase})",
        [RejectionReason.InsufficientResources] = "Cannot perform action: insufficient {resource} (need {required}, have {available})",
        [RejectionReason.RuleViolation] = "Cannot perform action: violates game rule ({rule})",
        [RejectionReason.GameEnded] = "Cannot perform action: game has already ended"
    };

    /// <summary>
    /// Gets a formatted explanation for a rejection reason with context-specific parameters.
    /// </summary>
    /// <param name="reason">The rejection reason.</param>
    /// <param name="parameters">Named parameters for template substitution (name, value pairs).</param>
    /// <returns>A formatted explanation string with parameters substituted.</returns>
    /// <example>
    /// <code>
    /// var explanation = GetExplanation(
    ///     RejectionReason.PathObstructed,
    ///     ("piece", "white-rook"),
    ///     ("blocker", "white-bishop"),
    ///     ("tile", "e4")
    /// );
    /// </code>
    /// </example>
    public static string GetExplanation(RejectionReason reason, params (string name, string value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!Templates.TryGetValue(reason, out var template))
        {
            return $"Move rejected: {reason}";
        }

        var result = template;

        foreach (var (name, value) in parameters)
        {
            var placeholder = $"{{{name}}}";
            result = result.Replace(placeholder, value, StringComparison.Ordinal);
        }

        return result;
    }

    /// <summary>
    /// Gets a formatted explanation for a rejection reason with artifact-aware parameter extraction.
    /// </summary>
    /// <param name="reason">The rejection reason.</param>
    /// <param name="context">Contextual information including event, artifacts, and state details.</param>
    /// <returns>A formatted explanation string.</returns>
    public static string GetExplanation(RejectionReason reason, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Extract common parameters from context
        var parameters = new List<(string, string)>();

        if (context.Event is MovePieceGameEvent movePiece)
        {
            parameters.Add(("piece", movePiece.Piece.Id));

            if (movePiece.Path.From is not null)
            {
                parameters.Add(("from", movePiece.Path.From.Id));
            }

            if (movePiece.Path.To is not null)
            {
                parameters.Add(("to", movePiece.Path.To.Id));
            }

            if (movePiece.Piece.Owner is not null)
            {
                parameters.Add(("owner", movePiece.Piece.Owner.Id));
            }
        }

        if (context.ActivePlayer is not null)
        {
            parameters.Add(("activePlayer", context.ActivePlayer.Id));
        }

        if (context.BlockingPiece is not null)
        {
            parameters.Add(("blocker", context.BlockingPiece.Id));
        }

        if (context.BlockingTile is not null)
        {
            parameters.Add(("tile", context.BlockingTile.Id));
        }

        if (context.OccupyingPiece is not null)
        {
            parameters.Add(("occupant", context.OccupyingPiece.Id));
        }

        if (context.Phase is not null)
        {
            parameters.Add(("phase", context.Phase));
        }

        if (context.Rule is not null)
        {
            parameters.Add(("rule", context.Rule));
        }

        if (context.Resource is not null)
        {
            parameters.Add(("resource", context.Resource));

            if (context.RequiredAmount.HasValue)
            {
                parameters.Add(("required", context.RequiredAmount.Value.ToString()));
            }

            if (context.AvailableAmount.HasValue)
            {
                parameters.Add(("available", context.AvailableAmount.Value.ToString()));
            }
        }

        return GetExplanation(reason, parameters.ToArray());
    }
}

/// <summary>
/// Contextual information for generating detailed move validation diagnostics.
/// </summary>
/// <remarks>
/// This record captures relevant artifacts, state details, and metadata to produce
/// context-aware, human-readable explanation messages.
/// </remarks>
public sealed record ValidationContext
{
    /// <summary>Gets the event being validated.</summary>
    public IGameEvent Event { get; init; } = null!;

    /// <summary>Gets the active player (if applicable).</summary>
    public Player? ActivePlayer { get; init; }

    /// <summary>Gets the piece blocking the path (for PathObstructed).</summary>
    public Piece? BlockingPiece { get; init; }

    /// <summary>Gets the tile where the path is blocked (for PathObstructed).</summary>
    public Tile? BlockingTile { get; init; }

    /// <summary>Gets the piece occupying the destination (for DestinationOccupied).</summary>
    public Piece? OccupyingPiece { get; init; }

    /// <summary>Gets the current game phase (for WrongPhase).</summary>
    public string? Phase { get; init; }

    /// <summary>Gets the violated rule name (for RuleViolation).</summary>
    public string? Rule { get; init; }

    /// <summary>Gets the insufficient resource type (for InsufficientResources).</summary>
    public string? Resource { get; init; }

    /// <summary>Gets the required amount of the resource (for InsufficientResources).</summary>
    public int? RequiredAmount { get; init; }

    /// <summary>Gets the available amount of the resource (for InsufficientResources).</summary>
    public int? AvailableAmount { get; init; }
}
