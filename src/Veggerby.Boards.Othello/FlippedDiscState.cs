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
    /// Initializes a new instance of the <see cref="FlippedDiscState"/> class.
    /// </summary>
    /// <param name="piece">The piece (disc) that was flipped.</param>
    /// <param name="flippedTo">The color the disc was flipped to.</param>
    public FlippedDiscState(Piece piece, OthelloDiscColor flippedTo) : base(piece)
    {
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
        return other != null
            && base.Equals(other)
            && FlippedTo == other.FlippedTo;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), FlippedTo);
    }
}
