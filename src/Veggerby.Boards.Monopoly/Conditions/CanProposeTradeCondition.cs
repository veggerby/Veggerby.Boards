using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a trade can be proposed.
/// </summary>
public class CanProposeTradeCondition : IGameEventCondition<ProposeTradeGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, ProposeTradeGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Cannot trade with yourself
        if (string.Equals(@event.Proposer.Id, @event.Target.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Cannot trade with yourself");
        }

        // Check no trade is already active
        var tradeState = state.GetExtras<TradeProposalState>();
        if (tradeState is not null && tradeState.IsActive)
        {
            return ConditionResponse.Fail("A trade is already in progress");
        }

        // Get player states
        var proposerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Proposer.Id, StringComparison.Ordinal));

        var targetState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Target.Id, StringComparison.Ordinal));

        if (proposerState is null || targetState is null)
        {
            return ConditionResponse.Ignore("Player states not found");
        }

        // Check proposer can afford offered cash
        if (@event.OfferedCash > proposerState.Cash)
        {
            return ConditionResponse.Fail("Proposer does not have enough cash");
        }

        // Check target has requested cash
        if (@event.RequestedCash > targetState.Cash)
        {
            return ConditionResponse.Fail("Target does not have enough cash");
        }

        // Get ownership state
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        // Check proposer owns all offered properties
        foreach (var pos in @event.OfferedProperties)
        {
            var owner = ownership.GetOwner(pos);
            if (!string.Equals(owner, @event.Proposer.Id, StringComparison.Ordinal))
            {
                return ConditionResponse.Fail($"Proposer does not own property at position {pos}");
            }

            // Cannot trade properties with houses
            if (ownership.GetHouseCount(pos) > 0)
            {
                return ConditionResponse.Fail($"Cannot trade property at position {pos} - has houses");
            }
        }

        // Check target owns all requested properties
        foreach (var pos in @event.RequestedProperties)
        {
            var owner = ownership.GetOwner(pos);
            if (!string.Equals(owner, @event.Target.Id, StringComparison.Ordinal))
            {
                return ConditionResponse.Fail($"Target does not own property at position {pos}");
            }

            // Cannot trade properties with houses
            if (ownership.GetHouseCount(pos) > 0)
            {
                return ConditionResponse.Fail($"Cannot trade property at position {pos} - has houses");
            }
        }

        // Check Get Out of Jail Free card availability
        if (@event.OfferedGetOutOfJailCard && !proposerState.HasGetOutOfJailCard)
        {
            return ConditionResponse.Fail("Proposer does not have a Get Out of Jail Free card");
        }

        if (@event.RequestedGetOutOfJailCard && !targetState.HasGetOutOfJailCard)
        {
            return ConditionResponse.Fail("Target does not have a Get Out of Jail Free card");
        }

        // Trade must have some content
        if (@event.OfferedCash == 0 &&
            @event.OfferedProperties.Count == 0 &&
            !@event.OfferedGetOutOfJailCard &&
            @event.RequestedCash == 0 &&
            @event.RequestedProperties.Count == 0 &&
            !@event.RequestedGetOutOfJailCard)
        {
            return ConditionResponse.Fail("Trade must include something");
        }

        return ConditionResponse.Valid;
    }
}
