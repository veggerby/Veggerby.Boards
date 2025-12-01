using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that switches to the next active player at end of turn.
/// </summary>
/// <remarks>
/// This mutator handles turn switching by:
/// <list type="bullet">
/// <item>Finding the currently active player</item>
/// <item>Marking them as inactive</item>
/// <item>Marking the next player (circularly) as active</item>
/// </list>
/// </remarks>
public class EndTurnStateMutator : IStateMutator<EndTurnGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EndTurnGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var players = engine.Game.Players.ToList();
        var currentActive = gameState.GetStates<ActivePlayerState>()
            .FirstOrDefault(aps => aps.IsActive);

        if (currentActive is null)
        {
            // No active player found, cannot switch
            return gameState;
        }

        int currentIndex = players.FindIndex(p => p.Equals(currentActive.Artifact));

        if (currentIndex == -1)
        {
            throw new InvalidOperationException($"Active player {currentActive.Artifact.Id} not found in player list");
        }

        int nextIndex = (currentIndex + 1) % players.Count;

        var updates = new List<IArtifactState>
        {
            new ActivePlayerState(currentActive.Artifact, false),
            new ActivePlayerState(players[nextIndex], true)
        };

        return gameState.Next(updates);
    }
}
