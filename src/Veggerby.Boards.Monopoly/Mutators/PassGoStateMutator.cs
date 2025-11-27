using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles passing Go (collecting $200).
/// </summary>
public class PassGoStateMutator : IStateMutator<PassGoGameEvent>
{
    /// <summary>
    /// The amount collected when passing Go.
    /// </summary>
    public const int PassGoAmount = 200;

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, PassGoGameEvent @event)
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

        var newPlayerState = playerState.AdjustCash(PassGoAmount);

        return gameState.Next([newPlayerState]);
    }
}
