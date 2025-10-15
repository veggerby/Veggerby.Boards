using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable state representing a piece positioned on a specific tile.
/// </summary>
public class PieceState : ArtifactState<Piece>
{
    /// <summary>
    /// Gets the tile on which the piece currently resides.
    /// </summary>
    public Tile CurrentTile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PieceState"/> class.
    /// </summary>
    /// <param name="piece">The piece.</param>
    /// <param name="currentTile">The tile the piece is on.</param>
    public PieceState(Piece piece, Tile currentTile) : base(piece)
    {
        ArgumentNullException.ThrowIfNull(currentTile);

        CurrentTile = currentTile;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as PieceState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState other)
    {
        return Equals(other as PieceState);
    }

    /// <summary>
    /// Typed equality comparison.
    /// </summary>
    public bool Equals(PieceState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && CurrentTile.Equals(other.CurrentTile);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, CurrentTile);
}