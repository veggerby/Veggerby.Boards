using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles starting a property auction.
/// </summary>
public class StartAuctionStateMutator : IStateMutator<StartAuctionGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, StartAuctionGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Get or create auction state
        var auctionState = gameState.GetExtras<AuctionState>()
            ?? new AuctionState();

        // Start the auction
        var playerIds = @event.EligiblePlayers.Select(p => p.Id);
        var newAuctionState = auctionState.StartAuction(@event.PropertyPosition, playerIds);

        return gameState.ReplaceExtras(newAuctionState);
    }
}
