using System;
using System.Linq;

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
        var currentTurnState = gameState.GetStates<TurnState>().FirstOrDefault();
        if (currentTurnState is null)
        {
            return gameState; // no turn state present
        }

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
        var activePlayerStates = gameState.GetStates<ActivePlayerState>();
        var currentActive = activePlayerStates.FirstOrDefault(x => x.IsActive);
        if (currentActive is null || !activePlayerStates.Any())
        {
            return gameState.Next([advancedTurnState]);
        }

        // Determine next player in circular order
        var players = engine.Game.Players;
        if (players is null || !players.Any())
        {
            return gameState.Next([advancedTurnState]);
        }

        var nextPlayer = players
            .Concat(players) // wrap
            .SkipWhile(p => !p.Equals(currentActive.Artifact))
            .Skip(1)
            .First();

        if (nextPlayer.Equals(currentActive.Artifact))
        {
            // Single player edge case – still advance turn state only
            return gameState.Next([advancedTurnState]);
        }

        var previousPlayerProjection = new ActivePlayerState(currentActive.Artifact, false);
        var nextPlayerProjection = new ActivePlayerState(nextPlayer, true);

        return gameState.Next([advancedTurnState, previousPlayerProjection, nextPlayerProjection]);
    }
}