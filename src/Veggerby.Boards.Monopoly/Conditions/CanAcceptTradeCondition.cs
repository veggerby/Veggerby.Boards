using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can accept a trade.
/// </summary>
public class CanAcceptTradeCondition : IGameEventCondition<AcceptTradeGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, AcceptTradeGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Get trade state
        var tradeState = state.GetExtras<TradeProposalState>();
        if (tradeState is null || !tradeState.IsActive)
        {
            return ConditionResponse.Fail("No active trade proposal");
        }

        // Player must be the target of the trade
        if (tradeState.TargetOffer is null ||
            !string.Equals(tradeState.TargetOffer.PlayerId, @event.Player.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Player is not the target of this trade");
        }

        return ConditionResponse.Valid;
    }
}
