using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable state representing a piece that has been captured (removed from the board).
/// </summary>
/// <remarks>
/// A captured piece has no current tile affinity. Presence of this state for a piece identity indicates
/// it should be excluded from tile occupancy, movement resolution, attack generation and mobility counts.
/// </remarks>
public sealed class CapturedPieceState : ArtifactState<Piece>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CapturedPieceState"/> class.
    /// </summary>
    /// <param name="piece">Captured piece artifact.</param>
    public CapturedPieceState(Piece piece) : base(piece)
    {
        ArgumentNullException.ThrowIfNull(piece);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as CapturedPieceState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState other) => Equals(other as CapturedPieceState);

    private bool Equals(CapturedPieceState? other)
    {
        if (other is null)
        {
            return false;
        }
        return Artifact.Equals(other.Artifact);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact);
}