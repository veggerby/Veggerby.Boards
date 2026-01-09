using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.SealedBidAuction;

/// <summary>
/// Condition validating that a <see cref="PlaceBidEvent"/> is applicable.
/// </summary>
internal sealed class PlaceBidCondition : IGameEventCondition<PlaceBidEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PlaceBidEvent @event)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.BidAmount < 0)
        {
            return ConditionResponse.Invalid;
        }

        return ConditionResponse.Valid;
    }
}
