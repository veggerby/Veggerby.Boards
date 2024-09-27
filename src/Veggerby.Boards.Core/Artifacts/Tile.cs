using System;

namespace Veggerby.Boards.Core.Artifacts;

public class Tile(string id) : Artifact(id), IEquatable<Tile>
{
    public bool Equals(Tile other) => base.Equals(other);

    public override bool Equals(object obj)
    {
        return Equals(obj as Tile);
    }

    public override int GetHashCode() => base.GetHashCode();
}