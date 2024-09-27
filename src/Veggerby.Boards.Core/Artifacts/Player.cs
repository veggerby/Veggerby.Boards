using System;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Player : Artifact, IEquatable<Player>
    {
        public Player(string id) : base(id)
        {
        }

        public bool Equals(Player other) => base.Equals(other);
    }
}