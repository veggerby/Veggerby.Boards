using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition of movement directions for a piece, optionally marking the pattern repeatable.
/// </summary>
public class PieceDirectionPatternDefinition(GameBuilder builder, PieceDefinition pieceDefinition) : DefinitionBase(builder)
{
    private readonly PieceDefinition _pieceDefinition = pieceDefinition;

    /// <summary>
    /// Gets the piece identifier this pattern belongs to.
    /// </summary>
    public string PieceId => _pieceDefinition.PieceId;
    /// <summary>
    /// Gets a value indicating whether the pattern may repeat beyond the first step.
    /// </summary>
    public bool IsRepeatable { get; private set; } = false;
    /// <summary>
    /// Gets the configured direction identifiers composing the pattern.
    /// </summary>
    public IEnumerable<string> DirectionIds { get; private set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Marks the pattern as repeatable.
    /// </summary>
    public PieceDefinition CanRepeat()
    {
        IsRepeatable = true;
        return _pieceDefinition;
    }

    /// <summary>
    /// Marks the pattern as non-repeatable.
    /// </summary>
    public PieceDefinition DoesNotRepeat()
    {
        IsRepeatable = false;
        return _pieceDefinition;
    }

    /// <summary>
    /// Adds one or more direction identifiers to the pattern.
    /// </summary>
    /// <param name="directions">Direction identifiers.</param>
    /// <exception cref="ArgumentException">Thrown when no directions provided or any is null/empty.</exception>
    public PieceDirectionPatternDefinition WithDirection(params string[] directions)
    {
        ArgumentNullException.ThrowIfNull(directions, nameof(directions));

        if (!directions.Any())
        {
            throw new ArgumentException(ExceptionMessages.AtLeastOneDirectionRequired, nameof(directions));
        }

        if (directions.Any(x => string.IsNullOrWhiteSpace(x)))
        {
            throw new ArgumentException(ExceptionMessages.DirectionsInvalid, nameof(directions));
        }

        DirectionIds = [.. (DirectionIds ?? Enumerable.Empty<string>()), .. directions];
        return this;
    }

    /// <summary>
    /// Returns to the parent piece definition.
    /// </summary>
    public PieceDefinition Done()
    {
        return _pieceDefinition;
    }
}