using System;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Tile : Artifact, IEquatable<Tile>
    {
        public Tile(string id) : base(id)
        {
        }

        public bool Equals(Tile other) => base.Equals(other);
    }
}