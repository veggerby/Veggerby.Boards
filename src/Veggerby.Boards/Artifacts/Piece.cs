using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts.Patterns;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Immutable piece artifact owned by a player and configured with movement patterns.
/// </summary>
public class Piece : Artifact, IEquatable<Piece>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Piece"/> class.
    /// </summary>
    /// <param name="id">Piece identifier.</param>
    /// <param name="owner">Owning player.</param>
    /// <param name="patterns">Movement patterns.</param>
    /// <param name="metadata">Optional game-specific metadata.</param>
    public Piece(string id, Player owner, IEnumerable<IPattern> patterns, IPieceMetadata? metadata = null) : base(id)
    {
        Owner = owner;
        Patterns = (patterns ?? Enumerable.Empty<IPattern>()).ToList().AsReadOnly();
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the owning player.
    /// </summary>
    public Player Owner { get; }

    /// <summary>
    /// Gets the movement patterns associated with the piece.
    /// </summary>
    public IEnumerable<IPattern> Patterns { get; }

    /// <summary>
    /// Gets optional game-specific metadata attached to this piece (e.g., chess role/color).
    /// </summary>
    public IPieceMetadata? Metadata { get; }

    /// <inheritdoc />
    public bool Equals(Piece? other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object? obj)
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