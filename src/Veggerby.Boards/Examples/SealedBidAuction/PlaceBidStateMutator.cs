using System;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.SealedBidAuction;

/// <summary>
/// Mutator handling <see cref="PlaceBidEvent"/> by recording the player's bid.
/// </summary>
internal sealed class PlaceBidStateMutator : IStateMutator<PlaceBidEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, PlaceBidEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var bidState = new PlayerBidState(@event.Player, @event.BidAmount);
        return gameState.Next([bidState]);
    }
}
