using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that marks the game as ended when only one player remains.
/// </summary>
public class MonopolyEndGameMutator : IStateMutator<IGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var playerStates = gameState.GetStates<MonopolyPlayerState>().ToList();
        var ownership = gameState.GetStates<PropertyOwnershipState>().FirstOrDefault();

        // Find the winner (last non-bankrupt player)
        var activePlayers = playerStates.Where(ps => !ps.IsBankrupt).ToList();
        if (activePlayers.Count != 1)
        {
            throw new InvalidOperationException($"Expected exactly 1 active player, found {activePlayers.Count}");
        }

        var winner = activePlayers[0].Player;

        // Build results
        var results = new List<MonopolyPlayerResult>();

        // Winner first
        var winnerState = activePlayers[0];
        var winnerProperties = ownership?.GetPropertiesOwnedBy(winner.Id).Count() ?? 0;
        results.Add(new MonopolyPlayerResult
        {
            Player = winner,
            IsBankrupt = false,
            FinalCash = winnerState.Cash,
            PropertiesOwned = winnerProperties
        });

        // Bankrupt players (in order of bankruptcy - we don't track this, so just add them)
        foreach (var ps in playerStates.Where(ps => ps.IsBankrupt))
        {
            results.Add(new MonopolyPlayerResult
            {
                Player = ps.Player,
                IsBankrupt = true,
                FinalCash = 0,
                PropertiesOwned = 0
            });
        }

        var outcome = new MonopolyOutcomeState(winner, results);
        var endedState = new GameEndedState();

        return gameState.Next([outcome, endedState]);
    }
}
