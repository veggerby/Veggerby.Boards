using System;

namespace Veggerby.Boards.Core.Artifacts;

/// <summary>
/// Base immutable identity used for all structural game elements (tiles, pieces, players, dice, etc.).
/// </summary>
public abstract class Artifact : IEquatable<Artifact>
{
    /// <summary>
    /// Gets the artifact identifier (unique within its game context).
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Artifact"/> class.
    /// </summary>
    /// <param name="id">Identifier for the artifact.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    protected Artifact(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Invalid Artifact Id", nameof(id));
        }

        Id = id;
    }

    /// <inheritdoc />
    public bool Equals(Artifact other)
    {
        return other is not null
            && GetType().Equals(other.GetType())
            && string.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Artifact)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// Returns a string representation including runtime type and identifier.
    /// </summary>
    public override string ToString() => $"{GetType().Name} {Id}";
}