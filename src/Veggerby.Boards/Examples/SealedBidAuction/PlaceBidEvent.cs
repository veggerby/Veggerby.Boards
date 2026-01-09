using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Examples.SealedBidAuction;

/// <summary>
/// Event representing a player placing a bid in a sealed-bid auction.
/// </summary>
/// <param name="Player">The player placing the bid.</param>
/// <param name="BidAmount">The amount being bid.</param>
public sealed record PlaceBidEvent(Player Player, int BidAmount) : IGameEvent
{
}
