using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo;

/// <summary>
/// Marker artifact for Ludo safe squares configuration.
/// </summary>
internal sealed class LudoSafeSquaresMarker : Artifact
{
    public LudoSafeSquaresMarker() : base("ludo-safe-squares-marker") { }
}

/// <summary>
/// Represents the set of safe squares where pieces cannot be captured.
/// </summary>
public class LudoSafeSquaresState : IArtifactState
{
    private static readonly LudoSafeSquaresMarker Marker = new();
    private readonly HashSet<string> _safeSquares;

    /// <summary>
    /// Gets the collection of safe square tile IDs.
    /// </summary>
    public IReadOnlyCollection<string> SafeSquares => _safeSquares;

    /// <summary>
    /// Initializes a new instance of the <see cref="LudoSafeSquaresState"/> class.
    /// </summary>
    /// <param name="safeSquares">Collection of safe square tile IDs.</param>
    public LudoSafeSquaresState(IEnumerable<string> safeSquares)
    {
        ArgumentNullException.ThrowIfNull(safeSquares);

        _safeSquares = new HashSet<string>(safeSquares, StringComparer.Ordinal);
    }

    /// <summary>
    /// Determines if the specified tile is a safe square.
    /// </summary>
    /// <param name="tileId">The tile ID to check.</param>
    /// <returns><c>true</c> if the tile is safe; otherwise <c>false</c>.</returns>
    public bool IsSafeSquare(string tileId)
    {
        return _safeSquares.Contains(tileId);
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is LudoSafeSquaresState lsss &&
               lsss._safeSquares.SetEquals(_safeSquares);
    }
}
