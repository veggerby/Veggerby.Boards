using System;

namespace Veggerby.Boards.Core.Artifacts;

public class Dice(string id) : Artifact(id), IEquatable<Dice>
{
    public bool Equals(Dice other) => base.Equals(other);

    public override bool Equals(object obj)
    {
        return Equals(obj as Dice);
    }

    public override int GetHashCode() => base.GetHashCode();
}