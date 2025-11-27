using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles property purchase.
/// </summary>
public class BuyPropertyStateMutator : IStateMutator<BuyPropertyGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, BuyPropertyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Get current ownership state
        var ownership = gameState.GetExtras<PropertyOwnershipState>()
            ?? new PropertyOwnershipState();

        // Update ownership
        var newOwnership = ownership.SetOwner(@event.PropertyPosition, @event.Player.Id);

        // Get player state and deduct cash
        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Player.Id}");
        }

        var newPlayerState = playerState.AdjustCash(-squareInfo.Price);

        // Update ownership via ReplaceExtras and player state via Next
        var stateWithOwnership = gameState.ReplaceExtras(newOwnership);

        return stateWithOwnership.Next([newPlayerState]);
    }
}
