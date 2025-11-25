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
/// Mutator that eliminates a bankrupt player and transfers their assets.
/// </summary>
public class EliminatePlayerStateMutator : IStateMutator<EliminatePlayerGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EliminatePlayerGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Player.Id}");
        }

        var newStates = new List<IArtifactState>();

        // Mark player as bankrupt
        var bankruptPlayerState = playerState.MarkBankrupt();
        newStates.Add(bankruptPlayerState);

        // Track if we need to update ownership
        PropertyOwnershipState? newOwnership = null;

        // Transfer properties to the creditor (if any) or release to bank
        var ownership = gameState.GetExtras<PropertyOwnershipState>();
        if (ownership is not null)
        {
            var playerProperties = ownership.GetPropertiesOwnedBy(@event.Player.Id).ToList();
            newOwnership = ownership;

            foreach (var position in playerProperties)
            {
                // If bankrupted by another player, transfer property to them
                // Otherwise, release to bank (null owner)
                newOwnership = newOwnership.SetOwner(position, @event.BankruptedBy?.Id);
            }

            // Only track if we actually changed properties
            if (playerProperties.Count == 0)
            {
                newOwnership = null;
            }
        }

        // If bankrupted by another player, transfer any remaining cash
        if (@event.BankruptedBy is not null && playerState.Cash > 0)
        {
            var creditorState = gameState.GetStates<MonopolyPlayerState>()
                .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.BankruptedBy.Id, StringComparison.Ordinal));

            if (creditorState is not null)
            {
                var newCreditorState = creditorState.AdjustCash(playerState.Cash);
                newStates.Add(newCreditorState);
            }
        }

        // First apply player state changes
        var result = gameState.Next(newStates);

        // Then apply ownership changes if any
        if (newOwnership is not null)
        {
            result = result.ReplaceExtras(newOwnership);
        }

        return result;
    }
}
