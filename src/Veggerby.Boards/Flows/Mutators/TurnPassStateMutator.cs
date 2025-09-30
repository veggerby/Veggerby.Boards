using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator that advances directly to the next turn (increment TurnNumber, reset Segment to Start,
/// and rotate active player) when a <see cref="TurnPassEvent"/> is handled under turn sequencing.
/// </summary>
/// <remarks>
/// This is a shortcut equivalent to ending all remaining segments in the current turn. It is inert
/// when turn sequencing is disabled or no current <see cref="TurnState"/> exists.
/// </remarks>
/// <summary>
/// State mutator applying a pass: increments numeric turn and pass streak, resets segment, rotates active player.
/// </summary>
internal sealed class TurnPassStateMutator : IStateMutator<TurnPassEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, TurnPassEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return gameState;
        }

        TurnState currentTurnState = null;
        foreach (var ts in gameState.GetStates<TurnState>()) { currentTurnState = ts; break; }
        if (currentTurnState is null) { return gameState; }

        var advancedTurnState = new TurnState(currentTurnState.Artifact, currentTurnState.TurnNumber + 1, TurnSegment.Start, currentTurnState.PassStreak + 1);

        ActivePlayerState currentActive = null;
        foreach (var aps in gameState.GetStates<ActivePlayerState>()) { if (aps.IsActive) { currentActive = aps; break; } }
        if (currentActive is null) { return gameState.Next([advancedTurnState]); }
        var playersEnumerable = engine.Game.Players;
        if (playersEnumerable is null) { return gameState.Next([advancedTurnState]); }
        Player[] players;
        if (playersEnumerable is Player[] arr)
        {
            players = arr;
        }
        else
        {
            var tempList = new System.Collections.Generic.List<Player>();
            foreach (var p in playersEnumerable) { tempList.Add(p); }
            players = tempList.ToArray();
        }
        var total = players.Length;
        if (total <= 1) { return gameState.Next([advancedTurnState]); }
        var idx = -1;
        for (var i = 0; i < total; i++) { if (players[i].Equals(currentActive.Artifact)) { idx = i; break; } }
        if (idx == -1) { return gameState.Next([advancedTurnState]); }
        var nextIndex = (idx + 1) % total;
        var nextPlayer = players[nextIndex];
        if (nextPlayer.Equals(currentActive.Artifact)) { return gameState.Next([advancedTurnState]); }
        var previousPlayerProjection = new ActivePlayerState(currentActive.Artifact, false);
        var nextPlayerProjection = new ActivePlayerState(nextPlayer, true);
        return gameState.Next([advancedTurnState, previousPlayerProjection, nextPlayerProjection]);
    }
}