using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

public class PieceState : ArtifactState<Piece>
{
    public Tile CurrentTile { get; }

    public PieceState(Piece piece, Tile currentTile) : base(piece)
    {
        ArgumentNullException.ThrowIfNull(currentTile);

        CurrentTile = currentTile;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as PieceState);
    }

    public override bool Equals(IArtifactState other)
    {
        return Equals(other as PieceState);
    }

    public bool Equals(PieceState other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && CurrentTile.Equals(other.CurrentTile);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, CurrentTile);
}