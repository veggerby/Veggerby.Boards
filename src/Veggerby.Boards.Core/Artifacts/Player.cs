using System;

namespace Veggerby.Boards.Core.Artifacts;

public class Player(string id) : Artifact(id), IEquatable<Player>
{
    public bool Equals(Player other) => base.Equals(other);

    public override bool Equals(object obj)
    {
        return Equals(obj as Player);
    }

    public override int GetHashCode() => base.GetHashCode();
}