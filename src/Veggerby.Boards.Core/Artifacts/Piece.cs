using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Patterns;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Piece : Artifact
    {
        public Player Owner { get; }
        public IEnumerable<IPattern> Patterns { get; }

        public Piece(string id, Player owner, IEnumerable<IPattern> patterns) : base(id)
        {
            Owner = owner;
            Patterns = patterns?.ToList().AsReadOnly();
        }
    }
}