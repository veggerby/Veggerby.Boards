using System;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Dice : Artifact, IEquatable<Dice>
    {
        public Dice(string id) : base(id)
        {
        }

        public bool Equals(Dice other) => base.Equals(other);
    }
}