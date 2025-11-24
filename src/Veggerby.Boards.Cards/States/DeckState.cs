using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.States;

using Veggerby.Boards.Cards.Artifacts;
namespace Veggerby.Boards.Cards.States;

/// <summary>
/// Immutable state representing ordered piles for a <see cref="Deck"/> and optional supply counts.
/// </summary>
public sealed class DeckState : ArtifactState<Deck>
{
    /// <summary>Gets the ordered cards per pile id.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<Card>> Piles
    {
        get;
    }

    /// <summary>Gets supply counts for card identifiers (optional).</summary>
    public IReadOnlyDictionary<string, int> Supply
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeckState"/> class.
    /// </summary>
    public DeckState(Deck deck, IDictionary<string, IList<Card>> piles, IDictionary<string, int>? supply = null) : base(deck)
    {
        ArgumentNullException.ThrowIfNull(deck);
        ArgumentNullException.ThrowIfNull(piles);
        // Validate deck piles
        foreach (var required in deck.Piles)
        {
            if (!piles.ContainsKey(required))
            {
                throw new ArgumentException($"Missing pile '{required}'", nameof(piles));
            }
        }
        // Freeze
        var frozen = new Dictionary<string, IReadOnlyList<Card>>(StringComparer.Ordinal);
        foreach (var kv in piles)
        {
            if (kv.Value is null)
            {
                frozen[kv.Key] = Array.Empty<Card>();
            }
            else
            {
                frozen[kv.Key] = kv.Value.ToList().AsReadOnly();
            }
        }
        Piles = frozen;
        Supply = supply is null
            ? new Dictionary<string, int>(StringComparer.Ordinal)
            : new Dictionary<string, int>(supply, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as DeckState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState other) => Equals(other as DeckState);

    private bool Equals(DeckState? other)
    {
        if (other is null)
        {
            return false;
        }
        if (!Artifact.Equals(other.Artifact))
        {
            return false;
        }
        if (Piles.Count != other.Piles.Count || Supply.Count != other.Supply.Count)
        {
            return false;
        }
        foreach (var k in Piles.Keys)
        {
            if (!other.Piles.ContainsKey(k))
                return false;
            var a = Piles[k];
            var b = other.Piles[k];
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                    return false;
            }
        }
        foreach (var kv in Supply)
        {
            if (!other.Supply.TryGetValue(kv.Key, out var v) || v != kv.Value)
                return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(GetType());
        code.Add(Artifact);
        foreach (var k in Piles.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            code.Add(k);
            foreach (var c in Piles[k])
            {
                code.Add(c);
            }
        }
        foreach (var kv in Supply.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            code.Add(kv.Key);
            code.Add(kv.Value);
        }
        return code.ToHashCode();
    }
}