using System;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Represents a dice artifact (value state tracked separately in game state).
/// </summary>
public class Dice(string id) : Artifact(id), IEquatable<Dice>
{
    /// <inheritdoc />
    public bool Equals(Dice? other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as Dice);
    }

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();
}