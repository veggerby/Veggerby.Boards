using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Placeholder state representing a piece whose details are hidden from the viewer.
/// </summary>
/// <remarks>
/// <para>
/// Redacted piece states are used in player-projected views to indicate the presence of a piece
/// without revealing its specific identity or properties. This enables imperfect-information games
/// where opponents can see that a piece exists on a tile but cannot see which piece it is.
/// </para>
/// <para>
/// Example use cases:
/// <list type="bullet">
/// <item><description>Stratego: Opponent sees a piece on a tile but not its rank</description></item>
/// <item><description>Battleship: Opponent sees a ship segment but not which ship</description></item>
/// <item><description>Card games: Opponent sees face-down cards but not their values</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class RedactedPieceState : ArtifactState<Piece>
{
    /// <summary>
    /// Gets the tile where the redacted piece is located.
    /// </summary>
    public Tile CurrentTile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedactedPieceState"/> class.
    /// </summary>
    /// <param name="piece">The piece artifact (identity only, no details exposed).</param>
    /// <param name="currentTile">The tile where the piece is located.</param>
    public RedactedPieceState(Piece piece, Tile currentTile) : base(piece)
    {
        ArgumentNullException.ThrowIfNull(currentTile);

        CurrentTile = currentTile;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as RedactedPieceState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState other)
    {
        return Equals(other as RedactedPieceState);
    }

    /// <summary>
    /// Typed equality comparison.
    /// </summary>
    public bool Equals(RedactedPieceState? other)
    {
        if (other is null)
        {
            return false;
        }

        // Redacted states are equal if they refer to the same artifact and tile
        // Note: We compare artifacts by identity (not piece details) since this is a redaction
        return Artifact.Equals(other.Artifact) && CurrentTile.Equals(other.CurrentTile);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, CurrentTile);

    /// <inheritdoc />
    public override string ToString() => $"RedactedPiece on {CurrentTile.Id}";
}
