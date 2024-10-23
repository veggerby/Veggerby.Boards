using System;

namespace Veggerby.Boards.Core.Artifacts;

public abstract class Artifact : IEquatable<Artifact>
{
    public string Id { get; }

    public Artifact(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Invalid Artifact Id", nameof(id));
        }

        Id = id;
    }

    public bool Equals(Artifact other)
    {
        return other is not null
            && GetType().Equals(other.GetType())
            && string.Equals(Id, other.Id);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Artifact)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    public override string ToString() => $"{GetType().Name} {Id}";
}