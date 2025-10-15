using System;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Immutable player artifact identified by id.
/// </summary>
public class Player(string id) : Artifact(id), IEquatable<Player>
{
    /// <inheritdoc />
    public bool Equals(Player? other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Player);

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();
}