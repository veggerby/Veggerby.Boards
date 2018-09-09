using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class PieceState : ArtifactState<Piece>
    {
        public Tile CurrentTile { get; }

        public PieceState(Piece piece, Tile currentTile) : base(piece)
        {
            if (currentTile == null)
            {
                throw new ArgumentNullException(nameof(currentTile));
            }

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
            if (other == null)
            {
                return false;
            }

            return Artifact.Equals(other.Artifact) && CurrentTile.Equals(other.CurrentTile);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.GetType().GetHashCode();
                hashCode = (hashCode*397) ^ Artifact.GetHashCode();
                hashCode = (hashCode*397) ^ CurrentTile.GetHashCode();
                return hashCode;
            }
        }
    }
}