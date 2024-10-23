using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Builder.Artifacts;

public class PieceDirectionPatternDefinition(GameBuilder builder, PieceDefinition pieceDefinition) : DefinitionBase(builder)
{
    private readonly PieceDefinition _pieceDefinition = pieceDefinition;

    public string PieceId => _pieceDefinition.PieceId;
    public bool IsRepeatable { get; private set; } = false;
    public IEnumerable<string> DirectionIds { get; private set; }

    public PieceDefinition CanRepeat()
    {
        IsRepeatable = true;
        return _pieceDefinition;
    }

    public PieceDefinition DoesNotRepeat()
    {
        IsRepeatable = false;
        return _pieceDefinition;
    }

    public PieceDirectionPatternDefinition WithDirection(params string[] directions)
    {
        ArgumentNullException.ThrowIfNull(directions);

        if (!directions.Any())
        {
            throw new ArgumentException("Must provide at least one direction", nameof(directions));
        }

        if (directions.Any(x => string.IsNullOrEmpty(x)))
        {
            throw new ArgumentException("All directions must be non-null and non-empty", nameof(directions));
        }

        DirectionIds = (DirectionIds ?? Enumerable.Empty<string>()).Concat(directions).ToList();
        return this;
    }

    public PieceDefinition Done()
    {
        return _pieceDefinition;
    }
}