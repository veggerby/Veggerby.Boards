using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.SealedBidAuction;

/// <summary>
/// Tracks a player's bid in a sealed-bid auction.
/// </summary>
public sealed class PlayerBidState : ArtifactState<Player>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerBidState"/> class.
    /// </summary>
    /// <param name="player">The player who placed the bid.</param>
    /// <param name="bidAmount">The amount bid by the player.</param>
    public PlayerBidState(Player player, int bidAmount)
        : base(player)
    {
        BidAmount = bidAmount;
    }

    /// <summary>
    /// Gets the bid amount.
    /// </summary>
    public int BidAmount { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as PlayerBidState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState? other)
    {
        return Equals(other as PlayerBidState);
    }

    /// <summary>
    /// Determines equality with another <see cref="PlayerBidState"/>.
    /// </summary>
    /// <param name="other">Other state.</param>
    /// <returns><c>true</c> if both reference the same player and have the same bid amount.</returns>
    public bool Equals(PlayerBidState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact) && BidAmount == other.BidAmount;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Artifact, BidAmount);
    }
}
