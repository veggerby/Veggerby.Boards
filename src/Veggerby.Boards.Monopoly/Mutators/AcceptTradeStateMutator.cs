using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles accepting a trade between players.
/// </summary>
public class AcceptTradeStateMutator : IStateMutator<AcceptTradeGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, AcceptTradeGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Get trade state
        var tradeState = gameState.GetExtras<TradeProposalState>()
            ?? throw new InvalidOperationException("No trade proposal found");

        if (!tradeState.IsActive || tradeState.ProposerOffer is null || tradeState.TargetOffer is null)
        {
            throw new InvalidOperationException("No active trade proposal");
        }

        // Get player states
        var proposerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, tradeState.ProposerOffer.PlayerId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Proposer state not found");

        var targetState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, tradeState.TargetOffer.PlayerId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Target state not found");

        // Get ownership state
        var ownership = gameState.GetExtras<PropertyOwnershipState>()
            ?? new PropertyOwnershipState();

        // Execute trade: Transfer cash
        var netCashToProposer = tradeState.TargetOffer.Cash - tradeState.ProposerOffer.Cash;
        var newProposerState = proposerState.AdjustCash(netCashToProposer);
        var newTargetState = targetState.AdjustCash(-netCashToProposer);

        // Transfer Get Out of Jail Free cards
        if (tradeState.ProposerOffer.GetOutOfJailCard)
        {
            newProposerState = newProposerState.WithGetOutOfJailCard(false);
            newTargetState = newTargetState.WithGetOutOfJailCard(true);
        }

        if (tradeState.TargetOffer.GetOutOfJailCard)
        {
            newTargetState = newTargetState.WithGetOutOfJailCard(false);
            newProposerState = newProposerState.WithGetOutOfJailCard(true);
        }

        // Transfer properties from proposer to target
        var newOwnership = ownership;
        foreach (var pos in tradeState.ProposerOffer.PropertyPositions)
        {
            newOwnership = newOwnership.SetOwner(pos, tradeState.TargetOffer.PlayerId);
        }

        // Transfer properties from target to proposer
        foreach (var pos in tradeState.TargetOffer.PropertyPositions)
        {
            newOwnership = newOwnership.SetOwner(pos, tradeState.ProposerOffer.PlayerId);
        }

        // Clear trade state
        var clearedTradeState = new TradeProposalState();

        // Update all states
        return gameState
            .ReplaceExtras(clearedTradeState)
            .ReplaceExtras(newOwnership)
            .Next(new List<MonopolyPlayerState> { newProposerState, newTargetState });
    }
}
