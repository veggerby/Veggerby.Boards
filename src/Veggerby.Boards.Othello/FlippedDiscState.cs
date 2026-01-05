using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello;

/// <summary>
/// State tracking that a disc has been flipped from one color to another in Othello.
/// </summary>
/// <remarks>
/// This state is used to represent disc flipping without modifying the piece artifact itself.
/// The current color of a disc is determined by counting the number of flip states for that disc.
/// An even number of flips (including zero) means the disc is its original color.
/// An odd number of flips means the disc is the opposite color.
/// This state also tracks the tile location, replacing PieceState for flipped discs.
/// </remarks>
public sealed class FlippedDiscState : ArtifactState<Piece>
{
    /// <summary>
    /// Gets the color the disc was flipped to.
    /// </summary>
    public OthelloDiscColor FlippedTo
    {
        get;
    }

    /// <summary>
    /// Gets the tile on which the disc currently resides.
    /// </summary>
    public Tile CurrentTile
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlippedDiscState"/> class.
    /// </summary>
    /// <param name="piece">The piece (disc) that was flipped.</param>
    /// <param name="currentTile">The tile the disc is on.</param>
    /// <param name="flippedTo">The color the disc was flipped to.</param>
    public FlippedDiscState(Piece piece, Tile currentTile, OthelloDiscColor flippedTo) : base(piece)
    {
        ArgumentNullException.ThrowIfNull(currentTile);

        CurrentTile = currentTile;
        FlippedTo = flippedTo;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as FlippedDiscState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState? other) => Equals(other as FlippedDiscState);

    /// <summary>
    /// Checks equality with another flipped disc state.
    /// </summary>
    /// <param name="other">Other state.</param>
    /// <returns>True if states are equal.</returns>
    public bool Equals(FlippedDiscState? other)
    {
        if (other == null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && CurrentTile.Equals(other.CurrentTile) && FlippedTo == other.FlippedTo;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), CurrentTile, FlippedTo);
    }
}
