using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Cards.States;

/// <summary>
/// Immutable state representing a single card with visibility information.
/// </summary>
public sealed class CardState : ArtifactState<Card>
{
    /// <summary>
    /// Gets a value indicating whether the card is face-up (visible to all).
    /// </summary>
    public bool IsFaceUp
    {
        get;
    }

    /// <summary>
    /// Gets the owner of the card (null if no owner, e.g., in deck or discard).
    /// </summary>
    public Player? Owner
    {
        get;
    }

    /// <summary>
    /// Gets the pile identifier where this card is located.
    /// </summary>
    public string PileId
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CardState"/> class.
    /// </summary>
    /// <param name="card">The card artifact.</param>
    /// <param name="pileId">The pile identifier where the card is located.</param>
    /// <param name="isFaceUp">Whether the card is face-up.</param>
    /// <param name="owner">The owner of the card (null if no owner).</param>
    public CardState(Card card, string pileId, bool isFaceUp = false, Player? owner = null)
        : base(card)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(pileId);

        PileId = pileId;
        IsFaceUp = isFaceUp;
        Owner = owner;
    }

    /// <summary>
    /// Gets the visibility of this card based on its state.
    /// </summary>
    /// <remarks>
    /// - Public: Face-up cards (everyone sees)
    /// - Private: Face-down with owner (only owner sees)
    /// - Hidden: No owner, e.g., in deck (no one sees)
    /// </remarks>
    public Visibility Visibility =>
        IsFaceUp
            ? Visibility.Public
            : (Owner != null ? Visibility.Private : Visibility.Hidden);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as CardState);

    /// <inheritdoc />
    public override bool Equals(IArtifactState? other) => Equals(other as CardState);

    private bool Equals(CardState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact)
            && PileId == other.PileId
            && IsFaceUp == other.IsFaceUp
            && Equals(Owner, other.Owner);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Artifact, PileId, IsFaceUp, Owner);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var visibility = IsFaceUp ? "face-up" : (Owner != null ? $"face-down (owner: {Owner.Id})" : "face-down (hidden)");
        return $"Card {Artifact.Id} in {PileId} ({visibility})";
    }
}
