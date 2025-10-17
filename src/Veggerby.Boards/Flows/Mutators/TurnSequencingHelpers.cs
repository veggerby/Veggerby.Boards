using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Internal helper methods supporting turn sequencing mutators (active player rotation and projection updates).
/// </summary>
internal static class TurnSequencingHelpers
{
    /// <summary>
    /// Produces a new <see cref="GameState"/> with active player rotated (if exactly one active player and multiple players exist).
    /// Returns the input state with only the supplied new turn state when rotation is not applicable (0 or 1 players, missing active, etc.).
    /// </summary>
    public static GameState ApplyTurnAndRotate(GameEngine engine, GameState prior, TurnState newTurnState)
    {
        ActivePlayerState? currentActive = null;
        foreach (var aps in prior.GetStates<ActivePlayerState>())
        {
            if (aps.IsActive)
            {
                currentActive = aps; break;
            }
        }
        if (currentActive is null)
        {
            return prior.Next([newTurnState]);
        }

        var playersEnumerable = engine.Game.Players;
        if (playersEnumerable is null)
        {
            return prior.Next([newTurnState]);
        }

        Player[] players;
        if (playersEnumerable is Player[] arr)
        {
            players = arr;
        }
        else
        {
            var tmp = new List<Player>();
            foreach (var p in playersEnumerable) { tmp.Add(p); }
            players = tmp.ToArray();
        }
        var total = players.Length;
        if (total <= 1)
        {
            return prior.Next([newTurnState]);
        }
        var idx = -1;
        for (var i = 0; i < total; i++) { if (players[i].Equals(currentActive.Artifact)) { idx = i; break; } }
        if (idx == -1)
        {
            return prior.Next([newTurnState]);
        }
        var nextIndex = (idx + 1) % total;
        var nextPlayer = players[nextIndex];
        if (nextPlayer.Equals(currentActive.Artifact))
        {
            return prior.Next([newTurnState]);
        }
        var previousProjection = new ActivePlayerState(currentActive.Artifact, false);
        var nextProjection = new ActivePlayerState(nextPlayer, true);
        return prior.Next([newTurnState, previousProjection, nextProjection]);
    }
}