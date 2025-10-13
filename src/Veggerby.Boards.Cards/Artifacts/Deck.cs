using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Cards;

/// <summary>
/// Immutable deck artifact representing a collection of ordered piles of cards (e.g., draw, discard, hand, in-play).
/// </summary>
public sealed class Deck : Artifact, IEquatable<Deck>
{
    /// <summary>
    /// Gets the defined pile identifiers for this deck.
    /// </summary>
    public IReadOnlyList<string> Piles { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Deck"/> class.
    /// </summary>
    /// <param name="id">Deck identifier.</param>
    /// <param name="piles">Pile identifiers (ordered, unique).</param>
    public Deck(string id, IEnumerable<string> piles) : base(id)
    {
        ArgumentNullException.ThrowIfNull(piles);
        var list = piles.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one pile required", nameof(piles));
        }
        if (list.Count != list.Distinct(StringComparer.Ordinal).Count())
        {
            throw new ArgumentException("Pile identifiers must be unique", nameof(piles));
        }
        Piles = list.AsReadOnly();
    }

    /// <inheritdoc />
    public bool Equals(Deck other) => base.Equals(other);
}