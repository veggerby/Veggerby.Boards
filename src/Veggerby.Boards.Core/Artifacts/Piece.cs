using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Patterns;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Piece : Artifact
    {
        public IEnumerable<IPattern> Patterns { get; }

        public Piece(string id, IEnumerable<IPattern> patterns) : base(id)
        {
            Patterns = (patterns ?? Enumerable.Empty<IPattern>()).ToList();
        }
    }
}