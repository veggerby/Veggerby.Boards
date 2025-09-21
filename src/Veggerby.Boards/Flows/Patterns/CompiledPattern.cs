using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Flows.Patterns;

/// <summary>
/// Enumerates supported compiled movement pattern kinds (minimal v1 scope).
/// </summary>
internal enum CompiledPatternKind
{
    Fixed = 0,
    Ray = 1,
    MultiRay = 2
}

/// <summary>
/// Represents a single compiled pattern independent of board topology.
/// </summary>
internal sealed class CompiledPattern
{
    public CompiledPatternKind Kind { get; }
    public Direction[] Directions { get; } // For Fixed this is the exact ordered steps; for Ray a single element; for MultiRay the candidate directions.
    public bool IsRepeatable { get; }

    private CompiledPattern(CompiledPatternKind kind, Direction[] directions, bool isRepeatable)
    {
        Kind = kind;
        Directions = directions;
        IsRepeatable = isRepeatable;
    }

    public static CompiledPattern Fixed(Direction[] steps)
    {
        if (steps is null || steps.Length == 0) throw new ArgumentException("Empty fixed steps", nameof(steps));
        return new CompiledPattern(CompiledPatternKind.Fixed, steps, false);
    }

    public static CompiledPattern Ray(Direction direction, bool repeatable)
    {
        if (direction is null) throw new ArgumentNullException(nameof(direction));
        return new CompiledPattern(CompiledPatternKind.Ray, new[] { direction }, repeatable);
    }

    public static CompiledPattern MultiRay(Direction[] directions, bool repeatable)
    {
        if (directions is null || directions.Length == 0) throw new ArgumentException("Empty multi-ray directions", nameof(directions));
        return new CompiledPattern(CompiledPatternKind.MultiRay, directions, repeatable);
    }
}

/// <summary>
/// Per-piece compiled pattern bundle.
/// </summary>
internal sealed class CompiledPiecePatterns
{
    public string PieceId { get; }
    public IReadOnlyList<CompiledPattern> Patterns { get; }

    public CompiledPiecePatterns(string pieceId, IReadOnlyList<CompiledPattern> patterns)
    {
        PieceId = pieceId ?? throw new ArgumentNullException(nameof(pieceId));
        Patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
    }
}

/// <summary>
/// Root compiled pattern table (index by piece id).
/// </summary>
internal sealed class CompiledPatternTable
{
    private readonly Dictionary<string, CompiledPiecePatterns> _byPiece = new();
    public IReadOnlyDictionary<string, CompiledPiecePatterns> ByPiece => _byPiece;

    public void Add(CompiledPiecePatterns entry)
    {
        _byPiece[entry.PieceId] = entry;
    }
}