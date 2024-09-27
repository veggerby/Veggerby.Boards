using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts.Patterns;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Piece : Artifact, IEquatable<Piece>
    {
        public Player Owner { get; }
        public IEnumerable<IPattern> Patterns { get; }

        public Piece(string id, Player owner, IEnumerable<IPattern> patterns) : base(id)
        {
            Owner = owner;
            Patterns = (patterns ?? Enumerable.Empty<IPattern>()).ToList().AsReadOnly();
        }

        public bool Equals(Piece other) => base.Equals(other);
    }
}