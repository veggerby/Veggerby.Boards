using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles rent payment.
/// </summary>
public class PayRentStateMutator : IStateMutator<PayRentGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, PayRentGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);
        var ownership = gameState.GetStates<PropertyOwnershipState>().FirstOrDefault();

        if (ownership is null)
        {
            throw new InvalidOperationException("Property ownership state not found");
        }

        var ownerId = ownership.GetOwner(@event.PropertyPosition);
        if (ownerId is null)
        {
            throw new InvalidOperationException($"Property at position {@event.PropertyPosition} is not owned");
        }

        var rent = RentCalculator.CalculateRent(squareInfo, ownerId, ownership, @event.DiceTotal);

        // Get payer state
        var payerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Payer.Id, StringComparison.Ordinal));

        if (payerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Payer.Id}");
        }

        // Get owner state
        var ownerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, ownerId, StringComparison.Ordinal));

        if (ownerState is null)
        {
            throw new InvalidOperationException($"Owner state not found for {ownerId}");
        }

        // Transfer rent (payer may go negative, bankruptcy handled separately)
        var newPayerState = payerState.AdjustCash(-rent);
        var newOwnerState = ownerState.AdjustCash(rent);

        return gameState.Next([newPayerState, newOwnerState]);
    }
}
