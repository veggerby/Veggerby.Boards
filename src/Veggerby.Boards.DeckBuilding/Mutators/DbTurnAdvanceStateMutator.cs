using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Minimal public wrapper applying turn advancement semantics for deck-building by delegating to internal logic pattern.
/// Replicates simplified behavior of internal TurnAdvanceStateMutator (non-last -> next segment; last -> new turn Start).
/// </summary>
public sealed class DbTurnAdvanceStateMutator : IStateMutator<EndTurnSegmentEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EndTurnSegmentEvent @event)
    {
        // locate existing TurnState
        TurnState current = null;
        foreach (var ts in gameState.GetStates<TurnState>()) { current = ts; break; }
        if (current is null) { return gameState; }
        if (current.Segment != @event.Segment) { return gameState; }
        // Hard-coded profile: Start -> Main -> End
        TurnSegment? next = current.Segment switch
        {
            TurnSegment.Start => TurnSegment.Main,
            TurnSegment.Main => TurnSegment.End,
            _ => null
        };
        if (next is not null)
        {
            var progressed = new TurnState(current.Artifact, current.TurnNumber, next.Value, current.PassStreak);
            return gameState.Next([progressed]);
        }
        // last segment -> increment turn number, reset to Start
        var advanced = new TurnState(current.Artifact, current.TurnNumber + 1, TurnSegment.Start, 0);
        return gameState.Next([advanced]);
    }
}