using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts.Patterns;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Immutable piece artifact owned by a player and configured with movement patterns.
/// </summary>
public class Piece(string id, Player owner, IEnumerable<IPattern> patterns) : Artifact(id), IEquatable<Piece>
{
    /// <summary>
    /// Gets the owning player.
    /// </summary>
    public Player Owner { get; } = owner;
    /// <summary>
    /// Gets the movement patterns associated with the piece.
    /// </summary>
    public IEnumerable<IPattern> Patterns { get; } = (patterns ?? Enumerable.Empty<IPattern>()).ToList().AsReadOnly();

    /// <inheritdoc />
    public bool Equals(Piece other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as Piece);
    }

    /// <inheritdoc />
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