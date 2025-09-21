using System;

namespace Veggerby.Boards.Core.Artifacts;

/// <summary>
/// Immutable board tile artifact identified by its id and used as endpoints in relations and paths.
/// </summary>
public class Tile(string id) : Artifact(id), IEquatable<Tile>
{
    /// <inheritdoc />
    public bool Equals(Tile other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as Tile);
    }

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();
}