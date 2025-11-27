using System;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that processes mortgaging a property.
/// </summary>
public class MortgagePropertyStateMutator : IStateMutator<MortgagePropertyGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, MortgagePropertyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);
        var mortgageValue = squareInfo.Price / 2;

        // Update property ownership state (mark as mortgaged)
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return state;
        }

        var newOwnership = ownership.Mortgage(@event.PropertyPosition);

        // Update player cash
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return state;
        }

        var newPlayerState = playerState.AdjustCash(mortgageValue);

        return state.ReplaceExtras(newOwnership).Next([newPlayerState]);
    }
}
