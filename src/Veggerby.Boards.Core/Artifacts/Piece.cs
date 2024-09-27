using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts.Patterns;

namespace Veggerby.Boards.Core.Artifacts;

public class Piece(string id, Player owner, IEnumerable<IPattern> patterns) : Artifact(id), IEquatable<Piece>
{
    public Player Owner { get; } = owner;
    public IEnumerable<IPattern> Patterns { get; } = (patterns ?? Enumerable.Empty<IPattern>()).ToList().AsReadOnly();

    public bool Equals(Piece other) => base.Equals(other);

    public override bool Equals(object obj)
    {
        return Equals(obj as Piece);
    }

    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(Id);
        code.Add(Owner);

        foreach (var pattern in Patterns)
        {
            code.Add(pattern);
        }

        return code.ToHashCode();
    }
}