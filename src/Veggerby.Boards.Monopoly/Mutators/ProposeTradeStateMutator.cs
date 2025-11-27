using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles proposing a trade between players.
/// </summary>
public class ProposeTradeStateMutator : IStateMutator<ProposeTradeGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, ProposeTradeGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Create trade proposal
        var proposerOffer = new TradeOffer(
            @event.Proposer.Id,
            @event.OfferedCash,
            @event.OfferedProperties,
            @event.OfferedGetOutOfJailCard);

        var targetOffer = new TradeOffer(
            @event.Target.Id,
            @event.RequestedCash,
            @event.RequestedProperties,
            @event.RequestedGetOutOfJailCard);

        var tradeState = new TradeProposalState(proposerOffer, targetOffer);

        return gameState.ReplaceExtras(tradeState);
    }
}
