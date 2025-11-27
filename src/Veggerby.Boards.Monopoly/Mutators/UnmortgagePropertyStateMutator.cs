using System;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that processes unmortgaging a property.
/// </summary>
public class UnmortgagePropertyStateMutator : IStateMutator<UnmortgagePropertyGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, UnmortgagePropertyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Unmortgage cost = mortgage value + 10% interest
        var mortgageValue = squareInfo.Price / 2;
        var unmortgageCost = mortgageValue + (mortgageValue / 10);

        // Update property ownership state (mark as unmortgaged)
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return state;
        }

        var newOwnership = ownership.Unmortgage(@event.PropertyPosition);

        // Update player cash
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return state;
        }

        var newPlayerState = playerState.AdjustCash(-unmortgageCost);

        return state.ReplaceExtras(newOwnership).Next([newPlayerState]);
    }
}
