using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Advances the global turn timeline. For non-terminal segments it progresses to the next segment; for the final
/// segment it increments the numeric turn, resets the segment to <see cref="TurnSegment.Start"/>, and (when present)
/// rotates the active player states in player enumeration order.
/// </summary>
/// <remarks>
/// Player rotation is a compatibility projection for legacy <see cref="ActivePlayerState"/> usage. Once turn
/// sequencing becomes authoritative, legacy active player state may be replaced by a TurnState projection. The
/// mutator only executes when <c>FeatureFlags.EnableTurnSequencing</c> is true and will otherwise return the
/// original <see cref="GameState"/> unchanged. No LINQ is used inside the tight branch except for simple sequencing
/// which is not considered a hot path (turn boundary events are sparse relative to move events).
/// </remarks>
/// <summary>
/// State mutator advancing the turn segment or, when at the final segment, incrementing the numeric turn and rotating the active player.
/// </summary>
/// <remarks>
/// Optimized to avoid LINQ allocations in the hot path: simple loops replace <c>FirstOrDefault</c>/<c>Concat</c>/<c>SkipWhile</c> patterns.
/// </remarks>
internal sealed class TurnAdvanceStateMutator : IStateMutator<EndTurnSegmentEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EndTurnSegmentEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return gameState; // inert when sequencing disabled
        }

        // Locate existing TurnState (shadow mode injects exactly one)
        TurnState currentTurnState = null;
        foreach (var ts in gameState.GetStates<TurnState>())
        {
            currentTurnState = ts; // only one expected; take first
            break;
        }
        if (currentTurnState is null) { return gameState; }

        var turnArtifact = currentTurnState.Artifact;
        if (currentTurnState.Segment != @event.Segment)
        {
            return gameState; // segment mismatch: ignore (rule should prevent this path)
        }

        var profile = TurnProfile.Default; // future: injectable
        if (!profile.IsLast(@event.Segment))
        {
            // Not last segment: begin next segment instead of full advancement (will be separate mutator in future)
            var nextSegment = profile.Next(@event.Segment)!.Value;
            var progressed = new TurnState(turnArtifact, currentTurnState.TurnNumber, nextSegment, currentTurnState.PassStreak);
            return gameState.Next([progressed]);
        }

        // Last segment: advance numeric turn and reset to Start + rotate active player (if any)
        var advancedTurnState = new TurnState(turnArtifact, currentTurnState.TurnNumber + 1, TurnSegment.Start, 0);

        // Active player projection compatibility layer: rotate exactly one active player if states exist
        ActivePlayerState currentActive = null;
        var activePlayerStates = gameState.GetStates<ActivePlayerState>();
        foreach (var aps in activePlayerStates)
        {
            if (aps.IsActive)
            {
                currentActive = aps; break;
            }
        }
        if (currentActive is null) { return gameState.Next([advancedTurnState]); }

        var playersEnumerable = engine.Game.Players;
        if (playersEnumerable is null) { return gameState.Next([advancedTurnState]); }
        // Materialize once to avoid multiple enumeration and enable indexing
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